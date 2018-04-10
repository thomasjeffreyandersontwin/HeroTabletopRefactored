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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using HeroVirtualTabletop.Movement;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class ManagedCharacterImpl : PropertyChangedBase, ManagedCharacter, CharacterActionContainer
    {
        private bool _maneuveringWithCamera;

        private const string IDENTITY_ACTION_GROUP_NAME = "Identities";
        public ManagedCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera,
            CharacterActionList<Identity> identities)
        {
            Targeter = targeter;
            Generator = generator;
            Camera = camera;
        }
        public ManagedCharacterImpl(DesktopCharacterTargeter targeter, KeyBindCommandGenerator generator, Camera camera)
            : this(targeter, generator, camera, null)
        {
        }

        public virtual void InitializeActionGroups()
        {
            if(this.CharacterActionGroups == null || this.CharacterActionGroups.Count == 0)
            {
                this.CharacterActionGroups = new ObservableCollection<CharacterActionGroup>();
                CreateIdentityActionGroup();
            }
        }

        private void CreateIdentityActionGroup()
        {
            var identitiesGroup = new CharacterActionListImpl<Identity>(CharacterActionType.Identity, Generator, this);
            identitiesGroup.Name = IDENTITY_ACTION_GROUP_NAME;
            
            IdentityImpl newId = new IdentityImpl();
            newId.Owner = this;
            newId.Name = Name;
            newId.Type = SurfaceType.Costume;
            newId.Surface = Name;
            newId.Generator = this.Generator;
            identitiesGroup.AddNew(newId);
            identitiesGroup.Active = newId;

            this.CharacterActionGroups.Add(identitiesGroup);
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
        private bool isGangLeader;
        public bool IsGangLeader
        {
            get
            {
                return isGangLeader;
            }
            set
            {
                isGangLeader = value;
                NotifyOfPropertyChange(() => IsGangLeader);
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
                if (completeEvent)
                {
                    MemoryInstance.Target();
                    WaitUntilTargetIsRegistered();
                }
                else
                    Generator.GenerateDesktopCommandText(DesktopCommand.TargetName, DesktopLabel);
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
        public void ToggleManeuveringWithCamera()
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

        public void AlignGhost()
        {

        }
        
        public CharacterActionList<Identity> Identities
        {
            get
            {
                return CharacterActionGroups.FirstOrDefault(ag => ag.Name == IDENTITY_ACTION_GROUP_NAME) as CharacterActionList<Identity>;
            }
        }
        public virtual Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> StandardActionGroups
        {
            get
            {
                Dictionary<string, CharacterAction> actions = new Dictionary<string, CharacterAction>();
                //Identities.Values.ForEach(x => actions[x.Name] = x);
                foreach (var x in Identities)
                {
                    actions[x.Name] = x;
                }
                Dictionary<CharacterActionType, Dictionary<string, CharacterAction>> actionsList
                    = new Dictionary<CharacterActionType, Dictionary<string, CharacterAction>>();
                actionsList.Add(CharacterActionType.Identity, actions);
                return actionsList;
            }
        }

        private ObservableCollection<CharacterActionGroup> actionGroups;
        [JsonProperty(Order = 1)]
        public ObservableCollection<CharacterActionGroup> CharacterActionGroups
        {
            get
            {
                return actionGroups;
            }
            set
            {
                actionGroups = value;
                NotifyOfPropertyChange(() => CharacterActionGroups);
            }
        }
        public Dictionary<string, Identity> IdentitiesList {
            get
            {
                var i = new Dictionary<string, Identity>();
                foreach (var x in Identities)
                {
                    i[x.Name] = x;
                }
               // Identities.ForEach(x => i[x.Key]= x.Value);
                return i;
            }
        }

        public Identity DefaultIdentity => Identities.Default;
        public Identity ActiveIdentity => Identities.Active;

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
            
            var active = Identities.Active ?? Identities.Default;
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
            (this as MovableCharacter).MoveForwardTo(this.Camera.Position);
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
        public void AlignFacingWith(ManagedCharacter character)
        {
            Vector3 leaderFacingVector = character.Position.FacingVector;
            Vector3 distantPointInSameDirection = character.Position.Vector + leaderFacingVector * 500;
            (this.Position as Position).Face(distantPointInSameDirection);
        }
        public void RemoveActionGroup(CharacterActionGroup actionGroup)
        {
            this.CharacterActionGroups.Remove(actionGroup);
        }

        public void RemoveActionGroupAt(int index)
        {
            this.CharacterActionGroups.RemoveAt(index);
        }

        public void AddActionGroup(CharacterActionGroup actionGroup)
        {
            this.CharacterActionGroups.Add(actionGroup);
        }

        public void InsertActionGroup(int index, CharacterActionGroup actionGroup)
        {
            this.CharacterActionGroups.Insert(index, actionGroup);
        }

        public string GetnewValidActionGroupName()
        {
            string baseName = "Custom Option Group";
            string validName = baseName;
            int i = 1;
            while (this.CharacterActionGroups.FirstOrDefault(a => a.Name == validName) != null)
            {
                validName = string.Format("{0} ({1})", baseName, i++);
            }

            return validName;
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

    public class CharacterActionComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 2)
            {
                return false;
            }
            return values[0] == values[1];
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}