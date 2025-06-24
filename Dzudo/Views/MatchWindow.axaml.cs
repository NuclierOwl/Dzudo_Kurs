using Avalonia.Controls;
using Avalonia.Interactivity;
using Kurs_Dzudo.Views;

namespace Kurs_Dzudo;

public partial class MatchWindow : Window
{
    public MatchWindow()
    {
        InitializeComponent();
    }

    public void Beak_Click(object sender, RoutedEventArgs e)
    {
        var next = new GestWindow();
        next.Show();
        this.Close();
    }
}