using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.ManagedCharacter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.ManagedCharacter
{
    public interface CharacterEditorViewModel
    {
        CharacterCrowdMember EditedCharacter { get; set; }
        ObservableCollection<CharacterActionGroupViewModel> CharacterActionGroups { get; set; }
        CharacterActionGroup SelectedCharacterActionGroup { get; set; }
        void ReOrderActionGroups(int sourceIndex, int targetIndex);
        IEventAggregator EventAggregator { get; set; }
    }
}
