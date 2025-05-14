using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Kurs_Dzudo.Hardik.Connector;
using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using GroupDao_2 = Dzudo.Hardik.Connector.Date.GroupDao_2;

namespace Kurs_Dzudo.ViewModels
{
    public class TablickaViewModel : ReactiveObject
    {
        private List<UkhasnikiDao> _participants;
        private List<GroupDao_2> _groups;
        private string _searchText;
        private string _filterCategory = "Все";

        public List<GroupDao_2> Groups
        {
            get => _groups;
            set => this.RaiseAndSetIfChanged(ref _groups, value);
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
            LoadParticipants();
            GenerateGroupsCommand = ReactiveCommand.CreateFromTask(GenerateGroupsAsync);
        }

        private async Task LoadParticipants()
        {
            await Task.Run(() =>
            {
                using var db = new Connector();
                _participants = db.Ukhasniki.ToList();
            });
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private async Task GenerateGroupsAsync()
        {
            var groups = new List<GroupDao_2>();

            await Task.Run(() =>
            {
                IEnumerable<IGrouping<string, UkhasnikiDao>> groupedParticipants;

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
                    default:
                        groupedParticipants = _participants.GroupBy(p => "Все участники");
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

                    foreach (var match in newGroup.Matches)
                    {
                        match.Winner = new Random().Next(2) == 0 ? match.Participant1 : match.Participant2;
                        match.Loser = match.Winner == match.Participant1 ? match.Participant2 : match.Participant1;
                        match.GroupId = newGroup.Id;
                    }

                    groups.Add(newGroup);
                }
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Groups = groups;
            });
        }

        private async void FilterGroups()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await GenerateGroupsAsync();
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = _groups?
                .Where(g => g.Participants?.Any(p =>
                    (p.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (p.SecName?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Club?.ToLower().Contains(searchLower) ?? false)) ?? false)
                .ToList();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Groups = filtered ?? new List<GroupDao_2>();
            });
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