using BudgetEtu.Views;

namespace BudgetEtu;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Écran 4_Saisie : page modale ouverte par le bouton (+).
        Routing.RegisterRoute("Transaction", typeof(TransactionPage));
    }
}
