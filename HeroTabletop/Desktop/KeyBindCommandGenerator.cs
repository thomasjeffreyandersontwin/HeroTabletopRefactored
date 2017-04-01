using System.Collections.Generic;

namespace HeroVirtualTableTop.Desktop
{
    public class KeyBindCommandGeneratorImpl : KeyBindCommandGenerator
    {
        protected internal static List<string> generatedKeybinds = new List<string>();
        private readonly IconInteractionUtility _iconInteracter;
        private string _lastCommand;

        public KeyBindCommandGeneratorImpl(IconInteractionUtility iconInteractor)
        {
            _iconInteracter = iconInteractor;
        }

        public string GeneratedCommandText { get; set; }


        public string Command { get; set; }

        public void GenerateDesktopCommandText(DesktopCommand desktopCommand, params string[] parameters)
        {
            var generatedCommandParameters = string.Empty;
            var command = _keyBindsStrings[desktopCommand];
            _lastCommand = command;
 
            foreach (var p in parameters)
                if (!string.IsNullOrWhiteSpace(p))
                {
                    generatedCommandParameters = $"{generatedCommandParameters} {p.Trim()}";
                    generatedCommandParameters = generatedCommandParameters.Trim();
                }

            if (!string.IsNullOrWhiteSpace(generatedCommandParameters))
            {
                if (!string.IsNullOrEmpty(GeneratedCommandText))
                    GeneratedCommandText += $"$${command} {generatedCommandParameters}";
                else
                    GeneratedCommandText = $"{command} {generatedCommandParameters}";
            }
            else
            {
                if (!string.IsNullOrEmpty(GeneratedCommandText))
                    GeneratedCommandText += $"$${command}";
                else
                    GeneratedCommandText = command;
            }
        }

        public string CompleteEvent()
        {
            var command = popEvents();
            _iconInteracter.ExecuteCmd(command);
            return command;
        }

        private string popEvents()
        {
            _lastCommand = GeneratedCommandText;
            generatedKeybinds.Add(_lastCommand);
            var generatedCommandtext = GeneratedCommandText;
            GeneratedCommandText = "";
            //return string.Format("\"{0}\"", GeneratedKeybindText);
            return generatedCommandtext;
        }

        #region KeyBinds Strings

        internal Dictionary<DesktopCommand, string> _keyBindsStrings = new Dictionary<DesktopCommand, string>
        {
            {DesktopCommand.TargetName, "target_name"},
            {DesktopCommand.PrevSpawn, "prev_spawn"},
            {DesktopCommand.NextSpawn, "next_spawn"},
            {DesktopCommand.RandomSpawn, "random_spawn"},
            {DesktopCommand.Fly, "fly"},
            {DesktopCommand.EditPos, "edit_pos"},
            {DesktopCommand.DetachCamera, "detach_camera"},
            {DesktopCommand.NoClip, "no_clip"},
            {DesktopCommand.AccessLevel, "access_level"},
            {DesktopCommand.Command, "~"},
            {DesktopCommand.SpawnNpc, "spawn_npc"},
            {DesktopCommand.Rename, "rename"},
            {DesktopCommand.LoadCostume, "load_costume"},
            {DesktopCommand.MoveNPC, "move_npc"},
            {DesktopCommand.DeleteNPC, "delete_npc"},
            {DesktopCommand.ClearNPC, "clear_npc"},
            {DesktopCommand.Move, "mov"},
            {DesktopCommand.TargetEnemyNear, "target_enemy_near"},
            {DesktopCommand.LoadBind, "load_bind"},
            {DesktopCommand.BeNPC, "benpc"},
            {DesktopCommand.SaveBind, "save_bind"},
            {DesktopCommand.GetPos, "getpos"},
            {DesktopCommand.CamDist, "camdist"},
            {DesktopCommand.Follow, "follow"},
            {DesktopCommand.LoadMap, "loadmap"},
            {DesktopCommand.BindLoadFile, "bind_load_file"},
            {DesktopCommand.Macro, "macro"},
            {DesktopCommand.NOP, "nop"},
            {DesktopCommand.PopMenu, "popmenu"}
        };
    }

    #endregion
}