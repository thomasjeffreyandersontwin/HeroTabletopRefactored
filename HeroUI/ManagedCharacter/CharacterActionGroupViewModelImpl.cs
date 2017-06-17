using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class CharacterActionGroupViewModelImpl<T> : PropertyChangedBase, CharacterActionGroupViewModel where T : CharacterAction
    {
        #region Private Fields

        private string originalName;

        #endregion

        #region Events

        public event EventHandler EditModeEnter;
        public void OnEditModeEnter(object sender, EventArgs e)
        {
            if (EditModeEnter != null)
                EditModeEnter(sender, e);
        }

        public event EventHandler EditModeLeave;
        public void OnEditModeLeave(object sender, EventArgs e)
        {
            if (EditModeLeave != null)
                EditModeLeave(sender, e);

        }

        #endregion

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private CharacterActionGroup actionGroup;
        public CharacterActionGroup ActionGroup
        {
            get
            {
                return actionGroup;
            }
            set
            {
                actionGroup = value;
                SetTooltips();
                NotifyOfPropertyChange(() => ActionGroup);
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }

            set
            {
                isReadOnly = value;
            }
        }

        private bool showOptions;
        public bool ShowOptions
        {
            get
            {
                return showOptions;
            }
            set
            {
                showOptions = value;
                NotifyOfPropertyChange(() => ShowOptions);
            }
        }

        private string addOptionTooltip;
        public string AddOptionTooltip
        {
            get
            {
                return addOptionTooltip;
            }
            set
            {
                addOptionTooltip = value;
                NotifyOfPropertyChange(() => AddOptionTooltip);
            }
        }
        private string removeOptionTooltip;
        public string RemoveOptionTooltip
        {
            get
            {
                return removeOptionTooltip;
            }
            set
            {
                removeOptionTooltip = value;
                NotifyOfPropertyChange(() => RemoveOptionTooltip);
            }
        }
        public bool NewActionGroupAdded { get; set; }

        #endregion

        #region Constructor

        public CharacterActionGroupViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            //this.Owner.PropertyChanged += Owner_PropertyChanged;
            //this.eventAggregator.GetEvent<AttackInitiatedEvent>().Subscribe(this.AttackInitiated);
            //this.eventAggregator.GetEvent<CloseActiveAttackEvent>().Subscribe(this.StopAttack);
            //if (!this.IsStandardOptionGroup)
            //{
            //    this.eventAggregator.GetEvent<RemoveOptionEvent>().Subscribe(this.RemoveOption);
            //}

            //clickTimer_AbilityPlay.AutoReset = false;
            //clickTimer_AbilityPlay.Interval = 2000;
            //clickTimer_AbilityPlay.Elapsed +=
            //    new ElapsedEventHandler(clickTimer_AbilityPlay_Elapsed);
            //SetKeyboardHooks();
        }

        #endregion;

        #region Tooltips

        private void SetTooltips()
        {
            switch (this.ActionGroup.Type)
            {
                case CharacterActionType.Ability:
                    this.AddOptionTooltip = "Add Power (Alt+Ctrl+Plus+A)";
                    this.RemoveOptionTooltip = "Remove Power (Alt+Ctrl+Minus+A)";
                    break;
                case CharacterActionType.Identity:
                    this.AddOptionTooltip = "Add Identity (Alt+Ctrl+Plus+I)";
                    this.RemoveOptionTooltip = "Remove Identity (Alt+Ctrl+Minus+I)";
                    break;
                case CharacterActionType.Movement:
                    this.AddOptionTooltip = "Add Movement (Alt+Ctrl+Plus+M)";
                    this.RemoveOptionTooltip = "Remove Movement (Alt+Ctrl+Minus+M)";
                    break;
                case CharacterActionType.Mixed:
                    this.AddOptionTooltip = "Add Custom Action"; // Not needed
                    this.RemoveOptionTooltip = "Remove Custom Action (Alt+Ctrl+Minus+X)";
                    break;
            }
        }

        #endregion


        #region Rename Option Group

        public void EnterEditMode(object state)
        {
            if (this.ActionGroup.IsStandardActionGroup)
                return;
            this.originalName = this.ActionGroup.Name;
            OnEditModeEnter(state, null);
        }

        public void CancelEditMode(object state)
        {
            this.ActionGroup.Name = this.originalName;
            this.originalName = null;
            OnEditModeLeave(state, null);
        }

        public void RenameActionGroup()
        {
            if (this.NewActionGroupAdded)
            {
                this.NewActionGroupAdded = false;
                this.ShowOptions = true;
                System.Action d = delegate ()
                {
                    this.EnterEditMode(null);
                };
                AsyncDelegateExecuter adex = new AsyncDelegateExecuter(d, 2000);
                adex.ExecuteAsyncDelegate();
            }
            
        }

        public void SubmitRename(object state)
        {
            if (this.originalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);
                bool duplicateName = this.ActionGroup.CheckDuplicateName(updatedName);
                if (!duplicateName)
                {
                    this.ActionGroup.Rename(updatedName);
                    originalName = null;
                    OnEditModeLeave(state, null);
                    this.SaveCharacterActionGroup();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelEditMode(state);
                }
            }
        }

        #endregion

        #region Insert/Remove Character Action

        public void InsertCharacterAction(int index, CharacterAction action)
        {
            
        }

        public void RemoveCharacterAction(int index)
        {
            
        }

        #endregion

        #region Save/Unload Character Action Group

        public void SaveCharacterActionGroup()
        {
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        public void UnloadCharacterActionGroup()
        {
            
        }

        #endregion
    }
}
