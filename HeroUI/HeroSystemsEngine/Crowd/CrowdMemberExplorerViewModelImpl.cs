using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop.Crowd;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using System.IO;

namespace HeroUI.HeroSystemsEngine.Crowd
{
    public class CrowdMemberExplorerViewModelImpl : PropertyChangedBase, CrowdMemberExplorerViewModel, IShell
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

        private CrowdClipboard crowdClipboard;
        public CrowdClipboard CrowdClipboard
        {
            get
            {
                return crowdClipboard;
            }

            set
            {
                crowdClipboard = value;
            }
        }

        private CrowdRepository crowdRepository;
        public CrowdRepository CrowdRepository
        {
            get
            {
                return crowdRepository;
            }

            set
            {
                crowdRepository = value;
            }
        }

        private CrowdMember selectedCrowdMember;
        public CrowdMember SelectedCrowdMember
        {
            get
            {
                return selectedCrowdMember;
            }

            set
            {
                selectedCrowdMember = value;
                NotifyOfPropertyChange(() => SelectedCrowdMember);
            }
        }
        public CrowdMemberExplorerViewModelImpl(CrowdRepository repository, CrowdClipboard clipboard)
        {
            this.CrowdRepository = repository;
            this.CrowdClipboard = clipboard;
            this.CrowdRepository.CrowdRepositoryPath = Path.Combine(Properties.Settings.Default.GameDirectory, Constants.GAME_DATA_FOLDERNAME, Constants.GAME_CROWD_REPOSITORY_FILENAME);
            this.CrowdRepository.LoadCrowds();
            this.CrowdCollection = new ObservableCollection<HeroVirtualTabletop.Crowd.Crowd>(this.CrowdRepository.Crowds);
        }

        public void AddCharacterCrowd()
        {
            
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
