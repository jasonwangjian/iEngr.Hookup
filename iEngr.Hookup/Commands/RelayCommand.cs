using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace iEngr.Hookup
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;
        private EventHandler _canExecuteChanged;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);

        public event EventHandler CanExecuteChanged
        {
            add
            {
                _canExecuteChanged += value;
                CommandManager.RequerySuggested += value; // 保留全局事件
            }
            remove
            {
                _canExecuteChanged -= value;
                CommandManager.RequerySuggested -= value; // 保留全局事件
            }
        }

        // 添加手动触发方法
        public void RaiseCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }    //public class RelayCommand<T> : ICommand
         //{
         //    private readonly Action<T> _execute;
         //    private readonly Predicate<T> _canExecute;

    //    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    //    {
    //        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    //        _canExecute = canExecute;
    //    }

    //    public bool CanExecute(object parameter) => true;
    //    public void Execute(object parameter) => _execute((T)parameter);
    //    public event EventHandler CanExecuteChanged
    //    {
    //        add => CommandManager.RequerySuggested += value;
    //        remove => CommandManager.RequerySuggested -= value;
    //    }
    //}
    // public class RelayCommand<T> : ICommand
    // {
    //     private readonly Action<T> _execute;
    //     private readonly Predicate<T> _canExecute;

    //     public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    //     {
    //         _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    //         _canExecute = canExecute;
    //     }
    //     public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
    //     public void Execute(object parameter) => _execute((T)parameter);

    //     public event EventHandler CanExecuteChanged
    //     {
    //         add => CommandManager.RequerySuggested += value;
    //         remove => CommandManager.RequerySuggested -= value;
    //     }
    //}
    //public class RelayCommand : ICommand
    //{
    //    private readonly Action<object> _execute;
    //    private readonly Predicate<object> _canExecute;

    //    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    //    {
    //        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    //        _canExecute = canExecute;
    //    }

    //    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    //    public void Execute(object parameter) => _execute(parameter);

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add => CommandManager.RequerySuggested += value;
    //        remove => CommandManager.RequerySuggested -= value;
    //    }
    //}
}
