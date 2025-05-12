using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Kurs_Dzudo.ViewModels
{
    public class ControlViewModel : ReactiveObject
    {
        private Match _currentMatch;
        private UkhasnikiDao _selectedTatami;
        private TimerService _timerService = new TimerService();

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

        private void LoadTatamis()
        {
            using var db = new DatabaseConnection();
            var tatamis = db.GetAllTatamis();
            foreach (var tatami in tatamis)
            {
                AvailableTatamis.Add(tatami);
            }
        }

        private void LoadGroups()
        {
            if (SelectedTatami == null) return;

            using var db = new DatabaseConnection();
            var groups = SelectedTatami.Id;
            MatchViewModel.LoadGroups(SelectedTatami.Id);
        }

        private void AddIppon(string participantColor)
        {

        }

        private void ToggleMainTimer()
        {
            if (_timerService.IsMainTimerRunning)
                _timerService.StopMainTimer();
            else
                _timerService.StartMainTimer();
        }



        private void EndMatch()
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
