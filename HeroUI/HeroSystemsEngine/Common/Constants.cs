using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeroUI.HeroSystemsEngine
{
    public sealed class Constants
    {
        #region Application
        public const string APPLICATION_NAME = "Hero Virtual";
        public const string DEFAULT_DELIMITING_CHARACTER = "¿";
        public const string DEFAULT_DELIMITING_CHARACTER_TRANSLATION = "Â¿";
        public const string SPACE_REPLACEMENT_CHARACTER = "§";
        public const string SPACE_REPLACEMENT_CHARACTER_TRANSLATION = "Â§";
        #endregion

        #region Regions
        public const string MAIN_REGION = "MainRegion";
        public const string BUSY_REGION = "BusyRegion";
        public const string NAVIGATION_BAR_REGION = "NavigationBarRegion";
        public const string HEROVIRTUALTABLETOP_REGION = "HeroVirtualTabletopRegion";
        #endregion

        #region Modules
        public const string HEROVIRTUALTABLETOP_MODULENAME = "HeroVirtualTabletopModule";
        #endregion

        #region Log
        public const string LOG_CONFIGURATION_FILENAME = "log4net.config";
        public const string LOG_FOLDERNAME = "Log";
        #endregion

        #region Resource Dictionary and Styles
        public const string RESOURCE_DICTIONARY_PATH = "/HeroSystemsEngine/Common/HeroResourceDictionary.xaml";
        public const string CUSTOM_MODELESS_TRANSPARENT_WINDOW_STYLENAME = "CustomModelessTransparentWindow";
        #endregion

        #region Game Files and Directories
        public const string INVALID_GAME_DIRECTORY_MESSAGE = "Invalid Game Directory! Please provide proper Game Directory for City of Heroes.";
        public const string INVALID_DIRECTORY_CAPTION = "Invalid Directory";

        public const string GAME_EXE_FILENAME = "cityofheroes.exe";
        public const string GAME_ICON_EXE_FILENAME = "icon.exe";
        public const string GAME_PROCESSNAME = "cityofheroes";
        public const string GAME_DATA_FOLDERNAME = "data";
        public const string GAME_COSTUMES_FOLDERNAME = "costumes";
        public const string GAME_COSTUMES_EXT = ".costume";
        public const string GAME_CROWD_REPOSITORY_FILENAME = "CrowdRepo.data";
        public const string GAME_MOVEMENT_REPOSITORY_FILENAME = "MovementRepo.data";
        public const string GAME_MOVE_REPOSITORY_FILENAME = "MoveRepo.data";
        public const string GAME_FX_REPOSITORY_FILENAME = "FxRepo.data";
        public const string GAME_SOUND_REPOSITORY_FILENAME = "SoundRepo.data";
        public const string GAME_KEYBINDS_FILENAME = "required_keybinds.txt";
        public const string GAME_MODELS_FILENAME = "Models.txt";
        public const string GAME_DATA_BACKUP_FOLDERNAME = "Backup";
        public const string GAME_SOUND_FOLDERNAME = "sound";
        public const string GAME_AREA_ATTACK_BINDSAVE_TARGET_FILENAME = "bindsavetarget.txt";
        public const string GAME_AREA_ATTACK_BINDSAVE_TARGET_EXECUTE_FILENAME = "bindsavetargetexecute.txt";
        public const string GAME_TEXTS_FOLDERNAME = "texts";
        public const string GAME_LANGUAGE_FOLDERNAME = "english";
        public const string GAME_MENUS_FOLDERNAME = "menus";
        public const string GAME_AREAATTACK_MENU_FILENAME = "areaattack.mnu";
        public const string GAME_CHARACTER_MENU_FILENAME = "character.mnu";
        public const string GAME_CHARACTER_BINDSAVE_SPAWN_FILENAME = "spawn.txt";
        public const string GAME_CHARACTER_BINDSAVE_PLACE_FILENAME = "place.txt";
        public const string GAME_CHARACTER_BINDSAVE_SAVEPOSITION_FILENAME = "saveposition.txt";
        public const string GAME_CHARACTER_BINDSAVE_MOVECAMERATOTARGET_FILENAME = "movecamera.txt";
        public const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOCAMERA_FILENAME = "movetarget.txt";
        public const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOMOUSELOCATION_FILENAME = "movetargetmouse.txt";
        public const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOCHARACTER_FILENAME = "movetargetcharacter.txt";
        public const string GAME_CHARACTER_BINDSAVE_MANUEVERWITHCAMERA_FILENAME = "manueverwithcamera.txt";
        public const string GAME_CHARACTER_BINDSAVE_CLEARFROMDESKTOP_FILENAME = "clear.txt";
        public const string GAME_CHARACTER_BINDSAVE_ACTIVATE_FILENAME = "activate.txt";
        public const string GAME_CHARACTER_BINDSAVE_CLONEANDLINK_FILENAME = "clonelink.txt";
        public const string GAME_CHARACTER_BINDSAVE_RESETORIENTATION_FILENAME = "resetorientation.txt";
        public const string GAME_ENABLE_CAMERA_FILENAME = "enable_camera.txt";
        public const string GAME_DISABLE_CAMERA_FILENAME = "disable_camera.txt";
        #endregion

        #region Character Explorer
        public const string DEFAULT_CHARACTER_NAME = "DEFAULT";
        public const string COMBAT_EFFECTS_CHARACTER_NAME = "COMBAT EFFECTS";
        public const string ALL_CHARACTER_CROWD_NAME = "All Characters";
        public const string CROWD_MEMBER_DRAG_FROM_CHAR_XPLORER_KEY = "CrowdMemberDragFromCharacterExplorer";
        #endregion

        #region Roster Explorer
        public const string NO_CROWD_CROWD_NAME = "No Crowd";
        #endregion

        #region Default Movements, Abilities and Animations

        public const string KNOCKBACK_MOVEMENT_NAME = "Knockback";
        public const string STANDUP_ABILITY_NAME = "Stand Up";
        public const string HIT_ABITIY_NAME = "Hit";
        public const string DODGE_ABILITY_NAME = "Dodge";
        public const string STUNNED_ABITIY_NAME = "Stunned";
        public const string UNCONCIOUS_ABITIY_NAME = "Unconcious";
        public const string DYING_ABILITY_NAME = "Dying";
        public const string DEAD_ABITIY_NAME = "Dead";
        public const string MISS_ABITIY_NAME = "Miss";
        public const string ANIMATION_DRAG_KEY = "AnimationElementDragFromAbilityEditor";
        #endregion

        #region Option Groups
        public const string ABILITY_OPTION_GROUP_NAME = "Powers";
        public const string IDENTITY_OPTION_GROUP_NAME = "Identities";
        public const string MOVEMENT_OPTION_GROUP_NAME = "Movements";

        public const string OPTION_DRAG_KEY = "CharacterOptionDrag";
        public const string OPTION_GROUP_DRAG_KEY = "CharacterOptionGroupDrag";
        #endregion
    }
}
