using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using ukhasnikis_BD_Sec.Hardik.Connect;
using GroupDao_2 = Dzudo.Hardik.Connector.Date.GroupDao_2;

namespace Kurs_Dzudo.ViewModels
{
    public class TablickaViewModel : ReactiveObject
    {
        private List<UkhasnikiDao> _participants = new List<UkhasnikiDao>();
        private List<GroupDao_2> _groups = new List<GroupDao_2>();
        private string _searchText;
        private string _filterCategory = "Все";

        public List<GroupDao_2> Groups
        {
            get => _groups;
            set => this.RaiseAndSetIfChanged(ref _groups, value ?? new List<GroupDao_2>());
        }

        public List<string> Categories { get; } = new List<string>
        {
            "Все",
            "По весу",
            "По возрасту",
            "По полу",
            "По клубу"
        };

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                this.RaiseAndSetIfChanged(ref _filterCategory, value);
                FilterGroups();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                FilterGroups();
            }
        }

        public ReactiveCommand<Unit, Unit> GenerateGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportGroupsCommand { get; }

        public TablickaViewModel(Window ownerWindow)
        {
            GenerateGroupsCommand = ReactiveCommand.Create(GenerateGroups);

            LoadParticipants();
            GenerateGroups();
        }

        private void LoadParticipants()
        {
            IsLoading = true;
            using var db = new DatabaseConnection();
            _participants = db.GetAllUkhasnikis()?.ToList() ?? new List<UkhasnikiDao>();
            IsLoading = false;
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public void GenerateGroups()
        {
            IsLoading = true;

            var groups = new List<GroupDao_2>();
            IEnumerable<IGrouping<string, UkhasnikiDao>> groupedParticipants = _participants.GroupBy(p => "Все участники");

            switch (FilterCategory)
            {
                case "По весу":
                    groupedParticipants = _participants
                        .GroupBy(p => GetWeightCategory(p.Ves));
                    break;
                case "По возрасту":
                    groupedParticipants = _participants
                        .GroupBy(p => GetAgeCategory(p.DateSorevnovaniy));
                    break;
                case "По полу":
                    groupedParticipants = _participants
                        .GroupBy(p => p.SecName?.EndsWith("а") == true ? "Женский" : "Мужской");
                    break;
                case "По клубу":
                    groupedParticipants = _participants
                        .GroupBy(p => p.Club ?? "Не указан");
                    break;
            }

            foreach (var group in groupedParticipants)
            {
                char gender = group.Key == "Женский" ? 'F' : 'M';

                var newGroup = new GroupDao_2(
                    ageCategory: FilterCategory == "По возрасту" ? group.Key : "Общая",
                    weightCategory: FilterCategory == "По весу" ? group.Key : "Общая",
                    gender: gender
                )
                {
                    Participants = group.ToList()
                };

                GenerateMatchesForGroup(newGroup);
                groups.Add(newGroup);
            }

            Groups = groups;
            IsLoading = false;
        }

        private void GenerateMatchesForGroup(GroupDao_2 group)
        {
            if (group.Participants == null || group.Participants.Count < 2)
                return;

            group.Matches = new List<Match>();

            for (int i = 0; i < group.Participants.Count; i++)
            {
                for (int j = i + 1; j < group.Participants.Count; j++)
                {
                    var participant1 = group.Participants[i];
                    var participant2 = group.Participants[j];

                    if (participant1?.Name == null || participant2?.Name == null)
                        continue;

                    var match = new Match
                    {
                        participant1_name = participant1.Name,
                        participant2_name = participant2.Name,
                        Participant1 = participant1,
                        Participant2 = participant2,
                        GroupId = group.Id,
                        tatamiid = 1
                    };
                    
                    //Тестовое, удолить по завершею

                    var winner = new Random().Next(2) == 0 ? participant1 : participant2;
                    match.Winner = winner;
                    match.winner_name = winner.Name;
                    match.Loser = winner == participant1 ? participant2 : participant1;
                    match.loser_name = match.Loser.Name;

                    group.Matches.Add(match);
                }
            }
        }

        private void FilterGroups()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                GenerateGroups();
                return;
            }

            var searchLower = SearchText.ToLower();
            var filteredGroups = Groups
                .Where(g => g.Participants.Any(p =>
                    p.Name?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) == true ||
                    p.SecName?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Club?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) == true
                ))
                .ToList();

            Groups = filteredGroups;
        }

        private string GetWeightCategory(decimal weight)
        {
            if (weight < 30) return "До 30 кг";
            if (weight < 40) return "30-40 кг";
            if (weight < 50) return "40-50 кг";
            if (weight < 60) return "50-60 кг";
            if (weight < 70) return "60-70 кг";
            return "70+ кг";
        }

        private string GetAgeCategory(DateOnly birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear) age--;

            if (age < 10) return "До 10 лет";
            if (age < 12) return "10-12 лет";
            if (age < 14) return "12-14 лет";
            if (age < 16) return "14-16 лет";
            if (age < 18) return "16-18 лет";
            return "18+ лет";
        }
    }
}
