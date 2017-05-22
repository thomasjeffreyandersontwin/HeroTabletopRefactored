using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Reflection;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;

namespace HeroUI
{
   public class HeroSystemsBootstrapper : BootstrapperBase
    {
       private SimpleContainer container = new SimpleContainer();
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
            return container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void Configure()
        {
            container = new SimpleContainer();
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, HeroSystemsShellViewModelImpl>();
            container.Singleton<CrowdRepository, CrowdRepositoryImpl>();
            container.Singleton<CrowdClipboard, CrowdClipboardImpl>();
            container.Singleton<HeroVirtualTabletopMainViewModel, HeroVirtualTabletopMainViewModelImpl>();
            container.Singleton<CrowdMemberExplorerViewModel, CrowdMemberExplorerViewModelImpl>();
            container.Singleton<BusyService, BusyServiceImpl>();
            container.Singleton<IconInteractionUtility, IconInteractionUtilityImpl>();
            container.Singleton<Camera, CameraImpl>();
            container.Singleton<KeyBindCommandGenerator, KeyBindCommandGeneratorImpl>();

            ViewLocator.NameTransformer.AddRule("ModelImpl$", "");
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetExecutingAssembly(), Assembly.Load("HeroVirtualTabletop"), };
        }
    }
}