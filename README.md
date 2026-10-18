# BudgetEtu — frontend .NET MAUI

Application de gestion de budget étudiant en FCFA. Frontend uniquement : les données viennent de services simulés (`Services/AuthService.cs`, `Services/BudgetService.cs`).

## Écrans (maquettes → fichiers)

| Maquette | Vue | ViewModel |
|---|---|---|
| 1 · Connexion | `Views/LoginPage.xaml` | `LoginViewModel` |
| 2 · Inscription | `Views/RegisterPage.xaml` | `RegisterViewModel` |
| 3 · Tableau de bord | `Views/DashboardPage.xaml` | `DashboardViewModel` |
| 4 · Saisie (+ version en erreur) | `Views/TransactionPage.xaml` | `TransactionViewModel` |
| 5 · Budgets | `Views/BudgetsPage.xaml` | `BudgetsViewModel` |
| 6 · Alertes | `Views/AlertsPage.xaml` | `AlertsViewModel` |
| 7 · Profil | `Views/ProfilePage.xaml` | `ProfileViewModel` |
| 8 · Réinitialisation du mot de passe | `Views/ForgotPasswordPage.xaml` | `ForgotPasswordViewModel` |

Autres fichiers clés : `AppShell.xaml` (navigation), `Resources/Styles/AppStyles.xaml` (couleurs + police + styles, source unique).

Composants réutilisables (`Controls/`) : `FormField` (champ + erreur), `ErrorBanner` (bandeau d'erreur/succès), `ErrorView` (écran d'erreur / état vide plein cadre), `DropDown` (liste ▾), `BottomNavBar` (barre du bas avec le bouton +).

## Lancer

### Sur PC Windows (sans SDK Android)

1. Visual Studio 2022 (17.8 ou plus) avec la charge de travail **Développement d'applications mobiles .NET** (ou, au minimum, **Développement .NET Desktop** + le workload MAUI : `dotnet workload install maui`).
2. Ouvrir `BudgetEtu.csproj`, choisir **Windows Machine** dans la barre du haut, puis F5.
3. L'application s'ouvre dans une fenêtre au format téléphone (420 × 860).
4. Les états d'erreur : menu **Démo** en haut de la fenêtre (ou 5 clics rapides sur le titre).

### Sur Android (optionnel)

Installer le SDK Android, puis passer `<WithAndroid>` à `true` dans `BudgetEtu.csproj` (ou ajouter `-p:WithAndroid=true`). Le projet cible **.NET 9** (`net9.0-windows10.0.19041.0`, `net9.0-android`, `net9.0-ios` sur Mac), compatible Visual Studio 2022 (17.12+). Pour passer à .NET 10 (Visual Studio 2026 requis), change `<Fx>` en `net10.0` dans `BudgetEtu.csproj`. Il faut le SDK .NET 9 et la charge de travail MAUI (`dotnet workload install maui`).


Compte de démonstration : `etudiant@univ.cm` / `budget123`.

## Navigation (déduite de l'ordre des maquettes)

```
1 Connexion ── Se connecter ─────────▶ 3 Tableau de bord
            ── S'inscrire ───────────▶ 2 Inscription ── S'inscrire ──▶ 3 Tableau de bord
            ── Mot de passe oublié ──▶ 8 Réinitialisation ── Retour ──▶ 1 Connexion
3 Tableau de bord ── barre du bas ───▶ Accueil · 5 Budgets · 6 Alertes · 7 Profil
                  ── bouton (+) ─────▶ 4 Saisie (modale) ── Enregistrer ──▶ 3 Accueil   (✕ ──▶ écran précédent)
7 Profil ── Se déconnecter ──────────▶ 1 Connexion
```

## États d'erreur (modèle : maquette « Saisie_Erreur »)

Contour + fond rouges, pictogramme `(!)` et message `⚠️` sous le champ → composant `FormField`.

- **Champs** : e-mail vide/invalide, mot de passe vide ou trop court, mots de passe différents, montant ≤ 0 (cas de la maquette), motif vide.
- **Formulaire** (bandeau rouge) : identifiants incorrects, e-mail déjà utilisé, compte introuvable, pas de connexion, serveur indisponible. Bandeau vert quand le lien de réinitialisation est envoyé.
- **Écran plein cadre** (`ErrorView`, sur Accueil, Budgets et Alertes) : pas de connexion, erreur serveur, session expirée, état vide propre à chaque écran.

**Pour les voir sans backend** : sur Windows, menu **Démo** en haut de la fenêtre ; sur téléphone, appuyer 5 fois de suite sur le titre « BudgetEtu » (Connexion), « Bonjour, … » (Accueil) ou « Mon Profil » ouvre un menu de simulation (`ViewModels/SimulationMenu.cs`) : à retirer avant la mise en production.

## Uniformisation (différences voulues avec les maquettes)

Détail en tête de `AppStyles.xaml` : une seule famille de vert, une seule famille de police (**Poppins**, licence OFL, dans `Resources/Fonts`) exposée par deux alias, `FontTitle` (titres) et `FontBody` (corps), modifiables en une ligne, un seul style de champ, liens verts au lieu de bleus, carte « Solde » en vert foncé au lieu de rose. Ajouts légers : barres de progression sur les budgets (orange à partir de 80 %), liseré de couleur sur les alertes, bouton Afficher/Masquer sur les mots de passe.

Données fictives : une 3e catégorie « Logement » (15 000 / 20 000) complète l'écran Budgets pour que le total 75 000 / 100 000 de la maquette soit égal à la somme des catégories. Les totaux de l'écran Accueil (150 000 / 200 000 / 50 000) sont ceux de la maquette ; ils ne sont pas reliés aux budgets (les maquettes ne sont pas cohérentes entre elles sur ce point).

## Brancher le vrai backend

Remplacer `AuthService` et `BudgetService` (interfaces `IAuthService`, `IBudgetService`) et faire lever `AppException(ErrorKind.…)` : les écrans affichent déjà la bonne réaction.
