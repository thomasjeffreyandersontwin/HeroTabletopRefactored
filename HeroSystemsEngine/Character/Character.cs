 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroSystemEngine.Manuevers;
using HeroSystemEngine.Dice;

using HeroVirtualTabletop;
using HeroVirtualTabletop.Movement;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.AnimatedAbility;

namespace HeroSystemEngine.Character
{
    public class Charasteristic
    {
        public int _maxValue;
        public int MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
                //if (CurrentValue == 0 || MaxValue < CurrentValue)
                //{
                    CurrentValue = MaxValue;
                //}
            }
        }
        public int CurrentValue=0;
        public void Deduct(int amount)
        {
            CurrentValue -= amount;
        }
    }
    
    public enum CharacterStateType { Stunned = AnimatableCharacterStateType.Stunned, Unconsious = AnimatableCharacterStateType.Unconsious,
        Dead = AnimatableCharacterStateType.Dead, Dying = AnimatableCharacterStateType.Dying, KnockBacked = AnimatableCharacterStateType.KnockBacked, KnockedDown = AnimatableCharacterStateType.KnockedDown, Attacking = AnimatableCharacterStateType.Attacking, Prone =8 };
    public class HeroCharacterState
    {
        public HeroSystemCharacter Character;
        public CharacterStateType Type;

        public Dictionary<ManueverType, Manuever> AllowedManuevers
        {
            get
            {
                return Character.Manuevers;
            }
        }
        public HeroCharacterState(HeroSystemCharacter character, CharacterStateType stateType)
        {
            Type = stateType;
            Character = character;
        }

        public void RemoveFromCharacter()
        {
            Character.RemoveState(this.Type);
            Character = null;
        }
    }

    public class HeroSystemCharacterRepository
    {
        public HeroTableTopCharacterRepository TableTopCharacterRepository;
        public Dictionary<string, HeroSystemCharacter> Characters = new Dictionary<string, HeroSystemCharacter>();

        private static HeroSystemCharacterRepository _instance;

        public static HeroSystemCharacterRepository GetInstance(HeroTableTopCharacterRepository tableTopCharacterRepository) {
            if (_instance == null)
            {
                _instance = new HeroSystemCharacterRepository(tableTopCharacterRepository);
            }
            return _instance;
        }

        public void DeleteCharacter(HeroSystemCharacter character)
        {
            if (Characters.ContainsKey(character.Name))
            {
                Characters.Remove(character.Name);
            }
        }

        public void AddCharacter(HeroSystemCharacter character)
        {
            if (Characters.ContainsKey(character.Name))
            {
                Characters.Remove(character.Name);
            }
            Characters[character.Name] = character;

        }
            
        public HeroSystemCharacterRepository(HeroTableTopCharacterRepository tableTopCharacterRepository)
        {
            TableTopCharacterRepository = tableTopCharacterRepository;
            AddCharacter(LoadBaseCharacter());

        }

        public HeroSystemCharacter LoadBaseCharacter(string name)
        {
            HeroSystemCharacter character = LoadBaseCharacter();
            character.Name = name;
            return character;

        }
        public HeroSystemCharacter LoadBaseCharacter()
        {

            HeroSystemCharacter baseChar = new HeroSystemCharacter("Default Character", TableTopCharacterRepository);
            
            baseChar.STR.MaxValue = 10;
            baseChar.CON.MaxValue = 10;
            baseChar.DEX.MaxValue = 10;
            baseChar.BOD.MaxValue = 10;
            baseChar.PRE.MaxValue = 10;
            baseChar.INT.MaxValue = 10;
            baseChar.EGO.MaxValue = 10;
            baseChar.COM.MaxValue = 10;

            baseChar.PD.MaxValue = 2;
            baseChar.ED.MaxValue = 2;
            baseChar.SPD.MaxValue = 2;
            baseChar.STUN.MaxValue = 20;
            baseChar.END.MaxValue = 20;

            baseChar.DCV.MaxValue = 3;
            baseChar.OCV.MaxValue = 3;

            baseChar.RPD.MaxValue = 10;
            baseChar.RED.MaxValue = 10;

            CombatManuever strike = new Strike(baseChar);


            return baseChar;
        }
    }

    public class HeroSystemCharacter
    {
        public string Name="";
        public HeroTableTopCharacter _tableTopCharacter;
        public HeroTableTopCharacter TableTopCharacter
        {
            get
            {
                if (_tableTopCharacter == null)
                {
                    if (TableTopCharacterRepository != null)
                    {
                        _tableTopCharacter = TableTopCharacterRepository.ReturnCharacter(Name);
                    }
                }
                if (_tableTopCharacter == null)
                {
                    if (TableTopCharacterRepository != null)
                    {
                      _tableTopCharacter = TableTopCharacterRepository.NewCharacter();
                        if (_tableTopCharacter != null) {
                            _tableTopCharacter.Name = Name;
                        }
                    }
                }
                return _tableTopCharacter;
            }
            set
            {
                _tableTopCharacter = value;
            }
            
        }

        public HeroTableTopCharacterRepository TableTopCharacterRepository;

        public Charasteristic STR = new Charasteristic();
        public Charasteristic DEX = new Charasteristic();
        public Charasteristic CON = new Charasteristic();
        public Charasteristic BOD = new Charasteristic();
        public Charasteristic INT = new Charasteristic();
        public Charasteristic EGO = new Charasteristic();
        public Charasteristic PRE = new Charasteristic();
        public Charasteristic COM = new Charasteristic();
        public Charasteristic SPD = new Charasteristic();
        public Charasteristic PD = new Charasteristic();
        public Charasteristic ED = new Charasteristic();
        public Charasteristic REC = new Charasteristic();
        public Charasteristic END = new Charasteristic();
        public Charasteristic STUN = new Charasteristic();

        public Charasteristic RPD = new Charasteristic();
        public Charasteristic RED = new Charasteristic();
        public Charasteristic DCV = new Charasteristic();
        public Charasteristic OCV = new Charasteristic();
        public Charasteristic ECV = new Charasteristic();

        public HeroSystemCharacter() { }
        public HeroSystemCharacter(string name, HeroTableTopCharacterRepository tableTopCharacterRepository)
        {
            TableTopCharacterRepository = tableTopCharacterRepository;
            Name = name;

        }
        public Dictionary<ManueverType, Manuever> Manuevers = new Dictionary<ManueverType, Manuever>();
        public void AddManuever(Manuever manuever)
        {
            RemoveManuever(manuever);
            Manuevers.Add(manuever.Type, manuever);
            if (TableTopCharacter != null)
            {
                AnimatedAbility ability = TableTopCharacter.Abilities[manuever.Name];

                if (ability == null)
                {
                    TableTopCharacterRepository.NewAbility(TableTopCharacter);
                }
            }

        }
        public void RemoveManuever(Manuever manuever)
        {
            if (Manuevers.ContainsKey(manuever.Type) == true)
            {
                Manuevers.Remove(manuever.Type);
                AnimatedAbility ability = TableTopCharacter.Abilities[manuever.Name];
                if (ability != null)
                {
                    TableTopCharacter.RemoveAnimatedAbility(ability);
                }
            }
        }

        public Dictionary<CharacterStateType, HeroCharacterState> State = new Dictionary<CharacterStateType, HeroCharacterState>();
        public HeroCharacterState AddState(CharacterStateType stateKey, bool renderImmediately = true)
        {
            HeroCharacterState state;
            if (State.ContainsKey(stateKey) == false)
            {
                state = new HeroCharacterState(this, stateKey);
                State.Add(stateKey, state);
                if (TableTopCharacter != null)
                {
                    TableTopCharacter.AddState((AnimatableCharacterStateType)stateKey, renderImmediately);
                }
            }
            else
            {
                state = State[stateKey];
            }
            return state; 
        }
        public void RemoveState(CharacterStateType stateKey)
        {
            if (State.ContainsKey(stateKey) == true)
            {
                State.Remove(stateKey);
                if (TableTopCharacter != null)
                {
                    TableTopCharacter.RemoveState((AnimatableCharacterStateType)stateKey);
                }

            }
        }

        public CombatManueverResult Attack(ManueverType manueverType, HeroSystemCharacter defender)
        {
            CombatManuever attack = (CombatManuever)Manuevers[manueverType];
            CombatManueverResult result= attack.Attack(defender);
            return result;

        }

        public Dictionary<CharacterStateType, HeroCharacterState> TakeDamage(Damage damage)
        {
            //to do: put state logic into body and stun charasteristics
            Charasteristic defense = determineDefense(damage);
            int stunDamage = Math.Max(0, damage.Stun - defense.CurrentValue);
            int bodyDamage = Math.Max(0, damage.Body - defense.CurrentValue);
            STUN.Deduct(stunDamage);
            BOD.Deduct(bodyDamage);

            Dictionary<CharacterStateType, HeroCharacterState> statesResultingFromDamage = new Dictionary<CharacterStateType, HeroCharacterState>();
            HeroCharacterState stateFromDamage = null;
            if (stunDamage > CON.CurrentValue)
            {
                stateFromDamage = AddState(CharacterStateType.Stunned, false);
                statesResultingFromDamage.Add(stateFromDamage.Type, stateFromDamage);
            }
            if (STUN.CurrentValue <= 0)
            {
                stateFromDamage = AddState(CharacterStateType.Unconsious, false);
                statesResultingFromDamage.Add(stateFromDamage.Type, stateFromDamage);
            }
            if (BOD.CurrentValue <= 0 && BOD.CurrentValue > (BOD.MaxValue * -1))
            {
                stateFromDamage = AddState(CharacterStateType.Dying, false) ;
                statesResultingFromDamage.Add(stateFromDamage.Type, stateFromDamage);
            }
            else
            {
                if (BOD.CurrentValue < 0 && BOD.CurrentValue < (BOD.MaxValue * -1))
                {
                    RemoveState(CharacterStateType.Dying);
                    stateFromDamage = AddState(CharacterStateType.Dead, false); ;
                    statesResultingFromDamage.Add(stateFromDamage.Type, stateFromDamage);
                }
            }
            
            return statesResultingFromDamage;
        }
        private Charasteristic determineDefense(Damage damage)
        {
            switch (damage.WorksAgainstDefense)
            {
                case DefenseType.PD:
                    return PD;
                case DefenseType.ED:
                    return PD;
                case DefenseType.RED:
                    return RED;
            }
            return null;

        }
    }

}
