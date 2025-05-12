using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Kurs_Dzudo.Hardik.Connector.Date;

namespace Kurs_Dzudo.Services;

public class FileImportService
{
    public async Task<List<UkhasnikiDao>> ImportParticipantsFromFile(Window parent)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select participants file",
            AllowMultiple = false,
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "CSV Files", Extensions = new List<string> { "csv" } },
                new() { Name = "Excel Files", Extensions = new List<string> { "xlsx" } }
            }
        };

        var result = await dialog.ShowAsync(parent);
        if (result == null || result.Length == 0) return null;

        var filePath = result[0];
        var extension = Path.GetExtension(filePath).ToLower();

        return extension switch
        {
            ".csv" => ImportFromCsv(filePath),
            ".xlsx" => ImportFromExcel(filePath),
            _ => null
        };
    }

    private List<UkhasnikiDao> ImportFromCsv(string filePath)
    {
        // Implement CSV import logic
        // Parse CSV and create UkhasnikiDao objects
        return new List<UkhasnikiDao>();
    }

    private List<UkhasnikiDao> ImportFromExcel(string filePath)
    {
        // Implement Excel import logic
        // Use a library like EPPlus or ClosedXML
        return new List<UkhasnikiDao>();
    }
}
