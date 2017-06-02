using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Framework.WPF.Library;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.Common;
using Caliburn.Micro;
using Newtonsoft.Json;
using System.Windows.Data;
using System;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class ManagedCharacterImpl : PropertyChangedBase, ManagedCharacter, CharacterActionContainer
    {
        private bool _maneuveringWithCamera;
        public ManagedCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera,
            CharacterActionList<Identity> identities)
        {
            Targeter = targeter;
            Generator = generator;
            Camera = camera;
            if (Identities == null)
                identities = new CharacterActionListImpl<Identity>(CharacterActionType.Identity, Generator, this);
            Identities = identities;
            foreach (var id in Identities.Values)
                id.Owner = this;
        }
        public ManagedCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera)
            : this(targeter, generator, camera, null)
        {
        }

        public DesktopMemoryCharacter MemoryInstance
        {
            get;
            set;
        }
        public DesktopCharacterTargeter Targeter
        {
            get;
            set;
        }
        [JsonIgnore]
        public KeyBindCommandGenerator Generator
        {
            get;
            set;
        }
        [JsonIgnore]
        public Camera Camera { get; set; }

        public Position Position => MemoryInstance.Position;
        private string name;
        [JsonProperty(Order = 0)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public virtual string DesktopLabel
        {
            get
            {
                if (MemoryInstance != null)
                    return MemoryInstance.Label;
                return Name;
            }
        }

        public void ToggleTargeted()
        {
            if (IsTargeted)
                UnTarget();
            else
                Target();
        }
        public bool IsTargeted
        {
            get
            {
                var targetedInstance = Targeter.TargetedInstance;
                if (this.DesktopLabel == targetedInstance.Label)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value && !IsTargeted)
                {
                    Target();
                }
                else if (!value && IsTargeted)
                {
                    UnTarget();
                }
            }
        }
        public virtual void Target(bool completeEvent = true)
        {
            if (MemoryInstance != null)
            {
                MemoryInstance.Target();
                WaitUntilTargetIsRegistered();
            }
            else
            {
                Generator.GenerateDesktopCommandText(DesktopCommand.TargetName, DesktopLabel);
                if (completeEvent)
                {
                    Generator.CompleteEvent();
                    MemoryInstance = WaitUntilTargetIsRegistered();
                }
            }
        }
        public virtual void UnTarget(bool completeEvent = true)
        {
            Generator.GenerateDesktopCommandText(DesktopCommand.TargetEnemyNear);
            UnFollow();
            if (completeEvent)
            {
                Generator.CompleteEvent();
                try
                {
                    var currentTarget = Targeter.TargetedInstance;
                    while (currentTarget.Label != string.Empty)
                    {
                        currentTarget = Targeter.TargetedInstance;
                        if (currentTarget.Label == null) break;
                    }
                }
                catch
                {
                }
            }
        }

        private DesktopMemoryCharacter WaitUntilTargetIsRegistered()
        {
            var w = 0;
            var currentTarget = Targeter.TargetedInstance;
            while (DesktopLabel != currentTarget.Label)
            {
                w++;
                currentTarget = Targeter.TargetedInstance;
                if (w > 5)
                {
                    currentTarget = null;
                    break;
                }
            }
            return currentTarget;
        }

        public bool IsFollowed { get; set; }
        public void UnFollow(bool completeEvent = true)
        {
            if (IsFollowed)
            {
                Generator.GenerateDesktopCommandText(DesktopCommand.Follow);
                Generator.CompleteEvent();
                IsFollowed = false;
            }
        }
        public void Follow(bool completeEvent = true)
        {
            IsFollowed = true;
            Generator.GenerateDesktopCommandText(DesktopCommand.Follow);
            Generator.CompleteEvent();
        }

        public void TargetAndMoveCameraToCharacter(bool completeEvent = true)
        {
            Target();
            Camera.MoveToTarget();
        }
        public void ToggleManueveringWithCamera()
        {
            IsManueveringWithCamera = !IsManueveringWithCamera;
        }
        public bool IsManueveringWithCamera
        {
            get { return _maneuveringWithCamera; }

            set
            {
                _maneuveringWithCamera = value;
                if (value)
                {
                    Camera.ManueveringCharacter = this;
                }
                else
                {
                    if (value == false)
                        Camera.ManueveringCharacter = null;
                }
            }
        }

        [JsonProperty(Order = 5)]
        public CharacterActionList<Identity> Identities { get; private set; }
        public virtual Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> CharacterActionGroups
        {
            get
            {
                Dictionary<string, CharacterAction> actions = new Dictionary<string, CharacterAction>();
                //Identities.Values.ForEach(x => actions[x.Name] = x);
                foreach (var x in Identities.Values)
                {
                    actions[x.Name] = x;
                }
                Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> actionsList
                    = new Dictionary<CharacterActionType, Dictionary<string, CharacterAction>>();
                actionsList.Add(CharacterActionType.Identity, actions);
                return actionsList;
            }
        }
        public Dictionary<string, Identity> IdentitiesList {
            get
            {
                var i = new Dictionary<string, Identity>();
                foreach (var x in Identities)
                {
                    i[x.Key] = x.Value;
                }
               // Identities.ForEach(x => i[x.Key]= x.Value);
                return i;
            }
        }

        public Identity DefaultIdentity => Identities.Default;

        public bool IsSpawned { get; set; }
        public void SpawnToDesktop(bool completeEvent = true)
        {
            if (IsManueveringWithCamera)
                IsManueveringWithCamera = false;
            if (IsSpawned)
                ClearFromDesktop();

            Generator.GenerateDesktopCommandText(DesktopCommand.TargetEnemyNear);
            Generator.GenerateDesktopCommandText(DesktopCommand.NOP);
                //No operation, let the game untarget whatever it has targeted

            IsSpawned = true;
            var spawnText = Name;
            if (DesktopLabel != null && DesktopLabel != "")
                spawnText = DesktopLabel;
            if (Identities == null)
                Identities = new CharacterActionListImpl<Identity>(CharacterActionType.Identity, Generator, this);
            if (Identities.Count == 0 && Identities.Active == null)
            {
                var newId = Identities.AddNew(new IdentityImpl());
                newId.Owner = this;
                newId.Name = Name;
                newId.Type = SurfaceType.Costume;
                newId.Surface = Name;
                newId.Generator = this.Generator;
                Identities.Active = newId;
            }
            var active = Identities.Active;
            string model = "model_statesman";
            if (active.Type == SurfaceType.Model)
            {
                model = active.Surface;
            }
            Generator.GenerateDesktopCommandText(DesktopCommand.SpawnNpc, model, spawnText);
            Target(false);
            active?.Play();
        }
        public void ClearFromDesktop(bool completeEvent = true, bool clearManueveringWithCamera = true)
        {
            Target();
            Generator.GenerateDesktopCommandText(DesktopCommand.DeleteNPC);
            if (completeEvent)
                Generator.CompleteEvent();
            IsSpawned = false;
            IsTargeted = false;
            if(clearManueveringWithCamera)
                IsManueveringWithCamera = false;
            IsFollowed = false;
            MemoryInstance = null;
        }
        public void MoveCharacterToCamera(bool completeEvent = true)
        {
            Target();
            Generator.GenerateDesktopCommandText(DesktopCommand.MoveNPC);
            Generator.CompleteEvent();
        }

        public void SyncWithGame()
        {
            var oldTarget = Targeter.TargetedInstance;
            Target();
            if (this.IsTargeted)
                this.IsSpawned = true;
            try
            {
                oldTarget.Target();
            }
            catch { }
        }

        public CharacterProgressBarStats ProgressBar { get; set; }
        
    }

    public class CharacterComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool retV = true;
            if (values.Count() > 0)
            {
                foreach (object o in values)
                {
                    if (!(o is ManagedCharacter))
                    {
                        retV = false;
                        break;
                    }
                }
                if (retV)
                {
                    foreach (ManagedCharacter c in values)
                    {
                        if (c != (ManagedCharacter)values[0])
                        {
                            retV = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                retV = false;
            }

            return retV;
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}