using Kurs_Dzudo.Hardik.Connector.Date;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Dzudo.ViewModels;
public class GestViewModel : INotifyPropertyChanged
{
    private readonly DatabaseConnection _dbConnection;
    private List<Tatami> _tatamis;
    private List<Group> _groups;
    private IEnumerable<object> _participants;
    private IEnumerable<object> _matches;

    public event PropertyChangedEventHandler? PropertyChanged;

    public List<Tatami> Tatamis
    {
        get => _tatamis;
        set
        {
            _tatamis = value;
            OnPropertyChanged();
        }
    }

    public List<Group> Groups
    {
        get => _groups;
        set
        {
            _groups = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<object> Participants
    {
        get => _participants;
        set
        {
            _participants = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<object> Matches
    {
        get => _matches;
        set
        {
            _matches = value;
            OnPropertyChanged();
        }
    }

    public GestViewModel()
    {
        _dbConnection = new DatabaseConnection();
        LoadTatamis();
    }

    private void LoadTatamis()
    {
        Tatamis = _dbConnection.GetAllTatamis();
    }

    public void LoadGroupsForTatami(int tatamiId)
    {
        Groups = _dbConnection.GetGroupsForTatami(tatamiId);
        Participants = Groups.SelectMany(g => g.Participants).Distinct().ToList();
    }

    public void LoadMatchesForGroup(Group group)
    {
        Matches = group.Matches;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}