using FluentAssertions;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Familles.Commands;
using Application.Tests.Common;
using SchoolFlow.Infrastructure.Data;
using Xunit;

namespace Application.Tests.Familles.Commands;

public class CreateFamilleCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationDbContext _dbContext;

    public CreateFamilleCommandHandlerTests()
    {
        _context = ApplicationDbContextFactory.Create();
        _dbContext = _context;
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesFamily()
    {
        // Arrange
        var handler = new CreateFamilleCommandHandler(_dbContext);
        var command = new CreateFamilleCommand(
            NomPere: "KOFFI",
            PrenomPere: "Jean",
            TelephonePere: "+225 07 11 22 33 44",
            EmailPere: "jean.koffi@example.com",
            ProfessionPere: "Ingénieur",
            NomMere: "YAO",
            PrenomMere: "Marie",
            TelephoneMere: "+225 05 55 66 77 88",
            EmailMere: "marie.yao@example.com",
            ProfessionMere: "Enseignante",
            Adresse: "Marcory Zone 4",
            QuartierCommune: "Marcory",
            Ville: "Abidjan",
            TelephonePrincipal: "+225 07 11 22 33 44",
            TelephoneSecondaire: "+225 05 55 66 77 88"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var famille = await _dbContext.Familles.FindAsync(Guid.Parse(result.Data!));
        famille.Should().NotBeNull();
        famille!.NomPere.Should().Be("KOFFI");
        famille.PrenomPere.Should().Be("Jean");
    }

    [Fact]
    public async Task Handle_MinimalData_CreatesFamily()
    {
        // Arrange
        var handler = new CreateFamilleCommandHandler(_dbContext);
        var command = new CreateFamilleCommand(
            NomPere: "DOE",
            PrenomPere: "John",
            TelephonePere: "+225 07 99 88 77 66",
            EmailPere: null,
            ProfessionPere: null,
            NomMere: null,
            PrenomMere: null,
            TelephoneMere: null,
            EmailMere: null,
            ProfessionMere: null,
            Adresse: "Abidjan",
            QuartierCommune: null,
            Ville: "Abidjan",
            TelephonePrincipal: "+225 07 99 88 77 66",
            TelephoneSecondaire: null
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        ApplicationDbContextFactory.Destroy(_context);
    }
}