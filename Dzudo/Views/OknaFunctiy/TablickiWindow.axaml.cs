using Avalonia.Controls;
using Avalonia.Interactivity;
using Kurs_Dzudo.ViewModels;
using Kurs_Dzudo.Views;

namespace Kurs_Dzudo
{
    public partial class TablickiWindow : Window
    {
        public TablickiWindow()
        {
            InitializeComponent();
            DataContext = new TablickaViewModel(this);
        }

        public void Beak_Click(object sender, RoutedEventArgs e)
        {
            var next = new AdminWindow();
            next.Show();
            this.Close();
        }
    }
}