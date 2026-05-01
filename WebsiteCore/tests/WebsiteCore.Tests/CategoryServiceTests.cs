using Microsoft.EntityFrameworkCore;
using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetMainMenuAsync_OrdersByOrdAscending()
    {
        await using var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);

        // Add multiple main-menu categories with explicit Ord
        db.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), NameL = "Liên hệ",     AliasL = "lien-he",     Type = "MAIN_MENU", Level = 1, ActiveFlag = 1, ShowOnHome = true, Ord = 7, SiteId = site.Id },
            new Category { Id = Guid.NewGuid(), NameL = "Chuyên Khoa", AliasL = "chuyen-khoa", Type = "MAIN_MENU", Level = 1, ActiveFlag = 1, ShowOnHome = true, Ord = 2, SiteId = site.Id },
            new Category { Id = Guid.NewGuid(), NameL = "Giới thiệu",  AliasL = "gioi-thieu",  Type = "MAIN_MENU", Level = 1, ActiveFlag = 1, ShowOnHome = true, Ord = 3, SiteId = site.Id }
        );
        await db.SaveChangesAsync();

        var svc = new CategoryService(db);
        var menu = await svc.GetMainMenuAsync(site.Id);

        // Phải đúng thứ tự: Chuyên Khoa(2) → Giới thiệu(3) → Tin tức(6) → Liên hệ(7)
        Assert.Equal(new[] { "chuyen-khoa", "gioi-thieu", "tin-tuc", "lien-he" },
                     menu.Select(m => m.AliasL).ToArray());
    }

    [Fact]
    public async Task GetMainMenuAsync_FiltersInactive()
    {
        await using var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        db.Categories.Add(new Category {
            Id = Guid.NewGuid(), NameL = "Hidden", AliasL = "hidden",
            Type = "MAIN_MENU", Level = 1, ActiveFlag = 0, ShowOnHome = true,
            Ord = 1, SiteId = site.Id
        });
        await db.SaveChangesAsync();

        var menu = await new CategoryService(db).GetMainMenuAsync(site.Id);
        Assert.DoesNotContain(menu, c => c.AliasL == "hidden");
    }

    [Fact]
    public async Task GetMainMenuAsync_FiltersShowOnHomeFalse()
    {
        await using var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        db.Categories.Add(new Category {
            Id = Guid.NewGuid(), NameL = "NotOnHome", AliasL = "noh",
            Type = "MAIN_MENU", Level = 1, ActiveFlag = 1, ShowOnHome = false,
            Ord = 1, SiteId = site.Id
        });
        await db.SaveChangesAsync();

        var menu = await new CategoryService(db).GetMainMenuAsync(site.Id);
        Assert.DoesNotContain(menu, c => c.AliasL == "noh");
    }

    [Fact]
    public async Task GetByAliasAsync_FindsActive()
    {
        await using var db = InMemoryDb.NewDb();
        InMemoryDb.Seed(db);
        var found = await new CategoryService(db).GetByAliasAsync("tin-tuc");
        Assert.NotNull(found);
        Assert.Equal("Tin tức", found!.NameL);
    }

    [Fact]
    public async Task GetByAliasAsync_ReturnsNullForUnknown()
    {
        await using var db = InMemoryDb.NewDb();
        InMemoryDb.Seed(db);
        var found = await new CategoryService(db).GetByAliasAsync("khong-ton-tai");
        Assert.Null(found);
    }
}
