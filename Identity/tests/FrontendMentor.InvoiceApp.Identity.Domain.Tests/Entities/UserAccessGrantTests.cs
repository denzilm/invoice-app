using FrontendMentor.InvoiceApp.Identity.Domain.Entities;
using FrontendMentor.InvoiceApp.Identity.Domain.Enums;

namespace FrontendMentor.InvoiceApp.Identity.Domain.Tests.Entities;

public sealed class UserAccessGrantTests
{

    [Fact]
    public void Create_ShouldThrow_WhenGrantedByUserIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccessGrant
                .Create(Guid.Empty, Guid.NewGuid(), null, CreateRole()));
    }

    [Fact]
    public void Create_ShouldThrow_WhenAssignedUserIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccessGrant
                .Create(Guid.NewGuid(), Guid.Empty, null, CreateRole()));
    }

    [Fact]
    public void Create_ShouldThrow_WhenBothRoleAndPermissionProvided()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccessGrant
                .Create(Guid.NewGuid(), Guid.NewGuid(), null, CreateRole(), CreatePermission()));
    }

    [Fact]
    public void Create_ShouldThrow_WhenNeitherRoleNorPermissionProvided()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccessGrant
                .Create(Guid.NewGuid(), Guid.NewGuid(), null, null));
    }

    [Fact]
    public void Create_ShouldThrow_WhenGlobalRoleAssignedToCompany()
    {
        var globalRole = CreateRole(isGlobal: true);

        Assert.Throws<ArgumentException>(() =>
            UserAccessGrant
                .Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), globalRole));
    }

    [Fact]
    public void Create_ShouldSucceed_WithRole()
    {
        var grant = CreateValidRoleGrant();

        Assert.NotNull(grant);
        Assert.NotNull(grant.GrantedOn);
        Assert.True(grant.IsActive);
        Assert.NotNull(grant.RoleId);
        Assert.Null(grant.PermissionId);
    }

    [Fact]
    public void Create_ShouldSucceed_WithPermission()
    {
        var grant = CreateValidPermissionGrant();

        Assert.NotNull(grant);
        Assert.NotNull(grant.GrantedOn);
        Assert.True(grant.IsActive);
        Assert.NotNull(grant.PermissionId);
        Assert.Null(grant.RoleId);
    }

    [Fact]
    public void Revoke_ShouldSetRevocationState()
    {
        var grant = CreateValidRoleGrant();
        var revokedBy = Guid.NewGuid();

        grant.Revoke(revokedBy);

        Assert.Equal(revokedBy, grant.RevokedByUserId);
        Assert.NotNull(grant.RevokedOn);
        Assert.False(grant.IsActive);
    }

    [Fact]
    public void Revoke_ShouldThrow_WhenAlreadyRevoked()
    {
        var grant = CreateValidRoleGrant();

        grant.Revoke(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            grant.Revoke(Guid.NewGuid()));
    }

    [Fact]
    public void Revoke_ShouldThrow_WhenRevokedByUserIsEmpty()
    {
        var grant = CreateValidRoleGrant();

        Assert.Throws<ArgumentException>(() =>
            grant.Revoke(Guid.Empty));
    }

    [Fact]
    public void IsActive_ShouldBeFalse_AfterRevocation()
    {
        var grant = CreateValidRoleGrant();

        grant.Revoke(Guid.NewGuid());

        Assert.False(grant.IsActive);
    }

    private static Role CreateRole(bool isGlobal = false)
    {
        return new Role(Guid.NewGuid(), "Admin", "Admin", RoleStatusEnum.Active, isGlobal);
    }

    private static Permission CreatePermission()
    {
        return new Permission(Guid.NewGuid(), "Read", "Read", PermissionStatusEnum.Active);
    }

    private static UserAccessGrant CreateValidRoleGrant()
    {
        return UserAccessGrant
            .Create(Guid.NewGuid(), Guid.NewGuid(), null, CreateRole());
    }

    private static UserAccessGrant CreateValidPermissionGrant()
    {
        return UserAccessGrant
            .Create(Guid.NewGuid(), Guid.NewGuid(), null, null, CreatePermission());
    }
}
