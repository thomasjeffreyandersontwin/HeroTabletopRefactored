using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Common;
using HeroVirtualTabletop.ManagedCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HeroVirtualTabletop.Desktop
{
    public enum ContextMenuEvent
    {
        AreaAttackContextMenuDisplayed,
        DefaultContextMenuDisplayed,
        AreaAttackTargetMenuItemSelected,
        AreaAttackTargetAndExecuteMenuItemSelect,
        SpawnMenuItemSelected,
        PlaceMenuItemSelected,
        SavePositionMenuItemSelected,
        MoveCameraToTargetMenuItemSelected,
        MoveTargetToCameraMenuItemSelected,
        ResetOrientationMenuItemSelected,
        ManueverWithCameraMenuItemSelected,
        ActivateMenuItemSelected,
        ClearFromDesktopMenuItemSelected,
        CloneAndLinkMenuItemSelected,
        MoveTargetToCharacterMenuItemSelected,
        ActivateCharacterOptionMenuItemSelected
    }

    public interface DesktopContextMenu
    {
        event EventHandler<CustomEventArgs<Object>> AreaAttackContextMenuDisplayed;
        event EventHandler<CustomEventArgs<Object>> DefaultContextMenuDisplayed;
        event EventHandler<CustomEventArgs<Object>> AreaAttackTargetMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> AreaAttackTargetAndExecuteMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> SpawnMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> PlaceMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> SavePositionMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> MoveCameraToTargetMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> MoveTargetToCameraMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> ResetOrientationMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> ManueverWithCameraMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> ActivateMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> ClearFromDesktopMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> CloneAndLinkMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> MoveTargetToCharacterMenuItemSelected;
        event EventHandler<CustomEventArgs<Object>> ActivateCharacterOptionMenuItemSelected;

        void GenerateAndDisplay(AnimatedCharacter character, string attackingCharacterName, bool showAreaAttackMenu);
        bool IsDisplayed { get; set; }
        void Configure();
    }

    public class DesktopContextMenuImpl : DesktopContextMenu
    {
        public static string GameDirectoryPath;
        private static FileSystemWatcher ContextCommandFileWatcher;
        
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_TEXTS_FOLDERNAME = "texts";
        private const string GAME_LANGUAGE_FOLDERNAME = "english";
        private const string GAME_MENUS_FOLDERNAME = "menus";
        private const string GAME_CHARACTER_MENU_FILENAME = "character.mnu";
        private const string DEFAULT_DELIMITING_CHARACTER = "¿";
        private const string DEFAULT_DELIMITING_CHARACTER_TRANSLATION = "Â¿";
        private const string SPACE_REPLACEMENT_CHARACTER = "§";
        private const string SPACE_REPLACEMENT_CHARACTER_TRANSLATION = "Â§";
        private const string GAME_CHARACTER_BINDSAVE_SPAWN_FILENAME = "spawn.txt";
        private const string GAME_CHARACTER_BINDSAVE_PLACE_FILENAME = "place.txt";
        private const string GAME_CHARACTER_BINDSAVE_SAVEPOSITION_FILENAME = "saveposition.txt";
        private const string GAME_CHARACTER_BINDSAVE_MOVECAMERATOTARGET_FILENAME = "movecamera.txt";
        private const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOCAMERA_FILENAME = "movetarget.txt";
        private const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOMOUSELOCATION_FILENAME = "movetargetmouse.txt";
        private const string GAME_CHARACTER_BINDSAVE_MOVETARGETTOCHARACTER_FILENAME = "movetargetcharacter.txt";
        private const string GAME_CHARACTER_BINDSAVE_MANUEVERWITHCAMERA_FILENAME = "manueverwithcamera.txt";
        private const string GAME_CHARACTER_BINDSAVE_CLEARFROMDESKTOP_FILENAME = "clear.txt";
        private const string GAME_CHARACTER_BINDSAVE_ACTIVATE_FILENAME = "activate.txt";
        private const string GAME_CHARACTER_BINDSAVE_CLONEANDLINK_FILENAME = "clonelink.txt";
        private const string GAME_CHARACTER_BINDSAVE_RESETORIENTATION_FILENAME = "resetorientation.txt";
        private const string GAME_AREA_ATTACK_BINDSAVE_TARGET_FILENAME = "bindsavetarget.txt";
        private const string GAME_AREA_ATTACK_BINDSAVE_TARGET_EXECUTE_FILENAME = "bindsavetargetexecute.txt";

        private KeyBindCommandGenerator generator;

        public AnimatedCharacter Character = null;
        public bool IsDisplayed { get; set; }

        public bool ShowAreaAttackMenu { get; set; }
        public string AttackingCharacterName { get; set; }

        public event EventHandler<CustomEventArgs<Object>> AreaAttackContextMenuDisplayed;
        public event EventHandler<CustomEventArgs<Object>> DefaultContextMenuDisplayed;
        public event EventHandler<CustomEventArgs<Object>> AreaAttackTargetMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> AreaAttackTargetAndExecuteMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> SpawnMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> PlaceMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> SavePositionMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> MoveCameraToTargetMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> MoveTargetToCameraMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> ResetOrientationMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> ManueverWithCameraMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> ActivateMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> ClearFromDesktopMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> CloneAndLinkMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> MoveTargetToCharacterMenuItemSelected;
        public event EventHandler<CustomEventArgs<Object>> ActivateCharacterOptionMenuItemSelected;
        private void FireContextMenuEvent(ContextMenuEvent contextMenuEvent, object sender, CustomEventArgs<Object> e)
        {
            switch (contextMenuEvent)
            {
                case ContextMenuEvent.AreaAttackContextMenuDisplayed:
                    if (AreaAttackContextMenuDisplayed != null)
                        AreaAttackContextMenuDisplayed(sender, e);
                    break;
                case ContextMenuEvent.DefaultContextMenuDisplayed:
                    if (DefaultContextMenuDisplayed != null)
                        DefaultContextMenuDisplayed(sender, e);
                    break;
                case ContextMenuEvent.AreaAttackTargetMenuItemSelected:
                    if (AreaAttackTargetMenuItemSelected != null)
                        AreaAttackTargetMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.AreaAttackTargetAndExecuteMenuItemSelect:
                    if (AreaAttackTargetAndExecuteMenuItemSelected != null)
                        AreaAttackTargetAndExecuteMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.SpawnMenuItemSelected:
                    if (SpawnMenuItemSelected != null)
                        SpawnMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.PlaceMenuItemSelected:
                    if (PlaceMenuItemSelected != null)
                        PlaceMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.SavePositionMenuItemSelected:
                    if (SavePositionMenuItemSelected != null)
                        SavePositionMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.MoveCameraToTargetMenuItemSelected:
                    if (MoveCameraToTargetMenuItemSelected != null)
                        MoveCameraToTargetMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.MoveTargetToCameraMenuItemSelected:
                    if (MoveTargetToCameraMenuItemSelected != null)
                        MoveTargetToCameraMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.ResetOrientationMenuItemSelected:
                    if (ResetOrientationMenuItemSelected != null)
                        ResetOrientationMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.ManueverWithCameraMenuItemSelected:
                    if (ManueverWithCameraMenuItemSelected != null)
                        ManueverWithCameraMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.ActivateMenuItemSelected:
                    if (ActivateMenuItemSelected != null)
                        ActivateMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.ClearFromDesktopMenuItemSelected:
                    if (ClearFromDesktopMenuItemSelected != null)
                        ClearFromDesktopMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.CloneAndLinkMenuItemSelected:
                    if (CloneAndLinkMenuItemSelected != null)
                        CloneAndLinkMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.MoveTargetToCharacterMenuItemSelected:
                    if (MoveTargetToCharacterMenuItemSelected != null)
                        MoveTargetToCharacterMenuItemSelected(sender, e);
                    break;
                case ContextMenuEvent.ActivateCharacterOptionMenuItemSelected:
                    if (ActivateCharacterOptionMenuItemSelected != null)
                        ActivateCharacterOptionMenuItemSelected(sender, e);
                    break;
            }
        }

        public DesktopContextMenuImpl(KeyBindCommandGenerator generator)
        {
            this.generator = generator;
        }

        public void Configure()
        {
            if (ContextCommandFileWatcher == null)
            {
                ContextCommandFileWatcher = new FileSystemWatcher();
                ContextCommandFileWatcher.Path = string.Format("{0}\\", Path.Combine(GameDirectoryPath, GAME_DATA_FOLDERNAME));
                ContextCommandFileWatcher.IncludeSubdirectories = false;
                ContextCommandFileWatcher.Filter = "*.txt";
                ContextCommandFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
                ContextCommandFileWatcher.Changed += fileSystemWatcher_Changed;
            }
            ContextCommandFileWatcher.EnableRaisingEvents = false;
            CreateBindSaveFilesForContextCommands();
            ContextCommandFileWatcher.EnableRaisingEvents = true;
        }

        public void GenerateAndDisplay(AnimatedCharacter character, string attackingCharacterName, bool showAreaAttackMenu)
        {
            Character = character;
            AttackingCharacterName = attackingCharacterName;
            ShowAreaAttackMenu = showAreaAttackMenu;
            GenerateAndDisplay();
            ContextCommandFileWatcher.EnableRaisingEvents = true;
        }

        public void GenerateMenu()
        {
            AnimatedCharacter character = Character;
            string fileCharacterMenu = Path.Combine(GameDirectoryPath, GAME_DATA_FOLDERNAME, GAME_TEXTS_FOLDERNAME, GAME_LANGUAGE_FOLDERNAME, 
                GAME_MENUS_FOLDERNAME, GAME_CHARACTER_MENU_FILENAME);
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = "HeroVirtualTabletop.Desktop.character.mnu";
            List<string> menuFileLines = new List<string>();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    menuFileLines.Add(line);
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < menuFileLines.Count - 1; i++)
                {
                    sb.AppendLine(menuFileLines[i]);
                }
                if (character.CharacterActionGroups != null && character.CharacterActionGroups.Count > 0)
                {
                    foreach (var actionGroup in character.CharacterActionGroups)
                    {
                        sb.AppendLine(string.Format("Menu \"{0}\"", actionGroup.Name));
                        sb.AppendLine("{");
                        IEnumerable actionList = null;
                        if (actionGroup is CharacterActionList<Identity>)
                            actionList = actionGroup as CharacterActionList<Identity>;
                        else if (actionGroup is CharacterActionList<AnimatedAbility.AnimatedAbility>)
                            actionList = actionGroup as CharacterActionList<AnimatedAbility.AnimatedAbility>;
                        else if (actionGroup is CharacterActionList<Movement.CharacterMovement>)
                            actionList = actionGroup as CharacterActionList<Movement.CharacterMovement>;
                        else
                            actionList = actionGroup as CharacterActionList<CharacterAction>;

                        foreach (var action in actionList)
                        {
                            var act = action as CharacterAction;
                            string whiteSpaceReplacedOptionGroupName = actionGroup.Name.Replace(" ", SPACE_REPLACEMENT_CHARACTER);
                            string whiteSpaceReplacedOptionName = act.Name.Replace(" ", SPACE_REPLACEMENT_CHARACTER);
                            sb.AppendLine(string.Format("Option \"{0}\" \"bind_save_file {1}{2}{3}.txt\"", act.Name, whiteSpaceReplacedOptionGroupName, DEFAULT_DELIMITING_CHARACTER, whiteSpaceReplacedOptionName));
                        }
                        sb.AppendLine("}");
                    }
                }
                sb.AppendLine(menuFileLines[menuFileLines.Count - 1]);

                File.WriteAllText(
                    fileCharacterMenu, sb.ToString()
                    );
                System.Threading.Thread.Sleep(200); // Delay so that the file write completes before calling the pop menu
            }
        }
        public void DisplayMenu()
        {
            generator.GenerateDesktopCommandText(DesktopCommand.PopMenu, "character");
            generator.CompleteEvent();

        }

        public void GenerateAndDisplay()
        {
            if (Character != null)
            {
                if (ShowAreaAttackMenu)
                {
                    if (AttackingCharacterName != Character.Name)
                    {
                        System.Threading.Thread.Sleep(200); // Delay so that the file write completes before calling the pop menu
                        DisplayAreaEffectMenu();
                        IsDisplayed = true;
                        FireContextMenuEvent(ContextMenuEvent.AreaAttackContextMenuDisplayed, null, new CustomEventArgs<object> { Value = Character });
                    }
                }
                else
                {
                    GenerateMenu();
                    DisplayMenu();
                    IsDisplayed = true;
                    FireContextMenuEvent(ContextMenuEvent.DefaultContextMenuDisplayed, null, new CustomEventArgs<object> { Value = Character });
                }
            }
        }

        private void DisplayAreaEffectMenu()
        {
            Character.Target();
            generator.GenerateDesktopCommandText(DesktopCommand.PopMenu, "areaattack"); 
            generator.CompleteEvent();
        }

        private void fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Action action = delegate ()
            {
                IsDisplayed = false;

                ContextCommandFileWatcher.EnableRaisingEvents = false;
                switch (e.Name)
                {
                    case GAME_AREA_ATTACK_BINDSAVE_TARGET_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.AreaAttackTargetMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_AREA_ATTACK_BINDSAVE_TARGET_EXECUTE_FILENAME:
                        ContextCommandFileWatcher.EnableRaisingEvents = false;
                        FireContextMenuEvent(ContextMenuEvent.AreaAttackTargetAndExecuteMenuItemSelect, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_SPAWN_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.SpawnMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_PLACE_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.PlaceMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_SAVEPOSITION_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.SavePositionMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_MOVECAMERATOTARGET_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.MoveCameraToTargetMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_MOVETARGETTOCAMERA_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.MoveTargetToCameraMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_RESETORIENTATION_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.ResetOrientationMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_MANUEVERWITHCAMERA_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.ManueverWithCameraMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_ACTIVATE_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.ActivateMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_CLEARFROMDESKTOP_FILENAME:
                        FireContextMenuEvent(ContextMenuEvent.ClearFromDesktopMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                        break;
                    case GAME_CHARACTER_BINDSAVE_CLONEANDLINK_FILENAME:
                        {
                            FireContextMenuEvent(ContextMenuEvent.CloneAndLinkMenuItemSelected, null, new CustomEventArgs<object> { Value = Character });
                            break;
                        }
                    default:
                        {
                            if (e.Name.StartsWith(GAME_CHARACTER_BINDSAVE_MOVETARGETTOCHARACTER_FILENAME))
                            {
                                int index = e.Name.IndexOf(DEFAULT_DELIMITING_CHARACTER);
                                if (index > 0)
                                {
                                    string whiteSpceReplacedCharacterName = e.Name.Substring(index + 1, e.Name.Length - index - 5); // to get rid of the .txt part
                                    string characterName = whiteSpceReplacedCharacterName.Replace(SPACE_REPLACEMENT_CHARACTER_TRANSLATION, " ");
                                    FireContextMenuEvent(ContextMenuEvent.MoveTargetToCharacterMenuItemSelected, null, new CustomEventArgs<object> { Value = characterName });
                                }
                            }
                            else
                            {
                                int index = e.Name.IndexOf(DEFAULT_DELIMITING_CHARACTER);
                                if (index > 0)
                                {
                                    string whiteSpaceReplacedOptionGroupName = e.Name.Substring(0, index - 1); // The special characters are translated to two characters, so need to subtract one additional character
                                    string whiteSpceReplacedOptionName = e.Name.Substring(index + 1, e.Name.Length - index - 5); // to get rid of the .txt part
                                    string optionGroupName = whiteSpaceReplacedOptionGroupName.Replace(SPACE_REPLACEMENT_CHARACTER_TRANSLATION, " ");
                                    string optionName = whiteSpceReplacedOptionName.Replace(SPACE_REPLACEMENT_CHARACTER_TRANSLATION, " ");
                                    FireContextMenuEvent(ContextMenuEvent.ActivateCharacterOptionMenuItemSelected, null, new CustomEventArgs<object> { Value = new object[] { Character, optionGroupName, optionName } });
                                }
                            }

                            break;
                        }

                }
            };
            System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
        }

        public void CreateBindSaveFilesForContextCommands()
        {
            string filePath = Path.Combine(GameDirectoryPath, GAME_DATA_FOLDERNAME, GAME_AREA_ATTACK_BINDSAVE_TARGET_FILENAME);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            filePath = Path.Combine(GameDirectoryPath, GAME_DATA_FOLDERNAME, GAME_AREA_ATTACK_BINDSAVE_TARGET_EXECUTE_FILENAME);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }
    }
}