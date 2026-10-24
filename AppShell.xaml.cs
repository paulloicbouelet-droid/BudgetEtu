using BudgetEtu.Views;

namespace BudgetEtu;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Écran 4_Saisie : page modale ouverte par le bouton (+).
        Routing.RegisterRoute("Transaction", typeof(TransactionPage));

        // Écrans ajoutés : formulaire d'enveloppe de budget et changement de mot de passe.
        Routing.RegisterRoute("BudgetForm", typeof(BudgetFormPage));
        Routing.RegisterRoute("ChangePassword", typeof(ChangePasswordPage));
    }
}
