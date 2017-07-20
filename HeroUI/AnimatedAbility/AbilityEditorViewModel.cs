using Caliburn.Micro;
using HeroUI;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.AnimatedAbility
{
    public interface AbilityEditorViewModel
    {
        event EventHandler<CustomEventArgs<bool>> AnimationAdded;
        event EventHandler AnimationElementDraggedFromGrid;
        event EventHandler EditModeEnter;
        event EventHandler EditModeLeave;
        event EventHandler SelectionChanged;
        event EventHandler<CustomEventArgs<ExpansionUpdateEvent>> ExpansionUpdateNeeded;

        AnimatedAbility CurrentAbility { get; set; }
        AnimationElement SelectedAnimationElementRoot { get; set; }
        AnimatedResourceManager AnimatedResourceMananger { get; set; }
        AnimationElement SelectedAnimationParent { get; set; }
        AnimationElement SelectedAnimationElement { get; set; }

        void EnterAnimationElementEditMode(object state);

        bool IsSequenceAbilitySelected { get; set; }
        bool IsShowingAbilityEditor { get; set; }
        void MoveSelectedAnimationElementAfter(AnimationElement animationElement);
        void MoveReferenceResourceToAnimationElements(ReferenceResource movedResource, AnimationElement elementAfter);
        void LoadResources();
        IEventAggregator EventAggregator { get; set; }
    }
}
