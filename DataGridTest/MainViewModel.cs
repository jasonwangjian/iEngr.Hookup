using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DataGridTest
{
    // 视图模型
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Discipline> Disciplines { get; set; }

        public MainViewModel()
        {
            // 初始化数据
            Students = new ObservableCollection<Student>
            {
                new Student { Id = 1, Name = "张三", Age = 20, DisciplineId = 1 },
                new Student { Id = 2, Name = "李四", Age = 21, DisciplineId = 2 },
                new Student { Id = 3, Name = "王五", Age = 22, DisciplineId = 3 }
            };

            Disciplines = new ObservableCollection<Discipline>
            {
                new Discipline { Id = 1, Name = "数学" },
                new Discipline { Id = 2, Name = "物理" },
                new Discipline { Id = 3, Name = "化学" },
                new Discipline { Id = 4, Name = "生物" }
            };

            // 为每个学生设置学科名称（用于显示）
            //foreach (var student in Students)
            //{
            //    var discipline = Disciplines.FirstOrDefault(d => d.Id == student.DisciplineId);
            //    student.DisciplineName = discipline?.Name;
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Student : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private int _age;
        private int _disciplineId;
        private string _disciplineName;

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public int Age
        {
            get { return _age; }
            set { _age = value; OnPropertyChanged(nameof(Age)); }
        }

        public int DisciplineId
        {
            get { return _disciplineId; }
            set
            {
                _disciplineId = value;
                OnPropertyChanged(nameof(DisciplineId));

                // 更新学科名称
                //var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                //if (mainViewModel != null)
                //{
                //    var discipline = mainViewModel.Disciplines.FirstOrDefault(d => d.Id == value);
                //    DisciplineName = discipline?.Name;
                //}
            }
        }

        //public string DisciplineName
        //{
        //    get { return _disciplineName; }
        //    set { _disciplineName = value; OnPropertyChanged(nameof(DisciplineName)); }
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Discipline
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
