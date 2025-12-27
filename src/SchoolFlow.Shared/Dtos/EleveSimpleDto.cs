namespace SchoolFlow.Shared.Dtos;

public record EleveSimpleDto(
    Guid Id,
    string Matricule, 
    string NomComplet,
    DateTime DateNaissance,
    string Sexe,
    string Classe,
    string? PhotoPath,
    decimal Solde
)
{
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateNaissance.Year;
            if (DateNaissance.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

}