using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Common;
using Newtonsoft.Json;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Movement;

namespace HeroVirtualTabletop.ManagedCharacter
{
    /// <summary>
    /// CharacterActionListImpl is Former Option Group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonObject]
    public class CharacterActionListImpl<T> : ObservableCollection<T> , CharacterActionList<T> where T : CharacterAction
    {
        private T _active;
        private T _default;

        private const string ABILITY_OPTION_GROUP_NAME = "Powers";
        private const string IDENTITY_OPTION_GROUP_NAME = "Identities";
        private const string MOVEMENT_OPTION_GROUP_NAME = "Movements";

        public CharacterActionListImpl(CharacterActionType type, KeyBindCommandGenerator generator,
            CharacterActionContainer owner)
        {
            Type = type;
            Generator = generator;
            //ListByOrder = new SortedDictionary<int, T>();
            Owner = (ManagedCharacter)owner;
        }
        [JsonConstructor]
        private CharacterActionListImpl()
        {

        }
        [JsonIgnore]
        public KeyBindCommandGenerator Generator { get; set; }        
        [JsonProperty]
        public virtual ManagedCharacter Owner
        {
            get;
            set;
        }
        [JsonProperty(Order = 1)]
        public CharacterActionType Type
        {
            get;
            set;
        }
        [JsonProperty(Order = 2)]
        public T Active
        {
            get
            {
                if (_active == null)
                    if (_default != null)
                    {
                        _active = _default;
                    }
                    else
                    {
                        if (Count > 0)
                            _active = this.First();
                    }
                return _active;
            }

            set
            {
                if (value != null)
                    _active = value;
                OnPropertyChanged("Active");
            }
        }

        public void Deactivate()
        {
            _active = default(T);
        }
        [JsonProperty(Order = 3)]
        public T Default
        {
            get
            {
                if (_default == null)
                    if (Count > 0)
                        _default = this.First();
                return _default;
            }
            set
            {
               // if (value != null)
                    if (this.Contains(value))
                        _default = value;

                OnPropertyChanged("Default");
            }
        }
        private string _name;
        [JsonProperty(Order =0)]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        [JsonIgnore]
        public T this[string key]
        {
            get
            {
                return this.FirstOrDefault(a => a.Name == key);
            }
            set
            {
                if(value.Name == key)
                {
                    var existing = this.FirstOrDefault(a => a.Name == key);
                    if(!existing.Equals(value))
                    {
                        this.Remove(existing);
                        this.InsertAction(value);
                    }
                }
            }
        }

        [JsonIgnore]
        public T this[int index]
        {
            get
            {
                return this.ElementAt(index);
            }
            set
            {
                //Need to check for valid index and existing T
                this.Insert(index, value);
            }
        }

        [JsonProperty(Order =1)]
        public T[] Actions
        {
            get
            {
                return this.ToArray();
            }
            set
            {
                if (value != null)
                    this.InsertMany(value.ToList());
            }
        }

        [JsonIgnore]
        public bool IsStandardActionGroup
        {
            get
            {
                return Name == IDENTITY_OPTION_GROUP_NAME || Name == ABILITY_OPTION_GROUP_NAME || Name == MOVEMENT_OPTION_GROUP_NAME;
            }
        }

        #region Add/Insert

        public T GetNewAction()
        {
            T action = default(T);
            if (Type == CharacterActionType.Identity)
            {
                action = (T)GetNewIdentity();
            }
            else if (Type == CharacterActionType.Ability)
            {
                action = (T)GetNewAbility();
            }
            else if (Type == CharacterActionType.Movement)
            {
                action = (T)GetNewMovement();
            }
            
            return action;
        }

        public string GetNewValidActionName(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (Type == CharacterActionType.Identity)
                    name = "Identity";
                if (Type == CharacterActionType.Movement)
                    name = "Movement";
                if (Type == CharacterActionType.Ability)
                    name = "Ability";
            }
            var suffix = string.Empty;
            var i = 0;

            while (this.Cast<CharacterAction>().Any(action => action.Name == name + suffix))
                suffix = $" ({++i})";
            return $"{name}{suffix}".Trim();
        }

       
        public void InsertAction(T action)
        {
            action.Owner = Owner;
            action.Generator = Generator;
            this.Add(action);
            FixOrders();
        }

        public void InsertAction(T action, int index)
        {
            var existingIndex = this.IndexOf((T)action);
            if (existingIndex >= 0)
            {
                this.RemoveAt(existingIndex);
                if (index > 0 && index >= this.Count)
                    index -= 1;
            }
            this.Insert(index, (T)action);
            FixOrders();
        }

        public void InsertMany(List<T> actions)
        {
            foreach(T action in actions)
            {
                InsertAction(action);
            }
        }

        public T AddNew(T newAction)
        {
            newAction.Owner = Owner;
            newAction.Generator = Generator;
            newAction.Name = GetNewValidActionName(newAction.Name);
            this.Add(newAction);
            FixOrders();
            return newAction;
        }

        public void InsertActionAfter(T elementToAdd, T elementToAddAfter)
        {
            var index = this.IndexOf(elementToAddAfter);
            if(index >= 0)
            {
                var position = index + 1;
                this.Insert(position, elementToAdd);
            }
            FixOrders();
        }

        public bool CheckDuplicateName(string newName)
        {
            bool isDuplicate = this.Name != newName && this.Owner.CharacterActionGroups.FirstOrDefault(a => a.Name == newName) != null;
            return isDuplicate;
        }

        private Identity GetNewIdentity()
        {
            IdentityImpl identity = new IdentityImpl();
            identity.Name = GetNewValidActionName();
            identity.Type = SurfaceType.Costume;
            identity.Surface = identity.Name;

            return identity;
        }

        private AnimatedAbility.AnimatedAbility GetNewAbility()
        {
            AnimatedAbility.AnimatedAbility ability = new AreaEffectAttackImpl();
            return ability;
        }

        private CharacterMovement GetNewMovement()
        {
            CharacterMovement characterMovement = new CharacterMovementImpl();
            return characterMovement;
        }

        #endregion

        #region Remove/Clear

        public void RemoveAction(T action)
        {
            if (this.Contains(action))
                this.Remove(action);
            FixOrders();
        }

        public void RemoveActionAt(int index)
        {
            this.RemoveAt(index);
            FixOrders();
        }

        public void ClearAll()
        {
            this.Clear();
        }

        #endregion

        #region Clone

        public CharacterActionList<T> Clone()
        {
            var cloneList = new CharacterActionListImpl<T>(Type, Generator, Owner);
            foreach (var anAction in this)
            {
                var clone = (T) anAction.Clone();
                cloneList.InsertAction(clone);
            }
            return cloneList;
        }

        #endregion

        #region Rename

        public void RenameAction(string oldName, string newName)
        {
            var obj = this.First(a => a.Name == oldName);
            obj.Name = newName;
            if (Type == CharacterActionType.Identity)
                (obj as Identity).Surface = newName;
        }

        public void Rename(string newName)
        {
            if(Name != newName)
            {
                this.Name = newName;
            }
        }

        #endregion

        public void PlayByKey(string shortcut)
        {
        }
        private void FixOrders()
        {
            for (int i = 0; i < this.Count; i++)
                this[i].Order = i + 1;
        }

        #region INotifyPropertyChanged

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion INotifyPropertyChanged
    }

    public abstract class CharacterActionImpl : PropertyChangedBase, CharacterAction
    {
        protected CharacterActionImpl(ManagedCharacter owner, string name, KeyBindCommandGenerator generator,
            string shortcut)
        {
            Name = name;
            Owner = owner;
            Generator = generator;
            KeyboardShortcut = shortcut;
        }
        public virtual CharacterActionContainer Owner { get; set; }
        protected CharacterActionImpl()
        {
        }

        [JsonIgnore]
        public string KeyboardShortcut { get; set; }
        [JsonIgnore]
        public virtual KeyBindCommandGenerator Generator { get; set; }

        private string _name;
        [JsonProperty]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public virtual int Order { get; set; }
        

        public abstract CharacterAction Clone();
        public abstract void Play(bool completeEvent=true);
        public virtual void Stop(bool completeEvent = true)
        {
            throw new NotImplementedException();
        }
    }
}