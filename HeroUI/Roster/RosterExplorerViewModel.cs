using Caliburn.Micro;
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
        void ClearFromDesktop();
        void MoveToCamera();
        void SavePosition();
        void Place();
        void ToggleTargeted();
        void ToggleManeuverWithCamera();
        void MoveCameraToTarget();
        void Activate();
        void ResetOrientation();
    }
}
