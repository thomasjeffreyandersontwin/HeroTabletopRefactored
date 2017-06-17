using HeroVirtualTabletop.ManagedCharacter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HeroUI
{
    public class ActionListBoxControl : ListBox
    {
        public CharacterAction DefaultAction
        {
            get { return (CharacterAction)GetValue(DefaultActionProperty); }
            set { SetValue(DefaultActionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultActionProperty =
            DependencyProperty.Register("DefaultAction", typeof(CharacterAction), typeof(ActionListBoxControl), new PropertyMetadata(null));

        public CharacterAction ActiveAction
        {
            get { return (CharacterAction)GetValue(ActiveActionProperty); }
            set { SetValue(ActiveActionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveActionProperty =
            DependencyProperty.Register("ActiveAction", typeof(CharacterAction), typeof(ActionListBoxControl), new PropertyMetadata(null));
    }
}
