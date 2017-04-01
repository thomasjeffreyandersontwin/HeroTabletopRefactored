using System.Collections.Generic;
using HeroVirtualTableTop.AnimatedAbility;
using HeroVirtualTableTop.Desktop;

namespace HeroVirtualTableTop.Attack
{
    public interface AttackInstructions
    {
        AnimatedCharacter Defender { get; set; }
        List<string> Impacts { get; }
        int KnockbackDistance { get; set; }
        bool AttackHit { get; set; }
        bool isCenterOfAreaEffectattack { get; set; }
    }
    public class AttackEffects
    {
        public static string Stunned => "Stunned";

        public static string Unconsious => "Unconsious";

        public static string Hit => "Hit";

        public static string Miss => "Miss";

        public static string Dead => "Dead";

        public static string Dying => "Dying";
    }
    public interface AnimatedAttack : AnimatedAbility.AnimatedAbility
    {
        AnimatedAbility.AnimatedAbility OnHitAnimation { get; set; }

        Position TargetDestination { set; }

        bool IsActive { get; set; }
        AnimatedCharacter Attacker { get; set; }
        AttackInstructions StartAttackCycle();
        KnockbackCollisionInfo PlayCompleteAttackCycle(AttackInstructions instructions);
        KnockbackCollisionInfo CompleteTheAttackCycle(AttackInstructions instructions);

        KnockbackCollisionInfo AnimateKnockBack();

        void FireAtDesktop(Position desktopPosition);
    }
    
    public interface AreaAttackInstructions : AttackInstructions
    {
        List<AttackInstructions> IndividualTargetInstructions { get; }
        Position AttackCenter { get; }
        List<AnimatedCharacter> Defenders { get; }
        List<AnimatedCharacter> DefendersHit { get; }
        List<AnimatedCharacter> DefendersMissed { get; }
        AttackInstructions AddTarget(AnimatedCharacter defender);

    }





    public interface AreaEffectAttack : AnimatedAttack
    {
        new AreaAttackInstructions StartAttackCycle();
        AreaAttackInstructions DetermineTargetsFromPositionOfAttack(int radius, Position attackCenter);
        List<KnockbackCollisionInfo> PlayCompleteAttackCycle(AreaAttackInstructions instructions);
        List<KnockbackCollisionInfo> CompleteTheAttackCycle(AreaAttackInstructions instructions);
    }

    public enum KnockbackCollisionType
    {
        Wall = 1,
        Floor = 2,
        Air = 3
    }
    public interface KnockbackCollisionInfo
    {
        KnockbackCollisionType Type { get; set; }
        string CharacterName { get; set; }
    }

}