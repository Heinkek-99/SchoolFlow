# SchoolFlow Desktop - Structure WPF MVVM

## 📁 Architecture du Projet

```
SchoolFlow.Desktop/
├── App.xaml                    # Configuration Application + Converters
├── App.xaml.cs                 # Injection de dépendances (DI)
├── SchoolFlow.Desktop.csproj   # Fichier projet .NET 8
│
├── Core/                       # Classes de base MVVM
│   ├── ObservableObject.cs     # INotifyPropertyChanged
│   ├── RelayCommand.cs         # ICommand (sync + async)
│   └── BaseViewModel.cs        # ViewModel de base
│
├── Services/                   # Services applicatifs
│   ├── Interfaces/
│   │   ├── IApiService.cs      # Appels HTTP vers l'API
│   │   ├── IAuthService.cs     # Authentification JWT
│   │   ├── ICacheService.cs    # Cache en mémoire
│   │   └── INavigationService.cs # Navigation MVVM
│   ├── ApiService.cs
│   ├── AuthService.cs
│   ├── CacheService.cs
│   └── NavigationService.cs
│
├── ViewModels/                 # Logique métier (MVVM)
│   ├── MainViewModel.cs        # Navigation principale
│   ├── LoginViewModel.cs       # Connexion
│   ├── DashboardViewModel.cs   # Tableau de bord
│   ├── FamillesViewModel.cs    # Gestion familles
│   ├── ElevesViewModel.cs      # Gestion élèves
│   ├── PaiementsViewModel.cs   # Gestion paiements + ventilation
│   └── ClassesViewModel.cs     # Gestion classes
│
├── Views/                      # Interfaces XAML
│   ├── MainWindow.xaml(.cs)    # Fenêtre principale + menu
│   ├── LoginView.xaml(.cs)     # Page connexion
│   ├── DashboardView.xaml(.cs) # Tableau de bord
│   ├── FamillesView.xaml(.cs)  # Liste/Détail familles
│   ├── ElevesView.xaml(.cs)    # Liste/Détail élèves
│   ├── PaiementsView.xaml(.cs) # Liste/Nouveau paiement
│   └── ClassesView.xaml(.cs)   # Liste/Détail classes
│
├── Converters/                 # Convertisseurs XAML
│   ├── Converters.cs           # BoolToVisibility, Currency, etc.
│   └── BoolToColorConverter.cs
│
├── Themes/                     # Styles et ressources
│   └── Styles.xaml             # Boutons, TextBox, DataGrid
│
└── Assets/                     # Ressources (icônes, images)
    └── schoolflow.ico
```

## 🛠️ Installation

### Prérequis
- .NET 8 SDK
- Visual Studio 2022 ou Rider
- Windows 10/11

### Étapes

1. **Copier les fichiers** dans votre projet `SchoolFlow.Desktop`

2. **Vérifier les références** dans le fichier `.csproj`:
   - `SchoolFlow.Shared` (DTOs partagés)

3. **Compiler**:
   ```bash
   cd src/SchoolFlow.Desktop
   dotnet build
   ```

4. **Configurer l'URL de l'API**:
   - Variable d'environnement: `SCHOOLFLOW_API_URL`
   - Ou modifier `App.xaml.cs` ligne 21

5. **Lancer**:
   ```bash
   dotnet run
   ```

## 🎯 Fonctionnalités Implémentées

### ✅ Core
- [x] Pattern MVVM complet
- [x] Injection de dépendances (DI)
- [x] Navigation entre vues
- [x] Gestion du cache
- [x] Authentification JWT

### ✅ Views
- [x] Login avec validation
- [x] Dashboard avec statistiques
- [x] Liste/Détail Familles
- [x] Liste/Détail Élèves avec filtres
- [x] Paiements avec **ventilation par enfant**
- [x] Liste/Détail Classes

### ✅ Styles
- [x] Thème cohérent (bleu/blanc)
- [x] Boutons, TextBox, DataGrid stylisés
- [x] Cartes statistiques
- [x] Indicateurs de chargement

## 🔗 Intégration API

Les ViewModels appellent l'API via `IApiService`:

| Endpoint | Méthode | Utilisation |
|----------|---------|-------------|
| `api/Auth/login` | POST | Connexion |
| `api/Dashboard/stats` | GET | Statistiques |
| `api/Familles` | GET/POST/PUT/DELETE | CRUD Familles |
| `api/Eleves` | GET/POST/PUT/DELETE | CRUD Élèves |
| `api/Paiements` | GET/POST | Paiements + Ventilation |
| `api/Classes` | GET | Liste classes |

## 📌 Points d'Extension

### Ajouter une nouvelle vue:

1. Créer `NouvelleView.xaml` + `.cs` dans `Views/`
2. Créer `NouvelleViewModel.cs` dans `ViewModels/`
3. Enregistrer dans `App.xaml.cs`:
   ```csharp
   services.AddTransient<NouvelleViewModel>();
   ```
4. Ajouter le DataTemplate dans `MainWindow.xaml`:
   ```xml
   <DataTemplate DataType="{x:Type vm:NouvelleViewModel}">
       <views:NouvelleView />
   </DataTemplate>
   ```

### Ajouter un service:

1. Créer l'interface dans `Services/Interfaces/`
2. Créer l'implémentation dans `Services/`
3. Enregistrer dans `App.xaml.cs`

## ⚠️ Notes Importantes

1. **Conflit namespace**: Utiliser `System.Windows.Application` explicitement
2. **PasswordBox**: Le binding nécessite un gestionnaire code-behind
3. **Cache**: Expiration par défaut = 5 minutes
4. **Ventilation**: Vérifier que `SUM(montants) == MontantTotal`

## 📚 Ressources

- [Documentation WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Pattern MVVM](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview)
- [AGENT.md](../AGENT.md) - Règles du projet SchoolFlow
