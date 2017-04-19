using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroUI
{
    public class HeroVirtualTabletopMainViewModelImpl : PropertyChangedBase, HeroVirtualTabletopMainViewModel, IShell
    {
        #region Private Members
        private IEventAggregator eventAggregator;
        

        #endregion

        #region Events
        public event EventHandler ViewLoaded;
        public void OnViewLoaded(object sender, EventArgs e)
        {
            if (ViewLoaded != null)
                ViewLoaded(sender, e);
        }
        #endregion

        #region Public Properties

        private bool isCharacterExplorerExpanded;
        public bool IsCharacterExplorerExpanded
        {
            get
            {
                return isCharacterExplorerExpanded;
            }
            set
            {
                isCharacterExplorerExpanded = value;
                NotifyOfPropertyChange("IsCharacterExplorerExpanded");
            }
        }

        private bool isRosterExplorerExpanded;
        public bool IsRosterExplorerExpanded
        {
            get
            {
                return isRosterExplorerExpanded;
            }
            set
            {
                isRosterExplorerExpanded = value;
                NotifyOfPropertyChange("IsRosterExplorerExpanded");
            }
        }

        private bool isCharacterEditorExpanded;
        public bool IsCharacterEditorExpanded
        {
            get
            {
                return isCharacterEditorExpanded;
            }
            set
            {
                isCharacterEditorExpanded = value;
                NotifyOfPropertyChange("IsCharacterEditorExpanded");
            }
        }

        private bool isIdentityEditorExpanded;
        public bool IsIdentityEditorExpanded
        {
            get
            {
                return isIdentityEditorExpanded;
            }
            set
            {
                isIdentityEditorExpanded = value;
                NotifyOfPropertyChange("IsIdentityEditorExpanded");
            }
        }

        private bool isAbilityEditorExpanded;
        public bool IsAbilityEditorExpanded
        {
            get
            {
                return isAbilityEditorExpanded;
            }
            set
            {
                isAbilityEditorExpanded = value;
                NotifyOfPropertyChange("IsAbilityEditorExpanded");
            }
        }

        private bool isMovementEditorExpanded;
        public bool IsMovementEditorExpanded
        {
            get
            {
                return isMovementEditorExpanded;
            }
            set
            {
                isMovementEditorExpanded = value;
                NotifyOfPropertyChange("IsMovementEditorExpanded");
            }
        }

        private bool isCrowdFromModelsExpanded;
        public bool IsCrowdFromModelsExpanded
        {
            get
            {
                return isCrowdFromModelsExpanded;
            }
            set
            {
                isCrowdFromModelsExpanded = value;
                NotifyOfPropertyChange("IsCrowdFromModelsExpanded");
            }
        }

        private CrowdMemberExplorerViewModel crowdMemberExplorerViewModel;
        public CrowdMemberExplorerViewModel CrowdMemberExplorerView
        {
            get
            {
                return crowdMemberExplorerViewModel;
            }
            set
            {
                crowdMemberExplorerViewModel = value;
                NotifyOfPropertyChange(() => CrowdMemberExplorerView);
            }
        }

        #endregion

        #region Constructor
        public HeroVirtualTabletopMainViewModelImpl(IEventAggregator eventAggregator, CrowdMemberExplorerViewModel crowdMemberExplorerViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.CrowdMemberExplorerView = crowdMemberExplorerViewModel;
            //this.eventAggregator.GetEvent<AddToRosterEvent>().Subscribe((IEnumerable<CrowdMemberModel> models) => { this.IsRosterExplorerExpanded = true; });
            //this.eventAggregator.GetEvent<EditCharacterEvent>().Subscribe((Tuple<ICrowdMemberModel, IEnumerable<ICrowdMemberModel>> tuple) => { this.IsCharacterEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditIdentityEvent>().Subscribe((Tuple<Identity, Character> tuple) => { this.IsIdentityEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditAbilityEvent>().Subscribe((Tuple<AnimatedAbility, Character> tuple) => { this.IsAbilityEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditMovementEvent>().Subscribe((CharacterMovement cm) => { this.IsMovementEditorExpanded = true; });
            //this.eventAggregator.GetEvent<CreateCrowdFromModelsEvent>().Subscribe((CrowdModel crowd) => { this.IsCrowdFromModelsExpanded = true; });
            //this.eventAggregator.GetEvent<AttackInitiatedEvent>().Subscribe((Tuple<Character, Attack> tuple) => { this.IsRosterExplorerExpanded = true; });
        }

        #endregion

        #region Methods
        public void LoadCharacterExplorer()
        {
            //CharacterExplorerView view = this.Container.Resolve<CharacterExplorerView>();
            //OnViewLoaded(view, null);
        }
        //public void LoadRosterExplorer()
        //{
        //    RosterExplorerView view = this.Container.Resolve<RosterExplorerView>();
        //    OnViewLoaded(view, null);
        //}
        //public void LoadCharacterEditor()
        //{
        //    CharacterEditorView view = this.Container.Resolve<CharacterEditorView>();
        //    OnViewLoaded(view, null);
        //}
        //public void LoadIdentityEditor()
        //{
        //    IdentityEditorView view = this.Container.Resolve<IdentityEditorView>();
        //    OnViewLoaded(view, null);
        //}
        //public void LoadAbilityEditor()
        //{
        //    AbilityEditorView view = this.Container.Resolve<AbilityEditorView>();
        //    OnViewLoaded(view, null);
        //}
        //public void LoadMovementEditor()
        //{
        //    MovementEditorView view = this.Container.Resolve<MovementEditorView>();
        //    OnViewLoaded(view, null);
        //}
        //public void LoadCrowdFromModelsView()
        //{
        //    CrowdFromModelsView view = this.Container.Resolve<CrowdFromModelsView>();
        //    OnViewLoaded(view, null);
        //}

        private void CollapsePanel(object state)
        {
            switch (state.ToString())
            {
                case "CharacterExplorer":
                    this.IsCharacterExplorerExpanded = false;
                    break;
                case "RosterExplorer":
                    this.IsRosterExplorerExpanded = false;
                    break;
                case "CharacterEditor":
                    this.IsCharacterEditorExpanded = false;
                    break;
                case "IdentityEditor":
                    this.IsIdentityEditorExpanded = false;
                    break;
                case "AbilityEditor":
                    this.IsAbilityEditorExpanded = false;
                    break;
                case "MovementEditor":
                    this.IsMovementEditorExpanded = false;
                    break;
                case "CrowdFromModelsView":
                    this.IsCrowdFromModelsExpanded = false;
                    break;
            }
        }
        #endregion
    }
}
