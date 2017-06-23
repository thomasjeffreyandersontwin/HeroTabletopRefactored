using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class IdentityEditorViewModelImpl : PropertyChangedBase, IdentityEditorViewModel, IHandle<EditIdentityEvent>
    {
        #region Private Fields

        private string originalName;
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_COSTUMES_FOLDERNAME = "costumes";
        private const string GAME_COSTUMES_EXT = ".costume";
        private const string GAME_MODELS_FILENAME = "Models.txt";

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

        private Identity editedIdentity;
        public Identity EditedIdentity
        {
            get
            {
                return editedIdentity;
            }
            set
            {
                if (editedIdentity != null)
                {
                    editedIdentity.PropertyChanged -= EditedIdentity_PropertyChanged;
                    (editedIdentity.Owner as ManagedCharacter).Identities.PropertyChanged -= OwnerIdentities_PropertyChanged;
                }
                editedIdentity = value;
                if (editedIdentity != null)
                {
                    editedIdentity.PropertyChanged += EditedIdentity_PropertyChanged;
                    (editedIdentity.Owner as ManagedCharacter).Identities.PropertyChanged += OwnerIdentities_PropertyChanged; 
                }
                NotifyOfPropertyChange(() => EditedIdentity);
            }
        }

        public ManagedCharacter Owner
        {
            get
            {
                return this.EditedIdentity?.Owner as ManagedCharacter;
            }
        }

        private string filter;
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                ModelsCVS.View.Refresh();
                CostumesCVS.View.Refresh();
                if (AbilitiesCVS != null)
                    AbilitiesCVS.View.Refresh();
                NotifyOfPropertyChange(() => Filter);
            }
        }

        private ObservableCollection<string> models;
        public ObservableCollection<string> Models
        {
            get
            {
                return models;
            }
            set
            {
                models = value;
                NotifyOfPropertyChange(() => Models);
            }
        }

        private ObservableCollection<string> costumes;
        public ObservableCollection<string> Costumes
        {
            get
            {
                return costumes;
            }
            set
            {
                costumes = value;
                NotifyOfPropertyChange(() => Costumes);
            }
        }

        private CollectionViewSource modelsCVS;
        public CollectionViewSource ModelsCVS
        {
            get
            {
                return modelsCVS;
            }
        }

        private CollectionViewSource costumesCVS;
        public CollectionViewSource CostumesCVS
        {
            get
            {
                return costumesCVS;
            }
        }

        private CollectionViewSource abilitiesCVS;
        public CollectionViewSource AbilitiesCVS
        {
            get
            {
                return abilitiesCVS;
            }
        }

        public bool IsDefault
        {
            get
            {
                return EditedIdentity != null && EditedIdentity == Owner.DefaultIdentity;
            }
            set
            {
                if (value == true)
                    Owner.Identities.Default = EditedIdentity;
                else if (value == false)
                    Owner.Identities.Default = null;
                NotifyOfPropertyChange(() => IsDefault);
                SaveIdentity();
            }
        }

        private bool isShowingIdentityEditor;
        public bool IsShowingIdentityEditor
        {
            get
            {
                return isShowingIdentityEditor;
            }
            set
            {
                isShowingIdentityEditor = value;
                NotifyOfPropertyChange(() => IsShowingIdentityEditor);
            }
        }

        #endregion

        #region Constructor

        public IdentityEditorViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.Subscribe(this);

            CreateModelsViewSource();
            CreateCostumesViewSource();
        }

        #endregion

        #region Methods

        #region Property Changed Event Handlers

        private void OwnerIdentities_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Default":
                    NotifyOfPropertyChange(() => IsDefault);
                    break;
            }
        }

        private void AvailableIdentities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                && e.OldItems.Contains(this.EditedIdentity))
            {
                this.UnloadIdentity();
            }
        }

        private void EditedIdentity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Surface" || e.PropertyName == "AnimationOnLoad")
            {
                if (Owner.IsSpawned)
                {
                    if (Owner.ActiveIdentity == EditedIdentity)
                    {
                        Owner.Target(false);
                        Owner.ActiveIdentity.Play();
                    }
                }
            }
            SaveIdentity();
        }

        #endregion

        #region Load Identity

        public void Handle(EditIdentityEvent message)
        {
            this.LoadIdentity(message.EditedIdentity);
        }

        private void LoadIdentity(Identity identity)
        {
            UnloadIdentity();
            Filter = null;
            this.EditedIdentity = identity;
            this.Owner.Identities.CollectionChanged += AvailableIdentities_CollectionChanged;
            this.IsShowingIdentityEditor = true;
            this.BeginLoadAbilities();
            NotifyOfPropertyChange(() => IsDefault);
        }

        #endregion

        #region Unload Identity and Close Editor

        private void UnloadIdentity()
        {
            this.EditedIdentity = null;
            if (Owner != null)
                this.Owner.Identities.CollectionChanged -= AvailableIdentities_CollectionChanged;
            this.IsShowingIdentityEditor = false;
        }

        public void CloseEditor()
        {
            this.UnloadIdentity();
        }

        #endregion

        #region Rename Identity
        public void EnterEditMode(object state)
        {
            this.originalName = EditedIdentity.Name;
            OnEditModeEnter(state, null);
        }

        public void CancelEditMode(object state)
        {
            EditedIdentity.Name = this.originalName;
            OnEditModeLeave(state, null);
        }

        public void SubmitIdentityRename(object state)
        {
            if (this.originalName != null)
            {
                string updatedName = ControlUtilities.GetTextFromControlObject(state);

                bool duplicateName = this.Owner.Identities.CheckDuplicateNameForActions(originalName, updatedName);

                if (!duplicateName)
                {
                    this.Owner.Identities.RenameAction(originalName, updatedName);
                    originalName = null;
                    OnEditModeLeave(state, null);
                    SaveIdentity();
                }
                else
                {
                    System.Windows.MessageBox.Show("The name already exists. Please choose another name!");
                    this.CancelEditMode(state);
                }
            }
        }

        #endregion

        #region Create Collectins - Models and Surfaces and Abilities

        private void CreateModelsViewSource()
        {
            models = new ObservableCollection<string>(File.ReadAllLines(Path.Combine(HeroUI.Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_MODELS_FILENAME)).OrderBy(m => m, new StringValueComparer()));
            modelsCVS = new CollectionViewSource();
            modelsCVS.Source = Models;
            modelsCVS.View.Filter += stringsCVS_Filter;
        }

        private void CreateCostumesViewSource()
        {
            costumes = new ObservableCollection<string>(
                Directory.EnumerateFiles
                    (Path.Combine(
                        HeroUI.Properties.Settings.Default.GameDirectory,
                        GAME_COSTUMES_FOLDERNAME),
                    "*.costume").Select((file) => { return Path.GetFileNameWithoutExtension(file); }).OrderBy(c => c, new StringValueComparer()));
            costumesCVS = new CollectionViewSource();
            costumesCVS.Source = Costumes;
            costumesCVS.View.Filter += stringsCVS_Filter;
        }


        private void CreateAbilitiesViewSource(ObservableCollection<AnimatedAbility.AnimatedAbility> abilities)
        {
            abilitiesCVS = new CollectionViewSource();
            AnimatedAbility.AnimatedAbility none = new AnimatedAbility.AnimatedAbilityImpl();
            none.Name = "None";
            abilities.Add(none);
            abilitiesCVS.Source = new ObservableCollection<AnimatedAbility.AnimatedAbility>(abilities.Where((an) => { return an.Owner == this.Owner; /*&& an.IsAttack == false;*/}).OrderBy(a => a.Order));
            abilitiesCVS.View.Filter += abilitiesCVS_Filter;
            AnimatedAbility.AnimatedAbility moveTo = null;
            if (EditedIdentity != null && EditedIdentity.AnimationOnLoad != null)
                moveTo = EditedIdentity.AnimationOnLoad;
            else
                moveTo = none;
            abilitiesCVS.View.MoveCurrentTo(moveTo);
            NotifyOfPropertyChange(() => AbilitiesCVS);
        }

        #endregion

        #region Filter

        private bool abilitiesCVS_Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }

            string strItem = (item as AnimatedAbility.AnimatedAbility).Name;
            if (EditedIdentity != null && EditedIdentity.AnimationOnLoad == item as AnimatedAbility.AnimatedAbility)
            {
                return true;
            }
            return new Regex(Filter, RegexOptions.IgnoreCase).IsMatch(strItem);
        }

        private bool stringsCVS_Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }

            string strItem = item as string;
            if (EditedIdentity != null && EditedIdentity.Surface == strItem)
            {
                return true;
            }
            return new Regex(Filter, RegexOptions.IgnoreCase).IsMatch(strItem);
        }

        #endregion

        #region Load Abilities

        private void BeginLoadAbilities()
        {

        }
            
        private void EndLoadAbilities()
        {

        }

        #endregion

        #region Save Identity
        private void SaveIdentity()
        {
            this.EventAggregator.Publish(new CrowdCollectionModifiedEvent(), action => System.Windows.Application.Current.Dispatcher.Invoke(action));
        }

        #endregion

        #endregion
    }
}
