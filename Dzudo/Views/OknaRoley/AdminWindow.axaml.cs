using Avalonia.Controls;
using Avalonia.Interactivity;
using Kurs_Dzudo.Views.OknaFunctiy;

namespace Kurs_Dzudo;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
    }

    private async void Control_Click(object sender, RoutedEventArgs e)
    {
        var login = new ControlWindow();
        await login.ShowDialog(this);
        this.Close();
    }

    private async void Tablicka_Click(object sender, RoutedEventArgs e)
    {
        var login = new TablickiWindow();
        await login.ShowDialog(this);
        this.Close();
    }
}