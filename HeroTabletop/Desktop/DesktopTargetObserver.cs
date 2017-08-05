using HeroVirtualTabletop.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Desktop
{
    public interface DesktopTargetObserver
    {
        event EventHandler<CustomEventArgs<string>> TargetChanged;
    }
    public class DesktopTargetObserverImpl : DesktopTargetObserver
    {
        private BackgroundWorker bgWorker;
        private DesktopCharacterTargeter desktopCharacterTargeter;

        private string currentTargetCharacterName;

        public DesktopTargetObserverImpl(DesktopCharacterTargeter desktopCharacterTargeter)
        {
            this.desktopCharacterTargeter = desktopCharacterTargeter;
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = false;
            bgWorker.DoWork += ListenForTargetChanged;
            bgWorker.RunWorkerCompleted += Restart;
            bgWorker.RunWorkerAsync();
        }

        private void Restart(object sender, RunWorkerCompletedEventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        private void ListenForTargetChanged(object sender, DoWorkEventArgs e)
        {
            string actualTargetName = desktopCharacterTargeter.TargetedInstance.Name;
            while (actualTargetName == currentTargetCharacterName)
            {
                Thread.Sleep(500);
                actualTargetName = desktopCharacterTargeter.TargetedInstance.Name;
            }
            currentTargetCharacterName = actualTargetName;
            OnTargetChanged(this, new CustomEventArgs<string> { Value = currentTargetCharacterName});
        }

        public event EventHandler<CustomEventArgs<string>> TargetChanged;

        private void OnTargetChanged(object sender, CustomEventArgs<string> e)
        {
            if (TargetChanged != null)
                TargetChanged(sender, e);
        }
    }
}
