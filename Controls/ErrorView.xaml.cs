using System.Windows.Input;

namespace BudgetEtu.Controls;

public partial class ErrorView : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(ErrorView), "🛠️");

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ErrorView), string.Empty);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ErrorView), string.Empty);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(ErrorView), string.Empty,
            propertyChanged: (b, _, _) => ((ErrorView)b).OnPropertyChanged(nameof(HasAction)));

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(ErrorView), null);

    public ErrorView()
    {
        InitializeComponent();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public bool HasAction => !string.IsNullOrEmpty(ActionText);
}
