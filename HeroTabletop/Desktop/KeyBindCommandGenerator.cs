using System.Collections.Generic;

namespace HeroVirtualTabletop.Desktop
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
            {DesktopCommand.TargetName, "targetname"},
            {DesktopCommand.PrevSpawn, "prevspawn"},
            {DesktopCommand.NextSpawn, "nextspawn"},
            {DesktopCommand.RandomSpawn, "randomspawn"},
            {DesktopCommand.Fly, "fly"},
            {DesktopCommand.EditPos, "editpos"},
            {DesktopCommand.DetachCamera, "detachcamera"},
            {DesktopCommand.NoClip, "noclip"},
            {DesktopCommand.AccessLevel, "accesslevel"},
            {DesktopCommand.Command, "~"},
            {DesktopCommand.SpawnNpc, "spawnnpc"},
            {DesktopCommand.Rename, "rename"},
            {DesktopCommand.LoadCostume, "loadcostume"},
            {DesktopCommand.MoveNPC, "movenpc"},
            {DesktopCommand.DeleteNPC, "deletenpc"},
            {DesktopCommand.ClearNPC, "clearnpc"},
            {DesktopCommand.Move, "mov"},
            {DesktopCommand.TargetEnemyNear, "targetenemynear"},
            {DesktopCommand.LoadBind, "loadbind"},
            {DesktopCommand.BeNPC, "benpc"},
            {DesktopCommand.SaveBind, "savebind"},
            {DesktopCommand.GetPos, "getpos"},
            {DesktopCommand.CamDist, "camdist"},
            {DesktopCommand.Follow, "follow"},
            {DesktopCommand.LoadMap, "loadmap"},
            {DesktopCommand.BindLoadFile, "bindloadfile"},
            {DesktopCommand.Macro, "macro"},
            {DesktopCommand.PopMenu, "popmenu"}
        };
    }

    #endregion
}