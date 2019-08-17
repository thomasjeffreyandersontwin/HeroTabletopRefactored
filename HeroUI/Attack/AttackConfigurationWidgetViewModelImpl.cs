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

        private string attackSummaryText;
        public string AttackSummaryText
        {
            get
            {
                return attackSummaryText;
            }
            set
            {
                attackSummaryText = value;
                NotifyOfPropertyChange(() => AttackSummaryText);
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
            this.SetAttackSummaryText();
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
            this.SetAttackSummaryText();
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
            this.SetAttackSummaryText();
        }

        public void UpdateAttackImpacts(DefenderAttackInstructions defenderInstructions, string impactName, bool isEffectEnabled)
        {
            if(this.CurrentAttackInstructions is MultiAttackInstructions)
            {
                MultiAttackInstructions multiInstructions = this.CurrentAttackInstructions as MultiAttackInstructions;
                if (multiInstructions.Defenders.Contains(defenderInstructions.Defender))
                {
                    foreach (var ins in multiInstructions.IndividualTargetInstructions.Where(i => i.Defender == defenderInstructions.Defender))
                        ChangeImpact(ins, impactName, isEffectEnabled);
                }
                else if(multiInstructions.Obstacles.Any(o => o.ObstacleTarget == defenderInstructions.Defender))
                {
                    var obstacle = multiInstructions.Obstacles.First(o => o.ObstacleTarget == defenderInstructions.Defender);
                    ChangeImpact(obstacle.ObstacleInstructions, impactName, isEffectEnabled);
                }

            }
            else
            {
                var instructions = defenderInstructions.AttackerHitInfo.Where(ahi => ahi.AttackInstructionsForAttacker.Defender == defenderInstructions.Defender)?.Select(ahi => ahi.AttackInstructionsForAttacker)?.FirstOrDefault();
                ChangeImpact(instructions, impactName, isEffectEnabled);
            }
            this.SetAttackSummaryText();
        }

        private void ChangeImpact(AttackInstructions instructions, string impactName, bool isEffectEnabled)
        {
            if (isEffectEnabled)
                instructions.AddImpact(impactName);
            else
                instructions.RemoveImpact(impactName);
            this.SetAttackSummaryText();
        }

        public void Handle(ConfigureAttackEvent message)
        {
            this.ConfiguringAttack = message.AttackToConfigure;
            this.Attackers = message.Attackers;
            this.CurrentAttackInstructions = message.AttackInstructions;
            this.SetDefenderInstructions();
            this.SetAttackSummaryText();
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

            foreach (var obst in CurrentAttackInstructions.Obstacles.Where(o => o.ObstacleTarget is AnimatedCharacter))
            {
                DefenderAttackInstructions defenderAttackInstructions = new DefenderAttackInstructions();
                defenderAttackInstructions.Defender = obst.ObstacleTarget as AnimatedCharacter;
                defenderAttackInstructions.AttackerHitInfo = new ObservableCollection<AttackerHitInfo>();
                defenderAttackInstructions.AttackerHitInfo.Add(new AttackerHitInfo { AttackInstructionsForAttacker = obst.ObstacleInstructions });

                this.DefenderAttackInstructions.Add(defenderAttackInstructions);
            }
            foreach(var dai in this.DefenderAttackInstructions)
                dai.PropertyChanged += DefenderAttackInstructions_PropertyChanged;
        }

        private void DefenderAttackInstructions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SetAttackSummaryText();
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
                    var obstacle = this.CurrentAttackInstructions.Obstacles.FirstOrDefault(o => o.Defender == defenderInstructions.Defender && o.ObstacleType == ObstacleType.Knockback);
                    AttackerHitInfo instructionToSetKnockback = null;
                    if (obstacle != null)
                    {
                        instructionToSetKnockback = defenderInstructions.AttackerHitInfo.LastOrDefault(hi => hi.AttackInstructionsForAttacker.AttackHit && hi.AttackInstructionsForAttacker.Attacker == obstacle.Attacker);
                    }
                    if(instructionToSetKnockback == null)
                        instructionToSetKnockback = defenderInstructions.AttackerHitInfo.LastOrDefault(hi => hi.AttackInstructionsForAttacker.AttackHit);
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

        #region Attack Summary

        private void SetAttackSummaryText()
        {
            this.AttackSummaryText = "";
            List<AnimatedCharacter> hitCharacters = this.DefenderAttackInstructions.Where(d => d.DefenderHitByAttack).Select(d => d.Defender).ToList();
            List<AnimatedCharacter> missCharacters = this.DefenderAttackInstructions.Where(d => !d.DefenderHitByAttack).Select(d => d.Defender).ToList();
            StringBuilder summary = new StringBuilder("The attack hit ");
            bool hitCharactersFound = hitCharacters.Count > 0;
            bool missCharactersFound = missCharacters.Count > 0;
            Dictionary<AnimatedCharacter, bool> summarizedCharacters = new Dictionary<AnimatedCharacter, bool>();
            foreach (AnimatedCharacter c in hitCharacters)
            {
                if (!summarizedCharacters.ContainsKey(c))
                    summarizedCharacters.Add(c, false);
            }
            if (hitCharactersFound)
            {
                for (int i = 0; i < hitCharacters.Count; i++)
                {
                    if (i == 0)
                        summary.Append(hitCharacters[0].Name);
                    else if (i == hitCharacters.Count - 1 && !missCharactersFound)
                        summary.AppendFormat(" and {0}", hitCharacters[i].Name);
                    else
                        summary.AppendFormat(", {0}", hitCharacters[i].Name);
                }
            }
            if (missCharactersFound)
            {
                if (!hitCharactersFound)
                    summary = new StringBuilder("The attack missed ");
                else
                    summary.Append(" and missed ");
                for (int i = 0; i < missCharacters.Count; i++)
                {
                    if (i == 0)
                        summary.Append(missCharacters[0].Name);
                    else if (i == missCharacters.Count - 1)
                        summary.AppendFormat(" and {0}", missCharacters[i].Name);
                    else
                        summary.AppendFormat(", {0}", missCharacters[i].Name);
                }
            }

            foreach (var character in hitCharacters)
            {
                if (summarizedCharacters[character])
                    continue;
                var defenderAttackInstruction = this.DefenderAttackInstructions.First(d => d.Defender == character);
                if (defenderAttackInstruction.DefenderKnockbackDistance > 0)
                {
                    summary.AppendLine();
                    summary.AppendFormat("{0} is knocked back {1} hexes", character.Name, defenderAttackInstruction.DefenderKnockbackDistance);
                }
                if (this.CurrentAttackInstructions.Obstacles.Any(o => o.Defender == character))
                {
                    foreach (var obstacle in this.CurrentAttackInstructions.Obstacles.Where(o => o.Defender == character && o.ObstacleTarget is AnimatedCharacter))
                    {
                        var obsCharacter = obstacle.ObstacleTarget as AnimatedCharacter;
                        summary.AppendLine();
                        if (obstacle.ObstacleType == ObstacleType.Knockback)
                            summary.AppendFormat("{0} collided with {1}", character.Name, obsCharacter.Name);
                        else
                            summary.AppendFormat("Attack is intercepted by {0}", obsCharacter.Name);
                        string obsEffect = GetEffectsStringWithBody(obsCharacter);
                        summary.AppendLine();
                        if (obsCharacter.Body != null && obsEffect != "")
                        {
                            summary.AppendFormat("{0} now has {1} BODY and is {2}", obsCharacter.Name, obsCharacter.Body, obsEffect);
                        }
                        else if (obsCharacter.Body != null)
                        {
                            summary.AppendFormat("{0} now has {1} BODY", obsCharacter.Name, obsCharacter.Body);
                        }
                        else
                        {
                            summary.AppendFormat("{0} is {1}", obsCharacter.Name, obsEffect);
                        }
                        summarizedCharacters[obsCharacter] = true;
                    }
                }
                //else
                {
                    string effects = GetEffectsStringWithBody(character);
                    if (character.Stun != null || character.Body != null)
                    {
                        summary.AppendLine();
                    }
                    if (character.Stun != null && character.Body != null)
                    {
                        summary.AppendFormat("{0} has {1} Stun and {2} BODY left", character.Name, character.Stun, character.Body);
                    }
                    else if (character.Stun != null)
                    {
                        summary.AppendFormat("{0} has {1} Stun left", character.Name, character.Stun);
                    }
                    else if (character.Body != null)
                    {
                        summary.AppendFormat("{0} has {1} BODY left", character.Name, character.Body);
                    }

                    
                    if (!string.IsNullOrEmpty(effects))
                    {
                        if (character.Stun == null && character.Body == null)
                        {
                            summary.AppendLine();
                            summary.AppendFormat("{0} is {1}", character.Name, effects);
                        }
                        else
                        {
                            summary.AppendFormat(" and is {0}", effects);
                        }
                    }

                }
                summarizedCharacters[character] = true;
            }
            this.AttackSummaryText = summary.ToString();
        }

        private string GetEffectsStringWithBody(AnimatedCharacter character)
        {
            Random rnd = new Random();
            List<string> effectsStr = new List<string>();
            string efstr = "";
            var instructions = this.DefenderAttackInstructions.First(d => d.Defender == character);
            if (instructions.DefenderHitByAttack)
            {
                character.Body = rnd.Next(81, 99);
            }
            else
            {
                character.Body = 100;
            }
            if (instructions.DefenderStunned)
            {
                effectsStr.Add("Stunned");
                character.Body = rnd.Next(51, 80);
            }
            if (instructions.DefenderUnconscious)
            {
                effectsStr.Add("Unconscious");
                character.Body = rnd.Next(31, 51);
            }
            if (instructions.DefenderDying)
            {
                effectsStr.Add("Dying");
                character.Body = rnd.Next(1, 10);
            }
            if (instructions.DefenderDead)
            {
                effectsStr.Add("Dead");
                character.Body = 0;
            }
            if(instructions.DefenderKnockbackDistance > 0 && !(instructions.DefenderDying || instructions.DefenderDead))
            {
                character.Body = rnd.Next(11, 50);
            }

            if (effectsStr.Count > 0)
            {
                efstr = String.Join(", ", effectsStr);
                if (efstr.IndexOf(",") != efstr.LastIndexOf(","))
                {
                    efstr = efstr.Replace(efstr[efstr.LastIndexOf(", ")].ToString(), " and");
                }
                efstr += ".";
            }
            return efstr;
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
            if (Desktop.WindowManager.CurrentActiveWindow == ActiveWindow.ATTACK)
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
