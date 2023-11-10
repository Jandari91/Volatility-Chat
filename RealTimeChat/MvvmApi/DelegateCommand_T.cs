using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmApi
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public DelegateCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute((parameter == null) ? default(T) : (T)parameter);
            //return _canExecute((parameter == null) ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));
        }

        public void Execute(object parameter)
        {
            _execute((parameter == null) ? default(T) : (T)parameter);
            //_execute((parameter == null) ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            // C# 6.0 Null-conditional operator: ?.
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            //
            // Vs.
            //
            //if (CanExecuteChanged != null)
            //    CanExecuteChanged(this, EventArgs.Empty);
        }

        // Vs.
        //
        // "CommandManager.RequerySuggested +=, -="을 이용하면
        //  - 장점: 명시적으로 "RaiseCanExecuteChanged()" 함수를 호출할 필요가 없다.
        //  - 단점: 수시로 자주 호출된다.
        //
        //public event EventHandler CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}
    }
}
