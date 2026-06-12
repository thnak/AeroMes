using System.Net;
using System.Net.Http.Json;
using AeroMes.Application.Master.Boms.Queries.CompareBomVersions;
using AeroMes.Application.Master.Boms.Queries.ExplodeBom;
using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using AeroMes.Application.Master.Boms.Queries.GetBomVersions;
using AeroMes.Application.Master.EngChanges.Queries.GetEngChangeByNumber;

namespace AeroMes.IntegrationTests.Master;

[Collection("Integration")]
public class BomVersioningTests(AeroMesWebFactory factory)
{
    [Fact]
    public async Task Draft_Submit_Approve_Activate_Lifecycle()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);

        var create = await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions",
            new { Version = "1.0", BaseQuantity = 1m });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        // Empty draft cannot be submitted.
        var earlySubmit = await client.PostAsJsonAsync(
            $"/api/v1/bom/{ctx.Parent}/versions/1.0/submit", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, earlySubmit.StatusCode);

        var lines = await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/lines", new
        {
            Lines = new[]
            {
                new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 2m, UoMCode = ctx.Uom },
                new { LineNo = 20, ComponentCode = ctx.CompB, RequiredQty = 0.5m, UoMCode = ctx.Uom },
            },
        });
        Assert.Equal(HttpStatusCode.NoContent, lines.StatusCode);

        // No active BOM yet.
        var noActive = await client.GetAsync($"/api/v1/bom/{ctx.Parent}");
        Assert.Equal(HttpStatusCode.NotFound, noActive.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/submit", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/approve", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/activate", new { })).StatusCode);

        var active = await client.GetFromJsonAsync<BomVersionDetailDto>($"/api/v1/bom/{ctx.Parent}");
        Assert.Equal("1.0", active!.Version);
        Assert.Equal("Active", active.Status);
        Assert.NotNull(active.EffectiveFrom);
        Assert.Equal(2, active.Lines.Count);
        Assert.Equal(ctx.CompA, active.Lines[0].ComponentCode);

        // Lines are frozen once the version leaves Draft.
        var frozen = await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/lines", new
        {
            Lines = new[] { new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 9m, UoMCode = ctx.Uom } },
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, frozen.StatusCode);
    }

    [Fact]
    public async Task Activate_NewVersion_SupersedesPrevious()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);
        await CreateActiveBomAsync(client, ctx.Parent, "1.0",
            new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 2m, UoMCode = ctx.Uom });

        // v2.0 cloned from v1.0, re-pointed at component B.
        var clone = await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions",
            new { Version = "2.0", CloneFromVersion = "1.0" });
        Assert.Equal(HttpStatusCode.Created, clone.StatusCode);

        var cloned = await client.GetFromJsonAsync<BomVersionDetailDto>(
            $"/api/v1/bom/{ctx.Parent}/versions/2.0");
        Assert.Single(cloned!.Lines); // clone copied v1's line

        await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/2.0/lines", new
        {
            Lines = new[] { new { LineNo = 10, ComponentCode = ctx.CompB, RequiredQty = 3m, UoMCode = ctx.Uom } },
        });
        await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/2.0/submit", new { });
        await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/2.0/approve", new { });
        await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/2.0/activate", new { });

        var versions = await client.GetFromJsonAsync<List<BomVersionDto>>(
            $"/api/v1/bom/{ctx.Parent}/versions");
        Assert.Equal(2, versions!.Count);
        var v1 = Assert.Single(versions, x => x.Version == "1.0");
        Assert.Equal("Superseded", v1.Status);
        Assert.NotNull(v1.EffectiveTo);
        var v2 = Assert.Single(versions, x => x.Version == "2.0");
        Assert.Equal("Active", v2.Status);

        var active = await client.GetFromJsonAsync<BomVersionDetailDto>($"/api/v1/bom/{ctx.Parent}");
        Assert.Equal("2.0", active!.Version);

        // Duplicate version numbers are rejected.
        var duplicate = await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions",
            new { Version = "2.0" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);
    }

    [Fact]
    public async Task Compare_Versions_ReportsAddedRemovedChanged()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);
        await CreateActiveBomAsync(client, ctx.Parent, "1.0",
            new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 2m, UoMCode = ctx.Uom },
            new { LineNo = 20, ComponentCode = ctx.CompB, RequiredQty = 1m, UoMCode = ctx.Uom });

        await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions", new { Version = "2.0" });
        await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/2.0/lines", new
        {
            Lines = new[]
            {
                new { LineNo = 10, ComponentCode = ctx.CompB, RequiredQty = 4m, UoMCode = ctx.Uom },
                new { LineNo = 20, ComponentCode = ctx.CompC, RequiredQty = 1m, UoMCode = ctx.Uom },
            },
        });

        var compare = await client.GetFromJsonAsync<BomCompareDto>(
            $"/api/v1/bom/{ctx.Parent}/compare?from=1.0&to=2.0");

        Assert.Equal(ctx.CompC, Assert.Single(compare!.Added).ComponentCode);
        Assert.Equal(ctx.CompA, Assert.Single(compare.Removed).ComponentCode);
        var changed = Assert.Single(compare.Changed);
        Assert.Equal(ctx.CompB, changed.ComponentCode);
        Assert.Equal(1m, changed.OldRequiredQty);
        Assert.Equal(4m, changed.NewRequiredQty);
    }

    [Fact]
    public async Task Explode_MultiLevel_CompoundsQuantitiesAndScrap()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);

        // CompA is a sub-assembly: 3 × CompC per unit with 10% scrap.
        await CreateActiveBomAsync(client, ctx.CompA, "1.0",
            new { LineNo = 10, ComponentCode = ctx.CompC, RequiredQty = 3m, UoMCode = ctx.Uom, ScrapFactor = 10m });
        // Parent: 2 × CompA per unit.
        await CreateActiveBomAsync(client, ctx.Parent, "1.0",
            new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 2m, UoMCode = ctx.Uom });

        var exploded = await client.GetFromJsonAsync<List<ExplodedBomLineDto>>(
            $"/api/v1/bom/{ctx.Parent}/explode?quantity=10");

        Assert.Equal(2, exploded!.Count);
        var level1 = Assert.Single(exploded, x => x.Level == 1);
        Assert.Equal(ctx.CompA, level1.ComponentCode);
        Assert.Equal(20m, level1.TotalRequiredQty);
        Assert.True(level1.HasChildBom);

        var level2 = Assert.Single(exploded, x => x.Level == 2);
        Assert.Equal(ctx.CompC, level2.ComponentCode);
        Assert.Equal(ctx.CompA, level2.ParentCode);
        Assert.Equal(66m, level2.TotalRequiredQty); // 20 × 3 × 1.10
        Assert.False(level2.HasChildBom);

        // Product without an active BOM cannot be exploded.
        var missing = await client.GetAsync($"/api/v1/bom/{ctx.CompB}/explode");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task BomLines_ParentAsComponent_Returns422()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);
        await client.PostAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions", new { Version = "1.0" });

        var selfRef = await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/lines", new
        {
            Lines = new[] { new { LineNo = 10, ComponentCode = ctx.Parent, RequiredQty = 1m, UoMCode = ctx.Uom } },
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, selfRef.StatusCode);

        var unknownComponent = await client.PutAsJsonAsync($"/api/v1/bom/{ctx.Parent}/versions/1.0/lines", new
        {
            Lines = new[] { new { LineNo = 10, ComponentCode = "NO-SUCH-PART", RequiredQty = 1m, UoMCode = ctx.Uom } },
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownComponent.StatusCode);
    }

    [Fact]
    public async Task EngChange_EcrToEco_ImplementCreatesBomDraft()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ctx = await SeedProductsAsync(client);
        await CreateActiveBomAsync(client, ctx.Parent, "1.0",
            new { LineNo = 10, ComponentCode = ctx.CompA, RequiredQty = 2m, UoMCode = ctx.Uom });

        var ecrNumber = UniqueCode("ECR");
        var ecoNumber = UniqueCode("ECO");

        var createEcr = await client.PostAsJsonAsync("/api/v1/eng-changes", new
        {
            EcNumber = ecrNumber,
            EcType = "Ecr",
            Title = "Đổi vật liệu thân vỏ",
            Reason = "SupplierChange",
            Priority = "High",
            AffectedProducts = ctx.Parent,
        });
        Assert.Equal(HttpStatusCode.Created, createEcr.StatusCode);

        // An ECR cannot be implemented and cannot become an ECO before approval.
        var earlyEco = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/eco",
            new { NewEcNumber = ecoNumber });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, earlyEco.StatusCode);

        var submit = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/submit", new { });
        Assert.True(submit.IsSuccessStatusCode, $"submit failed: {await submit.Content.ReadAsStringAsync()}");
        var approve = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/approve", new { });
        Assert.True(approve.IsSuccessStatusCode, $"approve failed: {await approve.Content.ReadAsStringAsync()}");

        var createEco = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/eco",
            new { NewEcNumber = ecoNumber });
        Assert.True(createEco.StatusCode == HttpStatusCode.Created,
            $"eco create failed: {await createEco.Content.ReadAsStringAsync()}");

        // Implementing requires the ECO itself to be approved.
        var earlyImplement = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecoNumber}/implement",
            new { ProductCode = ctx.Parent, NewVersion = "2.0" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, earlyImplement.StatusCode);

        await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecoNumber}/approve", new { });
        var implement = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecoNumber}/implement",
            new { ProductCode = ctx.Parent, NewVersion = "2.0" });
        Assert.Equal(HttpStatusCode.Created, implement.StatusCode);

        var detail = await client.GetFromJsonAsync<EngChangeDetailDto>($"/api/v1/eng-changes/{ecoNumber}");
        Assert.Equal("Implemented", detail!.Summary.Status);
        Assert.Equal(ecrNumber, detail.Summary.SourceEcrNumber);
        Assert.Equal(ctx.Parent, detail.NewBomProductCode);
        Assert.Equal("2.0", detail.NewBomVersion);

        // The new draft was cloned from the active version and tagged with the ECO.
        var draft = await client.GetFromJsonAsync<BomVersionDetailDto>(
            $"/api/v1/bom/{ctx.Parent}/versions/2.0");
        Assert.Equal("Draft", draft!.Status);
        Assert.Equal(ecoNumber, draft.EcoReference);
        Assert.Equal(ctx.CompA, Assert.Single(draft.Lines).ComponentCode);
    }

    [Fact]
    public async Task EngChange_Reject_BlocksFurtherTransitions()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var ecrNumber = UniqueCode("ECR");

        await client.PostAsJsonAsync("/api/v1/eng-changes", new
        {
            EcNumber = ecrNumber,
            EcType = "Ecr",
            Title = "Sẽ bị từ chối",
            Reason = "Other",
        });

        var reject = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/reject", new { });
        Assert.Equal(HttpStatusCode.NoContent, reject.StatusCode);

        var approveAfterReject = await client.PostAsJsonAsync($"/api/v1/eng-changes/{ecrNumber}/approve", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, approveAfterReject.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string UniqueCode(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private sealed record SeededProducts(string Parent, string CompA, string CompB, string CompC, string Uom);

    private static async Task<SeededProducts> SeedProductsAsync(HttpClient client)
    {
        var uomCode = UniqueCode("EA")[..8];
        var uomResp = await client.PostAsJsonAsync("/api/v1/uom",
            new { Code = uomCode, Name = $"Each {uomCode}", Group = "Quantity" });
        Assert.True(uomResp.IsSuccessStatusCode, $"UoM create failed: {await uomResp.Content.ReadAsStringAsync()}");

        var codes = new string[4];
        for (var i = 0; i < codes.Length; i++)
        {
            codes[i] = UniqueCode("PRD");
            var resp = await client.PostAsJsonAsync("/api/v1/products",
                new { Code = codes[i], Name = $"Product {codes[i]}", BaseUoMCode = uomCode });
            Assert.True(resp.IsSuccessStatusCode, $"Product create failed: {await resp.Content.ReadAsStringAsync()}");
        }
        return new SeededProducts(codes[0], codes[1], codes[2], codes[3], uomCode);
    }

    private static async Task CreateActiveBomAsync(
        HttpClient client, string productCode, string version, params object[] lines)
    {
        var create = await client.PostAsJsonAsync($"/api/v1/bom/{productCode}/versions",
            new { Version = version });
        Assert.True(create.IsSuccessStatusCode, $"Draft create failed: {await create.Content.ReadAsStringAsync()}");

        var update = await client.PutAsJsonAsync($"/api/v1/bom/{productCode}/versions/{version}/lines",
            new { Lines = lines });
        Assert.True(update.IsSuccessStatusCode, $"Lines update failed: {await update.Content.ReadAsStringAsync()}");

        foreach (var step in new[] { "submit", "approve", "activate" })
        {
            var resp = await client.PostAsJsonAsync(
                $"/api/v1/bom/{productCode}/versions/{version}/{step}", new { });
            Assert.True(resp.IsSuccessStatusCode, $"{step} failed: {await resp.Content.ReadAsStringAsync()}");
        }
    }
}
