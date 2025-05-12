using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using Kurs_Dzudo.Hardik.Connector.Date;
using ukhasnikis_BD_Sec.Hardik.Connect;
using System.Reactive;

namespace Kurs_Dzudo.ViewModels
{
    public class MatchViewModel : ReactiveObject
    {
        private Match _currentMatch;
        private Match _nextMatch;
        private UkhasnikiDao _selectTatami;
        private string _tatamiInfo = "ТАТАМИ: НЕ ВЫБРАНО";
        private string _mainTimer = "120";
        private string _holdTimer = "0";

        public MatchViewModel Matchik => PublicDisplayViewModel;

        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; }
        public ReactiveCommand<string, Unit> AddIpponCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleMainTimerCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleHoldTimerCommand { get; }
        public ReactiveCommand<Unit, Unit> EndMatchCommand { get; }

        public Match CurrentMatch
        {
            get => _currentMatch;
            set => this.RaiseAndSetIfChanged(ref _currentMatch, value);
        }

        public UkhasnikiDao SelectedTatami
        {
            get => _selectTatami;
            set => this.RaiseAndSetIfChanged(ref _selectTatami, value);
        }

        public Match NextMatch
        {
            get => _nextMatch;
            set => this.RaiseAndSetIfChanged(ref _nextMatch, value);
        }

        public string TatamiInfo
        {
            get => _tatamiInfo;
            set => this.RaiseAndSetIfChanged(ref _tatamiInfo, value);
        }

        public string MainTimer
        {
            get => _mainTimer;
            set => this.RaiseAndSetIfChanged(ref _mainTimer, value);
        }

        public string HoldTimer
        {
            get => _holdTimer;
            set => this.RaiseAndSetIfChanged(ref _holdTimer, value);
        }

        public ObservableCollection<Group> Groups { get; } = new ObservableCollection<Group>();
        public ObservableCollection<UkhasnikiDao> AvailableTatamis { get; } = new ObservableCollection<UkhasnikiDao>();
        public MatchViewModel PublicDisplayViewModel { get; } = new MatchViewModel();

        public void LoadGroups(int tatamiId)
        {
            Groups.Clear();

            using (var db = new DatabaseConnection())
            {
                var groups = db.GetGroupsForTatami(tatamiId);
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }

                if (Groups.Any())
                {
                    LoadFirstMatches();
                }
            }
        }

        private void LoadFirstMatches()
        {
            var firstGroup = Groups.First();
            CurrentMatch = firstGroup.Matches.First();

            if (firstGroup.Matches.Count > 1)
            {
                NextMatch = firstGroup.Matches[1];
            }
            else if (Groups.Count > 1)
            {
                NextMatch = Groups[1].Matches.First();
            }
        }

        public void UpdateTimerValues(string mainTime, string holdTime)
        {
            MainTimer = mainTime;
            HoldTimer = holdTime;
        }

        public void UpdateMatchScores(int participant1Score, int participant2Score)
        {
            if (CurrentMatch != null)
            {
                this.RaisePropertyChanged(nameof(CurrentMatch));
            }
        }

        public void ShowWinner(bool isParticipant1Winner)
        {
            if (CurrentMatch != null)
            {
                CurrentMatch.Winner = isParticipant1Winner ? CurrentMatch.Participant1 : CurrentMatch.Participant2;
                this.RaisePropertyChanged(nameof(CurrentMatch));
            }
        }

        public void MoveToNextMatch()
        {
            CurrentMatch = NextMatch;

            var currentGroup = Groups.FirstOrDefault(g => g.Matches.Contains(CurrentMatch));
            if (currentGroup != null)
            {
                var currentIndex = currentGroup.Matches.IndexOf(CurrentMatch);
                if (currentIndex + 1 < currentGroup.Matches.Count)
                {
                    NextMatch = currentGroup.Matches[currentIndex + 1];
                }
                else
                {
                    var nextGroup = Groups.SkipWhile(g => g != currentGroup).Skip(1).FirstOrDefault();
                    if (nextGroup != null && nextGroup.Matches.Any())
                    {
                        NextMatch = nextGroup.Matches.First();
                    }
                    else
                    {
                        NextMatch = null;
                    }
                }
            }

            this.RaisePropertyChanged(nameof(CurrentMatch));
            this.RaisePropertyChanged(nameof(NextMatch));
        }
    }
}