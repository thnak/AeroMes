#!/usr/bin/env python3
"""
Batch-transforms EntityNotFoundException usages in the Application layer.

Transformations applied:
  - Void command handlers (ICommandHandler<TCmd>)
      → Upgraded to ICommandHandler<TCmd, ValidationResult<Unit>>
      → Command record changed from ICommand to ICommand<ValidationResult<Unit>>
      → Controllers updated: await SendAsync → var r = await ...; if (!r.IsSuccess)...
  - Result command handlers (already return ValidationResult<T>)
      → ?? throw new EntityNotFoundException → null check + return NotFound
      → catch (EntityNotFoundException) blocks removed
  - All handlers: catch (EntityNotFoundException) blocks removed (defensive catches)
  - All handlers: using AeroMes.Domain.Exceptions removed when no longer needed
"""

import os
import re

SRC = "/home/nvthanh/works2/AeroMes/src"
APP = os.path.join(SRC, "AeroMes.Application")

# ── I/O ───────────────────────────────────────────────────────────────────────

def read(path):
    with open(path, encoding="utf-8") as f:
        return f.read()

def write(path, content):
    with open(path, "w", encoding="utf-8") as f:
        f.write(content)

# ── Using-directive helpers ───────────────────────────────────────────────────

def ensure_using(content, using_line):
    if using_line in content:
        return content
    lines = content.split("\n")
    last_using = max((i for i, l in enumerate(lines) if l.startswith("using ")), default=-1)
    if last_using >= 0:
        lines.insert(last_using + 1, using_line)
    else:
        lines.insert(0, using_line)
    return "\n".join(lines)

def remove_using(content, using_line):
    stripped = using_line.strip()
    return "\n".join(l for l in content.split("\n") if l.strip() != stripped)

# ── Block removal ─────────────────────────────────────────────────────────────

def remove_entity_not_found_catch_blocks(content):
    """Remove all  catch (EntityNotFoundException ...)  { ... }  blocks."""
    pat = re.compile(r'\s*catch\s*\(\s*EntityNotFoundException[^)]*\)\s*\{', re.MULTILINE)
    while True:
        m = pat.search(content)
        if not m:
            break
        start = m.start()
        open_brace = content.index('{', m.start())
        depth, i = 0, open_brace
        while i < len(content):
            if content[i] == '{':
                depth += 1
            elif content[i] == '}':
                depth -= 1
                if depth == 0:
                    break
            i += 1
        end = i + 1
        if end < len(content) and content[end] == '\n':
            end += 1
        content = content[:start] + content[end:]
    return content

# ── Throw-to-return replacements ──────────────────────────────────────────────

def replace_null_throw(content, vr_type):
    """
    Replace:
        var x = await something()\n            ?? throw new EntityNotFoundException("Ent", id);
    With:
        var x = await something();
        if (x is null) return ValidationResult<T>.NotFound($"Ent '{id}' was not found.");
    """
    pat = re.compile(
        r'([ \t]*)(var (\w+) = await [^\n]+)\n'
        r'[ \t]+\?\? throw new EntityNotFoundException\("([^"]+)",\s*([^)]+)\);',
        re.MULTILINE
    )
    def repl(m):
        indent   = m.group(1)
        assign   = m.group(2).rstrip()
        varname  = m.group(3)
        entname  = m.group(4)
        key      = m.group(5).strip()
        msg      = f"{entname} '{{{key}}}' was not found."
        return (
            f"{indent}{assign};\n"
            f"{indent}if ({varname} is null) return {vr_type}.NotFound($\"{msg}\");"
        )
    return pat.sub(repl, content)

def replace_nameof_null_throw(content, vr_type):
    """Same but entity name given as nameof(...)."""
    pat = re.compile(
        r'([ \t]*)(var (\w+) = await [^\n]+)\n'
        r'[ \t]+\?\? throw new EntityNotFoundException\(nameof\([^)]+\),\s*([^)]+)\);',
        re.MULTILINE
    )
    def repl(m):
        indent   = m.group(1)
        assign   = m.group(2).rstrip()
        varname  = m.group(3)
        key      = m.group(4).strip()
        msg      = f"Entity '{{{key}}}' was not found."
        return (
            f"{indent}{assign};\n"
            f"{indent}if ({varname} is null) return {vr_type}.NotFound($\"{msg}\");"
        )
    return pat.sub(repl, content)

def replace_standalone_throw(content, vr_type):
    """Replace bare  throw new EntityNotFoundException("Ent", id);  with a return."""
    pat = re.compile(
        r'([ \t]*)throw new EntityNotFoundException\("([^"]+)",\s*([^)]+)\);',
        re.MULTILINE
    )
    def repl(m):
        indent   = m.group(1)
        entname  = m.group(2)
        key      = m.group(3).strip()
        msg      = f"{entname} '{{{key}}}' was not found."
        return f"{indent}return {vr_type}.NotFound($\"{msg}\");"
    return pat.sub(repl, content)

def replace_all_throws(content, vr_type):
    content = replace_null_throw(content, vr_type)
    content = replace_nameof_null_throw(content, vr_type)
    content = replace_standalone_throw(content, vr_type)
    return content

# ── Handler type detection ────────────────────────────────────────────────────

def is_void_handler(content):
    """ICommandHandler<SingleType> — no ValidationResult, no query."""
    return bool(re.search(r': ICommandHandler<\w+>', content))

def get_validation_result_type(content):
    m = re.search(r'ValidationResult<([^>]+)>', content)
    return f"ValidationResult<{m.group(1)}>" if m else "ValidationResult<Unit>"

# ── Add return Ok at end of handler ──────────────────────────────────────────

def insert_return_ok_after_save(content):
    """
    Insert  return ValidationResult<Unit>.Ok(Unit.Value);
    right after  await uow.SaveChangesAsync(...);
    when followed by the closing brace of the method.
    """
    pat = re.compile(
        r'([ \t]*)(await uow\.SaveChangesAsync\([^)]*\);)(\s*\n)([ \t]*\})',
        re.MULTILINE
    )
    def repl(m):
        indent   = m.group(1)
        save     = m.group(2)
        nl       = m.group(3)
        closing  = m.group(4)
        return f"{indent}{save}{nl}{indent}return ValidationResult<Unit>.Ok(Unit.Value);{nl}{closing}"
    # Only replace first occurrence (inside the handler method)
    return pat.sub(repl, content, count=1)

# ── Transform functions ───────────────────────────────────────────────────────

def transform_void_handler(content):
    """Upgrade void ICommandHandler<TCmd> to ICommandHandler<TCmd, ValidationResult<Unit>>."""
    # 1. Change class declaration
    content = re.sub(
        r': ICommandHandler<(\w+)>',
        r': ICommandHandler<\1, ValidationResult<Unit>>',
        content, count=1
    )
    # 2. Change HandleAsync signature
    content = re.sub(
        r'public async Task HandleAsync\(',
        'public async Task<ValidationResult<Unit>> HandleAsync(',
        content, count=1
    )
    # 3. Replace throws
    content = replace_all_throws(content, "ValidationResult<Unit>")
    # 4. Insert return Ok after SaveChanges
    content = insert_return_ok_after_save(content)
    # 5. Remove EntityNotFoundException catches
    content = remove_entity_not_found_catch_blocks(content)
    # 6. Add/remove usings
    content = ensure_using(content, "using AeroMes.Application.Common;")
    content = remove_using(content, "using AeroMes.Domain.Exceptions;")
    return content

def transform_result_handler(content):
    """Result command handler: replace throws with null-check returns, remove dead catches."""
    vr_type = get_validation_result_type(content)
    content = replace_all_throws(content, vr_type)
    content = remove_entity_not_found_catch_blocks(content)
    if "EntityNotFoundException" not in content:
        content = remove_using(content, "using AeroMes.Domain.Exceptions;")
    return content

def clean_defensive_catch(content):
    """Remove EntityNotFoundException catch blocks from handlers that never throw it."""
    content = remove_entity_not_found_catch_blocks(content)
    if "EntityNotFoundException" not in content:
        content = remove_using(content, "using AeroMes.Domain.Exceptions;")
    return content

# ── Command record transformation ─────────────────────────────────────────────

def transform_command_record(content):
    """Change  : ICommand;  or  : ICommand,  to  : ICommand<ValidationResult<Unit>>."""
    if "ValidationResult<Unit>" in content:
        return content
    # Trailing ICommand at end of declaration
    content = re.sub(r':\s*ICommand\s*;', ': ICommand<ValidationResult<Unit>>;', content)
    # ICommand as last type in a base-list followed by more (unlikely but safe)
    content = re.sub(r':\s*ICommand\b(?!<)', ': ICommand<ValidationResult<Unit>>', content)
    content = ensure_using(content, "using AeroMes.Application.Common;")
    return content

# ── Controller transformation ─────────────────────────────────────────────────

def transform_controller_for_void_cmd(content, cmd_name):
    """
    Update:
        await commandMediator.SendAsync(new CmdName(...), null, ct);
        return NoContent();   (or return Ok(...) etc.)
    To:
        var result = await commandMediator.SendAsync(new CmdName(...), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    """
    # Match SendAsync call containing cmd_name, ending with ; then return
    pat = re.compile(
        r'([ \t]*)await commandMediator\.SendAsync\(((?:[^;]|\n)*?' + re.escape(cmd_name) + r'(?:[^;]|\n)*?)\);'
        r'((?:\s*\n)+)([ \t]*)(return [^\n]+;)',
        re.MULTILINE
    )
    def repl(m):
        indent1  = m.group(1)
        args     = m.group(2)
        between  = m.group(3)
        indent2  = m.group(4)
        ret      = m.group(5)
        return (
            f"{indent1}var result = await commandMediator.SendAsync({args});"
            f"{between}{indent1}if (!result.IsSuccess) return result.ToErrorResult();"
            f"{between}{indent2}{ret}"
        )
    return pat.sub(repl, content)

# ── Main ──────────────────────────────────────────────────────────────────────

# Void handlers that throw EntityNotFoundException — to upgrade to ValidationResult<Unit>
VOID_HANDLER_RELS = [
    "Auth/ApiKeys/Commands/RevokeApiKey/RevokeApiKeyHandler.cs",
    "Auth/Sessions/Commands/RevokeSession/RevokeSessionHandler.cs",
    "Auth/Sessions/Commands/RevokeAllSessions/RevokeAllSessionsHandler.cs",
    "Auth/PermissionOverrides/Commands/RemovePermissionOverride/RemovePermissionOverrideHandler.cs",
    "Auth/Roles/Commands/SetRolePermissions/SetRolePermissionsHandler.cs",
    "Master/AlertThresholds/Commands/DeleteAlertThreshold/DeleteAlertThresholdHandler.cs",
    "Master/BomItems/Commands/DeleteBomItem/DeleteBomItemHandler.cs",
    "Master/Boms/Commands/ActivateBomVersion/ActivateBomVersionHandler.cs",
    "Master/Boms/Commands/ApproveBom/ApproveBomHandler.cs",
    "Master/Boms/Commands/SubmitBomForReview/SubmitBomForReviewHandler.cs",
    "Master/Customers/Commands/DeleteCustomer/DeleteCustomerHandler.cs",
    "Master/Customers/Commands/RemoveCustomerPartNumber/RemoveCustomerPartNumberHandler.cs",
    "Master/Customers/Commands/RemoveCustomerQualitySpec/RemoveCustomerQualitySpecHandler.cs",
    "Master/DowntimeReasonCodes/Commands/DeleteDowntimeReasonCode/DeleteDowntimeReasonCodeHandler.cs",
    "Master/Employees/Commands/DeleteEmployee/DeleteEmployeeHandler.cs",
    "Master/Employees/Commands/EndShiftAssignment/EndShiftAssignmentHandler.cs",
    "Master/Employees/Commands/RemoveEmployeeSkill/RemoveEmployeeSkillHandler.cs",
    "Master/Employees/Commands/RemoveShiftAssignment/RemoveShiftAssignmentHandler.cs",
    "Master/EngChanges/Commands/ApproveEngChange/ApproveEngChangeHandler.cs",
    "Master/EngChanges/Commands/RejectEngChange/RejectEngChangeHandler.cs",
    "Master/EngChanges/Commands/SubmitEngChangeForReview/SubmitEngChangeForReviewHandler.cs",
    "Master/MachineProductConfigs/Commands/DeleteMachineProductConfig/DeleteMachineProductConfigHandler.cs",
    "Master/Machines/Commands/DeleteMachine/DeleteMachineHandler.cs",
    "Master/Molds/Commands/DeleteMold/DeleteMoldHandler.cs",
    "Master/Molds/Commands/RemoveMoldProduct/RemoveMoldProductHandler.cs",
    "Master/Molds/Commands/ScrapMold/ScrapMoldHandler.cs",
    "Master/Molds/Commands/UnmountMold/UnmountMoldHandler.cs",
    "Master/Operations/Commands/DeleteOperation/DeleteOperationHandler.cs",
    "Master/ProductAttributes/Commands/DeleteProductAttribute/DeleteProductAttributeHandler.cs",
    "Master/ProductAttributes/Commands/RemoveAttributeValue/RemoveAttributeValueHandler.cs",
    "Master/ProductAttributes/Commands/UnassignAttributeFromProduct/UnassignAttributeFromProductHandler.cs",
    "Master/ProductCategories/Commands/DeleteProductCategory/DeleteProductCategoryHandler.cs",
    "Master/ProductionTeams/Commands/DeleteProductionTeam/DeleteProductionTeamHandler.cs",
    "Master/ProductionTeams/Commands/RemoveTeamMember/RemoveTeamMemberHandler.cs",
    "Master/Products/Commands/DeleteProduct/DeleteProductHandler.cs",
    "Master/Products/Commands/RemoveProductSpecification/RemoveProductSpecificationHandler.cs",
    "Master/Products/Commands/RemoveProductUoMConversion/RemoveProductUoMConversionHandler.cs",
    "Master/Routings/Commands/DeleteRouting/DeleteRoutingHandler.cs",
    "Master/Routings/Commands/DeleteRoutingStep/DeleteRoutingStepHandler.cs",
    "Master/ShiftTemplates/Commands/DeleteShiftTemplate/DeleteShiftTemplateHandler.cs",
    "Master/StorageLocations/Commands/DeleteStorageLocation/DeleteStorageLocationHandler.cs",
    "Master/Suppliers/Commands/DeleteSupplier/DeleteSupplierHandler.cs",
    "Master/Suppliers/Commands/RemoveAvlItem/RemoveAvlItemHandler.cs",
    "Master/Tools/Commands/DeleteTool/DeleteToolHandler.cs",
    "Master/Tools/Commands/RemoveToolOperation/RemoveToolOperationHandler.cs",
    "Master/Tools/Commands/ScrapTool/ScrapToolHandler.cs",
    "Master/WorkCalendars/Commands/DeleteWorkCalendar/DeleteWorkCalendarHandler.cs",
    "Master/WorkCalendars/Commands/RemoveCalendarException/RemoveCalendarExceptionHandler.cs",
    "Master/WorkCenters/Commands/DeleteWorkCenter/DeleteWorkCenterHandler.cs",
    "Master/WorkOrderAutoRules/Commands/DeleteWorkOrderAutoRules/DeleteWorkOrderAutoRulesHandler.cs",
    "Master/WorkShifts/Commands/DeleteWorkShift/DeleteWorkShiftHandler.cs",
    "Quality/DefectCodes/Commands/DeleteDefectCode/DeleteDefectCodeHandler.cs",
    # Note: RotateApiKey is NOT void (returns RotateApiKeyResult) — handled separately
]

# Result handlers with EntityNotFoundException throws to be replaced
RESULT_HANDLER_RELS = [
    "Auth/PermissionOverrides/Commands/AddPermissionOverride/AddPermissionOverrideHandler.cs",
    "Downtime/Commands/EndDowntime/EndDowntimeHandler.cs",
    "Jobs/Commands/FinishJob/FinishJobHandler.cs",
    "Jobs/Commands/StartJob/StartJobHandler.cs",
    "Master/BomItems/Commands/UpdateBomItem/UpdateBomItemHandler.cs",
    "Master/Boms/Commands/UpdateBomLines/UpdateBomLinesHandler.cs",
    "Master/Customers/Commands/AddCustomerPartNumber/AddCustomerPartNumberHandler.cs",
    "Master/Customers/Commands/SetCustomerQualitySpec/SetCustomerQualitySpecHandler.cs",
    "Master/Customers/Commands/UpdateCustomerPartNumber/UpdateCustomerPartNumberHandler.cs",
    "Master/Employees/Commands/AddShiftAssignment/AddShiftAssignmentHandler.cs",
    "Master/Employees/Commands/SetEmployeeSkill/SetEmployeeSkillHandler.cs",
    "Master/Employees/Commands/UpdateEmployee/UpdateEmployeeHandler.cs",
    "Master/EngChanges/Commands/CreateEcoFromEcr/CreateEcoFromEcrHandler.cs",
    "Master/EngChanges/Commands/ImplementEco/ImplementEcoHandler.cs",
    "Master/MachineProductConfigs/Commands/UpsertMachineProductConfig/UpsertMachineProductConfigHandler.cs",
    "Master/MachineProductParams/Commands/DeleteMachineProductParam/DeleteMachineProductParamHandler.cs",
    "Master/MachineProductParams/Commands/UpsertMachineProductParam/UpsertMachineProductParamHandler.cs",
    "Master/Machines/Commands/UpdateMachine/UpdateMachineHandler.cs",
    "Master/Machines/Commands/UpdateMachineCapacity/UpdateMachineCapacityHandler.cs",
    "Master/Molds/Commands/AddMoldProduct/AddMoldProductHandler.cs",
    "Master/Molds/Commands/AssignMoldToMachine/AssignMoldToMachineHandler.cs",
    "Master/Molds/Commands/CompleteMoldMaintenance/CompleteMoldMaintenanceHandler.cs",
    "Master/Molds/Commands/RecordMoldShots/RecordMoldShotsHandler.cs",
    "Master/Molds/Commands/RegisterMold/RegisterMoldHandler.cs",
    "Master/Molds/Commands/SendMoldForMaintenance/SendMoldForMaintenanceHandler.cs",
    "Master/Molds/Commands/UpdateMold/UpdateMoldHandler.cs",
    "Master/Operations/Commands/UpdateOperation/UpdateOperationHandler.cs",
    "Master/OperatorCertifications/Commands/RecordOperatorCertification/RecordOperatorCertificationHandler.cs",
    "Master/ProductAttributes/Commands/AddAttributeValue/AddAttributeValueHandler.cs",
    "Master/ProductAttributes/Commands/AssignAttributeToProduct/AssignAttributeToProductHandler.cs",
    "Master/ProductAttributes/Commands/UpdateAttributeValue/UpdateAttributeValueHandler.cs",
    "Master/ProductAttributes/Commands/UpdateProductAttribute/UpdateProductAttributeHandler.cs",
    "Master/ProductCategories/Commands/UpdateProductCategory/UpdateProductCategoryHandler.cs",
    "Master/ProductionTeams/Commands/AddTeamMember/AddTeamMemberHandler.cs",
    "Master/ProductionTeams/Commands/DuplicateProductionTeam/DuplicateProductionTeamHandler.cs",
    "Master/ProductionTeams/Commands/UpdateProductionTeam/UpdateProductionTeamHandler.cs",
    "Master/Products/Commands/AddProductSpecification/AddProductSpecificationHandler.cs",
    "Master/Products/Commands/AddProductUoMConversion/AddProductUoMConversionHandler.cs",
    "Master/Products/Commands/ChangeLifecycleStatus/ChangeLifecycleStatusHandler.cs",
    "Master/Products/Commands/CreateProductVariant/CreateProductVariantHandler.cs",
    "Master/Products/Commands/UpdateProduct/UpdateProductHandler.cs",
    "Master/Products/Commands/UpdateProductSpecification/UpdateProductSpecificationHandler.cs",
    "Master/Products/Commands/UpdateProductUoMConversion/UpdateProductUoMConversionHandler.cs",
    "Master/Routings/Commands/AddRoutingStep/AddRoutingStepHandler.cs",
    "Master/Routings/Commands/UpdateRouting/UpdateRoutingHandler.cs",
    "Master/ShiftTemplates/Commands/UpdateShiftTemplate/UpdateShiftTemplateHandler.cs",
    "Master/StorageLocations/Commands/UpdateStorageLocation/UpdateStorageLocationHandler.cs",
    "Master/Suppliers/Commands/AddAvlItem/AddAvlItemHandler.cs",
    "Master/Suppliers/Commands/UpdateAvlItem/UpdateAvlItemHandler.cs",
    "Master/Tools/Commands/AddToolOperation/AddToolOperationHandler.cs",
    "Master/Tools/Commands/CheckoutTool/CheckoutToolHandler.cs",
    "Master/Tools/Commands/RecordToolMaintenance/RecordToolMaintenanceHandler.cs",
    "Master/Tools/Commands/RecordToolUsage/RecordToolUsageHandler.cs",
    "Master/Tools/Commands/ReturnTool/ReturnToolHandler.cs",
    "Master/Tools/Commands/SendToolForService/SendToolForServiceHandler.cs",
    "Master/Tools/Commands/UpdateTool/UpdateToolHandler.cs",
    "Master/UnitOfMeasures/Commands/UpdateUoM/UpdateUoMHandler.cs",
    "Master/WorkCalendars/Commands/AddCalendarException/AddCalendarExceptionHandler.cs",
    "Master/WorkCalendars/Commands/UpdateWorkCalendar/UpdateWorkCalendarHandler.cs",
    "Master/WorkCenters/Commands/UpdateWorkCenter/UpdateWorkCenterHandler.cs",
    "Master/WorkOrderAutoRules/Commands/UpsertWorkOrderAutoRules/UpsertWorkOrderAutoRulesHandler.cs",
    "Production/Commands/SubmitOutput/SubmitOutputHandler.cs",
    "Quality/DefectCodes/Commands/UpdateDefectCode/UpdateDefectCodeHandler.cs",
    "Wms/Commands/AddGrnLine/AddGrnLineHandler.cs",
    "Wms/Commands/ConfirmGrn/ConfirmGrnHandler.cs",
    "Wms/Commands/ConfirmPurchaseOrder/ConfirmPurchaseOrderHandler.cs",
    "Wms/Commands/CreateGrn/CreateGrnHandler.cs",
    "Wms/Commands/CreatePurchaseOrder/CreatePurchaseOrderHandler.cs",
    "WorkOrders/Commands/StartWorkOrder/StartWorkOrderHandler.cs",
]

def find_command_record(handler_path):
    d = os.path.dirname(handler_path)
    name = os.path.basename(handler_path).replace("Handler.cs", "Command.cs")
    c = os.path.join(d, name)
    return c if os.path.exists(c) else None

def find_controllers_using(cmd_name):
    ctrl_dir = os.path.join(SRC, "AeroMes.Api", "Controllers")
    results = []
    for fname in os.listdir(ctrl_dir):
        if not fname.endswith(".cs"):
            continue
        path = os.path.join(ctrl_dir, fname)
        if cmd_name in read(path):
            results.append(path)
    return results

def main():
    upgraded_commands = []  # (cmd_name, handler_path) for void→result upgrades

    # ── 1. Void command handlers ───────────────────────────────────────────────
    print("\n── Void command handlers ─────────────────────────────────────────────")
    for rel in VOID_HANDLER_RELS:
        path = os.path.join(APP, rel)
        if not os.path.exists(path):
            print(f"  MISS  {rel}")
            continue
        content = read(path)
        if not is_void_handler(content):
            print(f"  SKIP (not void)  {rel}")
            continue
        if "throw new EntityNotFoundException" not in content:
            print(f"  SKIP (no throw)  {rel}")
            continue
        new = transform_void_handler(content)
        write(path, new)
        print(f"  OK    {rel}")

        # Command record
        cmd_path = find_command_record(path)
        if cmd_path:
            cmd_content = read(cmd_path)
            new_cmd = transform_command_record(cmd_content)
            if new_cmd != cmd_content:
                write(cmd_path, new_cmd)
                cmd_name = os.path.basename(cmd_path).replace(".cs", "")
                upgraded_commands.append((cmd_name, path))
                print(f"         ↳ command record: {cmd_name}")

    # ── 2. Result command handlers ─────────────────────────────────────────────
    print("\n── Result command handlers ───────────────────────────────────────────")
    for rel in RESULT_HANDLER_RELS:
        path = os.path.join(APP, rel)
        if not os.path.exists(path):
            print(f"  MISS  {rel}")
            continue
        content = read(path)
        if "EntityNotFoundException" not in content:
            continue  # nothing to do
        new = transform_result_handler(content)
        write(path, new)
        print(f"  OK    {rel}")

    # ── 3. Blanket sweep: remove defensive catches in all handlers ─────────────
    print("\n── Blanket defensive-catch removal ───────────────────────────────────")
    for root, dirs, files in os.walk(APP):
        for fname in files:
            if not fname.endswith("Handler.cs"):
                continue
            path = os.path.join(root, fname)
            content = read(path)
            if "EntityNotFoundException" not in content:
                continue
            if "throw new EntityNotFoundException" in content:
                continue  # Throw handlers already handled above
            new = clean_defensive_catch(content)
            if new != content:
                write(path, new)
                rel = os.path.relpath(path, APP)
                print(f"  CATCH {rel}")

    # ── 4. Controller updates for void→result upgrades ─────────────────────────
    print("\n── Controller updates ────────────────────────────────────────────────")
    for cmd_name, handler_path in upgraded_commands:
        controllers = find_controllers_using(cmd_name)
        for ctrl in controllers:
            content = read(ctrl)
            new = transform_controller_for_void_cmd(content, cmd_name)
            if new != content:
                write(ctrl, new)
                rel = os.path.relpath(ctrl, SRC)
                print(f"  CTRL  {rel}  ←  {cmd_name}")
            else:
                rel = os.path.relpath(ctrl, SRC)
                print(f"  SKIP  {rel}  ←  {cmd_name} (no pattern match)")

    print("\nDone. Run: dotnet build src/AeroMes.Api/ 2>&1 | grep ' error ' | head -40")

if __name__ == "__main__":
    main()
