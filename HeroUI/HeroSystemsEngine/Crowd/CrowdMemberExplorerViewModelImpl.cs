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
        public IEventAggregator EventAggregator { get; set; }

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
        public CrowdMemberExplorerViewModelImpl(CrowdRepository repository, CrowdClipboard clipboard, IEventAggregator eventAggregator)
        {
            this.CrowdRepository = repository;
            this.CrowdClipboard = clipboard;
            this.EventAggregator = eventAggregator;
            this.CrowdRepository.CrowdRepositoryPath = Path.Combine(Properties.Settings.Default.GameDirectory, Constants.GAME_DATA_FOLDERNAME, Constants.GAME_CROWD_REPOSITORY_FILENAME);
            this.CrowdRepository.LoadCrowds();
            this.CrowdCollection = new ObservableCollection<HeroVirtualTabletop.Crowd.Crowd>(this.CrowdRepository.Crowds);
        }

        public void AddCharacterCrowd()
        {
            
        }

        public void AddCrowd()
        {
            
        }

        public void AddCrowdMemberToRoster(CrowdMember member)
        {
            
        }

        public void ApplyFilter()
        {
            
        }

        public void CloneCrowdMember(CrowdMember member)
        {
            
        }

        public void CutCrowdMember(CrowdMember member)
        {
            
        }

        public void DeleteCrowdMember()
        {
            
        }

        public void LinkCrowdMember(CrowdMember member)
        {
            
        }

        public void MoveCrowdMember(CrowdMember movingCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd)
        {
            
        }

        public void PasteCrowdMember(CrowdMember member)
        {
            
        }

        public void RenameCrowdMember(CrowdMember member, string newName)
        {
            
        }

        public void SortCrowds()
        {
            
        }

        public void MoveCrowdMember(CrowdMember movingCrowdMember, CrowdMember targetCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd)
        {
            
        }

        public void CreateCrowdFromModels()
        {
            
        }

        public void ApplyFilter(string filter)
        {
            
        }
    }
}
