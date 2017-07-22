using System;
using System.Collections.Generic;
using System.Linq;
using HeroVirtualTabletop.Crowd;
using Caliburn.Micro;
using System.Reflection;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public class AnimatedCharacterRepositoryImpl :  PropertyChangedBase, AnimatedCharacterRepository
    {
        public AnimatedCharacterRepositoryImpl()
        {
            Characters = new List<AnimatedCharacter>();
        }

        public Dictionary<string, AnimatedCharacter> CharacterByName
        {
            get { return Characters.ToDictionary(x => x.Name, y => y); }
        }

        public List<AnimatedCharacter> Characters { get; }

        public List<Crowd.Crowd> LoadSystemCrowdWithDefaultCharacter()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            ObservableCollection<Crowd.Crowd> crowdCollection = new ObservableCollection<Crowd.Crowd>();
            string resName = "HeroVirtualTabletop.AnimatedAbility.AnimatedCharacterRepository.data";
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = new StreamReader(assembly.GetManifestResourceStream(resName)))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {

                    serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    serializer.Formatting = Formatting.Indented;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;

                    crowdCollection = serializer.Deserialize<ObservableCollection<Crowd.Crowd>>(reader);
                }
            }

            return crowdCollection.ToList();
        }
    }
}