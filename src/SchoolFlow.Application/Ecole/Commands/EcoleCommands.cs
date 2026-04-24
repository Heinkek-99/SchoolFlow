using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
 
namespace SchoolFlow.Application.Ecoles.Commands;
 
// ─── CRÉER ÉCOLE (inscription publique) ─────────────────────────────────────
 
public record CreerEcoleCommand(
    string NomEcole,
    TypeEtablissement TypeEtablissement,
    string Adresse,
    string Ville,
    string? Quartier,
    string? Region,
    string Pays,
    string TelephonePrincipal,
    string? Email,
    string NomDirecteur,
    string PrenomDirecteur,
    string? TelephoneDirecteur,
    string? EmailDirecteur,
    // Compte admin initial créé automatiquement
    string UsernameAdmin,
    string PasswordAdmin
) : IRequest<Result<CreerEcoleResponse>>;
 
public record CreerEcoleResponse(
    Guid EcoleId,
    string CodeEcole,
    string NomEcole,
    StatutEcole Statut,
    string Message
);

 
// ─── VALIDER ÉCOLE (SuperAdmin uniquement) ───────────────────────────────────
 
public record ValiderEcoleCommand(Guid EcoleId)
    : IRequest<Result<string>>;
 
// ─── REJETER ÉCOLE ────────────────────────────────────────────────────────────
 
public record RejeterEcoleCommand(Guid EcoleId, string Motif)
    : IRequest<Result<string>>;
 
// ─── METTRE À JOUR INFOS ÉCOLE ────────────────────────────────────────────────
 
public record MettreAJourEcoleCommand(
    Guid EcoleId,
    string? Slogan,
    string? SiteWeb,
    string? TelephoneSecondaire,
    string? Email
) : IRequest<Result<string>>;
 
// ─── UPLOAD LOGO ─────────────────────────────────────────────────────────────
 
public record UploadLogoEcoleCommand(
    Guid EcoleId,
    string LogoPath
) : IRequest<Result<string>>;
 