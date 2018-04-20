using HeroVirtualTabletop.Desktop;
using Newtonsoft.Json;
using System.Linq;

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
        
        private string surface;
        [JsonProperty]
        public string Surface
        {
            get
            {
                return surface;
            }
            set
            {
                surface = value;
                NotifyOfPropertyChange(() => Surface);
            }
        }
        
        private SurfaceType type;
        [JsonProperty]
        public SurfaceType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }

        private AnimatedAbility.AnimatedAbility animationOnLoad;
        public AnimatedAbility.AnimatedAbility AnimationOnLoad
        {
            get
            {
                return animationOnLoad;
            }
            set
            {
                animationOnLoad = value;
                NotifyOfPropertyChange(() => AnimationOnLoad);
            }
        }

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
            if(this.AnimationOnLoad != null)
            {
                clone.AnimationOnLoad = this.AnimationOnLoad.Clone() as AnimatedAbility.AnimatedAbility;
            }
            return clone;
        }
    }
}