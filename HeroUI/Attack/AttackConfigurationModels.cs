using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Attack
{
    public class DefenderAttackInstructions : PropertyChangedBase
    {
        private AnimatedCharacter defender;
        public AnimatedCharacter Defender
        {
            get
            {
                return defender;
            }
            set
            {
                defender = value;
                NotifyOfPropertyChange(() => Defender);
            }
        }
        private bool hasMultipleAttackers;
        public bool HasMultipleAttackers
        {
            get
            {
                return hasMultipleAttackers;
            }
            set
            {
                hasMultipleAttackers = value;
                NotifyOfPropertyChange(() => HasMultipleAttackers);
            }
        }
        private bool isAttackCenter;
        public bool IsAttackCenter
        {
            get
            {
                return isAttackCenter;
            }
            set
            {
                isAttackCenter = value;
                NotifyOfPropertyChange(() => IsAttackCenter);
            }
        }
        private bool moveAttackersToDefender;
        public bool MoveAttackersToDefender
        {
            get
            {
                return moveAttackersToDefender;
            }
            set
            {
                moveAttackersToDefender = value;
                NotifyOfPropertyChange(() => MoveAttackersToDefender);
            }
        }
        private bool defenderHitByAttack;
        public bool DefenderHitByAttack
        {
            get
            {
                return defenderHitByAttack;
            }
            set
            {
                defenderHitByAttack = value;
                NotifyOfPropertyChange(() => DefenderHitByAttack);
            }
        }

        private ObservableCollection<AttackerHitInfo> attackerHitInfo;
        public ObservableCollection<AttackerHitInfo> AttackerHitInfo
        {
            get
            {
                return attackerHitInfo;
            }
            set
            {
                attackerHitInfo = value;
                NotifyOfPropertyChange(() => AttackerHitInfo);
            }
        }

        private bool defenderStunned;
        public bool DefenderStunned
        {
            get
            {
                return defenderStunned;
            }
            set
            {
                defenderStunned = value;
                NotifyOfPropertyChange(() => DefenderStunned);
            }
        }
        private bool defenderUnconscious;
        public bool DefenderUnconscious
        {
            get
            {
                return defenderUnconscious;
            }
            set
            {
                defenderUnconscious = value;
                NotifyOfPropertyChange(() => DefenderUnconscious);
            }
        }
        private bool defenderDying;
        public bool DefenderDying
        {
            get
            {
                return defenderDying;
            }
            set
            {
                defenderDying = value;
                NotifyOfPropertyChange(() => DefenderDying);
            }
        }
        private bool defenderDead;
        public bool DefenderDead
        {
            get
            {
                return defenderDead;
            }
            set
            {
                defenderDead = value;
                NotifyOfPropertyChange(() => DefenderDead);
            }
        }

        private int defenderKnockbackDistance;
        public int DefenderKnockbackDistance
        {
            get
            {
                return defenderKnockbackDistance;
            }
            set
            {
                defenderKnockbackDistance = value;
                NotifyOfPropertyChange(() => DefenderKnockbackDistance);
            }
        }
    }

    public class AttackerHitInfo : PropertyChangedBase
    {
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
            }
        }

        private AttackInstructions attackInstructionsForAttacker;
        public AttackInstructions AttackInstructionsForAttacker
        {
            get
            {
                return attackInstructionsForAttacker;
            }
            set
            {
                attackInstructionsForAttacker = value;
                NotifyOfPropertyChange(() => AttackInstructionsForAttacker);
            }
        }
    }
}
