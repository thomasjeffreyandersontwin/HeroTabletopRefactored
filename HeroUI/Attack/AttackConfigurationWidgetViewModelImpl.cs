using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HeroVirtualTabletop.Attack
{
    public class AttackConfigurationWidgetViewModelImpl : PropertyChangedBase, AttackConfigurationWidgetViewModel, IHandle<ConfigureAttackEvent>
    {
        #region Private Fields

        #endregion

        #region Public Properties

        public IEventAggregator EventAggregator { get; set; }

        private AnimatedCharacter attacker;
        public AnimatedCharacter Attacker
        {
            get
            {
                return attacker;
            }
            set
            {
                attacker = value;
                NotifyOfPropertyChange(() => Attacker);
                NotifyOfPropertyChange(() => IsConfiguringAreaEffect);
            }
        }

        public bool IsConfiguringAreaEffect
        {
            get
            {
                return this.Attacker.ActiveAttack is AreaEffectAttack;
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
                NotifyOfPropertyChange(() => IsConfiguringAreaEffect);
            }
        }

        private AttackInstructions currentAttackInstructions;
        public AttackInstructions CurrentAttackInstructions
        {
            get
            {
                return currentAttackInstructions;
            }
            set
            {
                currentAttackInstructions = value;
                NotifyOfPropertyChange(() => CurrentAttackInstructions);
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

        public void ChangeCenterTarget(object state)
        {
            AttackInstructions ins = state as AttackInstructions;
            if (ins.IsCenterOfAreaEffectAttack)
            {
                foreach (AttackInstructions ai in this.AttackInstructionsCollection.Where(instr => instr != ins))
                {
                    ai.IsCenterOfAreaEffectAttack = false;
                }
            }
            
        }

        public void UpdateAttackImpacts(AttackInstructions instructions, string impactName, bool isEffectEnabled)
        {
            if (isEffectEnabled)
                instructions.AddImpact(impactName);
            else
                instructions.RemoveImpact(impactName);
        }

        public void Handle(ConfigureAttackEvent message)
        {
            this.Attacker = message.Attacker;
            this.CurrentAttackInstructions = message.AttackInstructions;
            if (CurrentAttackInstructions is AreaAttackInstructions)
            {
                AreaAttackInstructions areaAttackInstructions = CurrentAttackInstructions as AreaAttackInstructions;
                this.AttackInstructionsCollection = areaAttackInstructions.IndividualTargetInstructions;
                
            }
            else
                this.AttackInstructionsCollection = new ObservableCollection<AttackInstructions> { CurrentAttackInstructions };
        }

        public void LaunchAttack()
        {
            //// Change mouse pointer to back to bulls eye
            //Cursor cursor = new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("HeroUI.Attack.Bullseye.cur"));
            //Mouse.OverrideCursor = cursor;

            this.EventAggregator.Publish(new CloseAttackConfigurationWidgetEvent(), action => Application.Current.Dispatcher.Invoke(action));
            this.EventAggregator.Publish(new LaunchAttackEvent(this.Attacker, this.CurrentAttackInstructions), action => Application.Current.Dispatcher.Invoke(action));
        }
       
        public void CancelAttack()
        {
            if (IsConfiguringAreaEffect)
                (this.Attacker.ActiveAttack as AreaEffectAttack).Cancel(this.CurrentAttackInstructions as AreaAttackInstructions);
            else
                this.Attacker.ActiveAttack.Cancel(this.CurrentAttackInstructions);
            this.EventAggregator.Publish(new CancelAttackEvent(this.Attacker, this.CurrentAttackInstructions), action => Application.Current.Dispatcher.Invoke(action));
        }

        #endregion
    }
}
