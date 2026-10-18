using System.Collections;

namespace BudgetEtu.Controls;

public partial class DropDown : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DropDown), string.Empty);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(DropDown), null);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DropDown), null, BindingMode.TwoWay);

    public static readonly BindableProperty TextAlignmentProperty =
        BindableProperty.Create(nameof(TextAlignment), typeof(TextAlignment), typeof(DropDown), TextAlignment.Start);

    public DropDown()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
}
