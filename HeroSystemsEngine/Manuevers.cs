using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroSystemEngine.Dice;
using HeroSystemEngine.Character;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Attack;

namespace HeroSystemEngine.Manuevers
{
   
    public enum HitResultType { Stunned = 1, Unconsious = 2, Dead = 3, Dying = 4};
    public enum KnockbackResultType { KnockBacked = 1 , KnockedDown = 2, None = 3, BreakFall = 4};

    public enum PhaseLength { Zero, Half, Full };

    public enum ManueverType { Strike = 1, Kick = 2 };
    
    
    public abstract class Manuever
    {
        public HeroSystemCharacter Character;
        public PhaseLength PhaseActionTakes;
        public Boolean IsAbortable;
        public String Name;
        public ManueverType Type;

        public Manuever(ManueverType type, HeroSystemCharacter character)
        {
            Character = character;
            Type = type;
            Character.AddManuever(this);
            
        }

        public Boolean CanPerform
        {
            get
            {
                return canPerform();
            }
        }
        public abstract Boolean canPerform();

        public void Perform()
        {
            if (canPerform() == true)
            {
                performManuever();
            };
        }

        public abstract void performManuever();
        public abstract void CanAbortDuringAttack(Attack attack);
    }

    public enum DefenseType { PD = 1, ED = 2, RPD = 3, RED = 4 };
    public enum DamageType { Normal = 1, Killing = 2, Ego=3, NND = 4}

    public enum HitResult { Hit = 1, Miss = 2}
    public class CombatManueverResult
    {
        public HitResult HitResult;
        public Damage DamageResult = new Damage();
        public Dictionary<CharacterStateType, HeroCharacterState> Results = new Dictionary<CharacterStateType, HeroCharacterState>();

        public KnockbackResult KnockbackResults;
    }
    public class KnockbackResult
    {
        public KnockbackResultType Result = KnockbackResultType.None;
        public int Knockback;
        public Damage Damage;
    }

    public class Damage
    {
        public int Body = 0;
        public int Stun = 0;
        public DefenseType WorksAgainstDefense = DefenseType.PD;
        public DamageType Type = DamageType.Normal;
        
    }
    public class CombatManuever : Manuever
    {
        public HeroSystemCharacter Defender;
        public DamageType DamageType;
        public int DamageDiceNumber;
        public DefenseType WorksAgainstDefense;
        public CombatManueverResult Result = new CombatManueverResult();
        public string ManueverName
        {
            get
            {
                return Type.ToString();
            }
        }
        public HeroSystemCharacter Attacker
        {
            get { return this.Character; }
        }

        public CombatManuever(ManueverType type, HeroSystemCharacter attacker, DamageType damageType, int damageDiceNumber, DefenseType worksAgainstDefense):base(type, attacker)
        {
            DamageDiceNumber = damageDiceNumber;
            DamageType = damageType;
            WorksAgainstDefense = worksAgainstDefense;
        }
        
        public CombatManueverResult Attack(HeroSystemCharacter defender)
        {
            this.Defender = defender;
            this.Perform();
            return Result;
        }

        public override void performManuever()
        {
            Result = new CombatManueverResult();
            if (AttackIsSuccessful(new DicePool(3).Roll()))
            {
                Result.HitResult = HitResult.Hit;
                HitDefender();
            }
            else
            {
                Result.HitResult = HitResult.Miss;
            }
        }
        
        public bool AttackIsSuccessful(int roll)
        {
            return RequiredToHitRoll >= roll;
        }
        public int RequiredToHitRoll
        {
            get
            {
                return Attacker.OCV.CurrentValue - Defender.DCV.CurrentValue + 11;
            }

        }

        public virtual Damage RollDamage()
        {
            DamageDicePool pool = DicePoolRepository.Instance.LoadDicePool(DamageType, DamageDiceNumber);
            //to do: Damage roll and take get pushed down into an Attack class MAYBE
            pool.Roll();
            Damage damage = pool.DamageResult;
            damage.WorksAgainstDefense = this.WorksAgainstDefense;
            return damage;
        }

        public CombatManueverResult HitDefender()
        {
            Result.HitResult = HitResult.Hit;
            Damage damage = RollDamage();
            Dictionary<CharacterStateType, HeroCharacterState> statesIncurredFromDamage = Defender.TakeDamage(damage);

            Result.Results = statesIncurredFromDamage;
            Result.DamageResult = damage;
            AnimateAttackResults(damage);
            return Result;
        }

        private CombatManueverResult AnimateAttackResults(Damage damage)
        {
            if (Character.TableTopCharacter != null)
            {
                //package instructions so that tabletop can render the attack cycle and tell us if the target hit anything due to knockback
                AnimatedAttack attack = (AnimatedAttack)Character.TableTopCharacter.GetAbility(ManueverName);
                AttackInstructions instructions = Character.TableTopCharacterRepository.NewAttackInstructions();
                
                instructions.defender = HeroSystemCharacterRepository.GetInstance(null).Characters[Defender.Name].TableTopCharacter;
                if (Result.HitResult == HitResult.Hit)
                {
                    instructions.AttackHit = true;
                    instructions.Impacts = instructions.defender.StatesThatHaveNotBeenRendered;
                    KnockbackCollisionInfo collisionInfo = attack.PlayCompleteAttackCycle(instructions);

                    KnockbackResult knockbackResults = KnockBackCharacter(Defender, damage, collisionInfo);
                    Result.KnockbackResults = knockbackResults;
                }
                else
                {
                    instructions.AttackHit = false;
                    attack.PlayCompleteAttackCycle(instructions);
                }
                instructions.defender.MarkAllAnimatableStatesAsRendered();
            }
            //we dont want any other states resulting from KB (eg stunned) to animate later, so mark them as already rendered
            return Result;

        } 

        public KnockbackResult KnockBackCharacter(HeroSystemCharacter defender,  Damage attackDamage, KnockbackCollisionInfo collisionInfo= null)
        {
            int knockback = 0;
            if (attackDamage.Type == DamageType.Normal)
            {
                knockback = attackDamage.Body - new DicePool(2).Roll();
            }
            else
            {
                knockback = attackDamage.Body - new DicePool(3).Roll();
            }

            KnockbackResult results = new KnockbackResult();
            double damageMultiplier = 0;
            results.Knockback = knockback;
            if (results.Knockback > 0)
            {
                defender.AddState(CharacterStateType.Prone);
                if (collisionInfo != null)
                {
                    if (collisionInfo.Type == KnockbackCollisionType.Wall)
                    {
                        damageMultiplier = 1;
                    }
                    else {
                        if (collisionInfo.Type == KnockbackCollisionType.Floor)
                        {
                            damageMultiplier = .5;
                        }
                        else
                        {
                            if (collisionInfo.Type == KnockbackCollisionType.Air)
                            {
                                damageMultiplier = 0;
                            }
                        }
                    } 
                    
                  }
                else
                {
                    damageMultiplier = .5;
                }

                NormalDamageDicePool knockbackDice = new NormalDamageDicePool((int)Math.Round(results.Knockback * damageMultiplier));
                knockbackDice.Roll();
                Damage knockBackDamage = knockbackDice.DamageResult;
                defender.TakeDamage(knockBackDamage);
                results.Damage = knockBackDamage;
                results.Result = KnockbackResultType.KnockBacked;
            }
            else
            {
                results.Result = KnockbackResultType.None;
            }
            return results;

        }


        public override void CanAbortDuringAttack(Attack attack)
        { }
        public override Boolean canPerform()
        {
            return true;
        }
    }
    public class Strike : CombatManuever
    {
        public Strike(HeroSystemCharacter attacker) : base (ManueverType.Strike, attacker, DamageType.Normal,  attacker.STR.CurrentValue/5, DefenseType.PD)
        { }

        public override Damage RollDamage()
        {
            DamageDiceNumber = Attacker.STR.CurrentValue / 5;
            return base.RollDamage();
        }
    }

    
    
    public class Attack
    {


    }
}
