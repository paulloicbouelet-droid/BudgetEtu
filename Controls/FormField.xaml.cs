namespace BudgetEtu.Controls;

public partial class FormField : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(FormField), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FormField), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(FormField), string.Empty);

    public static readonly BindableProperty SuffixProperty =
        BindableProperty.Create(nameof(Suffix), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, _, _) => ((FormField)b).OnPropertyChanged(nameof(HasSuffix)));

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(FormField), Keyboard.Default);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FormField), false,
            propertyChanged: (b, _, _) => ((FormField)b).ApplyPasswordMode());

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(FormField), string.Empty,
            propertyChanged: (b, _, _) =>
            {
                var field = (FormField)b;
                field.OnPropertyChanged(nameof(HasError));
                field.OnPropertyChanged(nameof(ErrorText));
            });

    private bool _passwordHidden = true;

    public FormField()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Unité affichée à droite du champ (ex. « FCFA »).</summary>
    public string Suffix
    {
        get => (string)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    /// <summary>Champ mot de passe avec bouton « Afficher / Masquer ».</summary>
    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    /// <summary>Message d'erreur ; vide = pas d'erreur.</summary>
    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuffix => !string.IsNullOrEmpty(Suffix);
    public string ErrorText => HasError ? $"⚠️ {ErrorMessage}" : string.Empty;

    private void ApplyPasswordMode()
    {
        _passwordHidden = true;
        Input.IsPassword = IsPassword;
        ToggleLabel.IsVisible = IsPassword;
        ToggleLabel.Text = "Afficher";
    }

    private void OnToggleTapped(object? sender, TappedEventArgs e)
    {
        _passwordHidden = !_passwordHidden;
        Input.IsPassword = _passwordHidden;
        ToggleLabel.Text = _passwordHidden ? "Afficher" : "Masquer";
    }
}
