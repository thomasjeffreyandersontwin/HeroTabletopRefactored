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
using System.IO;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public class ManagedCharacterImpl : PropertyChangedBase, ManagedCharacter, CharacterActionContainer
    {
        private bool _maneuveringWithCamera;
        private const string GAME_COSTUMES_EXT = ".costume";
        private const string GAME_GHOST_COSTUMENAME = "ghost";
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
            
            Identity newId = new IdentityImpl();
            newId.Owner = this;
            newId.Name = Name;
            newId.Type = SurfaceType.Costume;
            newId.Surface = Name;
            newId.Generator = this.Generator;
            identitiesGroup.AddNew(newId);
            identitiesGroup.Active = newId;

            this.CharacterActionGroups.Add(identitiesGroup);
        }
        [JsonIgnore]
        public ManagedCharacter GhostShadow
        {
            get; set;
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

        public Position Position => MemoryInstance?.Position;

        public float DistanceLimit { get; set; }
        
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
        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                NotifyOfPropertyChange(() => IsActive);
            }

        }
        public void Activate()
        {
            IsActive = true;
        }
        public void DeActivate()
        {
            IsActive = false;
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
            if (MemoryInstance != null && MemoryInstance.IsReal)
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

        #region Ghost
        public void AlignGhost()
        {
            if (this.ActiveIdentity?.Type == SurfaceType.Model)
            {
                if (this.GhostShadow == null)
                {
                    CreateGhostShadow();
                }
                if (!this.GhostShadow.IsSpawned)
                    this.GhostShadow.SpawnToDesktop();
                this.Target();
                this.GhostShadow.Position.Vector = this.Position.Vector;
                this.GhostShadow.Position.RotationMatrix = this.Position.RotationMatrix;
            }
        }

        public void Teleport(Position position = null)
        {
            if (this.MemoryInstance == null || !this.MemoryInstance.IsReal)
                return;
            if (position == null)
                position = this.Camera.AdjustedPosition;
            this.Position?.MoveTo(position);
            this.AlignGhost();
            this.UpdateDistanceCount();
        }

        public void UpdateDistanceCount()
        {
            this.Position.UpdateDistanceCount();
        }

        public void UpdateDistanceCount(Position position)
        {
            this.Position.UpdateDistanceCount(position);
        }

        public void ResetDistanceCount()
        {
            this.Position?.ResetDistanceCount();
        }
        public  virtual void CreateGhostShadow()
        {
            this.GhostShadow = new ManagedCharacterImpl(this.Targeter, this.Generator, this.Camera);
            this.GhostShadow.Name = "ghost_" + this.Name;
            this.GhostShadow.InitializeActionGroups();
            SetGhostIdentity();
        }
        public void SetGhostIdentity()
        {
            //CreateGhostCostumeFile("Director Solair");
            CreateGhostCostumeFile();
        }

        private void CreateGhostCostumeFile(string costumeName = null)
        {
            string costumePath = HeroVirtualTabletopGame.CostumeDirectory;
            string ghostShadowCostumeFileName = ("ghost_" + this.Name) + GAME_COSTUMES_EXT;
            string ghostShadowCostumeFile = Path.Combine(costumePath, ghostShadowCostumeFileName);
            string originalGhostCostumeFileName = (costumeName != null ? costumeName + "_original" : "ghost_original") + GAME_COSTUMES_EXT;
            string ghostCostumeFileName = (costumeName ?? GAME_GHOST_COSTUMENAME) + GAME_COSTUMES_EXT;
            string originalGhostCostumeFile = Path.Combine(costumePath, originalGhostCostumeFileName);
            string ghostCostumeFile = Path.Combine(costumePath, ghostCostumeFileName);
            if (File.Exists(originalGhostCostumeFile))
            {
                File.Copy(originalGhostCostumeFile, ghostShadowCostumeFile, true);
            }
            else if (File.Exists(ghostCostumeFile))
            {
                File.Copy(ghostCostumeFile, ghostShadowCostumeFile, true);
            }

        }
        public void RemoveGhost()
        {
            if (this.GhostShadow != null)
            {
                this.GhostShadow.ClearFromDesktop();
                this.GhostShadow = null;
            }
        }
        #endregion
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

        public void SpawnToPosition(Position position)
        {
            if (!this.IsSpawned)
                SpawnToDesktop();
            this.Teleport(position);
        }

        public void CloneAndSpawn(Position position)
        {
            var clone = this.Clone();
            clone.SpawnToPosition(position);
        }
        private ManagedCharacter Clone()
        {
            var clone = new ManagedCharacterImpl(this.Targeter, this.Generator, this.Camera);
            clone.Name = this.Name + " Clone";
            clone.InitializeActionGroups();
            foreach (var id in Identities)
                clone.Identities.InsertAction((Identity)id.Clone());

            return clone;
        }
        public void ClearFromDesktop(bool completeEvent = true, bool clearManueveringWithCamera = true)
        {
            if (IsSpawned)
            {
                Target(completeEvent);
                Generator.GenerateDesktopCommandText(DesktopCommand.DeleteNPC);
                if (completeEvent)
                    Generator.CompleteEvent();
                this.GhostShadow?.ClearFromDesktop(completeEvent);
            }
            
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
            var movableCharacter = this as MovableCharacter;
            movableCharacter?.MoveForwardTo(this.Camera.AdjustedPosition);
        }

        public void SyncWithGame()
        {
            var oldTarget = Targeter.TargetedInstance;
            Target();
            if (this.IsTargeted)
            {
                this.IsSpawned = true;
                this.SyncGhostWithGame();
            }
            try
            {
                oldTarget.Target();
            }
            catch { }
        }
        public void SyncGhostWithGame()
        {
            if(this.ActiveIdentity.Type == SurfaceType.Model)
            {
                if (this.GhostShadow == null)
                    this.CreateGhostShadow();
                this.GhostShadow.SyncWithGame();
                if (!this.GhostShadow.IsSpawned)
                    this.AlignGhost();
            }
        }
        public void AlignFacingWith(ManagedCharacter character)
        {
            Vector3 leaderFacingVector = character.Position.FacingVector;
            Vector3 distantPointInSameDirection = character.Position.Vector + leaderFacingVector * 500;
            (this.Position as Position).Face(distantPointInSameDirection);
        }
        public void ScanAndFixMemoryTargeter()
        {
            if (IsSpawned)
            {
                this.Target();
                var memoryElement = WaitUntilTargetIsRegistered();

                if (memoryElement == null)
                {
                    this.Target(false);
                    Generator.CompleteEvent();
                    DesktopCharacterTargeter currentTarget = new DesktopCharacterTargeterImpl();

                    if (DesktopLabel == currentTarget.TargetedInstance?.Label)
                    {
                        this.Targeter = currentTarget;
                        this.MemoryInstance = this.Targeter.TargetedInstance;
                    }
                }
            }
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

        public void CopyIdentitiesTo(ManagedCharacter targetCharacter)
        {
            foreach(var id in this.Identities.Where(i => i.Name != this.Name))
            {
                Identity identity = id.Clone() as Identity;
                identity.Name = targetCharacter.GetNewValidIdentityName(identity.Name);
                if (identity.AnimationOnLoad != null)
                    identity.AnimationOnLoad.Owner = targetCharacter;
                targetCharacter.Identities.InsertAction(identity);
            }
        }
        #region Reset Orientation

        public void ResetOrientation()
        {
            if (this.IsSpawned)
            {
                this.Position.ResetOrientation();
                AlignGhost();
            }
        }

        #endregion
        public CharacterProgressBarStats ProgressBar { get; set; }
        public string GetNewValidIdentityName(string name = "Identity")
        {
            string suffix = string.Empty;
            int i = 0;
            while ((this.Identities.Any((Identity identity) => { return identity.Name == name + suffix; })))
            {
                suffix = string.Format(" ({0})", ++i);
            }
            return string.Format("{0}{1}", name, suffix).Trim();
        }
        public void RemoveIdentities()
        {
            var identities = this.Identities.ToList();
            foreach(var id in identities.Where(i => i.Name != this.Name))
            {
                if (this.Identities.Count() <= 1)
                    break;
                var identityToRemove = this.Identities.FirstOrDefault(a => a.Name == id.Name);
                this.Identities.RemoveAction(identityToRemove);
            }
        }
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