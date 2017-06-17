using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Common;
using Newtonsoft.Json;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
                    //else
                      //  throw new ArgumentException("action cant be set to default it doesnt exist for character");
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
                        this.InsertElement(value);
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

        public void ClearAll()
        {
            this.Clear();
        }

        public void InsertElement(T action)
        {
            action.Owner = Owner;
            action.Generator = Generator;
            this.Add(action);
            FixOrders();
        }

        public void InsertMany(List<T> actions)
        {
            foreach(T action in actions)
            {
                InsertElement(action);
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

        public void InsertElementAfter(T elementToAdd, T elementToAddAfter)
        {
            var index = this.IndexOf(elementToAddAfter);
            if(index >= 0)
            {
                var position = index + 1;
                this.Insert(position, elementToAdd);
            }
            FixOrders();
        }

        public void RemoveElement(T element)
        {
            if (this.Contains(element))
                this.Remove(element);
            FixOrders();
        }

        public CharacterActionList<T> Clone()
        {
            var cloneList = new CharacterActionListImpl<T>(Type, Generator, Owner);
            foreach (var anAction in this)
            {
                var clone = (T) anAction.Clone();
                cloneList.InsertElement(clone);
            }
            return cloneList;
        }

        public void PlayByKey(string shortcut)
        {
        }

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
        public bool CheckDuplicateName(string newName)
        {
            bool isDuplicate = this.Name != newName && this.Owner.CharacterActionGroups.FirstOrDefault(a => a.Name == newName) != null;
            return isDuplicate;
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