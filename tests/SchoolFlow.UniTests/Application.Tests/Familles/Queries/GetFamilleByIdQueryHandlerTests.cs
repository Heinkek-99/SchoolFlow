using FluentAssertions;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Familles.Queries;
using Application.Tests.Common;
using SchoolFlow.Infrastructure.Data;
using Xunit;

namespace Application.Tests.Familles.Queries;

public class GetFamilleByIdQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationDbContext _dbContext;

    public GetFamilleByIdQueryHandlerTests()
    {
        _context = ApplicationDbContextFactory.Create();
        _dbContext = _context;
    }

    [Fact]
    public async Task Handle_ExistingFamilleId_ReturnsSuccessWithFamilleDetails()
    {
        // Arrange
        var familleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var handler = new GetFamilleByIdQueryHandler(_dbContext);
        var query = new GetFamilleByIdQuery(familleId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(familleId);
        result.Data.NomPere.Should().Be("TRAORE");
        result.Data.PrenomPere.Should().Be("Amadou");
    }

    [Fact]
    public async Task Handle_NonExistingFamilleId_ReturnsFailure()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var handler = new GetFamilleByIdQueryHandler(_dbContext);
        var query = new GetFamilleByIdQuery(nonExistingId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("introuvable");
    }

    [Fact]
    public async Task Handle_FamilleWithEleves_ReturnsCorrectEleveCount()
    {
        // Arrange
        var familleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var handler = new GetFamilleByIdQueryHandler(_dbContext);
        var query = new GetFamilleByIdQuery(familleId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.NombreEnfants.Should().BeGreaterThan(0);
        result.Data.Eleves.Should().NotBeEmpty();
    }

    public void Dispose()
    {
        ApplicationDbContextFactory.Destroy(_context);
    }
}