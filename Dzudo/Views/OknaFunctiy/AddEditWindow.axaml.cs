using Avalonia.Controls;
using Avalonia.Interactivity;
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

        public void Beak_Click (object e, RoutedEventArgs routedEventArgs)
        {
            var next = new UserWindow();
            next.Show();
            this.Close();
        }
    }
}