using WebsiteCore.Business;
using WebsiteCore.Business.Helpers;
using WebsiteCore.Business.Services;
using WebsiteCore.Business.ViewModels;
using WebsiteCore.Data.Entities;
using Xunit;

namespace WebsiteCore.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CheckLoginAsync_Md5LegacyPassword_Succeeds()
    {
        await using var db = InMemoryDb.NewDb();
        db.Users.Add(new User {
            Id = Guid.NewGuid(),
            UserName = "letan",
            Password = "e10adc3949ba59abbe56e057f20f883e", // MD5 của 123456
            ActiveFlag = 1,
            GroupId = Constants.ReceptionGroup,
            FullName = "Test"
        });
        await db.SaveChangesAsync();

        var u = await new UserService(db).CheckLoginAsync("letan", "123456");
        Assert.NotNull(u);
        Assert.Equal("letan", u!.UserName);
    }

    [Fact]
    public async Task CheckLoginAsync_Pbkdf2NewHash_Succeeds()
    {
        await using var db = InMemoryDb.NewDb();
        db.Users.Add(new User {
            Id = Guid.NewGuid(),
            UserName = "newuser",
            Password = StringHelper.HashPassword("StrongPass1"),
            ActiveFlag = 1,
            GroupId = Constants.MemberGroup,
            FullName = "New"
        });
        await db.SaveChangesAsync();

        Assert.NotNull(await new UserService(db).CheckLoginAsync("newuser", "StrongPass1"));
    }

    [Fact]
    public async Task CheckLoginAsync_WrongPassword_ReturnsNull()
    {
        await using var db = InMemoryDb.NewDb();
        db.Users.Add(new User {
            Id = Guid.NewGuid(), UserName = "u", Password = "e10adc3949ba59abbe56e057f20f883e",
            ActiveFlag = 1, GroupId = Constants.MemberGroup
        });
        await db.SaveChangesAsync();
        Assert.Null(await new UserService(db).CheckLoginAsync("u", "wrong"));
    }

    [Fact]
    public async Task CheckLoginAsync_InactiveUser_ReturnsNull()
    {
        await using var db = InMemoryDb.NewDb();
        db.Users.Add(new User {
            Id = Guid.NewGuid(), UserName = "locked", Password = "e10adc3949ba59abbe56e057f20f883e",
            ActiveFlag = 0, GroupId = Constants.MemberGroup
        });
        await db.SaveChangesAsync();
        Assert.Null(await new UserService(db).CheckLoginAsync("locked", "123456"));
    }

    [Fact]
    public async Task CheckLoginAsync_UnknownUser_ReturnsNull()
    {
        await using var db = InMemoryDb.NewDb();
        Assert.Null(await new UserService(db).CheckLoginAsync("ghost", "any"));
    }

    [Fact]
    public async Task RegisterMemberAsync_StoresPbkdf2Hash()
    {
        await using var db = InMemoryDb.NewDb();
        var svc = new UserService(db);
        var id = await svc.RegisterMemberAsync(new RegisterViewModel {
            UserName = "newpatient",
            Password = "Test1234",
            ConfirmPassword = "Test1234",
            FullName = "Test Patient",
            Phone = "0901234567",
            Email = "x@y.com",
            Gender = "male"
        });
        Assert.NotNull(id);
        var u = await svc.GetByIdAsync(id!.Value);
        Assert.StartsWith("pbkdf2$", u!.Password);
        Assert.Equal(Constants.MemberGroup, u.GroupId);
        Assert.Equal(1, u.ActiveFlag);
    }

    [Fact]
    public async Task RegisterMemberAsync_DuplicateUserName_ReturnsNull()
    {
        await using var db = InMemoryDb.NewDb();
        db.Users.Add(new User { Id = Guid.NewGuid(), UserName = "dup", Password = "x", ActiveFlag = 1, GroupId = "MEMBER" });
        await db.SaveChangesAsync();
        var id = await new UserService(db).RegisterMemberAsync(new RegisterViewModel {
            UserName = "dup", Password = "Test1234", ConfirmPassword = "Test1234",
            FullName = "x", Phone = "0900000000"
        });
        Assert.Null(id);
    }

    [Fact]
    public async Task ChangePasswordAsync_VerifiesCurrentPassword()
    {
        await using var db = InMemoryDb.NewDb();
        var u = new User {
            Id = Guid.NewGuid(), UserName = "u",
            Password = StringHelper.HashPassword("oldpass"),
            ActiveFlag = 1, GroupId = "MEMBER"
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();

        var svc = new UserService(db);
        Assert.False(await svc.ChangePasswordAsync(u.Id, "WRONGOLD", "newpass"));
        Assert.True(await svc.ChangePasswordAsync(u.Id, "oldpass", "newpass1"));
        Assert.True(StringHelper.VerifyPassword("newpass1", (await svc.GetByIdAsync(u.Id))!.Password));
    }

    [Fact]
    public async Task UpdateProfileAsync_SavesFields()
    {
        await using var db = InMemoryDb.NewDb();
        var u = new User { Id = Guid.NewGuid(), UserName = "u", Password = "x", ActiveFlag = 1, GroupId = "MEMBER" };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        var ok = await new UserService(db).UpdateProfileAsync(u.Id, "Tên Mới", "0911222333", "new@email.com", 1);
        Assert.True(ok);
        var fresh = await db.Users.FindAsync(u.Id);
        Assert.Equal("Tên Mới", fresh!.FullName);
        Assert.Equal("0911222333", fresh.Phone);
        Assert.Equal("new@email.com", fresh.Email);
        Assert.Equal(1, fresh.Gender);
    }
}
