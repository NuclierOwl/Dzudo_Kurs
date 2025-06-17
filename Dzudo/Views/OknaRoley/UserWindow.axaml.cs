using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Kurs_Dzudo.ViewModels;
using Kurs_Dzudo.Views;
using Kurs_Dzudo.Views.OknaFunctiy;

namespace Kurs_Dzudo;

public partial class UserWindow : Window
{
    public UserWindow()
    {
        InitializeComponent();
        DataContext = new ParticipantsViewModel(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void Izmenenia_Click(object sender, RoutedEventArgs e)
    {
        var login = new AddEditWindow();
        await login.ShowDialog(this);
        this.Close();
    }

    public void Beak_Click(object sender, RoutedEventArgs e)
    {
        var next = new MainWindow();
        next.Show();
        this.Close();
    }
}