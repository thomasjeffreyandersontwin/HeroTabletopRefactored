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
        Roster Roster { get; set; }
        IList SelectedParticipants { get; set; }
        void UpdateRosterSelection();
        void Target();
        void Spawn();
        void ClearFromDesktop();
        void MoveToCamera();
        void SavePosition();
        void Place();
        void ToggleTargeted();
        void ToggleManueverWithCamera();
        void MoveCameraToTarget();
        void Activate();
        void ResetOrientation();
    }
}
