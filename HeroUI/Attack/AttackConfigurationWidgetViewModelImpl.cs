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

        private AnimatedAttack configuringAttack;
        public AnimatedAttack ConfiguringAttack
        {
            get
            {
                return configuringAttack;
            }
            set
            {
                configuringAttack = value;
                NotifyOfPropertyChange(() => ConfiguringAttack);
            }
        }

        private List<AnimatedCharacter> attackers;
        public List<AnimatedCharacter> Attackers
        {
            get
            {
                return attackers;
            }
            set
            {
                attackers = value;
                NotifyOfPropertyChange(() => Attackers);
                NotifyOfPropertyChange(() => IsConfiguringAreaEffect);
            }
        }

        public bool IsConfiguringAreaEffect
        {
            get
            {
                return this.ConfiguringAttack is AreaEffectAttack;
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

        private ObservableCollection<DefenderAttackInstructions> defenderAttackInstructions;
        public ObservableCollection<DefenderAttackInstructions> DefenderAttackInstructions
        {
            get
            {
                return defenderAttackInstructions;
            }
            set
            {
                defenderAttackInstructions = value;
                NotifyOfPropertyChange(() => DefenderAttackInstructions);
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
            DefenderAttackInstructions dins = state as DefenderAttackInstructions;
            if (dins.IsAttackCenter)
            {
                AttackInstructions ins = this.AttackInstructionsCollection.FirstOrDefault(ai => ai.Defender == dins.Defender);
                ins.IsCenterOfAreaEffectAttack = true;
                foreach (AttackInstructions ai in this.AttackInstructionsCollection.Where(instr => instr != ins))
                {
                    ai.IsCenterOfAreaEffectAttack = false;
                }
            }
        }

        public void ChangeAttackHit(AttackerHitInfo attackHitInfo)
        {
            DefenderAttackInstructions defenderInstructions = this.DefenderAttackInstructions.FirstOrDefault(d => d.Defender == attackHitInfo.AttackInstructionsForAttacker.Defender);
            if(attackHitInfo.AttackInstructionsForAttacker.AttackHit)
                defenderInstructions.DefenderHitByAttack = attackHitInfo.AttackInstructionsForAttacker.AttackHit;
            else
            {
                if(!defenderInstructions.AttackerHitInfo.Any(ah => ah != attackHitInfo && ah.AttackInstructionsForAttacker.AttackHit))
                    defenderInstructions.DefenderHitByAttack = attackHitInfo.AttackInstructionsForAttacker.AttackHit;
            }
        }

        private void UpdateAttackImpactsForAllInstructions(string impactName, bool isEffectEnabled)
        {
            foreach (var defenderIns in this.DefenderAttackInstructions)
            {
                UpdateAttackImpacts(defenderIns, impactName, isEffectEnabled);
                switch (impactName)
                {
                    case DefaultAbilities.STUNNED:
                        defenderIns.DefenderStunned = isEffectEnabled;
                        break;
                    case DefaultAbilities.UNCONSCIOUS:
                        defenderIns.DefenderUnconscious = isEffectEnabled;
                        break;
                    case DefaultAbilities.DYING:
                        defenderIns.DefenderDying = isEffectEnabled;
                        break;
                    case DefaultAbilities.DEAD:
                        defenderIns.DefenderDead = isEffectEnabled;
                        break;
                }
            }
        }

        private void UpdateAttackHitForAllInstructions(bool hit)
        {
            foreach (var defenderIns in this.DefenderAttackInstructions)
            {
                defenderIns.DefenderHitByAttack = hit;
                foreach(var hitInfo in defenderIns.AttackerHitInfo)
                {
                    hitInfo.AttackInstructionsForAttacker.AttackHit = hit;
                }
            }
        }

        public void UpdateAttackImpacts(DefenderAttackInstructions defenderInstructions, string impactName, bool isEffectEnabled)
        {
            if(this.CurrentAttackInstructions is MultiAttackInstructions)
            {
                MultiAttackInstructions multiInstructions = this.CurrentAttackInstructions as MultiAttackInstructions;
                foreach (var ins in multiInstructions.IndividualTargetInstructions.Where(i => i.Defender == defenderInstructions.Defender))
                    ChangeImpact(ins, impactName, isEffectEnabled);
            }
            else
            {
                ChangeImpact(this.CurrentAttackInstructions, impactName, isEffectEnabled);
            }
        }

        private void ChangeImpact(AttackInstructions instructions, string impactName, bool isEffectEnabled)
        {
            if (isEffectEnabled)
                instructions.AddImpact(impactName);
            else
                instructions.RemoveImpact(impactName);
        }

        public void Handle(ConfigureAttackEvent message)
        {
            this.ConfiguringAttack = message.AttackToConfigure;
            this.Attackers = message.Attackers;
            this.CurrentAttackInstructions = message.AttackInstructions;
            this.SetDefenderInstructions();
        }

        private void SetDefenderInstructions()
        {
            this.DefenderAttackInstructions = new ObservableCollection<DefenderAttackInstructions>();

            if (CurrentAttackInstructions is MultiAttackInstructions)
            {
                MultiAttackInstructions multiAttackInstructions = CurrentAttackInstructions as MultiAttackInstructions;

                this.AttackInstructionsCollection = multiAttackInstructions.IndividualTargetInstructions;


                foreach (var defender in multiAttackInstructions.Defenders)
                {
                    DefenderAttackInstructions defenderAttackInstructions = new DefenderAttackInstructions();
                    defenderAttackInstructions.Defender = defender;
                    defenderAttackInstructions.AttackerHitInfo = new ObservableCollection<AttackerHitInfo>();
                    if (this.Attackers.Count > 1)
                        defenderAttackInstructions.HasMultipleAttackers = true;
                    foreach (var attacker in this.Attackers)
                    {
                        AttackInstructions instructionForAttacker = multiAttackInstructions.IndividualTargetInstructions.FirstOrDefault(i => i.Defender == defender && i.Attacker == attacker);
                        defenderAttackInstructions.AttackerHitInfo.Add(new AttackerHitInfo { Attacker = attacker, AttackInstructionsForAttacker = instructionForAttacker });
                    }

                    this.DefenderAttackInstructions.Add(defenderAttackInstructions);
                }

            }
            else
            {
                this.AttackInstructionsCollection = new ObservableCollection<AttackInstructions> { CurrentAttackInstructions };
                DefenderAttackInstructions defenderAttackInstructions = new DefenderAttackInstructions();
                defenderAttackInstructions.Defender = CurrentAttackInstructions.Defender;
                defenderAttackInstructions.AttackerHitInfo = new ObservableCollection<AttackerHitInfo>();
                defenderAttackInstructions.AttackerHitInfo.Add(new AttackerHitInfo { AttackInstructionsForAttacker = CurrentAttackInstructions });

                this.DefenderAttackInstructions.Add(defenderAttackInstructions);
            }
        }

        public void LaunchAttack()
        {
            //// Change mouse pointer to back to bulls eye
            //Cursor cursor = new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("HeroUI.Attack.Bullseye.cur"));
            //Mouse.OverrideCursor = cursor;
            SetAttackParameters();

            this.EventAggregator.Publish(new CloseAttackConfigurationWidgetEvent(), action => Application.Current.Dispatcher.Invoke(action));
            this.EventAggregator.Publish(new LaunchAttackEvent(this.ConfiguringAttack, this.Attackers, this.CurrentAttackInstructions), action => Application.Current.Dispatcher.Invoke(action));
        }
       
        public void CancelAttack()
        {
            this.ConfiguringAttack.Cancel(this.CurrentAttackInstructions);
            this.EventAggregator.Publish(new CancelAttackEvent(this.ConfiguringAttack, this.Attackers, this.CurrentAttackInstructions), action => Application.Current.Dispatcher.Invoke(action));
        }

        private void SetAttackParameters()
        {
            foreach(var defenderInstructions in this.DefenderAttackInstructions)
            {
                if (defenderInstructions.DefenderHitByAttack)
                {
                    if (!defenderInstructions.HasMultipleAttackers)
                    {
                        this.CurrentAttackInstructions.AttackHit = true;
                        defenderInstructions.AttackerHitInfo[0].AttackInstructionsForAttacker.AttackHit = true;
                    }
                }
                
                if (defenderInstructions.DefenderKnockbackDistance > 0)
                {
                    var instructionToSetKnockback = defenderInstructions.AttackerHitInfo.LastOrDefault(hi => hi.AttackInstructionsForAttacker.AttackHit);
                    if (instructionToSetKnockback != null)
                    {
                        instructionToSetKnockback.AttackInstructionsForAttacker.KnockbackDistance = defenderInstructions.DefenderKnockbackDistance;
                        ChangeImpact(instructionToSetKnockback.AttackInstructionsForAttacker, DefaultAbilities.KNOCKEDBACK, true);
                    }
                    else
                    {
                        this.CurrentAttackInstructions.KnockbackDistance = defenderInstructions.DefenderKnockbackDistance;
                        ChangeImpact(this.CurrentAttackInstructions, DefaultAbilities.KNOCKEDBACK, true);
                    }
                }
            }
            if (this.CurrentAttackInstructions is GangAreaAttackInstructions)
            {
                GangAreaAttackInstructions gangAreaInstructions = this.CurrentAttackInstructions as GangAreaAttackInstructions;
                foreach (var attacker in this.Attackers)
                {
                    AreaAttackInstructions areaInstructions = gangAreaInstructions.AttackInstructionsMap[attacker];
                    areaInstructions.AttackHit = this.DefenderAttackInstructions.Any(d => d.AttackerHitInfo.Any(h => h.Attacker == attacker && d.DefenderHitByAttack));
                }
            }
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
                    //foreach (var defenderIns in this.DefenderAttackInstructions)
                    {
                        if (inputKey == Key.H)
                        {
                            UpdateAttackHitForAllInstructions(true);
                        }
                        else if (inputKey == Key.M)
                        {
                            UpdateAttackHitForAllInstructions(false);
                        }
                        else if (inputKey == Key.S)
                        {
                            this.UpdateAttackImpactsForAllInstructions(DefaultAbilities.STUNNED, true);
                        }
                        else if (inputKey == Key.U)
                        {
                            this.UpdateAttackImpactsForAllInstructions(DefaultAbilities.UNCONSCIOUS, true);
                        }
                        else if (inputKey == Key.Y)
                        {
                            this.UpdateAttackImpactsForAllInstructions(DefaultAbilities.DYING, true);
                        }
                        else if (inputKey == Key.D)
                        {
                            this.UpdateAttackImpactsForAllInstructions(DefaultAbilities.DEAD, true);
                        }
                        else if (inputKey == Key.T)
                        {
                            foreach (var defenderIns in this.DefenderAttackInstructions)
                                defenderIns.MoveAttackersToDefender = true;
                        }
                        else if ((inputKey >= Key.D0 && inputKey <= Key.D9) || (inputKey >= Key.NumPad0 && inputKey <= Key.NumPad9))
                        {
                            var intkey = (inputKey >= Key.D0 && inputKey <= Key.D9) ? inputKey - Key.D0 : inputKey - Key.NumPad0;
                            foreach (var defenderIns in this.DefenderAttackInstructions)
                            {
                                if (defenderIns.DefenderKnockbackDistance > 0)
                                {
                                    string current = defenderIns.DefenderKnockbackDistance.ToString();
                                    current += intkey.ToString();
                                    defenderIns.DefenderKnockbackDistance = Convert.ToInt32(current);
                                }
                                else
                                {
                                    defenderIns.DefenderKnockbackDistance = intkey;
                                }
                            }
                        }
                    }
                }
            }
            return method;
        }


        #endregion
    }
}
