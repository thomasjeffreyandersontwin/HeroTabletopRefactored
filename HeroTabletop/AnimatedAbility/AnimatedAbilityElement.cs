using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using HeroVirtualTableTop.Desktop;
using IrrKlang;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeroVirtualTableTop.Common;

namespace HeroVirtualTableTop.AnimatedAbility
{
    public abstract class AnimationElementImpl : AnimationElement
    {
        public abstract string Name { get; set; }
        protected bool completeEvent;

        protected AnimationElementImpl(AnimatedCharacter owner)
        {
            Target = owner;
        }

        protected AnimationElementImpl()
        {
        }

        public int Order { get; set; }
        public AnimatedCharacter Target { get; set; }
        public bool PlayWithNext { get; set; }
        public bool Persistent { get; set; }
        public AnimationSequencer ParentSequence { get; set; }

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
        }

        public MovElementImpl()
        {
        }

        public override string Name
        {
            get { return Mov.Name; }
            set { }
        }
        public MovResource Mov { get; set; }

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
            clone = (MovElement) cloneBaseAttributes(clone);
            clone.Target = target;
            clone.Mov = Mov;
            return clone;
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            var generator = target.Generator;
            string[] para = {Mov.FullResourcePath};
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
        public override string Name
        {
            get { return FX.Name; }
            set { }
        }
        public static string COSTUME_DIR = Path.Combine(Settings.Default.CityOfHeroesGameDirectory, "costumes");

        public FXElementImpl(AnimatedCharacter owner, FXResource resource) : base(owner)
        {
            FX = resource;
        }

        public FXElementImpl()
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

        public FXResource FX { get; set; }
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color Color3 { get; set; }
        public Color Color4 { get; set; }

        public Position AttackDirection { get; set; }

        public string CostumeText => File.ReadAllText(CostumeFilePath);

        public string ModifiedCostumeFilePath
        {
            get
            {
                string costumeName;
                if (Target.Identities?.Active != null)
                    costumeName = Target.Name + "_" + Target.Identities.Active.Surface + "_Modified.costume";
                else costumeName = Target.Name + "_Modified.costume";
                return Path.Combine(COSTUME_DIR, costumeName);
            }
        }

        public string CostumeFilePath
        {
            get
            {
                string costume_name;
                if (Target.Identities?.Active != null)
                    costume_name = Target.Identities.Active.Surface + ".costume";
                else costume_name = Target.Name + ".costume";
                return Path.Combine(COSTUME_DIR, costume_name);
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

        public void BuildCostumeFileThatWillPlayFX()
        {
            throw new NotImplementedException();
        }

        public override void StopResource(AnimatedCharacter target)
        {
            removePreviouslyLoadedFX(ModifiedCostumeText);
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            FXElement clone = new FXElementImpl();
            clone = (FXElement) cloneBaseAttributes(clone);
            clone.Target = target;
            clone.FX = FX;
            clone.Color1 = Color1;
            clone.Color2 = Color2;
            clone.Color3 = Color3;
            clone.Color4 = Color4;
            return clone;
        }

        public Position Destination { get; set; }
        public bool IsDirectional { get; set; }

        public override void PlayResource(AnimatedCharacter target)
        {
            if (!File.Exists(CostumeFilePath))
                return;
            if (File.Exists(ModifiedCostumeFilePath) == false)
                File.Copy(CostumeFilePath, ModifiedCostumeFilePath);
            var fileStr = ModifiedCostumeText;

            fileStr = removePreviouslyLoadedFX(fileStr);
            fileStr = addEmptyCostumePart(fileStr);
            fileStr = insertFXIntoCostume(fileStr);

            File.Delete(ModifiedCostumeFilePath);
            File.AppendAllText(ModifiedCostumeFilePath, fileStr);
            loadCostumeWithFxInIt(target);
            target.LoadedFXs?.Add(this);
        }

        private string removePreviouslyLoadedFX(string fileStr)
        {
            if (Target.LoadedFXs != null)
                foreach (var fx in Target.LoadedFXs)
                    if (fx.Persistent != true)
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
            if (IsDirectional)
            {
                para = new string[2];
                para[0] = ModifiedCostumeFilePath;
                para[1] = parseFromDestination();
            }
            else
            {
                para = new string[1];
                para[0] = ModifiedCostumeFilePath;
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
        public override string Name
        {
            get { return Sound.Name; }
            set { }
        }
        public static string SOUND_DIR = Path.Combine(Settings.Default.CityOfHeroesGameDirectory, "sound");

        private bool _active;
        private Timer UpdateSoundPlayingPositionTimer;

        public SoundElementImpl(AnimatedCharacter owner, SoundResource resource) : base(owner)
        {
            Sound = resource;
        }

        public SoundElementImpl()
        {
        }

        public SoundResource Sound { get; set; }

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

        public string SoundFileName => Path.Combine(Settings.Default.CityOfHeroesGameDirectory, SOUND_DIR) + Sound.FullResourcePath;

        public SoundEngineWrapper SoundEngine { get; set; }

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
            clone = (SoundElement) cloneBaseAttributes(clone);
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
        public override string Name
        {
            get { return "Pause " + Duration; }
            set { }
        }
        private PauseBasedOnDistanceManager _distancemanager;

        private int _dur;

        public PauseElementImpl()
        {
            _distancemanager = new PauseBasedOnDistanceManagerImpl(this);
        }

        public PauseBasedOnDistanceManager DistanceDelayManager
        {
            get { return _distancemanager; }
            set
            {
                _distancemanager = value;
                if (_distancemanager.PauseElement != this)
                    _distancemanager.PauseElement = this;
            }
        }

        public int CloseDistanceDelay { get; set; }

        public int Duration
        {
            get
            {
                if (IsUnitPause == false)
                    return _dur;
                var delayManager = new DelayManager(this);
                var distance = Target.Position.DistanceFrom(TargetPosition);
                var delay = (int) delayManager.GetDelayForDistance(distance);
                return delay;
            }
            set { _dur = value; }
        }

        public int LongDistanceDelay { get; set; }
        public int MediumDistanceDelay { get; set; }
        public int ShortDistanceDelay { get; set; }
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
            clone = (PauseElement) cloneBaseAttributes(clone);
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
            //why are we doing this todo
            if (IsUnitPause)
            {
                DistanceDelayManager.Distance = 0d;
                Thread.Sleep((int) DistanceDelayManager.Duration);
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
                var nearestLowerHundredMultiplier = (int) (distance / 100);
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
        public override string Name
        {
            get { return "Sequencer " + Order; }
            set { }
        }
        private AnimationSequencer _sequencer;

        public SequenceElementImpl(AnimationSequencer cloneedSequence)
        {
            _sequencer = cloneedSequence;
        }
        public SequenceElementImpl()
        {
        }

        public List<AnimationElement> AnimationElements => Sequencer.AnimationElements;

        public AnimationSequencer Sequencer => _sequencer ?? (_sequencer = new AnimationSequencerImpl(Target));

        public SequenceType Type
        {
            get { return Sequencer.Type; }
            set { Sequencer.Type = value; }
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

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            var sequencer = (Sequencer as AnimationSequencerImpl)?.Clone(target) as AnimationSequencer;
            var clone = new SequenceElementImpl(sequencer);
            clone = (SequenceElementImpl) cloneBaseAttributes(clone);
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
    }

    public class AnimationSequencerImpl : AnimationElementImpl, AnimationSequencer
    {
        public override string Name
        {
            get { return "Sequencer "+ Order; }
            set { }
        }

        private OrderedCollectionImpl<AnimationElement> _animationElements;
        private OrderedCollectionImpl<AnimationElement> animationCollection => 
            _animationElements ?? (_animationElements =  new OrderedCollectionImpl<AnimationElement>());
            
        public AnimationSequencerImpl(AnimatedCharacter target)
        {
            Target = target;
        }
        
        public SequenceType Type { get; set; }

        public List<AnimationElement> AnimationElements => (from e in animationCollection.Values orderby e.Order select e).ToList();
        public void InsertMany(List<AnimationElement> animationElements)
        {
            foreach (var e in animationElements)
                InsertElement(e);
        }
        public void InsertElement(AnimationElement animationElement)
        {
            animationElement.Target = Target;
            animationElement.ParentSequence = this;
           animationCollection.InsertElement(animationElement);
        }
        public void InsertElementAfter(AnimationElement toInsert, AnimationElement insertAfter)
        {
            if (toInsert.ParentSequence == insertAfter.ParentSequence)
            {
                if (insertAfter.ParentSequence == this)
                {
                    animationCollection.InsertElementAfter( toInsert,  insertAfter);
                }
                else
                {
                    throw new ArgumentException(
                        "the target elements parent does not match the parent you are trying to add to");
                }
            }
            else
            {
                if (insertAfter.ParentSequence == this)
                {
                    toInsert.ParentSequence.RemoveElement(toInsert);
                    animationCollection.InsertElementAfter(toInsert, insertAfter);
                }
                else
                {
                    throw new ArgumentException(
                        "the target elements parent does not match the parent you are trying to add to");
                }
            }
        }
        public void RemoveElement(AnimationElement animationElement)
        {
            animationCollection.RemoveElement(animationElement);
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
                var rnd = new Random();
                var chosen = rnd.Next(0, AnimationElements.Count);
                AnimationElements[chosen].Play(targets);
            }
        }
        public override void PlayResource(AnimatedCharacter target)
        {
            //to do add asyncronous code
            if (Type == SequenceType.And)
            {
                foreach (var e in from e in AnimationElements orderby e.Order select e)
                    e.Play(target);
            }
            else
            {
                var rnd = new Random();
                var chosen = rnd.Next(0, AnimationElements.Count);
                AnimationElements[chosen].Play(target);
            }
        }
        public override void StopResource(AnimatedCharacter target)
        {
            //to do add asyncronous code
            if (Type == SequenceType.And)
            {
                foreach (var e in from e in AnimationElements orderby e.Order select e)
                    e.Stop(target);
            }
            else
            {
                var rnd = new Random();
                var chosen = rnd.Next(0, AnimationElements.Count);
                if (AnimationElements.Count > 0)
                {
                    AnimationElements[chosen].Play(target);
                }
            }
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

    }

    public class ReferenceElementImpl : AnimationElementImpl, ReferenceElement, AnimationSequencer
    {
        public override string Name
        {
            get { return Reference.Name; }
            set { }
        }

        public List<AnimationElement> AnimationElements => Reference.AnimationElements;

        public SequenceType Type
        {
            get { return Reference.Type; }

            set
            {
                if (Reference != null)
                    Reference.Type = value;
            }
        }


        public void InsertMany(List<AnimationElement> elements)
        {
            Reference.InsertMany(elements);
        }

        public void InsertElement(AnimationElement animationElement)
        {
            Reference.InsertElement(animationElement);
        }

        public void InsertElementAfter(AnimationElement toInsert, AnimationElement moveAfter)
        {
            Reference.InsertElementAfter(toInsert, moveAfter);
        }

        public void RemoveElement(AnimationElement animationElement)
        {
            Reference.RemoveElement(animationElement);
        }

        public AnimatedAbility Reference { get; set; }

        public override void Play(List<AnimatedCharacter> targets)
        {
            Reference.Play(targets);
        }

        public override void StopResource(AnimatedCharacter target)
        {
            Reference.Stop(target);
        }

        public SequenceElement Copy(AnimatedCharacter target)
        {
            var clonedSequence = (Reference.Sequencer as AnimationSequencerImpl)?.Clone(target) as AnimationSequencer;

            SequenceElement clone = new SequenceElementImpl(clonedSequence);
            clone = (SequenceElement) cloneBaseAttributes(clone);
            clone.Target = Target;
            return clone;
        }

        public override AnimationElement Clone(AnimatedCharacter target)
        {
            return Copy(target);
        }

        public override void PlayResource(AnimatedCharacter target)
        {
            Reference.Play(target);
        }

        public override bool Equals(object other)
        {
            if (other is ReferenceElementImpl == false)
                return false;
            var r = other as ReferenceElement;
            if (baseAttributesEqual(r) != true) return false;
            if (r.Reference.Equals(Reference) == false) return false;
            return true;
        }
    }
}