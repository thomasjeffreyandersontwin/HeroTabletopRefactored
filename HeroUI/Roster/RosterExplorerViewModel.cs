using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Roster
{
    public interface RosterExplorerViewModel
    {
        IEventAggregator EventAggregator { get; set; }
        Roster Roster { get; set; }
        IList SelectedParticipants { get; set; }
        void UpdateRosterSelection();
        void Target();
        void Spawn();
        void SpawnToPosition(Position position);
        void CloneAndSpawn(Position position);
        void ClearFromDesktop();
        void MoveToCamera();
        void SavePosition();
        void Place();
        void ToggleTargeted();
        void ToggleManeuverWithCamera();
        void MoveCameraToTarget();
        void ToggleActivate();
        void ResetOrientation();
        void ToggleGangMode();
        void ActivateCharacter();
        void ActivateSelectedCharactersAsGang();
        void ActivateCrowdAsGang();
        void ActivateGang(List<CharacterCrowdMember> gangMembers);
        void ToggleRelativePositioning();
        void Teleport();
        void ToggleSpawnOnClick();
        void ToggleCloneAndSpawn();
        void ToggleOverheadMode();
        void MovetoPosition(Position position);
        void ScanAndFixMemoryTargeter();
    }
}
