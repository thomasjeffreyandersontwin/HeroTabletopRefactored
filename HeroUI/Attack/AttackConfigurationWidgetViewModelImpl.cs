using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeroVirtualTabletop.Attack
{
    public class AttackConfigurationWidgetViewModelImpl : PropertyChangedBase, AttackConfigurationWidgetViewModel
    {
        #region Private Fields

        #endregion

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private AnimatedAttack activeAttack;
        public AnimatedAttack ActiveAttack
        {
            get
            {
                return activeAttack;
            }
            set
            {
                activeAttack = value;
                NotifyOfPropertyChange(() => ActiveAttack);
            }
        }

        private ObservableCollection<AttackInstructions> attackInstructionscollection;
        public ObservableCollection<AttackInstructions> AttackInstructionsCollection
        {
            get
            {
                return attackInstructionscollection;
            }
            set
            {
                attackInstructionscollection = value;
                NotifyOfPropertyChange(() => AttackInstructionsCollection);
            }
        }

        #endregion

        #region Constructor

        public AttackConfigurationWidgetViewModelImpl(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;

            this.EventAggregator.Subscribe(this);
            //this.eventAggregator.GetEvent<ConfigureActiveAttackEvent>().Subscribe(this.ConfigureActiveAttack);
            //this.eventAggregator.GetEvent<ConfirmAttackEvent>().Subscribe(this.SetActiveAttack);
        }

        #endregion
        
        #region Methods

        private void ChangeCenterTarget(object state)
        {
            AttackInstructions ins = state as AttackInstructions;
            foreach(AttackInstructions ai in this.AttackInstructionsCollection.Where(instr => instr != ins))
            {
                ai.IsCenterOfAreaEffectattack = false;
            }
        }

        private void ConfigureActiveAttack(AnimatedAttack attack, List<AttackInstructions> instructionsList)
        {
            this.AttackInstructionsCollection = new ObservableCollection<AttackInstructions>(instructionsList);
            this.ActiveAttack = attack;
        }

        private void SetActiveAttack(object state)
        {
            SetActiveAttack();
        }
        private void SetActiveAttack()
        {
            foreach (AttackInstructions ai in this.AttackInstructionsCollection)
            {
                SetAttackEffect(ai);
            }
            // Change mouse pointer to back to bulls eye
            Cursor cursor = new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("HeroUI.Attack.Bullseye.cur"));
            Mouse.OverrideCursor = cursor;

            //this.eventAggregator.GetEvent<CloseActiveAttackEvent>().Publish(this.ActiveAttack);
            //this.eventAggregator.GetEvent<SetActiveAttackEvent>().Publish(new Tuple<List<Character>, Attack>(this.DefendingCharacters.ToList(), this.ActiveAttack));
        }
        private void SetAttackEffect(AttackInstructions instructions)
        {
            if (instructions.Impacts.Contains(DefaultAbilities.STUNNED))
            {
                AnimatedAbility.AnimatedAbility stunAbility = instructions.Defender.Abilities[DefaultAbilities.STUNNED];
                AnimatableCharacterState state = new AnimatableCharacterStateImpl(stunAbility, instructions.Defender);
                instructions.Defender.AddState(state, false);
            }
            if (instructions.Impacts.Contains(DefaultAbilities.UNCONSCIOUS))
            {
                AnimatedAbility.AnimatedAbility unconsciousAbility = instructions.Defender.Abilities[DefaultAbilities.UNCONSCIOUS];
                AnimatableCharacterState state = new AnimatableCharacterStateImpl(unconsciousAbility, instructions.Defender);
                instructions.Defender.AddState(state, false);
            }
            if (instructions.Impacts.Contains(DefaultAbilities.DYING))
            {
                AnimatedAbility.AnimatedAbility dyingAbility = instructions.Defender.Abilities[DefaultAbilities.DYING];
                AnimatableCharacterState state = new AnimatableCharacterStateImpl(dyingAbility, instructions.Defender);
                instructions.Defender.AddState(state, false);
            }
            if (instructions.Impacts.Contains(DefaultAbilities.DEAD))
            {
                AnimatedAbility.AnimatedAbility deadAbility = instructions.Defender.Abilities[DefaultAbilities.DEAD];
                AnimatableCharacterState state = new AnimatableCharacterStateImpl(deadAbility, instructions.Defender);
                instructions.Defender.AddState(state, false);
            }
        }
        private void CancelActiveAttack()
        {
            foreach (var ins in this.AttackInstructionsCollection)
            {
                ins.Defender.RemoveStateByName(DefaultAbilities.DEAD);
                ins.Defender.RemoveStateByName(DefaultAbilities.DYING);
                ins.Defender.RemoveStateByName(DefaultAbilities.STUNNED);
                ins.Defender.RemoveStateByName(DefaultAbilities.UNCONSCIOUS);
                ins.Defender = null;
                ins.Impacts.Clear();
                ins.IsCenterOfAreaEffectattack = false;
                ins.KnockbackDistance = 0;
            }
            //this.eventAggregator.GetEvent<CloseActiveAttackEvent>().Publish(this.DefendingCharacters.ToList());
        }

        #endregion
    }
}
