using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using HeroVirtualTabletop.Desktop;
using IrrKlang;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroVirtualTabletop.Common;
using System.Windows.Data;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Reflection;
using HeroVirtualTabletop.Crowd;
using Newtonsoft.Json;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Attack;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public abstract class AnimationElementImpl : PropertyChangedBase, AnimationElement
    {

        protected bool completeEvent;

        protected AnimationElementImpl(AnimatedCharacter owner)
        {
            Target = owner;
        }

        protected AnimationElementImpl()
        {
        }
        [JsonProperty]
        public int Order
        {
            get;
            set;
        }
        [JsonProperty]
        public AnimatedCharacter Target
        {
            get;
            set;
        }
        private bool playWithNext;
        [JsonProperty]
        public bool PlayWithNext
        {
            get
            {
                return playWithNext;
            }
            set
            {
                playWithNext = value;
                NotifyOfPropertyChange(() => PlayWithNext);
            }
        }
        private bool persistent;
        [JsonProperty]
        public bool Persistent
        {
            get
            {
                return persistent;
            }
            set
            {
                persistent = value;
                NotifyOfPropertyChange(() => persistent);
            }

        }
        [JsonProperty]
        public AnimationSequencer ParentSequence
        {
            get;
            set;
        }

        private string _name;
        [JsonProperty]
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        private AnimationElementType animationElementType;
        [JsonProperty]
        public AnimationElementType AnimationElementType
        {
            get
            {
                return animationElementType;
            }

            set
            {
                animationElementType = value;
                NotifyOfPropertyChange(() => AnimationElementType);
            }
        }

        public void DeactivatePersistent()
        {
            throw new NotImplementedException();
        }

        public abstract AnimationElement Clone(AnimatedCharacter target);

        public void Play(AnimatedCharacter target)
        {
            completeEvent = true;
            if (target.IsTargeted == false)
                target.Target(false);
            PlayResource(target);
        }

        public abstract void Play(List<AnimatedCharacter> targets);

        public void Play()
        {
            Play(Target);
        }

        public abstract void StopResource(AnimatedCharacter target);

        public void Stop()
        {
            Stop(Target);
        }

        public void Stop(AnimatedCharacter target)
        {
            if (target.IsTargeted == false)
                target.Target(false);
            StopResource(target);
        }

        public List<AnimationElement> AddToFlattendedList(List<AnimationElement> list)
        {
            throw new NotImplementedException();
        }

        protected bool baseAttributesEqual(AnimationElement other)
        {
            if (other.PlayWithNext != PlayWithNext) return false;
            if (other.Persistent != Persistent) return false;
            if (other.Order != Order) return false;
            return true;
        }

        protected AnimationElement cloneBaseAttributes(AnimationElement clone)
        {
            clone.Persistent = Persistent;
            clone.Name = Name;
            clone.Order = Order;
            clone.PlayWithNext = PlayWithNext;
            return clone;
        }

        public abstract void PlayResource(AnimatedCharacter target);
    }


    public class MovElementImpl : AnimationElementImpl, MovElement
    {
        public MovElementImpl(AnimatedCharacter owner, MovResource resource) : base(owner)
        {
            Mov = resource;
            this.AnimationElementType = AnimationElementType.Mov;
        }

        public MovElementImpl() : this(null, null)
        {
        }

        private MovResource mov;
        [JsonProperty]
        public MovResource Mov
        {
            get
            {
                return mov;
            }
            set
            {
                mov = value;
                NotifyOfPropertyChange(() => Mov);
            }
        }

        public static MovResource LastMov { get; set; }

        public override void Play(List<AnimatedCharacter> targets)
        {
            completeEvent = false;
            foreach (var target in targets)
            {
                target.Target(false);
                PlayResource(target);
            }
            completeEvent = true;
            var firstOrDefault = targets.FirstOrDefault();
            firstOrDefault?.Generator.CompleteEvent();
        }

        public override void StopResource(AnimatedCharacter target)
        {
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            MovElement clone = new MovElementImpl();
            clone = (MovElement)cloneBaseAttributes(clone);
            clone.Target = target;
            clone.Mov = Mov;
            return clone;
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            var generator = target.Generator;
            string[] para = { Mov?.FullResourcePath ?? "none"};
            generator.GenerateDesktopCommandText(DesktopCommand.Move, para);
            if (completeEvent)
                if (PlayWithNext == false)
                    generator.CompleteEvent();
        }

        public override bool Equals(object other)
        {
            if (other is MovElement == false)
                return false;
            var otherMov = other as MovElement;
            if (baseAttributesEqual(otherMov) != true) return false;
            if (otherMov.Mov != Mov) return false;
            return true;
        }
    }

    public class FXElementImpl : AnimationElementImpl, FXElement
    {
        public FXElementImpl(AnimatedCharacter owner, FXResource resource) : base(owner)
        {
            FX = resource;
            this.AnimationElementType = AnimationElementType.FX;
        }

        public FXElementImpl() : this(null, null)
        {
        }

        public string ModifiedCostumeText
        {
            get
            {
                if (File.Exists(ModifiedCostumeFilePath))
                    return File.ReadAllText(ModifiedCostumeFilePath);
                return null;
            }
        }

        private FXResource fx;
        [JsonProperty]
        public FXResource FX
        {
            get
            {
                return fx;
            }
            set
            {
                if (fx != null)
                    removePreviousFXResource(fx);
                fx = value;
                NotifyOfPropertyChange(() => FX);
            }
        }

        public static FXResource LastFX { get; set; }
        private Color color1;
        [JsonProperty]
        public Color Color1
        {
            get
            {
                return color1;
            }
            set
            {
                color1 = value;
                NotifyOfPropertyChange(() => Color1);
            }
        }
        private Color color2;
        [JsonProperty]
        public Color Color2
        {
            get
            {
                return color2;
            }
            set
            {
                color2 = value;
                NotifyOfPropertyChange(() => Color2);
            }
        }
        private Color color3;
        [JsonProperty]
        public Color Color3
        {
            get
            {
                return color3;
            }
            set
            {
                color3 = value;
                NotifyOfPropertyChange(() => Color3);
            }
        }
        private Color color4;
        [JsonProperty]
        public Color Color4
        {
            get
            {
                return color4;
            }
            set
            {
                color4 = value;
                NotifyOfPropertyChange(() => Color4);
            }
        }

        private bool isNonDirectional;
        public bool IsNonDirectional
        {
            get
            {
                return isNonDirectional;
            }
            set
            {
                isNonDirectional = value;
                NotifyOfPropertyChange(() => IsNonDirectional);
            }
        }
        [JsonProperty]
        public string OverridingCostumeName { get; set; }

        public Position AttackDirection { get; set; }

        public string CostumeText => File.ReadAllText(CostumeFilePath);

        public string ModifiedCostumeFilePath
        {
            get
            {
                string costumeName;
                if (Target.Identities?.Active != null)
                {
                    if(Target.Identities.Active.Type == ManagedCharacter.SurfaceType.Model && Target.GhostShadow != null)
                        costumeName = Target.GhostShadow.Name + "_" + Target.GhostShadow.Identities.Active.Surface + "_Modified.costume";
                    else if(!string.IsNullOrEmpty(OverridingCostumeName))
                        costumeName = Target.Name + "_" + OverridingCostumeName + "_Modified.costume";
                    else
                        costumeName = Target.Name + "_" + Target.Identities.Active.Surface + "_Modified.costume";
                }
                else
                    costumeName = Target.Name + "_Modified.costume";
                return Path.Combine(HeroVirtualTabletopGame.CostumeDirectory, costumeName);
            }
        }

        public string CostumeFilePath
        {
            get
            {
                string costume_name;
                if (Target.Identities?.Active != null)
                {
                    if (Target.Identities.Active.Type == ManagedCharacter.SurfaceType.Model && Target.GhostShadow != null)
                        costume_name = Target.GhostShadow.Identities.Active.Surface + ".costume";
                    else if(!string.IsNullOrEmpty(OverridingCostumeName))
                        costume_name = OverridingCostumeName + ".costume";
                    else
                        costume_name = Target.Identities.Active.Surface + ".costume";
                }
                else costume_name = Target.Name + ".costume";
                return Path.Combine(HeroVirtualTabletopGame.CostumeDirectory, costume_name);
            }
        }

        public bool ModifiedCostumeContainsFX
        {
            get
            {
                if (File.Exists(ModifiedCostumeFilePath))
                {
                    var fxString = "Fx " + FX.FullResourcePath;
                    var re = new Regex(Regex.Escape(fxString));
                    return re.IsMatch(ModifiedCostumeText);
                }
                return false;
            }
        }

        public override void Play(List<AnimatedCharacter> targets)
        {
            var originalTarget = Target;
            completeEvent = false;
            foreach (var target in targets)
            {
                Target = target;
                target.Target(false);
                PlayResource(target);
            }
            completeEvent = true;
            targets.FirstOrDefault()?.Generator.CompleteEvent();
            Target = originalTarget;
        }

        public override void StopResource(AnimatedCharacter target)
        {
            string fileStr = removePreviouslyLoadedFX(ModifiedCostumeText, true);
            File.Delete(ModifiedCostumeFilePath);
            File.AppendAllText(ModifiedCostumeFilePath, fileStr);

            string[] para = new string[1];
            para[0] = Path.GetFileNameWithoutExtension(ModifiedCostumeFilePath);


            target.Generator?.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para);
            if (completeEvent)
                target.Generator?.CompleteEvent();
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            FXElement clone = new FXElementImpl();
            clone = (FXElement)cloneBaseAttributes(clone);
            clone.Target = target;
            clone.FX = FX;
            clone.Color1 = Color1;
            clone.Color2 = Color2;
            clone.Color3 = Color3;
            clone.Color4 = Color4;
            return clone;
        }

        public Position Destination { get; set; }
        private bool isDirectional;
        [JsonProperty]
        public bool IsDirectional
        {
            get
            {
                return isDirectional;
            }
            set
            {
                isDirectional = value;
                NotifyOfPropertyChange(() => IsDirectional);
            }
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            var originalTarget = Target;
            Target = target;
            bool playingOnGhost = false;
            if(target.ActiveIdentity.Type == ManagedCharacter.SurfaceType.Model)
            {
                Target = target.GhostShadow as AnimatedCharacter;
                playingOnGhost = true;
                if (Target == null)
                    return;
            }
            Target.Target(false);
            if (!File.Exists(CostumeFilePath))
                return;
            if (File.Exists(ModifiedCostumeFilePath) && (Target.LoadedFXs == null || Target.LoadedFXs.Count == 0))
                File.Delete(ModifiedCostumeFilePath);
            if (!File.Exists(ModifiedCostumeFilePath))
                File.Copy(CostumeFilePath, ModifiedCostumeFilePath);
            var fileStr = ModifiedCostumeText;

            fileStr = removePreviouslyLoadedFX(fileStr);
            fileStr = addEmptyCostumePart(fileStr);
            fileStr = insertFXIntoCostume(fileStr);

            File.Delete(ModifiedCostumeFilePath);
            File.AppendAllText(ModifiedCostumeFilePath, fileStr);
            loadCostumeWithFxInIt(Target);
            if (Target.LoadedFXs != null && !Target.LoadedFXs.Contains(this))
                Target.LoadedFXs.Add(this);
            if (playingOnGhost)
                target.Target(false);
            Target = originalTarget;
        }

        private void removePreviousFXResource(FXResource fxResource, bool removePersistent = false)
        {
            if (File.Exists(ModifiedCostumeFilePath) && !string.IsNullOrEmpty(ModifiedCostumeText))
            {
                string fileStr = ModifiedCostumeText;
                fileStr = removeFX(fileStr, this, removePersistent);
                File.Delete(ModifiedCostumeFilePath);
                File.AppendAllText(ModifiedCostumeFilePath, fileStr);
            }
        }

        private string removePreviouslyLoadedFX(string fileStr, bool removePersistent = false)
        {
            if (Target.LoadedFXs != null)
                foreach (var fx in Target.LoadedFXs)
                    fileStr = removeFX(fileStr, fx, removePersistent);

            return fileStr;
        }

        private string removeFX(string fileStr, FXElement fx, bool removePersistent = false)
        {
            if (!fx.Persistent || removePersistent)
            {
                var re = new Regex(Regex.Escape(fx.FX.FullResourcePath));
                if (re.IsMatch(fileStr))
                    fileStr = fileStr.Replace(fx.FX.FullResourcePath, "none");
            }
            return fileStr;
        }

        private void loadCostumeWithFxInIt(AnimatedCharacter target)
        {
            var generator = target.Generator;
            string[] para;
            if (IsDirectional && Destination != null)
            {
                para = new string[2];
                para[0] = Path.GetFileNameWithoutExtension(ModifiedCostumeFilePath);
                para[1] = parseFromDestination();
            }
            else
            {
                para = new string[1];
                para[0] = Path.GetFileNameWithoutExtension(ModifiedCostumeFilePath);
            }


            generator.GenerateDesktopCommandText(DesktopCommand.LoadCostume, para);
            if (completeEvent)
                if (PlayWithNext == false)
                    generator.CompleteEvent();
        }

        private string parseFromDestination()
        {
            return $"x={Destination.X} y={Destination.Y} z={Destination.Z}";
        }

        private string insertFXIntoCostume(string fileStr)
        {
            var fxNew = "Fx " + FX.FullResourcePath;
            var fxNone = "Fx none";
            var re = new Regex(Regex.Escape(fxNone));
            var reFx = new Regex(Regex.Escape(fxNew));
            if (!reFx.IsMatch(fileStr))
                fileStr = re.Replace(fileStr, fxNew, 1);
            var fxPos = fileStr.IndexOf(fxNew);
            var colorStart = fileStr.IndexOf("Color1", fxPos);
            var colorEnd = fileStr.IndexOf("}", fxPos);
            var outputStart = fileStr.Substring(0, colorStart - 1);
            var outputEnd = fileStr.Substring(colorEnd);
            var outputColors =
                string.Format("\tColor1 {0}, {1}, {2}" + Environment.NewLine +
                              "\tColor2 {3}, {4}, {5}" + Environment.NewLine +
                              "\tColor3 {6}, {7}, {8}" + Environment.NewLine +
                              "\tColor4 {9}, {10}, {11}" + Environment.NewLine,
                    Color1.R, Color1.G, Color1.B,
                    Color2.R, Color2.G, Color2.B,
                    Color3.R, Color3.G, Color3.B,
                    Color4.R, Color4.G, Color4.B
                );
            fileStr = outputStart + outputColors + outputEnd;
            return fileStr;
        }

        private string addEmptyCostumePart(string fileStr)
        {
            var fxNone = "Fx none";
            var re = new Regex(Regex.Escape(fxNone));
            if (!re.IsMatch(ModifiedCostumeText))
                fileStr +=
                    @"CostumePart """"
{
    Fx none
    Geometry none
    Texture1 none
    Texture2 none
    Color1  0,  0,  0
    Color2  0,  0,  0
    Color3  0,  0,  0
    Color4  0,  0,  0
}";

            return fileStr;
        }

        public override bool Equals(object other)
        {
            if (other is FXElementImpl == false)
                return false;
            var otherFX = other as FXElement;
            if (baseAttributesEqual(otherFX) != true) return false;
            if (otherFX.FX != FX) return false;
            if (otherFX.Color1 != Color1) return false;
            if (otherFX.Color2 != Color2) return false;
            if (otherFX.Color3 != Color3) return false;
            if (otherFX.Color4 != Color4) return false;

            return true;
        }
    }

    public class SoundElementImpl : AnimationElementImpl, SoundElement
    {
        private bool _active;
        private Timer UpdateSoundPlayingPositionTimer;

        public SoundElementImpl(AnimatedCharacter owner, SoundResource resource) : base(owner)
        {
            Sound = resource;
            this.AnimationElementType = AnimationElementType.Sound;
        }

        public SoundElementImpl() : this(null, null)
        {
        }

        private SoundResource sound;
        [JsonProperty]
        public SoundResource Sound
        {
            get
            {
                return sound;
            }
            set
            {
                if (sound != null)
                    StopResource(Target);
                sound = value;
                NotifyOfPropertyChange(() => Sound);
            }
        }

        public static SoundResource LastSound { get; set; }

        public bool Active
        {
            get { return _active; }

            set
            {
                _active = value;
                if (_active == false)
                {
                    UpdateSoundPlayingPositionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    SoundEngine?.StopAllSounds();
                }
            }
        }

        public Position PlayingLocation
        {
            get { return null; }

            set { }
        }

        public string SoundFileName => HeroVirtualTabletopGame.SoundDirectory + Sound.FullResourcePath;

        private SoundEngineWrapper soundEngine;
        public SoundEngineWrapper SoundEngine
        {
            get
            {
                return soundEngine ?? (soundEngine = new SoundEngineWrapperImpl());
            }
            set
            {
                soundEngine = value;
            }
        }

        public override void Play(List<AnimatedCharacter> targets)
        {
            PlayResource(targets.FirstOrDefault());
        }

        public override void StopResource(AnimatedCharacter target)
        {
            if (Active)
                Active = false;
            UpdateSoundPlayingPositionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            SoundEngine?.StopAllSounds();
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            SoundElement clone = new SoundElementImpl();
            clone = (SoundElement)cloneBaseAttributes(clone);
            clone.Target = target;
            clone.Sound = Sound;

            return clone;
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            var soundFileName = SoundFileName;
            if (soundFileName == null)
                return;
            if (Persistent)
                Active = true;

            var targetPositionVector = target.Position;
            target.Camera.RefreshPosition();
            var camPositionVector = target.Camera.Position;

            SoundEngine.SetListenerPosition(camPositionVector.X, camPositionVector.Y, camPositionVector.Z, 0, 0, 1);
            SoundEngine.Default3DSoundMinDistance = 10f;
            SoundEngine.Play3D(soundFileName, targetPositionVector.X, targetPositionVector.Y, targetPositionVector.Z,
                Persistent);

            if (Persistent)
            {
                UpdateSoundPlayingPositionTimer = new Timer(UpdateListenerPositionBasedOnCamera_CallBack, null,
                    Timeout.Infinite, Timeout.Infinite);
                var tokenSrc = new CancellationToken();
                Task.Factory.StartNew(() =>
                {
                    if (Active)
                        UpdateSoundPlayingPositionTimer.Change(1, Timeout.Infinite);
                }, tokenSrc);
            }
        }

        private void UpdateListenerPositionBasedOnCamera_CallBack(object state)
        {
            if (Active)
            {
                var camPosition = Target.Camera.Position;
                SoundEngine.SetListenerPosition(camPosition.X, camPosition.Y, camPosition.Z, 0, 0, 1);
                UpdateSoundPlayingPositionTimer.Change(500, Timeout.Infinite);
            }
        }

        public override bool Equals(object other)
        {
            if (other is SoundElement == false)
                return false;
            var otherSound = other as SoundElement;
            if (baseAttributesEqual(otherSound) != true) return false;
            if (otherSound.Sound != Sound) return false;
            return true;
        }
    }

    public class SoundEngineWrapperImpl : SoundEngineWrapper
    {
        private readonly ISoundEngine engine = new ISoundEngine();

        public void SetListenerPosition(float posX, float posY, float posZ, float lookDirX, float lookDirY,
            float lookDirZ)
        {
            engine.SetListenerPosition(posX, posY, posZ, lookDirX, lookDirY, lookDirZ);
        }

        public void StopAllSounds()
        {
            engine.StopAllSounds();
        }

        public float Default3DSoundMinDistance
        {
            set { engine.Default3DSoundMinDistance = value; }
        }

        public void Play3D(string soundFilename, float posX, float posY, float posZ, bool playLooped)
        {
            engine.Play3D(soundFilename, posX, posY, posZ, playLooped);
        }
    }

    public class PauseElementImpl : AnimationElementImpl, PauseElement
    {
        private PauseBasedOnDistanceManager _distancemanager;

        private int _dur;

        public PauseElementImpl()
        {
            this.AnimationElementType = AnimationElementType.Pause;
        }

        public PauseBasedOnDistanceManager DistanceDelayManager
        {
            get
            {
                if (_distancemanager == null)
                    _distancemanager = new PauseBasedOnDistanceManagerImpl(this);
                return _distancemanager;
            }
            set
            {
                _distancemanager = value;
                if (_distancemanager.PauseElement != this)
                    _distancemanager.PauseElement = this;
            }
        }
        [JsonProperty]
        public int CloseDistanceDelay { get; set; }
        [JsonProperty]
        public int Duration
        {
            get
            {
                if (IsUnitPause && TargetPosition != null)
                {
                    var delayManager = new DelayManager(this);
                    var distance = Target.Position.DistanceFrom(TargetPosition);
                    var delay = (int)delayManager.GetDelayForDistance(distance);
                    return delay;
                }
                else
                    return _dur;
            }
            set { _dur = value; }
        }
        [JsonProperty]
        public int LongDistanceDelay { get; set; }
        [JsonProperty]
        public int MediumDistanceDelay { get; set; }
        [JsonProperty]
        public int ShortDistanceDelay { get; set; }
        [JsonProperty]
        public bool IsUnitPause { get; set; }

        public Position TargetPosition { get; set; }

        public override void Play(List<AnimatedCharacter> targets)
        {
            PlayResource(targets.FirstOrDefault());
        }

        public override void StopResource(AnimatedCharacter target)
        {
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            PauseElement clone = new PauseElementImpl();
            clone = (PauseElement)cloneBaseAttributes(clone);
            clone.Target = target;
            clone.LongDistanceDelay = LongDistanceDelay; 
            clone.MediumDistanceDelay = MediumDistanceDelay;
            clone.ShortDistanceDelay = ShortDistanceDelay;
            clone.IsUnitPause = IsUnitPause;
            clone.Duration = Duration;

            return clone;
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            if (IsUnitPause && TargetPosition != null)
            {
                DistanceDelayManager.Distance = target.Position.DistanceFrom(TargetPosition);
                Thread.Sleep((int)DistanceDelayManager.Duration);
            }
            else
            {
                Thread.Sleep(Duration);
            }
        }

        public override bool Equals(object other)
        {
            if (other is PauseElement == false)
                return false;
            var otherPause = other as PauseElement;
            if (baseAttributesEqual(otherPause) != true) return false;
            if (other is PauseElement)
            {
                if (otherPause.LongDistanceDelay != LongDistanceDelay) return false;
                if (otherPause.ShortDistanceDelay != ShortDistanceDelay) return false;
                if (otherPause.MediumDistanceDelay != MediumDistanceDelay) return false;
                if (otherPause.IsUnitPause != IsUnitPause) return false;
                if (otherPause.Duration != Duration) return false;
                return true;
            }
            return false;
        }
    }

    public class DelayManager
    {
        private readonly PauseElement pauseElement;
        private Dictionary<double, double> distanceDelayMappingDictionary;

        public DelayManager(PauseElement pauseElement)
        {
            this.pauseElement = pauseElement;
            ConstructDelayDictionary();
        }

        private void ConstructDelayDictionary()
        {
            distanceDelayMappingDictionary = new Dictionary<double, double>
            {
                {10, pauseElement.CloseDistanceDelay},
                {20, pauseElement.ShortDistanceDelay},
                {50, pauseElement.MediumDistanceDelay},
                {100, pauseElement.LongDistanceDelay}
            };
            distanceDelayMappingDictionary.Add(15,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[10],
                    distanceDelayMappingDictionary[20], 0.70));
            distanceDelayMappingDictionary.Add(30,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[20],
                    distanceDelayMappingDictionary[50], 0.6));
            distanceDelayMappingDictionary.Add(40,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[20],
                    distanceDelayMappingDictionary[50], 0.875));
            distanceDelayMappingDictionary.Add(60,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.4));
            distanceDelayMappingDictionary.Add(70,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.5));
            distanceDelayMappingDictionary.Add(80,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.7));
            distanceDelayMappingDictionary.Add(90,
                GetPercentageDelayBetweenTwoDelays(distanceDelayMappingDictionary[50],
                    distanceDelayMappingDictionary[100], 0.87));
        }

        private double GetPercentageDelayBetweenTwoDelays(double firstDelay, double secondDelay, double percentage)
        {
            return firstDelay - (firstDelay - secondDelay) * percentage;
        }

        private double GetLinearDelayBetweenTwoDelays(double firstDistance, double firstDelay, double secondDistance,
            double secondDelay, double targetDistance)
        {
            // y - y1 = m(x - x1); m = (y2 - y1)/(x2 - x1)
            var m = (secondDelay - firstDelay) / (secondDistance - firstDistance);
            var targetDelay = firstDelay + m * (targetDistance - firstDistance);
            return targetDelay;
        }

        public double GetDelayForDistance(double distance)
        {
            double targetDelay;
            if (distanceDelayMappingDictionary.ContainsKey(distance))
            {
                targetDelay = distanceDelayMappingDictionary[distance];
            }
            else if (distance <= 10)
            {
                targetDelay = distanceDelayMappingDictionary[10];
            }
            else if (distance < 100)
            {
                var nearestLowerDistance = distanceDelayMappingDictionary.Keys.OrderBy(d => d).Last(d => d < distance);
                var nearestHigherDistance = distanceDelayMappingDictionary.Keys.OrderBy(d => d).First(d => d > distance);
                targetDelay = GetLinearDelayBetweenTwoDelays(nearestLowerDistance,
                    distanceDelayMappingDictionary[nearestLowerDistance], nearestHigherDistance,
                    distanceDelayMappingDictionary[nearestHigherDistance], distance);
            }
            else
            {
                var baseDelayDiff = distanceDelayMappingDictionary[50] - distanceDelayMappingDictionary[100];
                var baseDelay = distanceDelayMappingDictionary[100];
                var nearestLowerHundredMultiplier = (int)(distance / 100);
                var nearestHigherHundredMultiplier = nearestLowerHundredMultiplier + 1;
                double nearestLowerHundredDistance = nearestLowerHundredMultiplier * 100;
                double nearestHigherHundredDistance = nearestHigherHundredMultiplier * 100;
                var currentLowerDelay = baseDelay;
                var currentHigherDelay = baseDelay - baseDelayDiff * 0.5;
                for (var i = 1; i < nearestLowerHundredMultiplier; i++)
                {
                    baseDelayDiff = currentLowerDelay - currentHigherDelay;
                    currentLowerDelay = currentHigherDelay;
                    currentHigherDelay = currentLowerDelay - baseDelayDiff * 0.5;
                }
                targetDelay = GetLinearDelayBetweenTwoDelays(nearestLowerHundredDistance, currentLowerDelay,
                    nearestHigherHundredDistance, currentHigherDelay, distance);
            }
            var targetDistance = distance < 10 ? 10 : distance;
            return targetDelay * targetDistance;
        }
    }

    public class SequenceElementImpl : AnimationElementImpl, SequenceElement
    {
        private AnimationSequencer _sequencer;

        public SequenceElementImpl(AnimationSequencer cloneedSequence, AnimatedCharacter owner) : base(owner)
        {
            _sequencer = cloneedSequence;
            this.AnimationElementType = AnimationElementType.Sequence;
        }
        public SequenceElementImpl() : this(null, null)
        {
        }
        public SequenceElementImpl(AnimatedCharacter owner) : this(null, owner)
        {

        }

        public ObservableCollection<AnimationElement> AnimationElements => Sequencer.AnimationElements;

        [JsonProperty]
        public AnimationSequencer Sequencer
        {
            get
            {
                return _sequencer ?? (_sequencer = new AnimationSequencerImpl(Target));
            }
            set
            {
                _sequencer = value;
                NotifyOfPropertyChange(() => Sequencer);
            }
        }

        [JsonProperty]
        public SequenceType Type
        {
            get { return Sequencer.Type; }
            set
            {
                Sequencer.Type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }

        public void InsertMany(List<AnimationElement> elements)
        {
            Sequencer.InsertMany(elements);
        }
        public void InsertElement(AnimationElement animationElement)
        {
            Sequencer.InsertElement(animationElement);
        }
        public void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter)
        {
            _sequencer.InsertElementAfter(toInsert, moveAfter);
        }
        public void RemoveElement(AnimationElement animationElement)
        {
            _sequencer.RemoveElement(animationElement);
        }

        public override void Play(List<AnimatedCharacter> targets)
        {
            Sequencer.Play(targets);
        }
        public override void StopResource(AnimatedCharacter target)
        {
            _sequencer.Stop(target);
        }
        public override void PlayResource(AnimatedCharacter target)
        {
            _sequencer.Play(target);
        }

        public AnimationElement GetNewAnimationElement(AnimationElementType animationElementType)
        {
            return _sequencer.GetNewAnimationElement(animationElementType);
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            var sequencer = (Sequencer as AnimationSequencerImpl)?.Clone(target) as AnimationSequencer;
            var clone = new SequenceElementImpl(sequencer, target);
            clone = (SequenceElementImpl)cloneBaseAttributes(clone);
            return clone;
        }
        public override bool Equals(object other)
        {
            if (other is SequenceElementImpl == false)
                return false;
            var otherSequence = other as SequenceElement;
            if (baseAttributesEqual(otherSequence) != true) return false;
            if (otherSequence.Sequencer.Equals(Sequencer) == false) return false;
            return true;
        }

        public void Play(List<AnimatedAbility> abilities)
        {
            _sequencer.Play(abilities);
        }
    }

    public class AnimationSequencerImpl : AnimationElementImpl, AnimationSequencer
    {
        public override string Name
        {
            get { return "Sequencer " + Order; }
            set { }
        }



        private ObservableCollection<AnimationElement> _animationElements;

        [JsonProperty]
        public ObservableCollection<AnimationElement> AnimationElements
        {
            get
            {
                return _animationElements ?? (_animationElements = new ObservableCollection<AnimationElement>());
            }
            private set
            {
                _animationElements = value;
                NotifyOfPropertyChange(() => AnimationElements);
            }
        }

        public AnimationSequencerImpl(AnimatedCharacter target)
        {
            Target = target;
        }

        private SequenceType type;
        [JsonProperty]
        public SequenceType Type
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


        public void InsertMany(List<AnimationElement> animationElements)
        {
            foreach (var e in animationElements)
                InsertElement(e);
        }
        public void InsertElement(AnimationElement animationElement)
        {
            animationElement.Target = Target;
            animationElement.ParentSequence = this;
            AnimationElements.Add(animationElement);
            FixOrders();
            ChangeFXBehaviorWhenIdentityElementPresent();
        }
        public void InsertElementAfter(AnimationElement toInsert, AnimationElement insertAfter)
        {
            if (insertAfter.ParentSequence == this)
            {
                if (toInsert.ParentSequence != insertAfter.ParentSequence)
                {
                    toInsert.ParentSequence?.RemoveElement(toInsert);
                }
                InsertAfter(toInsert, insertAfter);
            }
            else
            {
                throw new ArgumentException(
                    "the target elements parent does not match the parent you are trying to add to");
            }
        }

        private void InsertAfter(AnimationElement toInsert, AnimationElement insertAfter)
        {
            var existingIndex = AnimationElements.IndexOf(toInsert);
            var index = AnimationElements.IndexOf(insertAfter);

            if (index >= 0)
            {
                var position = index + 1;
                if (existingIndex >= 0)
                {
                    AnimationElements.RemoveAt(existingIndex);
                    if (index > 0)
                        position = index;
                }
                AnimationElements.Insert(position, toInsert);
                toInsert.ParentSequence = this;
            }
            FixOrders();
            ChangeFXBehaviorWhenIdentityElementPresent();
        }
        public void InsertElement(AnimationElement animationElement, int index)
        {
            var existingIndex = AnimationElements.IndexOf(animationElement);
            if (existingIndex >= 0)
            {
                AnimationElements.RemoveAt(existingIndex);
                if (index > 0 && index >= AnimationElements.Count)
                    index -= 1;
            }
            AnimationElements.Insert(index, animationElement);
            animationElement.ParentSequence = this;
            FixOrders();
            ChangeFXBehaviorWhenIdentityElementPresent();
        }

        public void RemoveElement(AnimationElement animationElement)
        {
            if (AnimationElements.Contains(animationElement))
                AnimationElements.Remove(animationElement);
            else
            { // In case where a cloned object matches all the properties
                var animElement = AnimationElements.FirstOrDefault(ae => ae.Name == animationElement.Name);
                if (animElement != null)
                    AnimationElements.Remove(animElement);
            }
            //FixOrders(); // don't need to reset orders actually. Plus it helps to differentiate between two otherwise identical elements - one just deleted and one right after it that was cloned from the deleted one
            ChangeFXBehaviorWhenIdentityElementPresent();
        }
        private void ChangeFXBehaviorWhenIdentityElementPresent()
        {
            var animationElements = GetFlattenedAnimationList(this.AnimationElements);

            foreach (FXElement fxElement in animationElements.Where(e => e is FXElement))
            {
                fxElement.OverridingCostumeName = null;
                if (animationElements.Any(e => e.AnimationElementType == AnimationElementType.LoadIdentity))
                {
                    int fxIndex = animationElements.IndexOf(fxElement);
                    var identityElement = animationElements.LastOrDefault(e => e.AnimationElementType == AnimationElementType.LoadIdentity && animationElements.IndexOf(e) < fxIndex) as LoadIdentityElement;
                    if (identityElement != null)
                    {
                        int identityIndex = animationElements.IndexOf(identityElement);
                        if (!animationElements.Any(e => e.AnimationElementType == AnimationElementType.FX && animationElements.IndexOf(e) < fxIndex && animationElements.IndexOf(e) > identityIndex))
                        {
                            fxElement.OverridingCostumeName = identityElement.Reference.Identity.Surface;
                        }
                    }
                }
            }
        }
        public override void Play(List<AnimatedCharacter> targets)
        {
            if (Type == SequenceType.And)
            {
                foreach (var e in from e in AnimationElements orderby e.Order select e)
                    e.Play(targets);
            }
            else
            {
                AnimationElement randomElement = getRandomElement(this);
                randomElement.Play(targets);
            }
        }
        
        public override void PlayResource(AnimatedCharacter target)
        {
            if (Type == SequenceType.And)
            {
                foreach (var e in from e in AnimationElements orderby e.Order select e)
                    e.Play(target);
            }
            else
            {
                AnimationElement randomElement = getRandomElement(this);
                randomElement.Play(target);
            }
        }
        public override void StopResource(AnimatedCharacter target)
        {
            // to do CHECK FOR PERSISTENT SHIT
            foreach (var e in from e in AnimationElements orderby e.Order select e)
                e.Stop(target);
        }

        public void Play(List<AnimatedAbility> abilities)
        {
            List<AnimationElement> flattenedElements = new List<AnimationElement>();
            foreach (AnimatedAbility ability in abilities)
            {
                var clonedAbility = ability.Clone(ability.Target);
                List<AnimationElement> animationElements = getAnimationListFromAbility(clonedAbility);
                flattenedElements.AddRange(animationElements);
            }
            var lastElement = flattenedElements.Last();
            for(int i = 0; i < flattenedElements.Count; i++)
            {
                // Every mov or fx element is chained with next element unless it is the last or-
                // The next element is not a mov or fx
                AnimationElement currentElement = flattenedElements[i];
                if (i == flattenedElements.Count - 1)
                    currentElement.PlayWithNext = false;
                else
                {
                    if(currentElement is MovElement || currentElement is FXElement)
                    {
                        AnimationElement nextElement = flattenedElements[i + 1];
                        if (nextElement is MovElement || nextElement is FXElement)
                            currentElement.PlayWithNext = true;
                    }
                }
            }

            foreach (AnimationElement element in flattenedElements)
                element.Play();

        }

        private List<AnimationElement> getAnimationListFromAbility(AnimatedAbility ability)
        {
            List<AnimationElement> flattenedElements = null;
            if (ability.Type == SequenceType.And)
                flattenedElements = GetFlattenedAnimationListEligibleForPlay(ability.AnimationElements);
            else
            {
                AnimationElement randomElement = getRandomElement(ability);
                List<AnimationElement> listToFlatten = new List<AnimationElement> { randomElement };
                flattenedElements = GetFlattenedAnimationListEligibleForPlay(listToFlatten);
            }

            flattenedElements.ForEach(fe => fe.Target = ability.Target);

            return flattenedElements;
        }

        private static AnimationElement getRandomElement(AnimationSequencer sequencer)
        {
            var rnd = new Random();
            var chosen = rnd.Next(0, sequencer.AnimationElements.Count);
            return sequencer.AnimationElements[chosen];
        }

        public override bool Equals(object other)
        {
            if (other is AnimationSequencerImpl == false)
                return false;
            var otherSequence = other as AnimationSequencer;
            if (otherSequence.Type != Type) return false;
            if (otherSequence.AnimationElements.Count != AnimationElements.Count) return false;
            foreach (var otherElement in otherSequence.AnimationElements)
            {
                var match = false;
                foreach (var originalElement in AnimationElements)
                    if (otherElement.Equals(originalElement))
                        match = true;
                if (match == false)
                    return false;
            }
            return true;
        }
        public override AnimationElement Clone(AnimatedCharacter target)
        {
            AnimationSequencer clone = new AnimationSequencerImpl(target);
            clone = (AnimationSequencer)cloneBaseAttributes(clone as AnimationElement);
            clone.Type = Type;
            foreach (var element in AnimationElements)
                clone.InsertElement(element.Clone(target));
            return clone as AnimationElement;
        }

        public AnimationElement GetNewAnimationElement(AnimationElementType animationElementType)
        {
            AnimationElement animationElement = null;
            List<AnimationElement> flattenedList = GetFlattenedAnimationList(AnimationElements.ToList());
            string fullName = "";
            switch (animationElementType)
            {
                case AnimationElementType.Mov:
                    animationElement = new MovElementImpl(this.Target, null);
                    MovResource movResource = MovElementImpl.LastMov;
                    fullName = GetAppropriateAnimationName(AnimationElementType.Mov, flattenedList);
                    if (movResource == null)
                    {
                        animationElement.Name = fullName;
                    }
                    else
                    {
                        (animationElement as MovElement).Mov = movResource;
                        animationElement.Name = Path.GetFileNameWithoutExtension(movResource.FullResourcePath);
                    }

                    break;
                case AnimationElementType.FX:
                    animationElement = new FXElementImpl(this.Target, null);
                    FXResource fxResource = FXElementImpl.LastFX;
                    fullName = GetAppropriateAnimationName(AnimationElementType.FX, flattenedList);
                    if (fxResource == null)
                    {
                        animationElement.Name = fullName;
                    }
                    else
                    {
                        (animationElement as FXElement).FX = fxResource;
                        animationElement.Name = Path.GetFileNameWithoutExtension(fxResource.Name);
                    }
                    break;
                case AnimationElementType.Sound:
                    animationElement = new SoundElementImpl(this.Target, null);
                    SoundResource soundResource = SoundElementImpl.LastSound;
                    fullName = GetAppropriateAnimationName(AnimationElementType.Sound, flattenedList);
                    if (soundResource == null)
                    {
                        animationElement.Name = fullName;
                    }
                    else
                    {
                        (animationElement as SoundElement).Sound = soundResource;
                        animationElement.Name = Path.GetFileNameWithoutExtension(soundResource.FullResourcePath);
                    }
                    break;
                case AnimationElementType.Sequence:
                    animationElement = new SequenceElementImpl(Target);
                    (animationElement as SequenceElement).Type = SequenceType.And;
                    animationElement.Name = "Sequence: " + (animationElement as SequenceElement).Type.ToString();
                    break;
                case AnimationElementType.Pause:
                    animationElement = new PauseElementImpl();
                    (animationElement as PauseElement).Duration = 1;
                    animationElement.Target = this.Target;
                    animationElement.Name = "Pause " + (animationElement as PauseElement).Duration.ToString();
                    break;
                case AnimationElementType.Reference:
                    animationElement = new ReferenceElementImpl();
                    ReferenceResource refResource = ReferenceElementImpl.LastReference;
                    fullName = GetAppropriateAnimationName(AnimationElementType.Reference, flattenedList);
                    if (refResource == null)
                    {
                        animationElement.Name = fullName;
                    }
                    else
                    {
                        (animationElement as ReferenceElement).Reference = refResource;
                        animationElement.Name = refResource.Ability.Name;
                    }
                    break;
                case AnimationElementType.LoadIdentity:
                    animationElement = new LoadIdentityElementImpl();
                    IdentityResource idResource = LoadIdentityElementImpl.LastIdentityReference;
                    fullName = GetAppropriateAnimationName(AnimationElementType.LoadIdentity, flattenedList);
                    if (idResource == null || idResource.Identity == null || idResource.Identity.Owner != this.Target)
                        animationElement.Name = fullName;
                    else
                    {
                        (animationElement as LoadIdentityElement).Reference = idResource;
                        animationElement.Name = idResource.Identity.Name;
                    }
                    break;
            }

            return animationElement;
        }

        public static List<AnimationElement> GetFlattenedAnimationList(IEnumerable<AnimationElement> animationElementList)
        {
            List<AnimationElement> _list = new List<AnimationElement>();
            foreach (AnimationElement animationElement in animationElementList)
            {
                if (animationElement is SequenceElement)
                {
                    SequenceElement sequenceElement = (animationElement as SequenceElement);
                    if (sequenceElement.AnimationElements != null && sequenceElement.AnimationElements.Count > 0)
                        _list.AddRange(GetFlattenedAnimationList(sequenceElement.AnimationElements.ToList()));
                }
                else if (animationElement is ReferenceElement)
                {
                    ReferenceElement refElement = (animationElement as ReferenceElement);
                    if (refElement.Reference?.Ability?.AnimationElements?.Count > 0)
                    {
                        _list.AddRange(GetFlattenedAnimationList(refElement.Reference.Ability.AnimationElements));
                    }
                }
                _list.Add(animationElement);
            }
            return _list;
        }

        public static List<AnimationElement> GetFlattenedAnimationListEligibleForPlay(IEnumerable<AnimationElement> animationElementList)
        {
            List<AnimationElement> _list = new List<AnimationElement>();
            foreach (AnimationElement animationElement in animationElementList)
            {
                if (animationElement is SequenceElement)
                {
                    SequenceElement sequenceElement = (animationElement as SequenceElement);
                    if (sequenceElement.AnimationElements != null && sequenceElement.AnimationElements.Count > 0)
                    {
                        if(sequenceElement.Type == SequenceType.Or)
                        {
                            AnimationElement chosenElement = getRandomElement(sequenceElement);
                            List<AnimationElement> listToFlatten = new List<AnimationElement> { chosenElement };
                            _list.AddRange(GetFlattenedAnimationListEligibleForPlay(listToFlatten));
                        }
                        else
                            _list.AddRange(GetFlattenedAnimationListEligibleForPlay(sequenceElement.AnimationElements.ToList()));
                    }
                }
                else if (animationElement is ReferenceElement)
                {
                    ReferenceElement refElement = (animationElement as ReferenceElement);
                    if (refElement.Reference?.Ability?.AnimationElements?.Count > 0)
                    {
                        _list.AddRange(GetFlattenedAnimationListEligibleForPlay(refElement.Reference.Ability.AnimationElements));
                    }
                }
                _list.Add(animationElement);
            }
            return _list;
        }

        public static string GetAppropriateAnimationName(AnimationElementType animationType, List<AnimationElement> collection)
        {
            string name = "";
            switch (animationType)
            {
                case AnimationElementType.Mov:
                    name = "Mov Element";
                    break;
                case AnimationElementType.FX:
                    name = "FX Element";
                    break;
                case AnimationElementType.Pause:
                    name = "Pause Element";
                    break;
                case AnimationElementType.Sequence:
                    name = "Seq Element";
                    break;
                case AnimationElementType.Sound:
                    name = "Sound Element";
                    break;
                case AnimationElementType.Reference:
                    name = "Ref Element";
                    break;
                case AnimationElementType.LoadIdentity:
                    name = "Identity Element";
                    break;
            }

            string suffix = " 1";
            string rootName = name;
            int i = 1;
            Regex reg = new Regex(@"\d+");
            MatchCollection matches = reg.Matches(name);
            if (matches.Count > 0)
            {
                int k;
                Match match = matches[matches.Count - 1];
                if (int.TryParse(match.Value.Substring(1, match.Value.Length - 2), out k))
                {
                    i = k + 1;
                    suffix = string.Format(" {0}", i);
                    rootName = name.Substring(0, match.Index).TrimEnd();
                }
            }
            string newName = rootName + suffix;
            while (collection.Where(a => a.Name == newName).FirstOrDefault() != null)
            {
                suffix = string.Format(" {0}", ++i);
                newName = rootName + suffix;
            }
            return newName;
        }

        private void FixOrders()
        {
            for (int i = 0; i < AnimationElements.Count; i++)
                AnimationElements[i].Order = i + 1;
        }

    }

    public class ReferenceElementImpl : AnimationElementImpl, ReferenceElement, AnimationSequencer
    {
        public ReferenceElementImpl()
        {
            this.AnimationElementType = AnimationElementType.Reference;
        }

        public ObservableCollection<AnimationElement> AnimationElements => Reference?.Ability?.AnimationElements;

        public SequenceType Type
        {
            get { return Reference != null && Reference.Ability != null ? Reference.Ability.Type : SequenceType.And; }

            set
            {
                if (Reference != null && Reference.Ability != null)
                    Reference.Ability.Type = value;
                NotifyOfPropertyChange(() => Type);
            }
        }


        public void InsertMany(List<AnimationElement> elements)
        {
            Reference?.Ability?.InsertMany(elements);
        }

        public void InsertElement(AnimationElement animationElement)
        {
            Reference?.Ability?.InsertElement(animationElement);
        }

        public void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter)
        {
            Reference?.Ability?.InsertElementAfter(toInsert, moveAfter);
        }

        public void RemoveElement(AnimationElement animationElement)
        {
            Reference?.Ability?.RemoveElement(animationElement);
        }


        private ReferenceResource reference;
        [JsonProperty]
        public ReferenceResource Reference
        {
            get
            {
                return reference;
            }
            set
            {
                reference = value;
                NotifyOfPropertyChange(() => Reference);
            }
        }

        public static ReferenceResource LastReference { get; set; }

        public override void Play(List<AnimatedCharacter> targets)
        {
            Reference?.Ability?.Play(targets);
        }

        public override void StopResource(AnimatedCharacter target)
        {
            Reference?.Ability?.Stop(target);
        }

        public AnimationElement GetNewAnimationElement(AnimationElementType animationElementType)
        {
            return Reference?.Ability?.GetNewAnimationElement(animationElementType);
        }

        public SequenceElement Copy(AnimatedCharacter target)
        {
            var clonedSequence = (Reference?.Ability?.Sequencer as AnimationSequencerImpl)?.Clone(target) as AnimationSequencer;

            SequenceElement clone = new SequenceElementImpl(clonedSequence, target);
            clone = (SequenceElement)cloneBaseAttributes(clone);
            clone.Name = "Sequence: " + clone.Type.ToString();

            return clone;
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            ReferenceElement refElement = new ReferenceElementImpl();
            refElement = (ReferenceElement)cloneBaseAttributes(refElement);
            refElement.Reference = Reference;
            refElement.Target = target;

            return refElement;
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            Reference?.Ability?.Play(target);
        }

        public override bool Equals(object other)
        {
            if (other is ReferenceElementImpl == false)
                return false;
            var r = other as ReferenceElement;
            if (baseAttributesEqual(r) != true) return false;
            if (Reference != null && r.Reference != null && r.Reference.Equals(Reference) == false) return false;
            return true;
        }

        public void Play(List<AnimatedAbility> abilities)
        {
            this.Reference?.Ability?.Play(abilities);
        }
    }

    public class LoadIdentityElementImpl : AnimationElementImpl, LoadIdentityElement
    {

        private IdentityResource identity;
        [JsonProperty]
        public IdentityResource Reference
        {
            get
            {
                return identity;
            }
            set
            {
                identity = value;
                NotifyOfPropertyChange(() => Reference);
            }
        }
        public static IdentityResource LastIdentityReference { get; set; }
        public LoadIdentityElementImpl(AnimatedCharacter owner, IdentityResource identity): base(owner)
        {
            this.Reference = identity;
            this.AnimationElementType = AnimationElementType.LoadIdentity;
        }

        public LoadIdentityElementImpl() : this(null, null)
        {

        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            LoadIdentityElement clonedElement = new LoadIdentityElementImpl(target, this.Reference);
            clonedElement.Name = this.Name;
            return clonedElement;
        }

        public override void Play(List<AnimatedCharacter> targets)
        {
            completeEvent = false;
            foreach (var target in targets)
            {
                target.Target(false);
                PlayResource(target);
            }
            completeEvent = true;
            var firstOrDefault = targets.FirstOrDefault();
            firstOrDefault?.Generator.CompleteEvent();
        }

        public override void StopResource(AnimatedCharacter target)
        {
            var targetToPlay = target ?? this.Target;
            targetToPlay.Target(false);
            target.ActiveIdentity.Play();
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            var targetToPlay = target ?? this.Target;
            var generator = targetToPlay.Generator;
            if (this.Reference != null)
            {
                targetToPlay.Target(false);
                this.Reference.Identity.Play(false);
            }
            if (completeEvent)
                generator.CompleteEvent();
        }
    }

    public class AnimatedResourceImpl : PropertyChangedBase, AnimatedResource
    {
        private string name;
        [JsonProperty]
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

        private string tag;
        [JsonProperty]
        public string Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
                NotifyOfPropertyChange(() => Tag);
            }
        }

        public static bool operator ==(AnimatedResourceImpl a, AnimatedResourceImpl b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            return a.Tag == b.Tag && a.Name == b.Name;
        }

        public static bool operator !=(AnimatedResourceImpl a, AnimatedResourceImpl b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            bool areEqual = false;
            if (obj is AnimatedResourceImpl)
                areEqual = this == (AnimatedResourceImpl)obj;
            return areEqual;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class MovResourceImpl : AnimatedResourceImpl, MovResource
    {
        private string fullResourcePath;
        [JsonProperty]
        public string FullResourcePath
        {
            get
            {
                return fullResourcePath;
            }

            set
            {
                fullResourcePath = value;
                NotifyOfPropertyChange(() => FullResourcePath);
            }
        }
    }

    public class FXResourceImpl : AnimatedResourceImpl, FXResource
    {
        private string fullResourcePath;
        [JsonProperty]
        public string FullResourcePath
        {
            get
            {
                return fullResourcePath;
            }

            set
            {
                fullResourcePath = value;
                NotifyOfPropertyChange(() => FullResourcePath);
            }
        }
    }

    public class SoundResourceImpl : AnimatedResourceImpl, SoundResource
    {
        private string fullResourcePath;
        [JsonProperty]
        public string FullResourcePath
        {
            get
            {
                return fullResourcePath;
            }

            set
            {
                fullResourcePath = value;
                NotifyOfPropertyChange(() => FullResourcePath);
            }
        }
    }

    public class IdentityResourceImpl : AnimatedResourceImpl, IdentityResource
    {
        private Identity identity;
        [JsonProperty]
        public Identity Identity
        {
            get
            {
                return identity;
            }

            set
            {
                identity = value;
                NotifyOfPropertyChange(() => Identity);
            }
        }
    }

    public class ReferenceResourceImpl : PropertyChangedBase, ReferenceResource
    {
        private AnimatedAbility ability;
        [JsonProperty]
        public AnimatedAbility Ability
        {
            get
            {
                return ability;
            }

            set
            {
                ability = value;
                NotifyOfPropertyChange(() => Ability);
            }
        }

        private AnimatedCharacter character;
        [JsonProperty]
        public AnimatedCharacter Character
        {
            get
            {
                return character;
            }

            set
            {
                character = value;
                NotifyOfPropertyChange(() => Character);
            }
        }
        public static bool operator ==(ReferenceResourceImpl a, ReferenceResourceImpl b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            return a.Character == b.Character && a.Ability == b.Ability;
        }

        public static bool operator !=(ReferenceResourceImpl a, ReferenceResourceImpl b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            bool areEqual = false;
            if (obj is ReferenceResourceImpl)
                areEqual = this == (ReferenceResourceImpl)obj;
            return areEqual;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class AnimatedResourceManagerImpl : PropertyChangedBase, AnimatedResourceManager
    {
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_SOUND_FOLDERNAME = "sound";
        private const string GAME_MOVE_REPOSITORY_FILENAME = "MovRepository.data";
        private const string GAME_FX_REPOSITORY_FILENAME = "FXRepository.data";
        private const string GAME_SOUND_REPOSITORY_FILENAME = "SoundRepository.data";
        private string movRepositoryPath;
        private string fxRepositoryPath;
        private string soundRepositoryPath;
        private string gameDirectory;
        public string GameDirectory
        {
            get
            {
                return gameDirectory;
            }
            set
            {
                gameDirectory = value;
                this.movRepositoryPath = Path.Combine(gameDirectory, GAME_DATA_FOLDERNAME, GAME_MOVE_REPOSITORY_FILENAME);
                this.fxRepositoryPath = Path.Combine(gameDirectory, GAME_DATA_FOLDERNAME, GAME_FX_REPOSITORY_FILENAME);
                this.soundRepositoryPath = Path.Combine(gameDirectory, GAME_DATA_FOLDERNAME, GAME_SOUND_REPOSITORY_FILENAME);
            }
        }

        public CrowdRepository CrowdRepository { get; set; }

        private ObservableCollection<FXResource> fxElements;
        public ObservableCollection<FXResource> FXElements
        {
            get
            {
                if (fxElements == null)
                    fxElements = GetFXResources();
                return fxElements;
            }

            set
            {
                fxElements = value;
                NotifyOfPropertyChange(() => FXElements);
            }
        }

        private ObservableCollection<MovResource> movElements;
        public ObservableCollection<MovResource> MovElements
        {
            get
            {
                if (movElements == null)
                    movElements = GetMovResources();
                return movElements;
            }

            set
            {
                movElements = value;
                NotifyOfPropertyChange(() => MovElements);
            }
        }

        private ObservableCollection<ReferenceResource> referenceElements;
        public ObservableCollection<ReferenceResource> ReferenceElements
        {
            get
            {
                if (referenceElements == null)
                    referenceElements = GetReferenceResources();
                return referenceElements;
            }

            set
            {
                referenceElements = value;
                NotifyOfPropertyChange(() => ReferenceElements);
            }
        }

        private ObservableCollection<SoundResource> soundElements;
        public ObservableCollection<SoundResource> SoundElements
        {
            get
            {
                if (soundElements == null)
                    soundElements = GetSoundResources();
                return soundElements;
            }

            set
            {
                soundElements = value;
                NotifyOfPropertyChange(() => SoundElements);
            }
        }

        private ObservableCollection<IdentityResource> identityElements;
        public ObservableCollection<IdentityResource> IdentityElements
        {
            get
            {
                if (identityElements == null)
                    identityElements = GetIdentityResources();
                return identityElements;
            }
            set
            {
                identityElements = value;
                NotifyOfPropertyChange(() => IdentityElements);
            }
        }

        private CollectionViewSource movResourcesCVS;
        public CollectionViewSource MOVResourcesCVS
        {
            get
            {
                return movResourcesCVS;
            }
            set
            {
                movResourcesCVS = value;
                NotifyOfPropertyChange(() => MOVResourcesCVS);
            }
        }

        private CollectionViewSource fxResourcesCVS;
        public CollectionViewSource FXResourcesCVS
        {
            get
            {
                return fxResourcesCVS;
            }
            set
            {
                fxResourcesCVS = value;
                NotifyOfPropertyChange(() => FXResourcesCVS);
            }
        }

        private CollectionViewSource soundResourcesCVS;
        public CollectionViewSource SoundResourcesCVS
        {
            get
            {
                return soundResourcesCVS;
            }
            set
            {
                soundResourcesCVS = value;
                NotifyOfPropertyChange(() => SoundResourcesCVS);
            }
        }

        private CollectionViewSource referenceElementsCVS;
        public CollectionViewSource ReferenceElementsCVS
        {
            get
            {
                return referenceElementsCVS;
            }
            set
            {
                referenceElementsCVS = value;
                NotifyOfPropertyChange(() => ReferenceElementsCVS);
            }
        }

        private CollectionViewSource identityElementsCVS;
        public CollectionViewSource IdentityElementsCVS
        {
            get
            {
                return identityElementsCVS;
            }
            set
            {
                identityElementsCVS = value;
                NotifyOfPropertyChange(() => IdentityElementsCVS);
            }
        }

        private AnimatedAbility currentAbility;
        public AnimatedAbility CurrentAbility
        {
            get
            {
                return currentAbility;
            }
            set
            {
                currentAbility = value;
                NotifyOfPropertyChange(() => CurrentAbility);
            }
        }

        private AnimationElement currentAnimationElement;
        public AnimationElement CurrentAnimationElement
        {
            get
            {
                return currentAnimationElement;
            }

            set
            {
                currentAnimationElement = value;
                NotifyOfPropertyChange(() => CurrentAnimationElement);
            }
        }
        private string filter;
        public string Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
                if (CurrentAnimationElement != null)
                {
                    if (value.Length > 2 || value.Length == 0)
                    {
                        switch (CurrentAnimationElement.AnimationElementType)
                        {
                            case AnimationElementType.Mov:
                                //Helper.SaveUISettings("Ability_MoveFilter", value);
                                MOVResourcesCVS.View.Refresh();
                                break;
                            case AnimationElementType.FX:
                                //Helper.SaveUISettings("Ability_FxFilter", value);
                                FXResourcesCVS.View.Refresh();
                                break;
                            case AnimationElementType.Sound:
                                //Helper.SaveUISettings("Ability_SoundFilter", value);
                                SoundResourcesCVS.View.Refresh();
                                break;
                            case AnimationElementType.Reference:
                                //Helper.SaveUISettings("Ability_ReferenceFilter", value);
                                ReferenceElementsCVS.View.Refresh();
                                break;
                            case AnimationElementType.LoadIdentity:
                                IdentityElementsCVS.View.Refresh();
                                break;
                        }
                    }
                }

                NotifyOfPropertyChange(() => Filter);
            }
        }

        public ObservableCollection<MovResource> GetMovResources()
        {
            if (string.IsNullOrEmpty(GameDirectory))
                throw new Exception("Game Directory Not Set!");
            var movResources = CommonLibrary.GetDeserializedJSONFromFile<ObservableCollection<MovResource>>(movRepositoryPath);

            if (movResources == null || movResources.Count == 0)
            {
                movResources = new ObservableCollection<MovResource>();
                Assembly assembly = Assembly.GetExecutingAssembly();

                string resName = "HeroVirtualTabletop.AnimatedAbility.MOVElements.csv";
                using (StreamReader Sr = new StreamReader(assembly.GetManifestResourceStream(resName)))
                {
                    while (!Sr.EndOfStream)
                    {
                        string resLine = Sr.ReadLine();
                        string[] resArray = resLine.Split(';');
                        movResources.Add(new MovResourceImpl { FullResourcePath = resArray[1], Name = Path.GetFileNameWithoutExtension(resArray[1]), Tag = resArray[0] });
                    }
                }
                movResources = new ObservableCollection<MovResource>(movResources.OrderBy(x => x, new AnimatedResourceComparer()));
                SaveMoveResources(movResources);
            }

            return movResources;
        }
        public void SaveMoveResources(ObservableCollection<MovResource> movResources)
        {
            CommonLibrary.SerializeObjectAsJSONToFile(movRepositoryPath, movResources);
        }

        public ObservableCollection<FXResource> GetFXResources()
        {
            if (string.IsNullOrEmpty(GameDirectory))
                throw new Exception("Game Directory Not Set!");
            var fxResources = CommonLibrary.GetDeserializedJSONFromFile<ObservableCollection<FXResource>>(fxRepositoryPath);
            if (fxResources == null || fxResources.Count == 0)
            {
                fxResources = new ObservableCollection<FXResource>();
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resName = "HeroVirtualTabletop.AnimatedAbility.FXElements.csv";
                using (StreamReader Sr = new StreamReader(assembly.GetManifestResourceStream(resName)))
                {
                    while (!Sr.EndOfStream)
                    {
                        string resLine = Sr.ReadLine();
                        string[] resArray = resLine.Split(';');
                        fxResources.Add(new FXResourceImpl { FullResourcePath = resArray[2], Name = resArray[1], Tag = resArray[0] });
                    }
                }
                fxResources = new ObservableCollection<FXResource>(fxResources.OrderBy(x => x, new AnimatedResourceComparer()));
                SaveFXResources(fxResources);
            }

            return fxResources;
        }
        public void SaveFXResources(ObservableCollection<FXResource> fxResources)
        {
            CommonLibrary.SerializeObjectAsJSONToFile(fxRepositoryPath, fxResources);
        }

        public ObservableCollection<SoundResource> GetSoundResources()
        {
            if (string.IsNullOrEmpty(GameDirectory))
                throw new Exception("Game Directory Not Set!");
            var soundResources = CommonLibrary.GetDeserializedJSONFromFile<ObservableCollection<SoundResource>>(soundRepositoryPath);
            if (soundResources == null || soundResources.Count == 0)
            {
                soundResources = new ObservableCollection<SoundResource>();
                string soundDir = Path.Combine(GameDirectory, GAME_SOUND_FOLDERNAME);
                if (!Directory.Exists(soundDir))
                {
                    Directory.CreateDirectory(soundDir);
                }
                var soundFiles = Directory.EnumerateFiles
                            (soundDir,
                            "*.ogg", SearchOption.AllDirectories);//.OrderBy(x => { return Path.GetFileNameWithoutExtension(x); });

                foreach (string file in soundFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string resourceVal = file.Substring(soundDir.Length);
                    string[] tmpTags = file.Substring(soundDir.Length).Split('\\').Where((s) =>
                    {
                        return !string.IsNullOrWhiteSpace(s);
                    }).ToArray();
                    string[] tags = new string[1];

                    string sound = tmpTags[tmpTags.Length - 1];

                    string tag = tmpTags.Length >= 2 ? tmpTags[tmpTags.Length - 2] : "Sound";
                    tag = tag[0].ToString().ToUpper() + tag.Substring(1);

                    Regex re = new Regex(@"_{1}");
                    if (!re.IsMatch(sound, 1))
                        re = new Regex(@"[A-Z,0-9,\-]{1}");
                    string tmp;
                    if (re.IsMatch(sound, 1))
                    {
                        tmp = sound.Substring(0, re.Match(sound, 1).Index);
                        tmp = tmp[0].ToString().ToUpper() + tmp.Substring(1);
                        tag += tmp;
                    }

                    tags[0] = tag;
                    soundResources.Add(new SoundResourceImpl { FullResourcePath = resourceVal, Name = name, Tag = tag });
                }
                soundResources = new ObservableCollection<SoundResource>(soundResources.OrderBy(x => x, new AnimatedResourceComparer()));
            }

            SaveSoundResources(soundResources);

            return soundResources;
        }

        public void SaveSoundResources(ObservableCollection<SoundResource> soundResources)
        {
            CommonLibrary.SerializeObjectAsJSONToFile(soundRepositoryPath, soundResources);
        }

        public ObservableCollection<ReferenceResource> GetReferenceResources()
        {
            var abilityCollection = this.CrowdRepository.AllMembersCrowd.Members.Where(m => m is CharacterCrowdMember).SelectMany((member) =>
            {
                return (member as CharacterCrowdMember).Abilities;
            }).Distinct();
            
            var referenceResources = abilityCollection
                .Select(x =>
            {
                return new ReferenceResourceImpl { Ability = x, Character = x.Target };
            }).ToList();
            foreach (var attackObj in abilityCollection.Where(a => a is AnimatedAttack))
            {
                AnimatedAttack attack = attackObj as AnimatedAttack;
                attack.OnHitAnimation.Name = attack.Name + " - OnHit";
                if (attack.OnHitAnimation.Owner == null)
                {
                    attack.OnHitAnimation.Owner = attack.Owner;
                }
                referenceResources.Add(new ReferenceResourceImpl { Ability = attack.OnHitAnimation, Character = attack.OnHitAnimation.Owner as AnimatedCharacter });
            }
            referenceResources = referenceResources.Where(x => !(x.Ability != null && (x.Ability.AnimationElements.Count == 0 || (x.Ability.AnimationElements.Count == 1 && x.Ability.AnimationElements[0] is ReferenceElement)))).ToList();

            referenceResources = referenceResources.OrderBy(x => x, new ReferenceResourceComparer()).ToList();
            var referenceResourceCollection = new ObservableCollection<ReferenceResource>(referenceResources);

            return referenceResourceCollection;
        }

        public ObservableCollection<IdentityResource> GetIdentityResources()
        {
            ObservableCollection<IdentityResource> identityResources = null;
            if(this.CurrentAbility != null && this.CurrentAbility.Owner != null)
            {
                var currentOwner = this.CurrentAbility.Owner as AnimatedCharacter;
                List<IdentityResource> identityCollection = new List<IdentityResource>(currentOwner.Identities.Select(x =>
                {
                    return new IdentityResourceImpl { Identity = x as Identity, Name = x.Name, Tag = "" };
                }));
                identityCollection = identityCollection.OrderBy(x => x, new IdentityResourceComparer()).ToList();
                identityResources = new ObservableCollection<IdentityResource>(identityCollection);
            }

            return identityResources;
        }

        #region Load Resources
        public void LoadResources()
        {
            MOVResourcesCVS = new CollectionViewSource();
            MOVResourcesCVS.Source = this.MovElements;
            MOVResourcesCVS.View.Filter += AnimatedResourceCVS_Filter;
            NotifyOfPropertyChange(() => MOVResourcesCVS);

            FXResourcesCVS = new CollectionViewSource();
            FXResourcesCVS.Source = this.FXElements;
            FXResourcesCVS.View.Filter += AnimatedResourceCVS_Filter;
            NotifyOfPropertyChange(() => FXResourcesCVS);

            SoundResourcesCVS = new CollectionViewSource();
            SoundResourcesCVS.Source = this.SoundElements;
            SoundResourcesCVS.View.Filter += AnimatedResourceCVS_Filter;
            NotifyOfPropertyChange(() => SoundResourcesCVS);

            LoadReferenceResource();
            LoadIdentityResource();
        }

        public void LoadReferenceResource()
        {
            this.referenceElements = null; // to force reload
            referenceElementsCVS = new CollectionViewSource();
            referenceElementsCVS.Source = this.ReferenceElements;
            referenceElementsCVS.View.Filter += ReferenceResourcesCVS_Filter;
            NotifyOfPropertyChange(() => ReferenceElementsCVS);
        }

        public void LoadIdentityResource()
        {
            this.identityElements = null;
            identityElementsCVS = new CollectionViewSource();
            identityElementsCVS.Source = this.IdentityElements;
            if(identityElementsCVS.View != null)
                identityElementsCVS.View.Filter += AnimatedResourceCVS_Filter;
            NotifyOfPropertyChange(() => IdentityElementsCVS);
        }

        private bool AnimatedResourceCVS_Filter(object item)
        {
            AnimatedResource animationRes = item as AnimatedResource;
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }
            if (CurrentAnimationElement != null)
            {
                if (CurrentAnimationElement is MovElement && (CurrentAnimationElement as MovElement).Mov == animationRes)
                    return true;
                else if (CurrentAnimationElement is FXElement && (CurrentAnimationElement as FXElement).FX == animationRes)
                    return true;
                else if (CurrentAnimationElement is SoundElement && (CurrentAnimationElement as SoundElement).Sound == animationRes)
                    return true;
                else if (CurrentAnimationElement is LoadIdentityElement && (CurrentAnimationElement as LoadIdentityElement).Reference == animationRes)
                    return true;
            }
            // Replace non-alphanumeric characters with empty string
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            string filter = rgx.Replace(Filter, "");
            return new Regex(filter, RegexOptions.IgnoreCase).IsMatch(animationRes.Tag) || new Regex(filter, RegexOptions.IgnoreCase).IsMatch(animationRes.Name);
        }

        private bool ReferenceResourcesCVS_Filter(object item)
        {
            ReferenceResource animationRes = item as ReferenceResource;
            if (animationRes.Ability == this.CurrentAbility)
                return false;
            if (animationRes.Ability != null)
            {
                AnimatedAbility reference = animationRes.Ability;
                if (reference != null && reference.AnimationElements != null && reference.AnimationElements.Count > 0)
                {
                    if (reference.AnimationElements.FirstOrDefault(ae => ae is ReferenceElement && (ae as ReferenceElement).Reference == this.CurrentAbility) != null)
                        return false;
                    List<AnimationElement> elementList = this.GetFlattenedAnimationList(reference.AnimationElements.ToList());
                    if (elementList.Where((an) => { return an.AnimationElementType == AnimationElementType.Reference; }).Any((an) => { return (an as ReferenceElement).Reference == this.CurrentAbility; }))
                        return false;
                }
            }
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }
            // Replace non-alphanumeric characters with empty string
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            string filter = rgx.Replace(Filter, "");

            bool caseReferences = false;
            if (animationRes.Ability != null && animationRes.Character != null)
            {
                caseReferences = new Regex(filter, RegexOptions.IgnoreCase).IsMatch(animationRes.Ability.Name) || new Regex(filter, RegexOptions.IgnoreCase).IsMatch(animationRes.Character.Name);
            }
            return caseReferences;
        }
        private List<AnimationElement> GetFlattenedAnimationList(List<AnimationElement> animationElementList)
        {
            List<AnimationElement> _list = new List<AnimationElement>();
            foreach (AnimationElement animationElement in animationElementList)
            {
                if (animationElement is SequenceElement)
                {
                    SequenceElement sequenceElement = (animationElement as SequenceElement);
                    if (sequenceElement.AnimationElements != null && sequenceElement.AnimationElements.Count > 0)
                        _list.AddRange(GetFlattenedAnimationList(sequenceElement.AnimationElements.ToList()));
                }
                _list.Add(animationElement);
            }
            return _list;
        }
        #endregion
    }

    public class AnimatedResourceComparer : IComparer<AnimatedResource>
    {
        public int Compare(AnimatedResource ar1, AnimatedResource ar2)
        {
            string s1 = ar1.Tag;
            string s2 = ar2.Tag;
            if (ar1.Tag == ar2.Tag)
            {
                s1 = ar1.Name;
                s2 = ar2.Name;
            }

            return CommonLibrary.CompareStrings(s1, s2);
        }
    }
    public class ReferenceResourceComparer : IComparer<ReferenceResource>
    {
        public int Compare(ReferenceResource ar1, ReferenceResource ar2)
        {
            string s1 = ar1.Character.Name;
            string s2 = ar2.Character.Name;
            if (s1 == s2)
            {
                s1 = ar1.Ability.Name;
                s2 = ar2.Ability.Name;
            }

            return CommonLibrary.CompareStrings(s1, s2);
        }
    }

    public class IdentityResourceComparer : IComparer<IdentityResource>
    {
        public int Compare(IdentityResource ir1, IdentityResource ir2)
        {
            string s1 = string.Empty;
            string s2 = string.Empty;
            if (ir1.Identity != null && ir2.Identity != null && ir1.Identity.Name == ir2.Identity.Name)
            {
                s1 = ir1.Identity.Surface;
                s2 = ir2.Identity.Surface;
            }

            return CommonLibrary.CompareStrings(s1, s2);
        }
    }
    public class AnimationTypeToAnimationIconTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string iconText = null;
            AnimationElementType animationType = (AnimationElementType)value;
            switch (animationType)
            {
                case AnimationElementType.Mov:
                    iconText = "\uf008";
                    break;
                case AnimationElementType.FX:
                    iconText = "\uf0d0";
                    break;
                case AnimationElementType.Sound:
                    iconText = "\uf001";
                    break;
                case AnimationElementType.Pause:
                    iconText = "\uf04c";
                    break;
                case AnimationElementType.Sequence:
                    iconText = "\uf126";
                    break;
                case AnimationElementType.Reference:
                    iconText = "\uf08e";
                    break;
                case AnimationElementType.LoadIdentity:
                    iconText = "\uf129";
                    break;
            }
            return iconText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}