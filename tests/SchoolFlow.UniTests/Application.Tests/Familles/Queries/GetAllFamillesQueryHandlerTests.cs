using FluentAssertions;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Familles.Queries;
using Application.Tests.Common;
using SchoolFlow.Infrastructure.Data;
using Xunit;

namespace Application.Tests.Familles.Queries;

public class GetAllFamillesQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationDbContext _dbContext;

    public GetAllFamillesQueryHandlerTests()
    {
        _context = ApplicationDbContextFactory.Create();
        _dbContext = _context;
    }

    [Fact]
    public async Task Handle_WithoutFilters_ReturnsAllFamilles()
    {
        // Arrange
        var handler = new GetAllFamillesQueryHandler(_dbContext);
        var query = new GetAllFamillesQuery(
            Recherche: null,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
        result.Data.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var handler = new GetAllFamillesQueryHandler(_dbContext);
        var query = new GetAllFamillesQuery(
            Recherche: "TRAORE",
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().NotBeEmpty();
        result.Data.Items.Should().Contain(f => f.NomPere == "TRAORE");
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var handler = new GetAllFamillesQueryHandler(_dbContext);
        var query = new GetAllFamillesQuery(
            Recherche: null,
            PageNumber: 1,
            PageSize: 5
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(5);
    }

    public void Dispose()
    {
        ApplicationDbContextFactory.Destroy(_context);
    }
}