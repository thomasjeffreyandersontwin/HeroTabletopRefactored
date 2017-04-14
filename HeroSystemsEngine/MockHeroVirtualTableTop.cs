
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeroVirtualTabletop;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;

using Moq;


namespace HeroSystemEngine.Character
{

    public class TestHelperFactory
    {
        private static TestHelperFactory _instance = null;

        public HeroSystemCharacterRepository CharacterRepositoryWithMockTableTopRepo;

        public Mock<HeroTableTopCharacterRepository> MockHeroTableTopCharacterRepositoryContext;
        public Mock<AnimatedAttack> MockAttackContext;
        public Mock<HeroTableTopCharacter> MockTableTopCharacterContext;
        public Mock<AttackInstructions> MockAttackInstructionsContext;

        public void CleanUpMocks()
        {
          //  MockHeroTableTopCharacterRepositoryContext.Reset();
           // MockAttackContext.Reset();
            //MockTableTopCharacterContext.Reset();
            //MockAttackInstructionsContext.Reset();
        }
            
        private TestHelperFactory()
        {
            MockHeroTableTopCharacterRepositoryContext = new Mock<HeroTableTopCharacterRepository>();;
            MockAttackContext = new Mock<AnimatedAttack>();
            MockTableTopCharacterContext = new Mock<HeroTableTopCharacter>();
            MockAttackInstructionsContext = new Mock<AttackInstructions>().SetupAllProperties();
            ConfigureCharacterRepo();
        }
        private HeroSystemCharacterRepository ConfigureCharacterRepo()
        {
            HeroTableTopCharacterRepository repo = MockHeroTableTopCharacterRepositoryContext.Object;
            CharacterRepositoryWithMockTableTopRepo = HeroSystemCharacterRepository.GetInstance(repo);
            return CharacterRepositoryWithMockTableTopRepo;
        }

        public HeroSystemCharacter AddDefaultCharacterAndConfigureMockTableTopRepo(string characterName)
        {
            HeroSystemCharacter character = CharacterRepositoryWithMockTableTopRepo.LoadBaseCharacter(characterName);
            MockHeroTableTopCharacterRepositoryContext.Setup(t => t.ReturnCharacter(characterName)).Returns(It.Is<HeroTableTopCharacter>(t => t.Name == characterName));
            CharacterRepositoryWithMockTableTopRepo.AddCharacter(character);
            character.TableTopCharacterRepository = MockHeroTableTopCharacterRepositoryContext.Object;

            return character;
        }

        public HeroSystemCharacter CreateADefaultCharacterAndAssociateItWithMockTableTopCharacter(string characterName)
        {
            HeroSystemCharacter character = CharacterRepositoryWithMockTableTopRepo.LoadBaseCharacter(characterName);
            character.TableTopCharacter = MockTableTopCharacterContext.Object;
            CharacterRepositoryWithMockTableTopRepo.AddCharacter(character);
            character.TableTopCharacterRepository = MockHeroTableTopCharacterRepositoryContext.Object;

            return character;
        }

        public AnimatedAttack AddMockTabletopAttackToMockTabletopCharacter(string attackName)
        {
     

            MockAttackContext.Setup(t => t.Name).Returns(attackName);
            AnimatedAttack ability = MockAttackContext.Object;

            MockTableTopCharacterContext.Setup(t => t.GetAbility(attackName)).Returns(ability);
            return ability;
        }

        public AttackInstructions CreateMockAttackInstructions()
        {
            AttackInstructions instructions = MockAttackInstructionsContext.Object;
             instructions.defender = MockTableTopCharacterContext.Object;

            MockHeroTableTopCharacterRepositoryContext.Setup(t => t.NewAttackInstructions()).Returns(instructions);
            return instructions;
        } 

        public static TestHelperFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestHelperFactory();
                }
                return _instance;
            }
        }
    }
}

