using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTableTop.Desktop;
using HeroVirtualTableTop.Common;
namespace HeroVirtualTableTop.ManagedCharacter
{
    public class CharacterActionListImpl<T> : OrderedCollectionImpl<T> , CharacterActionList<T> where T : CharacterAction
    {
        private T _active;
        private T _default;

        public CharacterActionListImpl(CharacterActionType type, KeyBindCommandGenerator generator,
            CharacterActionContainer owner)
        {
            Type = type;
            Generator = generator;
            //ListByOrder = new SortedDictionary<int, T>();
            Owner = (ManagedCharacter)owner;
        }

        public KeyBindCommandGenerator Generator { get; set; }
        
        
        public virtual ManagedCharacter Owner { get; }

        public CharacterActionType Type { get; set; }

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
                        if (Count > 1)
                            _active = Values.First();
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

        public T Default
        {
            get
            {
                if (_default == null)
                    if (Count > 1)
                        _default = Values.First();
                return _default;
            }
            set
            {
               // if (value != null)
                    if (ContainsValue(value))
                        _default = value;
                    //else
                      //  throw new ArgumentException("action cant be set to default it doesnt exist for character");
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

            while (Values.Cast<CharacterAction>().Any(action => action.Name == name + suffix))
                suffix = $" ({++i})";
            return $"{name}{suffix}".Trim();
        }

        public override void InsertElement(T action)
        {
            action.Owner = Owner;
            action.Generator = Generator;
            base.InsertElement(action);
        }

        public T AddNew(T newAction)
        {
            newAction.Owner = Owner;
            newAction.Generator = Generator;
            if (ListByOrder.Count == 0)
                newAction.Order = 1;
            else
                newAction.Order = ListByOrder.Last().Key + 1;
            newAction.Name = GetNewValidActionName(newAction.Name);
            Add(newAction.Name, newAction);
            return newAction;
        }

        public CharacterActionList<T> Clone()
        {
            var cloneList = new CharacterActionListImpl<T>(Type, Generator, Owner);
            foreach (var anAction in Values)
            {
                var clone = (T) anAction.Clone();
                cloneList.InsertElement(clone);
            }
            return cloneList;
        }

        public void PlayByKey(string shortcut)
        {
        }
    }

    public abstract class CharacterActionImpl : CharacterAction
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

        public string KeyboardShortcut { get; set; }
        public virtual KeyBindCommandGenerator Generator { get; set; }

        public virtual string Name { get; set; }
        public virtual int Order { get; set; }
        

        public abstract CharacterAction Clone();
        public abstract void Play(bool completeEvent=true);
        public virtual void Stop(bool completeEvent = true)
        {
            throw new NotImplementedException();
        }
    }
}