using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using ukhasnikis_BD_Sec.Hardik.Connect;
using Group = Kurs_Dzudo.Hardik.Connector.Date.Group;

namespace Kurs_Dzudo.ViewModels
{
    public class TablickaViewModel : ReactiveObject
    {
        private List<UkhasnikiDao> _participants;
        private List<Group> _groups;
        private string _searchText;
        private string _filterCategory = "Все";

        public List<Group> Groups
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
            GenerateGroupsCommand = ReactiveCommand.Create(GenerateGroups);
        }

        private void LoadParticipants()
        {
            using var db = new DatabaseConnection();
            _participants = db.GetAllUkhasnikis();
        }

        private void GenerateGroups()
        {
            var groups = new List<Group>();

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
                var newGroup = new Group(

                    name: group.Key,
                    participants: group.ToList()
                );

                foreach (var match in newGroup.Matches) // для теста
                {
                    match.Winner = new Random().Next(2) == 0 ? match.Participant1 : match.Participant2;
                    match.Winner = match.Winner == match.Participant1 ? match.Participant2 : match.Participant1;
                }
                groups.Add(newGroup);
            }
            Groups = groups;
        }
        

        private void FilterGroups()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                GenerateGroups();
                return;
            }

            var searchLower = SearchText.ToLower();
            Groups = _groups
                .Where(g => g.Participants.Any(p =>
                    (p.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (p.SecName?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Club?.ToLower().Contains(searchLower) ?? false)))
                .ToList();
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