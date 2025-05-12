using Avalonia.Controls;
using System.Threading.Tasks;

namespace Kurs_Dzudo.Hardik.Dop
{
    public static class DopFunctii
    {
        public static async Task ShowError(Window parent, string message)
        {
            var dialog = new Window
            {
                Title = "Ошибка",
                Content = message,
                Width = 300,
                Height = 200
            };
            await dialog.ShowDialog(parent);
        }
    }
}
