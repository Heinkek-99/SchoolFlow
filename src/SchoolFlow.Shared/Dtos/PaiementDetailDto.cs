namespace SchoolFlow.Shared.Dtos;
public record PaiementDetailDto(
    Guid Id,
    string NumeroPaiement,
    DateTime DatePaiement,
    decimal MontantTotal,
    string ModePaiement,
    string Famille,
    List<VentilationDetailDto> Ventilations
);

