using WebsiteCore.Business.Services;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

public class NewsServiceTests
{
    private static News MakeNews(Guid siteId, string title, int activeFlag = 1, bool hot = true, bool home = false, DateTime? created = null, string? alias = null) =>
        new News
        {
            Id = Guid.NewGuid(),
            TitleL = title,
            AliasL = alias ?? title.ToLower().Replace(' ', '-'),
            DescriptionL = "desc " + title,
            ActiveFlag = activeFlag,
            HotNew = hot,
            ShowOnHome = home,
            CreatedDate = created ?? DateTime.Now,
            SiteId = siteId,
            Type = "NEWS"
        };

    [Fact]
    public async Task GetTopAsync_ReturnsActiveOnly()
    {
        await using var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        db.News.AddRange(
            MakeNews(site.Id, "Tin 1"),
            MakeNews(site.Id, "Tin 2", activeFlag: 0),  // inactive
            MakeNews(site.Id, "Tin 3", activeFlag: -2)  // deleted
        );
        await db.SaveChangesAsync();
        var top = await new NewsService(db).GetTopAsync(site.Id);
        Assert.Single(top);
        Assert.Equal("Tin 1", top[0].TitleL);
    }

    [Fact]
    public async Task GetTopAsync_OrdersByCreatedDateDesc()
    {
        await using var db = InMemoryDb.NewDb();
        var (site, _) = InMemoryDb.Seed(db);
        var today = DateTime.Today;
        db.News.AddRange(
            MakeNews(site.Id, "Old",    created: today.AddDays(-7), alias: "old"),
            MakeNews(site.Id, "Newest", created: today,             alias: "newest"),
            MakeNews(site.Id, "Middle", created: today.AddDays(-3), alias: "middle")
        );
        await db.SaveChangesAsync();

        var top = await new NewsService(db).GetTopAsync(site.Id);
        Assert.Equal(new[] { "newest", "middle", "old" }, top.Select(n => n.AliasL).ToArray());
    }
}
