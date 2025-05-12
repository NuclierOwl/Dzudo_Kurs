using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Kurs_Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.Views.OknaFunctiy;
using ReactiveUI;
using ukhasnikis_BD_Sec.Hardik.Connect;

namespace Kurs_Dzudo.ViewModels
{
    public class ParticipantsViewModel : ReactiveObject
    {
        private readonly Window _ownerWindow;
        private List<UkhasnikiDao> _allParticipants;
        private List<UkhasnikiDao> _displayedParticipants;
        private string _searchText;
        private UkhasnikiDao _selectedParticipant;

        public ReactiveCommand<Unit, Unit> AddNewParticipantCommand { get; }
        public ReactiveCommand<Unit, Unit> EditParticipantCommand { get; }

        public ParticipantsViewModel(Window ownerWindow)
        {
            _ownerWindow = ownerWindow;
            LoadParticipants();

            AddNewParticipantCommand = ReactiveCommand.CreateFromTask(AddNewParticipantAsync);
            EditParticipantCommand = ReactiveCommand.CreateFromTask(EditParticipantAsync,
                this.WhenAnyValue(x => x.SelectedParticipant).Select(p => p != null));
        }

        public List<UkhasnikiDao> DisplayedParticipants
        {
            get => _displayedParticipants;
            set => this.RaiseAndSetIfChanged(ref _displayedParticipants, value);
        }

        public UkhasnikiDao SelectedParticipant
        {
            get => _selectedParticipant;
            set => this.RaiseAndSetIfChanged(ref _selectedParticipant, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText, value);
                FilterParticipants();
            }
        }

        private void LoadParticipants()
        {
            using var db = new DatabaseConnection();
            _allParticipants = db.GetAllUkhasnikis();
            DisplayedParticipants = new List<UkhasnikiDao>(_allParticipants);
        }

        private void FilterParticipants()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayedParticipants = new List<UkhasnikiDao>(_allParticipants);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                DisplayedParticipants = _allParticipants
                    .Where(p => (p.Name?.ToLower().Contains(searchLower) ?? false ||
                               (p.SecName?.ToLower().Contains(searchLower) ?? false ||
                               (p.Club?.ToLower().Contains(searchLower) ?? false ||
                               (p.Adres?.ToLower().Contains(searchLower) ?? false)))))
                    .ToList();
            }
        }

        public void AddParticipant(UkhasnikiDao participant)
        {
            using var db = new DatabaseConnection();
            db.Updateukhasniki(participant);
            LoadParticipants();
        }

        public void UpdateParticipant(UkhasnikiDao participant)
        {
            using var db = new DatabaseConnection();
            db.Updateukhasniki(participant);
            LoadParticipants();
        }

        public void DeleteParticipant(string name)
        {
            LoadParticipants();
        }

        public List<UkhasnikiDao> ImportParticipants(List<UkhasnikiDao> participants)
        {
            foreach (var participant in participants)
            {
                AddParticipant(participant);
            }
            return participants;
        }

        private async Task AddNewParticipantAsync()
        {
            var dialog = new AddEditWindow();
            var result = await dialog.ShowDialog<bool>(_ownerWindow);

            if (result && dialog.DataContext is AddEditViewModel vm)
            {
                AddParticipant(vm.CurrentParticipant);
            }
        }

        private async Task EditParticipantAsync()
        {
            if (SelectedParticipant == null) return;

            var dialog = new AddEditWindow(new UkhasnikiDao
            {
                Name = SelectedParticipant.Name,
                SecName = SelectedParticipant.SecName,
                DateSorevnovaniy = SelectedParticipant.DateSorevnovaniy,
                Club = SelectedParticipant.Club,
                Adres = SelectedParticipant.Adres,
                Ves = SelectedParticipant.Ves
            });

            var result = await dialog.ShowDialog<bool>(_ownerWindow);

            if (result && dialog.DataContext is AddEditViewModel vm)
            {
                UpdateParticipant(vm.CurrentParticipant);
            }
        }
    }
}