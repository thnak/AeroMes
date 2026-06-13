namespace AeroMes.Domain.Wms;

public enum ZoneType { Receiving, Storage, Staging, Shipping, Scrap }
public enum TemperatureZone { Ambient, Cold, Frozen }
public enum BinType { Bulk, Pick, Overflow, Quarantine, QcHold }
public enum PoStatus { Draft, Confirmed, PartiallyReceived, FullyReceived, Closed, Cancelled }
public enum GrnStatus { Draft, Confirmed, Quarantine, Rejected }
public enum QcStatus { Pending, Passed, Quarantine, Rejected }
public enum MovementType { Receive, Issue, Transfer, Adjust, Return }
public enum FactoryReceiptType { ProductRequest, MaterialSupplyRequest, Other }
public enum FactoryReceiptStatus { Draft, Confirmed }
public enum FactoryExportType { MaterialIssuance, Scrap, Transfer, Other }
public enum FactoryExportStatus { Draft, Confirmed }
public enum MaterialTransferType { Internal, BranchTransfer, Other }
public enum MaterialTransferStatus { Draft, Confirmed }
public enum MaterialSupplyRequestType { MaterialIssuance, ProductionRequest, Other }
public enum MaterialSupplyRequestStatus { Draft, Submitted, Approved, Rejected }
public enum MaterialRequisitionStatus { Draft, Sent, Recalled, Fulfilled }
public enum IntakeRequestPurpose { ManufacturedOutput, SubcontractedGoods, SubcontractExcessMaterials, DisassemblyRecovery, OtherSurplus }
public enum IntakeWarehouseType { FactoryWarehouse, CompanyWarehouse }
public enum IntakeRequestStatus { Draft, Sent, Recalled, Received }
public enum CycleCountPlanType { Full, AbcA, AbcB, LocationBased, RandomSample }
public enum CycleCountPlanStatus { Draft, InProgress, PendingApproval, Completed, Cancelled }
public enum CycleCountLineStatus { Pending, Counted, Approved, Rejected }
public enum ReplenishmentAlertStatus { Open, Acknowledged, PoCreated, Resolved }
public enum ReturnDirection { SupplierReturn, CustomerReturn }
public enum RmaStatus { Draft, Authorized, Received, Dispositioned, Closed, Cancelled }
public enum RmaDisposition { Rework, Scrap, ReturnToStock, ReturnToSupplier, Quarantine }
public enum ShipmentStatus { Draft, Picking, Packed, Dispatched, Cancelled }
public enum PickListStatus { Pending, InProgress, Completed }
public enum CartonStatus { Open, Sealed, Shipped }
