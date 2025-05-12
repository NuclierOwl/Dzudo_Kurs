using Avalonia.Controls;
using Avalonia.Interactivity;
using Kurs_Dzudo.Hardik.Dop;
using System.Linq;

namespace Kurs_Dzudo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ProverkaPorola_Click(object sender, RoutedEventArgs e)
    {
        string username = UserNameText.Text;
        string password = PassTextBox.Text;

        using (var dbConnection = new ukhasnikis_BD_Sec.Hardik.Connect.DatabaseConnection())
        {
            var organizators = dbConnection.GetAllOrganizators();
            var user = organizators.FirstOrDefault(u => u.login == username && u.pass == password);

            if (user != null)
            {
                Window next = null;
                switch (user.pozition)
                {
                    case "Admin":
                        next = new AdminWindow();
                        break;
                    case "User":
                        next = new UserWindow();
                        break;
                    case "Gost":
                        next = new GestWindow();
                        break;
                    default:
                        await DopFunctii.ShowError(this, "Роль не найдена");
                        return;
                }

                if (next != null)
                {
                    next.Show();
                    this.Close();
                }
            }
            else
            {
                await DopFunctii.ShowError(this, "Пользователь не найден");
            }
        }
    }

    private void Izmenenia_Click(object ob, RoutedEventArgs e)
    {
        var logo = new GestWindow();
        logo.Show();
        this.Close();
    }
}