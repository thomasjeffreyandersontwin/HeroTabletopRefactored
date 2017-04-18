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

        public HeroVirtualTabletop.Crowd.Crowd SelectedCrowdMemberParent
        {
            get
            {
                if (SelectedCrowdMember is HeroVirtualTabletop.Crowd.Crowd)
                    return SelectedCrowdMember as HeroVirtualTabletop.Crowd.Crowd;
                else return SelectedCrowdMember.Parent;
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
            var charCrowd = this.CrowdRepository.NewCharacterCrowdMember();
            this.SelectedCrowdMemberParent.AddCrowdMember(charCrowd);
            this.CrowdRepository.SaveCrowds();
        }

        public void AddCrowd()
        {
            var crowd = this.CrowdRepository.NewCrowd();
            this.SelectedCrowdMemberParent.AddCrowdMember(crowd);
            this.CrowdRepository.SaveCrowds();
        }

        public void AddCrowdMemberToRoster(CrowdMember member)
        {
            
        }

        public void ApplyFilter(string filter)
        {
            foreach (var crowd in this.CrowdRepository.Crowds)
            {
                foreach (var mem in crowd.Members)
                {
                    mem.ApplyFilter(filter);
                }
            }
        }

        public void CloneCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.CopyToClipboard(this.SelectedCrowdMember);
        }

        public void CutCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.CutToClipboard(this.SelectedCrowdMember);
        }

        public void DeleteCrowdMember()
        {
            this.SelectedCrowdMemberParent.RemoveMember(this.SelectedCrowdMember);
            this.CrowdRepository.SaveCrowds();
        }

        public void LinkCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.LinkToClipboard(this.SelectedCrowdMember);
        }

        public void PasteCrowdMember(CrowdMember member)
        {
            this.CrowdClipboard.PasteFromClipboard(this.SelectedCrowdMember);
        }

        public void RenameCrowdMember(CrowdMember member, string newName)
        {
            IEnumerable<CrowdMember> allMembers = this.CrowdRepository.Crowds;
            var isDuplicate = member.CheckIfNameIsDuplicate(newName, allMembers.ToList());
            if (!isDuplicate)
            {
                member.Rename(newName);
            }
        }

        public void SortCrowds()
        {
            
        }

        public void MoveCrowdMember(CrowdMember movingCrowdMember, CrowdMember targetCrowdMember, HeroVirtualTabletop.Crowd.Crowd destinationCrowd)
        {
            destinationCrowd.MoveCrowdMemberAfter(targetCrowdMember, movingCrowdMember);
        }

        public void CreateCrowdFromModels()
        {
            
        }
    }
}
