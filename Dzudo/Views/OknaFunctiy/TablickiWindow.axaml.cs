using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kurs_Dzudo.ViewModels;

namespace Kurs_Dzudo
{
    public partial class TablickiWindow : Window
    {
        public TablickiWindow()
        {
            InitializeComponent();
            DataContext = new TablickaViewModel(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}