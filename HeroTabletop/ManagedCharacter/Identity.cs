using HeroVirtualTabletop.Desktop;
using Newtonsoft.Json;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class IdentityImpl : CharacterActionImpl, Identity
    {
        public IdentityImpl(ManagedCharacter owner, string name, string surface, SurfaceType type,
            KeyBindCommandGenerator generator, string shortcut) : base(owner, name, generator, shortcut)
        {
            Type = type;
            Surface = surface;
        }
        [JsonConstructor]
        public IdentityImpl()
        {
        }
        [JsonProperty]
        public string Surface
        { get;
            set; }
        public SurfaceType Type { get; set; }

        public override void Play(bool completeEvent)
        {
            switch (Type)
            {
                case SurfaceType.Model:
                {
                    Generator.GenerateDesktopCommandText(DesktopCommand.BeNPC, Surface);
                    break;
                }
                case SurfaceType.Costume:
                {
                    Generator.GenerateDesktopCommandText(DesktopCommand.LoadCostume, Surface);
                    break;
                }
            }
            if (completeEvent)
                Generator.CompleteEvent();
            ((ManagedCharacter)Owner)?.Target(completeEvent);
        }

        public override CharacterAction Clone()
        {
            Identity clone = new IdentityImpl(((ManagedCharacter)Owner), Name, Surface, Type, Generator, KeyboardShortcut);
            return clone;
        }
    }
}