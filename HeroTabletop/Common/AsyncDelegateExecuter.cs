using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Common
{
    public class AsyncDelegateExecuter
    {
        private Timer timer;
        private int dueTime;
        private int period = Timeout.Infinite;
        private bool isRecurring;
        private Action delegateToExecute;

        public AsyncDelegateExecuter(Action delegateToExecute, int dueTime, bool isRecurring = false, int period = Timeout.Infinite)
        {
            this.delegateToExecute = delegateToExecute;
            this.isRecurring = isRecurring;
            this.dueTime = dueTime;
            if (isRecurring)
                this.period = period;
            timer = new Timer(Timer_Callback);
        }

        public void ExecuteAsyncDelegate()
        {
            timer.Change(dueTime, period);
        }

        private void Timer_Callback(object state)
        {
            //System.Windows.Application.Current.Dispatcher.BeginInvoke(delegateToExecute);
            //System.Windows.Application.Current.Dispatcher.Invoke(delegateToExecute);
            //Task t = new Task(delegateToExecute);
            //t.RunSynchronously();
            Task.Factory.StartNew(delegateToExecute);
        }
    }
}
