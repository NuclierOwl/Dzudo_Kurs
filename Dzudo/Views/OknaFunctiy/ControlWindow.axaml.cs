using Avalonia.Controls;
using Avalonia.Interactivity;
using Kurs_Dzudo.ViewModels;

namespace Kurs_Dzudo.Views.OknaFunctiy;

public partial class ControlWindow : Window
{
    public ControlWindow()
    {
        InitializeComponent();
    }

    public void Beak_Click(object sender, RoutedEventArgs e)
    {
        var next = new AdminWindow();
        next.Show();
        this.Close();
    }
}