﻿using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Crowd
{
    public interface CrowdMemberExplorerViewModel
    {
        CrowdRepository CrowdRepository { get; set; }
        Crowd SelectedCrowdMember { get; set; }
        CharacterCrowdMember SelectedCharacterCrowdMember { get; set; }
        CrowdClipboard CrowdClipboard { get; set; }
        IEventAggregator EventAggregator { get; set; }
        int FlattenNumber { get; set; }
        //KeyBoardHook keyBoardHook { get; set; } // To do under desktops
        void AddCrowd();
        void AddCharacterCrowdMember();
        void RenameCrowdMember(string updatedName);
        void DeleteCrowdMember();
        void CloneCrowdMember();
        void CutCrowdMember();
        void LinkCrowdMember();
        void CloneLinkCharacter(CrowdMember crowdMember);
        void PasteCrowdMember();
        Task SyncCrowdMembersWithRoster();
        void ApplyFilter(string filter);
        Task LoadCrowdCollection();
        Task SaveCrowdCollection();
        void CopyAllActions();
        void PasteAllActions();
        void RemoveAllActions();
        void FlattenCopyCrowd();
        void NumberedFlattenCopyCrowd();
        void CloneMemberships();
    }
}
