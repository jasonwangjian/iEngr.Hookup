using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace iEngr.Hookup
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Person> _people;
        private Person _selectedPerson;

        public ObservableCollection<Person> People
        {
            get => _people;
            set { _people = value; OnPropertyChanged(); }
        }

        public Person SelectedPerson
        {
            get => _selectedPerson;
            set { _selectedPerson = value; OnPropertyChanged(); }
        }

        public ICommand AddPersonCommand { get; }
        public ICommand DeletePersonCommand { get; }

        public MainViewModel()
        {
            People = new ObservableCollection<Person>
        {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 }
        };

            AddPersonCommand = new RelayCommand<object>(_ => AddPerson());
            DeletePersonCommand = new RelayCommand<object>(_ => DeletePerson(), _ => SelectedPerson != null);
        }

        private void AddPerson()
        {
            People.Add(new Person { Name = "New", Age = 0 });
        }

        private void DeletePerson()
        {
            if (SelectedPerson != null)
                People.Remove(SelectedPerson);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
