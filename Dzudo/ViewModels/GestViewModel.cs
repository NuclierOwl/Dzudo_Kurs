using Dzudo.Hardik.Connector.Date;
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
    private List<GroupDao_2> _groups;
    private IEnumerable<UkhasnikiDao> _participants;
    private IEnumerable<Match> _matches;

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

    public IEnumerable<UkhasnikiDao> Participants
    {
        get => _participants;
        set
        {
            _participants = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Match> Matches
    {
        get => _matches;
        set
        {
            _matches = value;
            OnPropertyChanged();
        }
    }

    public List<GroupDao_2> Groups
    {
        get => _groups;
        set
        {
            _groups = value;
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

    public void LoadMatchesForGroup(GroupDao_2 group)
    {
        Matches = group.Matches ?? Enumerable.Empty<Match>();
    }

    public void OnTatamiSelectionChanged(Tatami selectedTatami)
    {
        if (selectedTatami != null)
        {
            LoadGroupsForTatami(selectedTatami.Id);
        }
    }

    public void OnGroupSelectionChanged(GroupDao_2 selectedGroup)
    {
        if (selectedGroup != null)
        {
            LoadMatchesForGroup(selectedGroup);
        }
    }

    private Tatami _selectedTatami;
    public Tatami SelectedTatami
    {
        get => _selectedTatami;
        set
        {
            _selectedTatami = value;
            OnPropertyChanged();
            OnTatamiSelectionChanged(value);
        }
    }

    private GroupDao_2 _selectedGroup;
    public GroupDao_2 SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            _selectedGroup = value;
            OnPropertyChanged();
            OnGroupSelectionChanged(value);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}