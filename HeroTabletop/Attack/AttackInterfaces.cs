using System.Collections.Generic;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Desktop;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HeroVirtualTabletop.Attack
{
    public class AttackEffects
    {
        public static string Stunned => "Stunned";

        public static string Unconscious => "Unconscious";

        public static string Hit => "Hit";

        public static string Miss => "Miss";

        public static string Dead => "Dead";

        public static string Dying => "Dying";
    }

    public enum ObstacleType
    {
        Hit,
        Knockback
    }

    public interface Obstacle
    {
        AnimatedCharacter Attacker { get; set; }
        AnimatedCharacter Defender { get; set; }
        object ObstacleTarget { get; set; }
        Position ObstaclePosition { get; set; }
        ObstacleType ObstacleType { get; set; }
        AttackInstructions ObstacleInstructions { get; set; }
    }

    public interface AttackInstructions: INotifyPropertyChanged
    {
        AnimatedCharacter Attacker { get; set; }
        AnimatedCharacter Defender { get; set; }
        ObservableCollection<string> Impacts { get; }
        float KnockbackDistance { get; set; }
        bool AttackHit { get; set; }
        bool IsCenterOfAreaEffectAttack { get; set; }
        List<Obstacle> Obstacles { get; set; }
        void AddImpact(string impactName);
        void SetImpactToDefender(string impactName);
        void RemoveImpact(string impactName);
        void RemoveImpactFromDefender(string impactName);
        void AddObstacle(Obstacle obstacle);
        void RemoveFromObstacles(AnimatedCharacter obstacleCharacter);
    }

    public interface MultiAttackInstructions : AttackInstructions
    {
        ObservableCollection<AttackInstructions> IndividualTargetInstructions { get; }
        List<AnimatedCharacter> Defenders { get; }
        List<AnimatedCharacter> DefendersHit { get; }
        List<AnimatedCharacter> DefendersMissed { get; }
        AttackInstructions AddTarget(AnimatedCharacter attacker, AnimatedCharacter defender);
        List<AnimatedCharacter> GetDefendersByImpactBasedOnSeverity(string impactName);
        void Clear();
    }

    public interface AreaAttackInstructions : MultiAttackInstructions
    {
        Position AttackCenter { get; }
    }

    public interface GangAttackInstructions: MultiAttackInstructions
    {
        Dictionary<AnimatedCharacter, List<AttackInstructions>> AttackInstructionsMap { get; } // Key = Defender, Value = Instructions for each attacker for this defender
        Dictionary<AnimatedCharacter, List<AnimatedCharacter>> AttackersMap { get; } // Key = Defender, Value = Attackers for that Defender
    }
    public interface GangAreaAttackInstructions: AreaAttackInstructions
    {
        Dictionary<AnimatedCharacter, AreaAttackInstructions> AttackInstructionsMap { get; } // Key = attacker, Value = Area Instructions for this attacker
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
        void Cancel(AttackInstructions instructions);
        AreaEffectAttack TransformToAreaEffectAttack();
        MultiAttack TransformToMultiAttack();
        GangAttack TransformToGangAttack();
        AnimatedAbility.AnimatedAbility TransformToAbility();
        void InitiateFrom(AnimatedAttack attackToCopy);
    }

    public interface AreaEffectAttack : AnimatedAttack
    {
        new AreaAttackInstructions StartAttackCycle();
        AreaAttackInstructions DetermineTargetsFromPositionOfAttack(int radius, Position attackCenter);
        List<KnockbackCollisionInfo> PlayCompleteAttackCycle(AreaAttackInstructions instructions);
        List<KnockbackCollisionInfo> CompleteTheAttackCycle(AreaAttackInstructions instructions);
        GangAreaAttack TransformToGangAreaAttack();
        void Cancel(AreaAttackInstructions instructions);
    }

    public interface MultiAttack : AnimatedAttack
    {
        new MultiAttackInstructions StartAttackCycle();
        List<KnockbackCollisionInfo> PlayCompleteAttackCycle(MultiAttackInstructions instructions);
        List<KnockbackCollisionInfo> CompleteTheAttackCycle(MultiAttackInstructions instructions);
        void Cancel(MultiAttackInstructions instructions);
    }

    public interface GangAttack : AnimatedAttack
    {
        List<AnimatedCharacter> GangMembers { get; set; }
        new GangAttackInstructions StartAttackCycle();
        List<KnockbackCollisionInfo> PlayCompleteAttackCycle(GangAttackInstructions instructions);
        List<KnockbackCollisionInfo> CompleteTheAttackCycle(GangAttackInstructions instructions);
        void Cancel(GangAttackInstructions instructions);
    }

    public interface GangAreaAttack : AnimatedAttack
    {
        List<AnimatedCharacter> GangMembers { get; set; }
        new GangAreaAttackInstructions StartAttackCycle();
        List<KnockbackCollisionInfo> PlayCompleteAttackCycle(GangAreaAttackInstructions instructions);
        List<KnockbackCollisionInfo> CompleteTheAttackCycle(GangAreaAttackInstructions instructions);
        void Cancel(GangAreaAttackInstructions instructions);
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