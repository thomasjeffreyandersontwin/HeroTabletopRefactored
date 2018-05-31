using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
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

        public DesktopKeyEventHandler DesktopKeyEventHandler { get; set; }
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

        public AttackConfigurationWidgetViewModelImpl(DesktopKeyEventHandler desktopKeyEventHandler, IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.DesktopKeyEventHandler = desktopKeyEventHandler;

            this.EventAggregator.Subscribe(this);

            RegisterKeyEventHandlers();
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

        #region Desktop Key Handling

        private void RegisterKeyEventHandlers()
        {
            this.DesktopKeyEventHandler.AddKeyEventHandler(HandleDesktopKeyEvent);
        }
        public EventMethod HandleDesktopKeyEvent(System.Windows.Forms.Keys vkCode, System.Windows.Input.Key inputKey)
        {
            EventMethod method = null;
            if (DesktopFocusManager.CurrentActiveWindow == ActiveWindow.ATTACK)
            {
                if (inputKey == Key.Enter)
                {
                    method = LaunchAttack;
                }
                else if (inputKey == Key.Escape)
                {
                    method = CancelAttack;
                }
                else if (inputKey == Key.H || inputKey == Key.M || inputKey == Key.S || inputKey == Key.U
                    || inputKey == Key.Y || inputKey == Key.D || inputKey == Key.K || inputKey == Key.N || inputKey == Key.T
                    || (inputKey >= Key.D0 && inputKey <= Key.D9) || (inputKey >= Key.NumPad0 && inputKey <= Key.NumPad9))
                {
                    //foreach (var defender in this.DefendingCharacters)
                    {
                        if (inputKey == Key.H)
                        {
                            //if (!defender.AttackConfigurationMap[AttackConfigKey].Item2.HasMultipleAttackers)
                            //    defender.AttackConfigurationMap[AttackConfigKey].Item2.IsHit = true;
                            //else
                            //{
                            //    foreach (var ar in defender.AttackConfigurationMap[AttackConfigKey].Item2.AttackResults)
                            //    {
                            //        ar.IsHit = true;
                            //    }
                            //}
                            this.CurrentAttackInstructions.AttackHit = true;
                        }
                        else if (inputKey == Key.M)
                        {
                            //if (!defender.AttackConfigurationMap[AttackConfigKey].Item2.HasMultipleAttackers)
                            //    defender.AttackConfigurationMap[AttackConfigKey].Item2.IsHit = false;
                            //else
                            //{
                            //    foreach (var ar in defender.AttackConfigurationMap[AttackConfigKey].Item2.AttackResults)
                            //    {
                            //        ar.IsHit = false;
                            //    }
                            //}
                            this.CurrentAttackInstructions.AttackHit = false;
                        }
                        else if (inputKey == Key.S)
                        {
                            //defender.AttackConfigurationMap[AttackConfigKey].Item2.IsStunned = true;
                            this.UpdateAttackImpacts(this.CurrentAttackInstructions, DefaultAbilities.STUNNED, true);
                        }
                        else if (inputKey == Key.U)
                        {
                            //defender.AttackConfigurationMap[AttackConfigKey].Item2.IsUnconcious = true;
                            this.UpdateAttackImpacts(this.CurrentAttackInstructions, DefaultAbilities.UNCONSCIOUS, true);
                        }
                        else if (inputKey == Key.Y)
                        {
                            //defender.AttackConfigurationMap[AttackConfigKey].Item2.IsDying = true;
                            this.UpdateAttackImpacts(this.CurrentAttackInstructions, DefaultAbilities.DYING, true);
                        }
                        else if (inputKey == Key.D)
                        {
                            //defender.AttackConfigurationMap[AttackConfigKey].Item2.IsDead = true;
                            this.UpdateAttackImpacts(this.CurrentAttackInstructions, DefaultAbilities.DEAD, true);
                        }
                        else if (inputKey == Key.T)
                        {
                            //defender.AttackConfigurationMap[AttackConfigKey].Item2.MoveAttackerToTarget = true;
                        }
                        else if ((inputKey >= Key.D0 && inputKey <= Key.D9) || (inputKey >= Key.NumPad0 && inputKey <= Key.NumPad9))
                        {
                            var intkey = (inputKey >= Key.D0 && inputKey <= Key.D9) ? inputKey - Key.D0 : inputKey - Key.NumPad0;
                            //if (defender.AttackConfigurationMap[AttackConfigKey].Item2.KnockBackDistance > 0)
                            //{
                            //    string current = defender.AttackConfigurationMap[AttackConfigKey].Item2.KnockBackDistance.ToString();
                            //    current += intkey.ToString();
                            //    defender.AttackConfigurationMap[AttackConfigKey].Item2.KnockBackDistance = Convert.ToInt32(current);
                            //}
                            //else
                            //{
                            //    defender.AttackConfigurationMap[AttackConfigKey].Item2.KnockBackDistance = intkey;
                            //}
                            if(this.CurrentAttackInstructions.KnockbackDistance > 0)
                            {
                                string current = this.CurrentAttackInstructions.KnockbackDistance.ToString();
                                current += intkey.ToString();
                                this.CurrentAttackInstructions.KnockbackDistance = Convert.ToInt32(current);
                            }
                            else
                            {
                                this.CurrentAttackInstructions.KnockbackDistance = intkey;
                            }
                        }

                        //defender.RefreshAttackConfigurationParameters();
                    }
                }
            }
            return method;
        }


        #endregion
    }
}
