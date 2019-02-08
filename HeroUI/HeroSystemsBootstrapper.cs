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
using HeroVirtualTabletop.Roster;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Movement;
using HeroVirtualTabletop.Attack;

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
            container.Singleton<IWindowManager, Caliburn.Micro.WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, HeroSystemsShellViewModelImpl>();
            container.Singleton<CrowdRepository, CrowdRepositoryImpl>();
            container.Singleton<CrowdClipboard, CrowdClipboardImpl>();
            container.Singleton<HeroVirtualTabletopMainViewModel, HeroVirtualTabletopMainViewModelImpl>();
            container.Singleton<CrowdMemberExplorerViewModel, CrowdMemberExplorerViewModelImpl>();
            container.Singleton<RosterExplorerViewModel, RosterExplorerViewModelImpl>();
            container.Singleton<CharacterEditorViewModel, CharacterEditorViewModelImpl>();
            container.Singleton<IdentityEditorViewModel, IdentityEditorViewModelImpl>();
            container.Singleton<AbilityEditorViewModel, AbilityEditorViewModelImpl>();
            container.Singleton<MovementEditorViewModel, MovementEditorViewModelImpl>();
            container.Singleton<BusyService, BusyServiceImpl>();
            container.Singleton<PopupService, PopupServiceImpl>();
            container.Singleton<IconInteractionUtility, IconInteractionUtilityImpl>();
            container.Singleton<Camera, CameraImpl>();
            container.Singleton<Roster, RosterImpl>();
            container.Singleton<KeyBindCommandGenerator, KeyBindCommandGeneratorImpl>();
            container.Singleton<DesktopCharacterTargeter, DesktopCharacterTargeterImpl>();
            container.Singleton<DesktopMouseEventHandler, DesktopMouseEventHandlerImpl>();
            container.Singleton<DesktopKeyEventHandler, DesktopKeyEventHandlerImpl>();
            container.Singleton<DesktopMouseHoverElement, DesktopMouseHoverElementImpl>();
            container.Singleton<DesktopContextMenu, DesktopContextMenuImpl>();
            container.Singleton<DesktopTargetObserver, DesktopTargetObserverImpl>();
            container.Singleton<AnimatedResourceManager, AnimatedResourceManagerImpl>();
            container.Singleton<AbilityClipboard, AbilityClipboardImpl>();
            container.Singleton<ActiveCharacterWidgetViewModel, ActiveCharacterWidgetViewModelImpl>();
            container.Singleton<AttackConfigurationWidgetViewModel, AttackConfigurationWidgetViewModelImpl>();

            container.PerRequest<DesktopNavigator, DesktopNavigatorImpl>();
            container.PerRequest<CharacterActionGroupViewModelImpl<Identity>, CharacterActionGroupViewModelImpl<Identity>>();
            container.PerRequest<CharacterActionGroupViewModelImpl<AnimatedAbility>, CharacterActionGroupViewModelImpl<AnimatedAbility>>();
            container.PerRequest<CharacterActionGroupViewModelImpl<CharacterMovement>, CharacterActionGroupViewModelImpl<CharacterMovement>>();
            container.PerRequest<CharacterActionGroupViewModelImpl<CharacterAction>, CharacterActionGroupViewModelImpl<CharacterAction>>();

            ViewLocator.NameTransformer.AddRule("ModelImpl$", "");
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetExecutingAssembly(), Assembly.Load("HeroVirtualTabletop"), };
        }
    }
}