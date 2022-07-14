using System;
using System.Windows.Input;

namespace PWARAbilityTool.Client.Commands
{
    public class Command : ICommand
    {
        #region properties
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecutePredicate;
        #endregion 

        public Command(Action executeAction, Func<bool> canExecutePredicate = null)
        {
            _executeAction = executeAction;
            _canExecutePredicate = canExecutePredicate;
        }

        #region methods
        public void Execute(object parameter)
        {
            _executeAction?.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecutePredicate?.Invoke() ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion
    }
}
