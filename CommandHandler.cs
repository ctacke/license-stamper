using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LicenseStamper
{
    public class CommandHandler : ICommand
    {
        private Action m_action;
        private Func<bool> m_validate;

        public event EventHandler CanExecuteChanged;

        public CommandHandler(Action action, Func<bool> validator)
        {
            m_action = action;
            m_validate = validator;
        }

        public CommandHandler(Action action)
            : this(action, () => true)
        {
        }

        public void Execute(object parameter)
        {
            if (m_validate())
            {
                m_action();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
