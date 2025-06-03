using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Kurs_Dzudo.ViewModels
{
    public class AddEditViewModel : ReactiveObject
    {
        private UkhasnikiDao _currentParticipant;

        public UkhasnikiDao CurrentParticipant
        {
            get => _currentParticipant;
            set => this.RaiseAndSetIfChanged(ref _currentParticipant, value);
        }

        public DateTimeOffset? SelectedDate
        {
            get
            {
                if (CurrentParticipant?.DateSorevnovaniy == default ||
                    CurrentParticipant.DateSorevnovaniy.Year < 1 ||
                    CurrentParticipant.DateSorevnovaniy.Year > 9999)
                {
                    return null;
                }

                try
                {
                    var dateTime = CurrentParticipant.DateSorevnovaniy.ToDateTime(TimeOnly.MinValue);
                    return new DateTimeOffset(dateTime);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (CurrentParticipant != null && value.HasValue)
                {
                    try
                    {
                        if (value.Value.Year >= 1 && value.Value.Year <= 9999)
                        {
                            CurrentParticipant.DateSorevnovaniy = DateOnly.FromDateTime(value.Value.DateTime);
                        }
                        else
                        {
                            CurrentParticipant.DateSorevnovaniy = default;
                        }
                    }
                    catch
                    {
                        CurrentParticipant.DateSorevnovaniy = default;
                    }
                    this.RaisePropertyChanged(nameof(SelectedDate));
                }
            }
        }

        public string WindowTitle => CurrentParticipant?.Name == null ? "Добавить участника" : "Редактировать участника";

        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

        public AddEditViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                CurrentParticipant = new UkhasnikiDao();
                SetupCommands();
            });
        }

        public AddEditViewModel(UkhasnikiDao participant)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                CurrentParticipant = participant;
                SetupCommands();
            });
        }

        private void SetupCommands()
        {
            SaveCommand = ReactiveCommand.CreateFromTask(SaveUkhasnikiAsync);
            CancelCommand = ReactiveCommand.Create(() => { });
        }

        private async Task SaveUkhasnikiAsync()
        {
            try
            {
                using (var db = new DatabaseConnection())
                {
                    if (CurrentParticipant.Name == null)
                    {
                        await db.AddUkhasnikiAsync(CurrentParticipant);
                    }
                    else
                    {
                        await Task.Run(() => db.Updateukhasniki(CurrentParticipant));
                    }
                }
                RxApp.MainThreadScheduler.Schedule(() => {
                });
            }
            catch (Exception ex)
            {
            }
        }
    }
}
