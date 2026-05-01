using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

public class SiteServiceTests
{
    [Fact]
    public async Task GetCurrentAsync_ReturnsActiveLowestOrd()
    {
        await using var db = InMemoryDb.NewDb();
        db.Sites.AddRange(
            new Site { Id = Guid.NewGuid(), NameCompanyL = "Inactive",  ActiveFlag = 0, Ord = 1 },
            new Site { Id = Guid.NewGuid(), NameCompanyL = "Secondary", ActiveFlag = 1, Ord = 5 },
            new Site { Id = Guid.NewGuid(), NameCompanyL = "Primary",   ActiveFlag = 1, Ord = 1 }
        );
        await db.SaveChangesAsync();

        var s = await new SiteService(db).GetCurrentAsync();
        Assert.NotNull(s);
        Assert.Equal("Primary", s!.NameCompanyL);
    }

    [Fact]
    public async Task UpdateDashboardImageAsync_PersistsPath()
    {
        await using var db = InMemoryDb.NewDb();
        var site = new Site { Id = Guid.NewGuid(), NameCompanyL = "X", ActiveFlag = 1, Ord = 1 };
        db.Sites.Add(site);
        await db.SaveChangesAsync();

        var svc = new SiteService(db);
        await svc.UpdateDashboardImageAsync(site.Id, "/assets/admin/images/dashboard/new.jpg");
        var saved = await svc.GetDashboardImageAsync(site.Id);
        Assert.Equal("/assets/admin/images/dashboard/new.jpg", saved);
    }

    [Fact]
    public async Task GetAllActiveAsync_ExcludesInactive()
    {
        await using var db = InMemoryDb.NewDb();
        db.Sites.AddRange(
            new Site { Id = Guid.NewGuid(), NameCompanyL = "A", ActiveFlag = 1, Ord = 1 },
            new Site { Id = Guid.NewGuid(), NameCompanyL = "B", ActiveFlag = 0, Ord = 2 }
        );
        await db.SaveChangesAsync();
        var list = await new SiteService(db).GetAllActiveAsync();
        Assert.Single(list);
        Assert.Equal("A", list[0].NameCompanyL);
    }
}
