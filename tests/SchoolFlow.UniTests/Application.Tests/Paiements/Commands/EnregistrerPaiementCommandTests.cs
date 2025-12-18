using FluentAssertions;
using SchoolFlow.Application.Paiements.Commands;
using SchoolFlow.Domain.Entities;
using Xunit;

namespace SchoolFlow.Tests.Unit.Application.Tests.Paiements.Commands;

public class EnregistrerPaiementCommandTests
{
    [Fact]
    public void Validator_Should_Fail_When_MontantTotal_IsZero()
    {
        // Arrange
        var command = new EnregistrerPaiementCommand(
            Guid.NewGuid(),
            0, // Montant invalide
            DateTime.Now,
            "Espèces",
            null,
            null,
            new List<VentilationInput>
            {
                new(Guid.NewGuid(), 0, null)
            }
        );

        var validator = new EnregistrerPaiementCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MontantTotal));
    }

    [Fact]
    public void Validator_Should_Fail_When_DatePaiement_IsInFuture()
    {
        // Arrange
        var command = new EnregistrerPaiementCommand(
            Guid.NewGuid(),
            100000,
            DateTime.Now.AddDays(1), // Date future
            "Espèces",
            null,
            null,
            new List<VentilationInput>
            {
                new(Guid.NewGuid(), 100000, null)
            }
        );

        var validator = new EnregistrerPaiementCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.DatePaiement));
    }

    [Fact]
    public void Validator_Should_Fail_When_Ventilations_DoNotMatchTotal()
    {
        // Arrange
        var command = new EnregistrerPaiementCommand(
            Guid.NewGuid(),
            150000,
            DateTime.Now,
            "Espèces",
            null,
            null,
            new List<VentilationInput>
            {
                new(Guid.NewGuid(), 50000, null),
                new(Guid.NewGuid(), 50000, null) // Total = 100000 != 150000
            }
        );

        var validator = new EnregistrerPaiementCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Should_Pass_When_AllFields_AreValid()
    {
        // Arrange
        var command = new EnregistrerPaiementCommand(
            Guid.NewGuid(),
            150000,
            DateTime.Now,
            "Espèces",
            null,
            null,
            new List<VentilationInput>
            {
                new(Guid.NewGuid(), 100000, null),
                new(Guid.NewGuid(), 50000, null)
            }
        );

        var validator = new EnregistrerPaiementCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class PaiementEntityTests
{
    [Fact]
    public void Create_Should_Generate_ValidPaiement()
    {
        // Arrange
        var numeroPaiement = "PAY-2025-00001";
        var familleId = Guid.NewGuid();
        var montantTotal = 500000m;
        var datePaiement = DateTime.Now;
        var modePaiement = "Espèces";
        var utilisateurId = Guid.NewGuid();

        // Act
        var paiement = Paiement.Create(
            numeroPaiement,
            familleId,
            montantTotal,
            datePaiement,
            modePaiement,
            utilisateurId
        );

        // Assert
        paiement.Should().NotBeNull();
        paiement.NumeroPaiement.Should().Be(numeroPaiement);
        paiement.FamilleId.Should().Be(familleId);
        paiement.MontantTotal.Should().Be(montantTotal);
        paiement.ModePaiement.Should().Be(modePaiement);
    }

    [Fact]
    public void AjouterVentilation_Should_Add_Ventilation()
    {
        // Arrange
        var paiement = Paiement.Create(
            "PAY-2025-00001",
            Guid.NewGuid(),
            200000m,
            DateTime.Now,
            "Espèces",
            Guid.NewGuid()
        );

        var eleveId = Guid.NewGuid();
        var montant = 100000m;

        // Act
        paiement.AjouterVentilation(eleveId, montant);

        // Assert
        paiement.Ventilations.Should().HaveCount(1);
        paiement.Ventilations.First().EleveId.Should().Be(eleveId);
        paiement.Ventilations.First().Montant.Should().Be(montant);
    }

    [Fact]
    public void AjouterVentilation_Should_Throw_When_TotalExceedsMontantPaiement()
    {
        // Arrange
        var paiement = Paiement.Create(
            "PAY-2025-00001",
            Guid.NewGuid(),
            100000m,
            DateTime.Now,
            "Espèces",
            Guid.NewGuid()
        );

        paiement.AjouterVentilation(Guid.NewGuid(), 80000m);

        // Act & Assert
        var act = () => paiement.AjouterVentilation(Guid.NewGuid(), 50000m);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*dépasse le montant du paiement*");
    }

    [Fact]
    public void EstVentilationComplete_Should_Return_True_When_Complete()
    {
        // Arrange
        var paiement = Paiement.Create(
            "PAY-2025-00001",
            Guid.NewGuid(),
            200000m,
            DateTime.Now,
            "Espèces",
            Guid.NewGuid()
        );

        paiement.AjouterVentilation(Guid.NewGuid(), 120000m);
        paiement.AjouterVentilation(Guid.NewGuid(), 80000m);

        // Act
        var result = paiement.EstVentilationComplete();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EstVentilationComplete_Should_Return_False_When_Incomplete()
    {
        // Arrange
        var paiement = Paiement.Create(
            "PAY-2025-00001",
            Guid.NewGuid(),
            200000m,
            DateTime.Now,
            "Espèces",
            Guid.NewGuid()
        );

        paiement.AjouterVentilation(Guid.NewGuid(), 100000m);

        // Act
        var result = paiement.EstVentilationComplete();

        // Assert
        result.Should().BeFalse();
    }
}