using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Kurs_Dzudo.ViewModels
{
    public class ControlViewModel : ReactiveObject
    {
        public Match _currentMatch;
        public UkhasnikiDao _selectedTatami;
        public TimerService _timerService = new TimerService();

        public ObservableCollection<Tatami> AvailableTatamis { get; } = new();
        public MatchViewModel MatchViewModel { get; } = new();

        public string MainTimer => _timerService.MainTime.ToString(@"mm\:ss");
        public string HoldTimer => _timerService.HoldTime.ToString(@"ss");

        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; }
        public ReactiveCommand<string, Unit> AddIpponCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleMainTimerCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleHoldTimerCommand { get; }
        public ReactiveCommand<Unit, Unit> EndMatchCommand { get; }

        public ControlViewModel(TimerService timerService)
        {
            _timerService = timerService;

            LoadGroupsCommand = ReactiveCommand.Create(LoadGroups);
            AddIpponCommand = ReactiveCommand.Create<string>(AddIppon);
            ToggleMainTimerCommand = ReactiveCommand.Create(ToggleMainTimer);
            EndMatchCommand = ReactiveCommand.Create(EndMatch);

            LoadTatamis();
        }

        public UkhasnikiDao SelectedTatami
        {
            get => _selectedTatami;
            set => this.RaiseAndSetIfChanged(ref _selectedTatami, value);
        }

        public void LoadTatamis()
        {
            using var db = new DatabaseConnection();
            var tatamis = db.GetAllTatamis();
            foreach (var tatami in tatamis)
            {
                AvailableTatamis.Add(tatami);
            }
        }

        public void LoadGroups()
        {
            if (SelectedTatami == null) return;

            using var db = new DatabaseConnection();
            var groups = SelectedTatami.Id;
            MatchViewModel.LoadGroups(SelectedTatami.Id);
        }

        public void AddIppon(string participantColor)
        {
            if (MatchViewModel.CurrentMatch == null) return;

            if (participantColor == "White")
            {
                MatchViewModel.CurrentMatch.winner_name = MatchViewModel.CurrentMatch.participant1_name;
                MatchViewModel.CurrentMatch.loser_name = MatchViewModel.CurrentMatch.participant2_name;
            }
            else if (participantColor == "Red")
            {
                MatchViewModel.CurrentMatch.winner_name = MatchViewModel.CurrentMatch.participant2_name;
                MatchViewModel.CurrentMatch.loser_name = MatchViewModel.CurrentMatch.participant1_name;
            }

            //EndMatch();
        }

        public void ToggleMainTimer()
        {
            if (_timerService.IsMainTimerRunning)
                _timerService.StopMainTimer();
            else
                _timerService.StartMainTimer();
        }



        public void EndMatch()
        {
            _timerService.StopMainTimer();
            if (MatchViewModel.CurrentMatch != null)
            {
                using var db = new DatabaseConnection();
                db.SaveMatchResults(MatchViewModel.CurrentMatch);
            }

            MatchViewModel.MoveToNextMatch();
            _timerService.ResetMainTimer();
        }
    }
}
