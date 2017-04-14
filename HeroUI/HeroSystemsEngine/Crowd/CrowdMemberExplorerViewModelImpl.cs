using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.Crowd;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace HeroUI.HeroSystemsEngine.Crowd
{
    class CrowdMemberExplorerViewModelImpl : PropertyChangedBase, CrowdMemberExplorerViewModel
    {
        private ObservableCollection<HeroVirtualTabletop.Crowd.Crowd> crowdCollection;
        public ObservableCollection<HeroVirtualTabletop.Crowd.Crowd> CrowdCollection
        {
            get
            {
                return crowdCollection;
            }
            set
            {
                crowdCollection = value;
                NotifyOfPropertyChange(() => CrowdCollection);
            }
        }

        public CrowdClipboard CrowdClipboard
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public CrowdRepository CrowdRepository
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public CrowdMember SelectedCrowdMember
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void AddCharacterCrowd()
        {
            throw new NotImplementedException();
        }

        public void AddCrowd()
        {
            throw new NotImplementedException();
        }

        public void AddCrowdFromModels()
        {
            throw new NotImplementedException();
        }

        public void AddCrowdMemberToRoster(CrowdMember member)
        {
            throw new NotImplementedException();
        }

        public void ApplyFilter()
        {
            throw new NotImplementedException();
        }

        public void CloneCrowdMember(CrowdMember member)
        {
            throw new NotImplementedException();
        }

        public void CutCrowdMember(CrowdMember member)
        {
            throw new NotImplementedException();
        }

        public void DeleteCrowdMember()
        {
            throw new NotImplementedException();
        }

        public void LinkCrowdMember(CrowdMember member)
        {
            throw new NotImplementedException();
        }

        public void MoveCrowdMember(CrowdMember movingCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd)
        {
            throw new NotImplementedException();
        }

        public void PasteCrowdMember(CrowdMember member)
        {
            throw new NotImplementedException();
        }

        public void RenameCrowdMember(CrowdMember member, string newName)
        {
            throw new NotImplementedException();
        }

        public void SortCrowds()
        {
            throw new NotImplementedException();
        }
    }
}
