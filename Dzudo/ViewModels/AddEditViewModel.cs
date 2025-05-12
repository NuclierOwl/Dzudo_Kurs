using Kurs_Dzudo.Hardik.Connector.Date;
using ReactiveUI;
using System;
using System.Reactive;

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
                    this.RaisePropertyChanged();
                }
            }
        }

        public string WindowTitle => CurrentParticipant?.Name == null ? "Добавить участника" : "Редактировать участника";

        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

        public AddEditViewModel()
        {
            CurrentParticipant = new UkhasnikiDao();
            SetupCommands();
        }

        public AddEditViewModel(UkhasnikiDao participant)
        {
            CurrentParticipant = participant;
            SetupCommands();
        }

        private void SetupCommands()
        {
            SaveCommand = ReactiveCommand.Create(() => { });
            CancelCommand = ReactiveCommand.Create(() => { });
        }
    }
}