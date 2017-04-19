using Caliburn.Micro;
using HeroVirtualTabletop.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeroUI
{
    public class HeroSystemsShellViewModelImpl : Conductor<object>, IShell
    {
        public HeroSystemsShellViewModelImpl()
        {
            var heroVirtualTabletopMainViewModel = IoC.Get<HeroVirtualTabletopMainViewModel>();
            ActivateItem(heroVirtualTabletopMainViewModel);
        }
    }
}
