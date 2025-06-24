using Avalonia.Controls;
using Avalonia.Interactivity;
using Dzudo.Hardik.Connector.Date;
using Dzudo.ViewModels;
using Kurs_Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.Views;
using System.Collections.Generic;
using System.Linq;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Kurs_Dzudo;

public partial class GestWindow : Window
{
    private readonly DatabaseConnection _dbConnection;
    private List<Tatami> _tatamis;
    private List<GroupDao_2> _groups;

    //public DataGrid ParticipantsDataGrid { get; private set; }
    //public DataGrid MatchesDataGrid { get; private set; }

    public GestWindow()
    {
        InitializeComponent();
        _dbConnection = new DatabaseConnection();
        DataContext = new GestViewModel();
    }

    public void Beak_Click(object sender, RoutedEventArgs e)
    {
        var next = new MainWindow();
        next.Show();
        this.Close();
    }

    public void Match_Click(object sender, RoutedEventArgs e)
    {
        var next = new MatchWindow();
        next.Show();
        this.Close();
    }
}