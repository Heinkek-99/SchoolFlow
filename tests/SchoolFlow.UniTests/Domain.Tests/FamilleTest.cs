using FluentAssertions;
using SchoolFlow.Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class FamilleTests
{
    [Fact]
    public void Famille_Creation_ShouldInitializeWithDefaultValues()
    {
        // Act
        var famille = new Famille();

        // Assert
        famille.Id.Should().NotBeEmpty();
        famille.IsArchived.Should().BeFalse();
        famille.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Famille_WithRequiredFields_ShouldBeValid()
    {
        // Arrange & Act
        var famille = new Famille
        {
            NomPere = "TRAORE",
            PrenomPere = "Amadou",
            TelephonePere = "+225 07 12 34 56 78",
            Adresse = "Cocody",
            Ville = "Abidjan"
        };

        // Assert
        famille.NomPere.Should().Be("TRAORE");
        famille.TelephonePere.Should().Be("+225 07 12 34 56 78");
    }

    [Fact]
    public void Famille_Archive_ShouldSetArchivedProperties()
    {
        // Arrange
        var famille = new Famille
        {
            NomPere = "TEST",
            Adresse = "Test",
            Ville = "Test"
        };

        // Act
        famille.Archive("Test d'archivage");

        // Assert
        famille.IsArchived.Should().BeTrue();
        famille.ArchivedAt.Should().NotBeNull();
        famille.ArchiveReason.Should().Be("Test d'archivage");
    }
}