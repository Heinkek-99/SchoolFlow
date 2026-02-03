# SchoolFlow Desktop - PRD (Product Requirements Document)

## 📋 Problem Statement Original
Application Desktop de Gestion Scolaire (WPF .NET 8) destinée aux établissements privés en Afrique francophone, avec focus sur le suivi des paiements des familles ayant plusieurs enfants inscrits.

## 🎯 Architecture
- **Frontend**: WPF .NET 8 (Desktop Windows)
- **Backend**: ASP.NET Core 8 Web API (existant)
- **Base de données**: SQL Server Express
- **Architecture**: Clean Architecture + CQRS (MediatR)
- **Pattern UI**: MVVM

## 👥 User Personas
1. **Admin** - Gestion complète, création utilisateurs
2. **Directeur** - Vue dashboard, gestion familles/élèves
3. **Secrétaire** - Inscription élèves, paiements
4. **Comptable** - Enregistrement paiements uniquement

## 📦 Core Requirements
- Authentification JWT
- Gestion des familles multi-enfants
- Inscription des élèves avec matricule unique
- Ventilation des paiements par enfant
- Calcul du solde familial consolidé
- Dashboard statistiques

---

## ✅ What's Been Implemented (Jan 2026)

### Session 1 - Structure Desktop WPF Complète

#### Core MVVM (3 fichiers)
- `ObservableObject.cs` - INotifyPropertyChanged
- `RelayCommand.cs` - ICommand sync + async
- `BaseViewModel.cs` - ViewModel de base avec chargement/erreurs

#### Services (8 fichiers)
- `IApiService.cs` + `ApiService.cs` - Appels HTTP REST
- `IAuthService.cs` + `AuthService.cs` - Authentification JWT
- `ICacheService.cs` + `CacheService.cs` - Cache mémoire avec expiration
- `INavigationService.cs` + `NavigationService.cs` - Navigation MVVM

#### ViewModels (9 fichiers)
- `MainViewModel.cs` - Navigation principale + menu
- `LoginViewModel.cs` - Connexion utilisateur
- `DashboardViewModel.cs` - Statistiques + accès rapides
- `FamillesViewModel.cs` - CRUD familles + détail enfants
- `ElevesViewModel.cs` - CRUD élèves + dossier + filtres
- `PaiementsViewModel.cs` - Paiements + **ventilation par enfant**
- `ClassesViewModel.cs` - Liste classes + effectifs
- **`FamilleFormViewModel.cs`** - Formulaire création/édition famille
- **`EleveFormViewModel.cs`** - Formulaire inscription/édition élève

#### Views XAML (10 fichiers + code-behind)
- `MainWindow.xaml` - Shell + menu latéral
- `LoginView.xaml` - Formulaire connexion
- `DashboardView.xaml` - Cartes stats + graphiques
- `FamillesView.xaml` - DataGrid + panneau détail
- `ElevesView.xaml` - DataGrid + filtres + dossier
- `PaiementsView.xaml` - Formulaire ventilation
- `ClassesView.xaml` - Vue cartes
- **`FamilleFormView.xaml`** - Formulaire famille complet
- **`EleveFormView.xaml`** - Formulaire élève complet

#### Validators FluentValidation (2 fichiers)
- **`FamilleValidator.cs`** - Validation famille (nom, téléphone, email)
- **`EleveValidator.cs`** - Validation élève (nom, date naissance, classe)

#### Converters (4 fichiers)
- `Converters.cs` - BoolToVisibility, Currency, Date, SoldeToColor, NullToVisibility
- `BoolToColorConverter.cs` - Ventilation validée
- **`BoolToTextConverter.cs`** - Texte conditionnel

#### Styles (1 fichier)
- `Styles.xaml` - Thème complet (boutons, inputs, DataGrid, DatePicker)

#### Configuration (3 fichiers)
- `App.xaml` - Ressources + converters
- `App.xaml.cs` - DI Container (tous ViewModels enregistrés)
- `SchoolFlow.Desktop.csproj` - Projet .NET 8 + FluentValidation

---

## 📊 Prioritized Backlog

### P0 - Critical (Pour MVP)
- [x] Structure MVVM complète
- [x] Navigation entre vues
- [x] Authentification
- [x] Dashboard
- [x] Gestion familles
- [x] Gestion élèves
- [x] Paiements avec ventilation

### P1 - High Priority
- [ ] Formulaires CRUD complets (Create/Edit)
- [ ] Validation FluentValidation
- [ ] Impression reçus PDF
- [ ] Gestion des erreurs réseau (mode offline)

### P2 - Medium Priority
- [ ] Graphiques statistiques (LiveCharts)
- [ ] Export Excel
- [ ] Recherche avancée
- [ ] Thème sombre

### P3 - Nice to Have
- [ ] Notifications push
- [ ] Rappels impayés
- [ ] Multi-langues
- [ ] Raccourcis clavier

---

## 🚀 Next Tasks

1. **Copier les fichiers** vers le dépôt GitHub local
2. **Tester la compilation** avec `dotnet build`
3. **Connecter à l'API** (vérifier endpoints)
4. **Implémenter les formulaires** Create/Edit
5. **Ajouter la validation** FluentValidation
6. **Tester l'intégration** complète

---

## 📝 Notes Techniques

- URL API configurable via `SCHOOLFLOW_API_URL`
- Cache expire après 5 minutes par défaut
- Ventilation: `SUM(montants) == MontantTotal` (contrôle strict)
- Matricule format: `EL{Année}{Séquence:D5}`
