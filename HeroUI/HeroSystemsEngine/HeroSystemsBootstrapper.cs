using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace HeroUI.HeroSystemsEngine
{


   public class HeroSystemsBootstrapper : BootstrapperBase
    {
       private SimpleContainer _container = new SimpleContainer();
        public HeroSystemsBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, HeroSystemsShellViewModel>();
            _container.PerRequest<CombatSequence.CombatSequenceViewModel, CombatSequence.CombatSequenceViewModel>();
            _container.PerRequest<CombatSequence.CombatSequence, CombatSequence.CombatSequenceImpl>();
        }


    }
}