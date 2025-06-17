using NUnit.Framework;
using Kurs_Dzudo.Hardik.Connector.Date;
using Kurs_Dzudo.ViewModels;
using System;
using Kurs_Dzudo.Views.OknaFunctiy;
using System.Reactive.Linq;

namespace Kurs_Dzudo.Tests
{
    [TestFixture]
    public class AddEditTests
    {
        [Test]
        public void AddEditWindow_DefaultConstructor_SetsDataContext()
        {
            var window = new AddEditWindow();

            Assert.That(window.DataContext, Is.Not.Null);
            Assert.That(window.DataContext, Is.InstanceOf<AddEditViewModel>());
        }

        [Test]
        public void AddEditWindow_ConstructorWithParticipant_SetsDataContext()
        {
            var participant = new UkhasnikiDao { Name = "Test" };

            var window = new AddEditWindow(participant);

            Assert.That(window.DataContext, Is.Not.Null);
            var vm = window.DataContext as AddEditViewModel;
            Assert.That(vm, Is.Not.Null);
            Assert.That(vm.CurrentParticipant.Name, Is.EqualTo("Test"));
        }

        [Test]
        public void AddEditViewModel_DefaultConstructor_CreatesNewParticipant()
        {
            var vm = new AddEditViewModel();

            Assert.That(vm.CurrentParticipant, Is.Not.Null);
            Assert.That(vm.CurrentParticipant.Name, Is.Null);
        }

        [Test]
        public void AddEditViewModel_ConstructorWithParticipant_SetsCurrentParticipant()
        {
            var participant = new UkhasnikiDao { Name = "John", SecName = "Doe" };

            var vm = new AddEditViewModel(participant);

            Assert.That(vm.CurrentParticipant.Name, Is.EqualTo("John"));
            Assert.That(vm.CurrentParticipant.SecName, Is.EqualTo("Doe"));
        }

        [Test]
        public void WindowTitle_NewParticipant_ReturnsAddTitle()
        {
            var vm = new AddEditViewModel();

            Assert.That(vm.WindowTitle, Is.EqualTo("Добавить участника"));
        }

        [Test]
        public void WindowTitle_ExistingParticipant_ReturnsEditTitle()
        {
            var participant = new UkhasnikiDao { Name = "Timerlan" };
            var vm = new AddEditViewModel(participant);

            Assert.That(vm.WindowTitle, Is.EqualTo("Редактировать участника"));
        }

        [Test]
        public void SelectedDate_ValidDate_ConvertsCorrectly()
        {
            var vm = new AddEditViewModel();
            var testDate = new DateTimeOffset(2023, 5, 15, 0, 0, 0, TimeSpan.Zero);

            vm.SelectedDate = testDate;

            Assert.That(vm.CurrentParticipant.DateSorevnovaniy,
                Is.EqualTo(DateOnly.FromDateTime(new DateTime(2023, 5, 15))));
        }

        [Test]
        public void SelectedDate_InvalidDate_SetsToDefault()
        {
            var vm = new AddEditViewModel();
            var invalidDate = new DateTimeOffset(10000, 1, 1, 0, 0, 0, TimeSpan.Zero);

            vm.SelectedDate = invalidDate;

            Assert.That(vm.CurrentParticipant.DateSorevnovaniy, Is.EqualTo(default(DateOnly)));
        }

        [Test]
        public void SaveCommand_NewParticipant_ExecutesWithoutException()
        {
            var vm = new AddEditViewModel();
            vm.CurrentParticipant.Name = "Test";

            Assert.DoesNotThrowAsync(async () => await vm.SaveCommand.Execute().FirstAsync());
        }

        [Test]
        public void CancelCommand_ExecutesWithoutException()
        {
            var vm = new AddEditViewModel();

            Assert.DoesNotThrow(() => vm.CancelCommand.Execute().Subscribe());
        }

        [Test]
        public void CurrentParticipant_PropertyChanged_RaisesEvents()
        {
            var vm = new AddEditViewModel();
            var raised = false;
            vm.PropertyChanged += (s, e) => raised = e.PropertyName == nameof(vm.CurrentParticipant);

            vm.CurrentParticipant = new UkhasnikiDao();

            Assert.That(raised, Is.True);
        }
    }
}