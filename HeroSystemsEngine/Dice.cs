using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroSystemEngine.Manuevers;
using HeroSystemEngine.Character;
namespace HeroSystemEngine.Dice 
{
    class DicePoolRepository
    {
        public static DicePoolRepository Instance = new DicePoolRepository();

        public DamageDicePool LoadDicePool(DamageType damageType, int damageDiceNumber)
        {
            if (damageType == DamageType.Normal)
            {
                return new NormalDamageDicePool(damageDiceNumber);
            }
            else
            {
                return new KillingDamageDicePool(damageDiceNumber);
            }
        }
    }
    public enum RandomnessState { Rand = 1, average = 2, max = 3 }
    public class Dice
    {
        public static RandomnessState RandomnessState = RandomnessState.Rand;
        public int Result = 0;
        public static int LastResult = 0;
        Random rnd = new Random();

        public virtual int Roll()
        {
            if (RandomnessState == RandomnessState.Rand)
            {
                Result = rnd.Next(1, 6);
            }
            else
            {
                if (LastResult == 3)
                {
                    Result = 4;
                }
                else
                {
                    Result = 3;
                }

                LastResult = Result;
            }
            return Result;
        }
    }
    public class DicePool
    {
        public Dice[] Pool = null;

        public DicePool(int num)
        {
            Pool = new Dice[num];
            for (int i = 0; i < Pool.Length; i++)
            {
                Pool[i] = new Dice();
            }
        }

        public virtual int Roll()
        {
            int result = 0;
            for (int i = 0; i < Pool.Length; i++)
            {
                Dice dice = Pool[i];
                result += dice.Roll();
            }
            Dice.LastResult = 0;
            return result;
        }
    }


    public class NormalDamageDice : Dice
    {
        public int Stun { get { return Result; } set { Result = value; } }
        public int Body {
            get {
                if (Stun > 4)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
    public class KillingDamageDice : Dice
    {
        public int Body { get { return Result; } set { Result = value; } }


    }
    public interface DamageDicePool
    {
        Damage DamageResult { get; }
        int Roll();

    }
    public class NormalDamageDicePool : DicePool, DamageDicePool
    {

        public NormalDamageDicePool(int num) : base(num)
        {
            for (int i = 0; i < Pool.Length; i++)
            {
                Pool[i] = new NormalDamageDice();
            }
        }

        public Damage DamageResult
        {
            get
            {
                Damage theDamage = new Damage();
                for (int i = 0; i < Pool.Length; i++)
                {
                    theDamage.Stun += ((NormalDamageDice)Pool[i]).Stun;
                    theDamage.Body += ((NormalDamageDice)Pool[i]).Body;
                }
                theDamage.Type = DamageType.Normal;
                return theDamage;
            }
        }
    }
    public class KillingDamageDicePool : DicePool, DamageDicePool
    {
        public int Multiplier = 0;

        public KillingDamageDicePool(int num) : base(num)
        {
            for (int i = 0; i < Pool.Length; i++)
            {
                Pool[i] = new KillingDamageDice();
            }
        }

        public override int Roll()
        {
            int result = base.Roll();
            Multiplier = new Dice().Roll() - 1;
            if (Multiplier < 1) { Multiplier = 1; }
            return result;
        }

        public Damage DamageResult
        {
            get
            {
                Damage theDamage = new Damage();
                for (int i = 0; i < Pool.Length; i++)
                {
                    theDamage.Body += ((NormalDamageDice)Pool[i]).Body;
                }
                theDamage.Stun = theDamage.Body * Multiplier;
                theDamage.Type = DamageType.Killing;
                return theDamage;
            }
        }
    }



}


