using System;
using System.Collections.Generic;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Common;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AnimatedAbilityImpl : CharacterActionImpl, AnimatedAbility
    {
        private AnimationSequencer _sequencer;
        private AnimatedCharacter _target;
        public AnimatedAbilityImpl(AnimationSequencer clonedsequencer)
        {
            _sequencer = clonedsequencer;
        }
        public AnimatedAbilityImpl()
        {
        }
        [JsonProperty]
        public bool Persistant { get; set; }
        [JsonProperty]
        public AnimatedCharacter Target
        {
            get { return _target; }
            set
            {
                if (_sequencer is AnimationElement)
                    (_sequencer as AnimationElement).Target = value;
                _target = value;
            }
        }
         
        public override CharacterActionContainer Owner
        {
            get { return Target as ManagedCharacter.ManagedCharacter; }

            set
            {
                if (value is AnimatedCharacter)
                    Target = value as AnimatedCharacter;
            }
        }
        [JsonProperty]
        public AnimatedAbility StopAbility { get; set; }   
        [JsonProperty]
        public AnimationSequencer Sequencer
        {
            get
            {
                return _sequencer ?? (_sequencer = new AnimationSequencerImpl(Target));
            }
            set
            {
                _sequencer = value;
                NotifyOfPropertyChange(() => Sequencer);
            }
        }
        public override void Play(bool completeEvent=true)
        {
            Play(Target);
        }
        public void Play(List<AnimatedCharacter> targets)
        {
            Sequencer.Play(targets);
            foreach (var t in targets)
            {
                addStateToTargetsIfPersistent(t);
            }
        }
        public void Play(AnimatedCharacter target)
        {
            Sequencer.Play(target);
            addStateToTargetsIfPersistent(target);
        }
        private void addStateToTargetsIfPersistent(AnimatedCharacter target)
        {
            if (Persistant)
            {
                AnimatableCharacterState newstate = new AnimatableCharacterStateImpl(this, Target);
                newstate.AbilityAlreadyPlayed = true;
                target.AddState(newstate);
            }
        }

        public AnimationElement GetNewAnimationElement(AnimationElementType animationElementType)
        {
            return Sequencer.GetNewAnimationElement(animationElementType);
        }

        public virtual void Stop(bool completedEvent = true)
        {
            Stop(Target);
            
        }
        public void Stop(AnimatedCharacter target)
        {
            Sequencer.Stop(target);
            target.RemoveStateByName(Name);
        }
        public void Stop(List<AnimatedCharacter> targets)
        {
        }
        public ObservableCollection<AnimationElement> AnimationElements => _sequencer?.AnimationElements;
        [JsonProperty]
        public SequenceType Type
        {
            get {
                return Sequencer.Type;
            }

            set {
                Sequencer.Type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }

        public void InsertMany(List<AnimationElement> animationElements)
        {
            Sequencer.InsertMany(animationElements);
            NotifyOfPropertyChange(() => AnimationElements);
        }
        public void InsertElement(AnimationElement animationElement)
        {
            Sequencer.InsertElement(animationElement);
            NotifyOfPropertyChange(() => AnimationElements);
        }
        public void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter)
        {
            Sequencer.InsertElementAfter(toInsert, moveAfter);
            NotifyOfPropertyChange(() => AnimationElements);
        }
        public void RemoveElement(AnimationElement animationElement)
        {
            Sequencer.RemoveElement(animationElement);
            NotifyOfPropertyChange(() => AnimationElements);
        }

        public AnimatedAbility Clone(AnimatedCharacter target)
        {
            var clonedSequence = ((AnimationSequencerImpl) Sequencer).Clone(target) as AnimationSequencer;

            AnimatedAbility clone = new AnimatedAbilityImpl(clonedSequence);
            clone.Target = target;
            clone.Name = Name;
            clone.KeyboardShortcut = KeyboardShortcut;
            clone.Persistant = Persistant;
            clone.Target = Target;
            clone.StopAbility = StopAbility.Clone(target);
            return clone;
        }
        public override CharacterAction Clone()
        {
            return Clone(Target);
        }
        public bool Equals(AnimatedAbility other)
        {
            if (other.KeyboardShortcut != KeyboardShortcut) return false;
            if (other.Name != Name) return false;
            if (other.Order != Order) return false;
            if (other.Persistant != Persistant) return false;
            if (other.Sequencer.Equals(Sequencer) == false) return false;
            return true;
        }

        public void Rename(string newName)
        {
            this.Name = newName;
        }
    }
}