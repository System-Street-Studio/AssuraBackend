using System.Text.Json;
using Assura.Application.Features.AssetRequests.Queries;
using Assura.Application.Tests.Common;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Tests;

// Covers the API-contract audit finding: GET /api/AssetRequests/{id} returned the raw,
// EF-tracked AssetRequest entity. With no ReferenceHandler configured anywhere in
// Program.cs, AssetRequest.User -> User.AssetRequests -> back to the same tracked
// instance (and separately AssetAttachment.AssetRequest -> back to the same instance)
// forms a reference cycle that System.Text.Json throws on when serializing real data
// (a User with any AssetRequests navigation populated). The fix maps to AssetRequestDto
// instead, which has no navigation properties. This test proves the fix by actually
// running the result through System.Text.Json — the same serializer ASP.NET Core uses —
// rather than only asserting on handler output, since the entity-shaped bug was
// invisible to any test that never serialized the result.
public class GetAssetRequestByIdSerializationTests
{
    [Fact]
    public async Task Handle_ResultSerializesToJson_WithoutThrowingOnReferenceCycle()
    {
        using var db = TestContextFactory.CreateContext();

        var requester = new User
        {
            Id = 1,
            Username = "emp",
            FirstName = "Employee",
            LastName = "One",
            Email = "emp@example.com",
            PasswordHash = "x",
            Role = UserRole.Employee
        };
        db.Users.Add(requester);

        db.AssetRequests.Add(new AssetRequest
        {
            Id = 30,
            AssetName = "Monitor",
            Priority = "Normal",
            RequesterId = "1",
            RequesterName = "Employee One",
            RequestType = "NewAsset",
            UserId = 1,
            Status = RequestStatus.Pending
        });
        await db.SaveChangesAsync();

        var handler = new GetAssetRequestByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetAssetRequestByIdQuery { Id = 30, UserId = 1, Role = UserRole.Employee }, CancellationToken.None);

        Assert.NotNull(result);

        // This is the actual regression check: serializing the raw entity (the old
        // behavior) throws here because the change tracker fixes up User.AssetRequests
        // back to this same instance. Serializing the DTO must not throw.
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(result, options);

        Assert.Contains("\"assetName\":\"Monitor\"", json);
    }
}
