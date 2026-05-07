using Microsoft.EntityFrameworkCore;
using WebsiteCore.Data;
using WebsiteCore.Data.Entities;

namespace WebsiteCore.Tests;

/// <summary>Helper tạo TtytlpDbContext in-memory với data cơ bản đã seed.</summary>
internal static class InMemoryDb
{
    public static TtytlpDbContext NewDb(string? name = null)
    {
        var opts = new DbContextOptionsBuilder<TtytlpDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new TtytlpDbContext(opts);
    }

    public static (Site site, Category mainCat) Seed(TtytlpDbContext db)
    {
        var site = new Site
        {
            Id = Guid.NewGuid(),
            NameCompanyL = "TTYT Test",
            ActiveFlag = 1,
            Ord = 1
        };
        db.Sites.Add(site);

        var dept = new Department
        {
            Id = Guid.NewGuid(),
            NameL = "Khoa Khám bệnh",
            Alias = "khoa-kham-benh",
            ActiveFlag = 1,
            Ord = 1,
            SiteId = site.Id,
            IsClinicalDept = false
        };
        db.Departments.Add(dept);

        var cat = new Category
        {
            Id = Guid.NewGuid(),
            NameL = "Tin tức",
            AliasL = "tin-tuc",
            Type = "MAIN_MENU",
            Level = 1,
            ParentId = null,
            ActiveFlag = 1,
            ShowOnHome = true,
            Ord = 6,
            SiteId = site.Id
        };
        db.Categories.Add(cat);

        db.SaveChanges();
        return (site, cat);
    }
}
