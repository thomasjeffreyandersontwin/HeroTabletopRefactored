using System;
using System.Collections.Generic;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Common;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using HeroVirtualTabletop.Crowd;
using System.Linq;
using HeroVirtualTabletop.Attack;
using System.Windows.Input;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AnimatedAbilityImpl : CharacterActionImpl, AnimatedAbility
    {
        private AnimationSequencer _sequencer;
        private AnimatedCharacter _target;
        public const string ATTACK_ONHIT_NAME_EXTENSION = " - OnHit";
        public AnimatedAbilityImpl(AnimationSequencer clonedsequencer)
        {
            _sequencer = clonedsequencer;
        }
        public AnimatedAbilityImpl()
        {
        }
        private bool persistent;
        [JsonProperty]
        public bool Persistent
        {
            get
            {
                return persistent;
            }
            set
            {
                persistent = value;
                NotifyOfPropertyChange(() => Persistent);
            }
        }
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
        private Key activationKey;
        [JsonProperty]
        public Key ActivationKey
        {
            get
            {
                return activationKey;
            }
            set
            {
                activationKey = value;
                NotifyOfPropertyChange(() => ActivationKey);
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
            // If there is an ability with the same name in any of the targets, we need to construct a list of different abilities to play
            bool customAbilityExists = targets.Any(t => t.Abilities.Any(a => a.Name == this.Name));
            if (customAbilityExists)
            {
                List<AnimatedAbility> abilities = new List<AnimatedAbility>();
                foreach(AnimatedCharacter target in targets)
                {
                    if(target.Abilities.Any(a => a.Name == this.Name && a != this))
                    {
                        AnimatedAbility ability = target.Abilities[this.Name];
                        ability.Target = target;
                        abilities.Add(ability);
                    }
                    else
                    {
                        AnimatedAbility cloneAbility = this.Clone(target);
                        abilities.Add(cloneAbility);
                    }   
                }
                Play(abilities);
                foreach(var ability in abilities.Where(a => a.Persistent))
                {
                    addStateToTargetIfPersistent(ability, ability.Target);
                }
            }
            else
            {
                Sequencer.Play(targets);
                foreach (var t in targets)
                {
                    addStateToTargetIfPersistent(this, t);
                }
            }
        }

        
        public void Play(AnimatedCharacter target)
        {
            // If there is an ability with the same name in the target, we should play that ability instead
            var abilityWithSameName = target.Abilities?.FirstOrDefault(a => a.Name == this.Name && a != this);
            if(abilityWithSameName != null)
            {
                abilityWithSameName.Play(target);
            }
            else
            {
                Sequencer.Play(target);
                addStateToTargetIfPersistent(this, target);
            }
        }
        private void addStateToTargetIfPersistent(AnimatedAbility ability, AnimatedCharacter target)
        {
            if (ability.Persistent)
            {
                AnimatableCharacterState newstate = new AnimatableCharacterStateImpl(ability, target);
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
            target.RemoveStateFromActiveStates(Name);
        }
        public void Stop(List<AnimatedCharacter> targets)
        {
        }
        public ObservableCollection<AnimationElement> AnimationElements => Sequencer?.AnimationElements;
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
            clone.Persistent = Persistent;
            clone.StopAbility = StopAbility?.Clone(target);
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
            if (other.Persistent != Persistent) return false;
            if (other.Sequencer.Equals(Sequencer) == false) return false;
            return true;
        }

        public void Rename(string newName)
        {
            this.Name = newName;
        }

        public AnimatedAttack TransformToAttack()
        {
            AnimatedAttackImpl attack = new AnimatedAttackImpl();
            attack.Name = this.Name;
            attack.Order = this.Order;
            attack.Owner = this.Owner;
            attack.Sequencer = this.Sequencer;
            attack.OnHitAnimation = new AnimatedAbilityImpl();
            attack.OnHitAnimation.Rename(attack.Name + ATTACK_ONHIT_NAME_EXTENSION);
            attack.OnHitAnimation.Target = this.Target;
            attack.Persistent = this.Persistent;
            attack.Generator = this.Generator;
            attack.KeyboardShortcut = this.KeyboardShortcut;
            attack.Target = this.Target;
            attack.Type = this.Type;

            return attack;
        }

        public void Play(List<AnimatedAbility> abilities)
        {
            Sequencer.Play(abilities);
        }
    }

    public class AbilityClipboardImpl : AbilityClipboard
    {
        private object currentClipboardObject;
        public ClipboardAction CurrentClipboardAction
        {
            get; set;
        }

        public void CopyToClipboard(AnimatedAbility ability)
        {
            CurrentClipboardAction = ClipboardAction.Clone;
            currentClipboardObject = ability;
        }

        public void CopyToClipboard(AnimationElement animationElement)
        {
            CurrentClipboardAction = ClipboardAction.Clone;
            currentClipboardObject = animationElement;
        }

        public void CutToClipboard(AnimationElement animationElement, AnimationSequencer sourceSequence)
        {
            this.CurrentClipboardAction = ClipboardAction.Cut;
            this.currentClipboardObject = new object[] { animationElement, sourceSequence };
        }

        public bool CheckPasteEligibilityFromClipboard(AnimationSequencer destinationSequence)
        {
            bool canPaste = false;
            switch (this.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                    if (this.currentClipboardObject != null)
                    {
                        if (this.currentClipboardObject is AnimationSequencer)
                        {
                            AnimationSequencer seqElement = this.currentClipboardObject as AnimationSequencer;
                            List<AnimationElement> elementList = AnimationSequencerImpl.GetFlattenedAnimationList(seqElement.AnimationElements);
                            if (!(elementList.Where((an) => { return an.AnimationElementType == AnimationElementType.Reference; }).Any((an) => { return (an as ReferenceElement).Reference?.Ability == destinationSequence; })))
                                canPaste = true;
                        }
                        else if (this.currentClipboardObject is ReferenceElement)
                        {
                            ReferenceElement refAbility = this.currentClipboardObject as ReferenceElement;
                            if (refAbility.Reference?.Ability == destinationSequence)
                                canPaste = false;
                            else if (refAbility.Reference?.Ability?.AnimationElements != null && refAbility.Reference?.Ability?.AnimationElements.Count > 0)
                            {
                                bool refexists = false;
                                if (refAbility.Reference.Ability.AnimationElements.Contains(destinationSequence as AnimationElement))
                                    refexists = true;
                                List<AnimationElement> elementList = AnimationSequencerImpl.GetFlattenedAnimationList(refAbility.Reference.Ability.AnimationElements);
                                if (elementList.Where((an) => { return an.AnimationElementType == AnimationElementType.Reference; }).Any((an) => { return (an as ReferenceElement).Reference.Ability == destinationSequence; }))
                                    refexists = true;
                                if (!refexists)
                                    canPaste = true;
                            }
                            else
                                canPaste = true;
                        }
                        else
                            canPaste = true;
                    }
                    break;
                case ClipboardAction.Cut:
                    if (this.currentClipboardObject != null)
                    {
                        object[] clipObj = this.currentClipboardObject as object[];
                        AnimationElement clipboardAnimationElement = clipObj[0] as AnimationElement;
                        AnimationSequencer clipboardAnimationSequencer = clipObj[1] as AnimationSequencer;
                        if (clipboardAnimationElement is SequenceElement)
                        {
                            SequenceElement seqElement = clipboardAnimationElement as SequenceElement;
                            List<AnimationElement> elementList = AnimationSequencerImpl.GetFlattenedAnimationList(seqElement.AnimationElements);
                            if (!(elementList.Where((an) => { return an.AnimationElementType == AnimationElementType.Reference; }).Any((an) => { return (an as ReferenceElement).Reference.Ability == destinationSequence; })))
                                canPaste = true;
                        }
                        else if (clipboardAnimationElement is ReferenceElement)
                        {
                            ReferenceElement refAbility = clipboardAnimationElement as ReferenceElement;
                            if (refAbility.Reference?.Ability == destinationSequence)
                                canPaste = false;
                            else if (refAbility.Reference?.Ability?.AnimationElements != null && refAbility.Reference?.Ability?.AnimationElements.Count > 0)
                            {
                                bool refexists = false;
                                if (refAbility.Reference.Ability.AnimationElements.Contains(destinationSequence as AnimationElement))
                                    refexists = true;
                                List<AnimationElement> elementList = AnimationSequencerImpl.GetFlattenedAnimationList(refAbility.Reference.Ability.AnimationElements);
                                if (elementList.Where((an) => { return an.AnimationElementType == AnimationElementType.Reference; }).Any((an) => { return (an as ReferenceElement).Reference.Ability == destinationSequence; }))
                                    refexists = true;
                                if (!refexists)
                                    canPaste = true;
                            }
                            else
                                canPaste = true;
                        }
                        else
                            canPaste = true;
                    }
                    break;
            }
            return canPaste;
        }

        public AnimationElement PasteFromClipboard(AnimationSequencer destinationSequence)
        {
            AnimationElement pastedMember = null;
            switch (this.CurrentClipboardAction)
            {
                case ClipboardAction.Clone:
                    pastedMember = cloneAndPaste(destinationSequence);
                    break;
                case ClipboardAction.Cut:
                    pastedMember = cutAndPaste(destinationSequence);
                    break;
            }
            this.CurrentClipboardAction = ClipboardAction.None;
            this.currentClipboardObject = null;

            return pastedMember;
        }

        private AnimationElement cloneAndPaste(AnimationSequencer destinationSequence)
        {
            AnimationElement clonedElement = null;
            if (currentClipboardObject is AnimationElement)
            {
                var cloningElement = currentClipboardObject as AnimationElement;
                clonedElement = cloningElement.Clone(cloningElement.Target);

                destinationSequence.InsertElement(clonedElement);
            }
            else
            {
                var cloningAbility = currentClipboardObject as AnimatedAbility;
                AnimationSequencer sequencer = cloningAbility.Clone() as AnimationSequencer;

                AnimatedCharacter target = null;
                if (destinationSequence is AnimatedAbility)
                    target = (destinationSequence as AnimatedAbility).Target;
                else
                    target = (destinationSequence as SequenceElement).Target;

                SequenceElementImpl sequenceElement = new HeroVirtualTabletop.AnimatedAbility.SequenceElementImpl(sequencer, target);
                sequenceElement.Type = sequencer.Type;
                sequenceElement.Name = "Sequence: " + sequenceElement.Type.ToString();
                clonedElement = sequenceElement;

                destinationSequence.InsertElement(sequenceElement);
            }
            return clonedElement;
        }

        private AnimationElement cutAndPaste(AnimationSequencer destinationSequence)
        {
            object[] clipboardObj = this.currentClipboardObject as object[];
            AnimationElement cutElement = clipboardObj[0] as AnimationElement;
            AnimationSequencer parentSequence = clipboardObj[1] as AnimationSequencer;

            parentSequence.RemoveElement(cutElement);
            destinationSequence.InsertElement(cutElement);

            return cutElement;
        }

    }
}