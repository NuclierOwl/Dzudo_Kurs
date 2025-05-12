using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Kurs_Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.ViewModels;

namespace Kurs_Dzudo.Views.OknaFunctiy
{
    public partial class AddEditWindow : Window
    {
        public AddEditWindow()
        {
            InitializeComponent();
            DataContext = new AddEditViewModel();
        }

        public AddEditWindow(UkhasnikiDao participant)
        {
            InitializeComponent();
            DataContext = new AddEditViewModel(participant);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}