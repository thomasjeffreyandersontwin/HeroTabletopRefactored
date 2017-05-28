using Caliburn.Micro;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Roster;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeroUI
{
    public class HeroVirtualTabletopMainViewModelImpl : PropertyChangedBase, HeroVirtualTabletopMainViewModel, IShell
    {
        #region Private Members
        private IEventAggregator eventAggregator;
        private IconInteractionUtility iconInteractionUtility;
        private Camera camera;
        System.Threading.Timer gameInitializeTimer;

        private const string GAME_EXE_FILENAME = "cityofheroes.exe";
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_KEYBINDS_FILENAME = "required_keybinds.txt";
        private const string GAME_ENABLE_CAMERA_FILENAME = "enable_camera.txt";
        private const string GAME_DISABLE_CAMERA_FILENAME = "disable_camera.txt";
        private const string SELECT_GAME_DIRECTORY_MESSAGE = "Please select City of Heroes Game Directory";
        private const string INVALID_GAME_DIRECTORY_MESSAGE = "Invalid Game Directory! Please provide proper Game Directory for City of Heroes.";
        private const string INVALID_DIRECTORY_CAPTION = "Invalid Directory";
        private const string GAME_TEXTS_FOLDERNAME = "texts";
        private const string GAME_LANGUAGE_FOLDERNAME = "english";
        private const string GAME_MENUS_FOLDERNAME = "menus";
        private const string GAME_AREAATTACK_MENU_FILENAME = "areaattack.mnu";
        private const string GAME_MODELS_FILENAME = "Models.txt";
        private const string GAME_SOUND_FOLDERNAME = "sound";
        private const string GAME_COSTUMES_FOLDERNAME = "costumes";
        private const string DEFAULT_DELIMITING_CHARACTER_TRANSLATION = "Â¿";

        #endregion

        #region Events
        public event EventHandler ViewLoaded;
        public void OnViewLoaded(object sender, EventArgs e)
        {
            if (ViewLoaded != null)
                ViewLoaded(sender, e);
        }
        #endregion

        #region Public Properties

        private bool isCharacterExplorerExpanded;
        public bool IsCharacterExplorerExpanded
        {
            get
            {
                return isCharacterExplorerExpanded;
            }
            set
            {
                isCharacterExplorerExpanded = value;
                NotifyOfPropertyChange("IsCharacterExplorerExpanded");
            }
        }

        private bool isRosterExplorerExpanded;
        public bool IsRosterExplorerExpanded
        {
            get
            {
                return isRosterExplorerExpanded;
            }
            set
            {
                isRosterExplorerExpanded = value;
                NotifyOfPropertyChange("IsRosterExplorerExpanded");
            }
        }

        private bool isCharacterEditorExpanded;
        public bool IsCharacterEditorExpanded
        {
            get
            {
                return isCharacterEditorExpanded;
            }
            set
            {
                isCharacterEditorExpanded = value;
                NotifyOfPropertyChange("IsCharacterEditorExpanded");
            }
        }

        private bool isIdentityEditorExpanded;
        public bool IsIdentityEditorExpanded
        {
            get
            {
                return isIdentityEditorExpanded;
            }
            set
            {
                isIdentityEditorExpanded = value;
                NotifyOfPropertyChange("IsIdentityEditorExpanded");
            }
        }

        private bool isAbilityEditorExpanded;
        public bool IsAbilityEditorExpanded
        {
            get
            {
                return isAbilityEditorExpanded;
            }
            set
            {
                isAbilityEditorExpanded = value;
                NotifyOfPropertyChange("IsAbilityEditorExpanded");
            }
        }

        private bool isMovementEditorExpanded;
        public bool IsMovementEditorExpanded
        {
            get
            {
                return isMovementEditorExpanded;
            }
            set
            {
                isMovementEditorExpanded = value;
                NotifyOfPropertyChange("IsMovementEditorExpanded");
            }
        }

        private bool isCrowdFromModelsExpanded;
        public bool IsCrowdFromModelsExpanded
        {
            get
            {
                return isCrowdFromModelsExpanded;
            }
            set
            {
                isCrowdFromModelsExpanded = value;
                NotifyOfPropertyChange("IsCrowdFromModelsExpanded");
            }
        }

        private CrowdMemberExplorerViewModel crowdMemberExplorerViewModel;
        public CrowdMemberExplorerViewModel CrowdMemberExplorerViewModel
        {
            get
            {
                return crowdMemberExplorerViewModel;
            }
            set
            {
                crowdMemberExplorerViewModel = value;
                NotifyOfPropertyChange(() => CrowdMemberExplorerViewModel);
            }
        }

        private RosterExplorerViewModel rosterExplorerViewModel;
        public RosterExplorerViewModel RosterExplorerViewModel
        { 
            get
            {
                return rosterExplorerViewModel;
            }
            set
            {
                rosterExplorerViewModel = value;
                NotifyOfPropertyChange(() => RosterExplorerViewModel);
            }
        }

        #endregion

        #region Constructor
        public HeroVirtualTabletopMainViewModelImpl(IEventAggregator eventAggregator, CrowdMemberExplorerViewModel crowdMemberExplorerViewModel, RosterExplorerViewModel rosterExplorerViewModel, IconInteractionUtility iconInteractionUtility, Camera camera)
        {
            this.eventAggregator = eventAggregator;
            this.CrowdMemberExplorerViewModel = crowdMemberExplorerViewModel;
            this.RosterExplorerViewModel = rosterExplorerViewModel;
            this.iconInteractionUtility = iconInteractionUtility;
            this.camera = camera;
            gameInitializeTimer = new System.Threading.Timer(gameInitializeTimer_Callback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            LaunchGame();
            //this.eventAggregator.GetEvent<AddToRosterEvent>().Subscribe((IEnumerable<CrowdMemberModel> models) => { this.IsRosterExplorerExpanded = true; });
            //this.eventAggregator.GetEvent<EditCharacterEvent>().Subscribe((Tuple<ICrowdMemberModel, IEnumerable<ICrowdMemberModel>> tuple) => { this.IsCharacterEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditIdentityEvent>().Subscribe((Tuple<Identity, Character> tuple) => { this.IsIdentityEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditAbilityEvent>().Subscribe((Tuple<AnimatedAbility, Character> tuple) => { this.IsAbilityEditorExpanded = true; });
            //this.eventAggregator.GetEvent<EditMovementEvent>().Subscribe((CharacterMovement cm) => { this.IsMovementEditorExpanded = true; });
            //this.eventAggregator.GetEvent<CreateCrowdFromModelsEvent>().Subscribe((CrowdModel crowd) => { this.IsCrowdFromModelsExpanded = true; });
            //this.eventAggregator.GetEvent<AttackInitiatedEvent>().Subscribe((Tuple<Character, Attack> tuple) => { this.IsRosterExplorerExpanded = true; });
        }

        #endregion

        #region Methods
       
        private void CollapsePanel(object state)
        {
            switch (state.ToString())
            {
                case "CharacterExplorer":
                    this.IsCharacterExplorerExpanded = false;
                    break;
                case "RosterExplorer":
                    this.IsRosterExplorerExpanded = false;
                    break;
                case "CharacterEditor":
                    this.IsCharacterEditorExpanded = false;
                    break;
                case "IdentityEditor":
                    this.IsIdentityEditorExpanded = false;
                    break;
                case "AbilityEditor":
                    this.IsAbilityEditorExpanded = false;
                    break;
                case "MovementEditor":
                    this.IsMovementEditorExpanded = false;
                    break;
                case "CrowdFromModelsView":
                    this.IsCrowdFromModelsExpanded = false;
                    break;
            }
        }

        #region Game Launch

        private void LaunchGame()
        {
            bool directoryExists = CheckGameDirectory();
            if (!directoryExists)
                SetGameDirectory();
            iconInteractionUtility.InitializeGame(Properties.Settings.Default.GameDirectory);
            gameInitializeTimer.Change(50, System.Threading.Timeout.Infinite);
        }

        private bool CheckGameDirectory()
        {
            bool directoryExists = false;
            string gameDir = Properties.Settings.Default.GameDirectory;
            if (!string.IsNullOrEmpty(gameDir) && Directory.Exists(gameDir) && File.Exists(Path.Combine(gameDir, GAME_EXE_FILENAME)))
            {
                directoryExists = true;
            }
            return directoryExists;
        }

        private void SetGameDirectory()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.Description = SELECT_GAME_DIRECTORY_MESSAGE;
            while (true)
            {
                System.Windows.Forms.DialogResult dr = dialog.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK && Directory.Exists(dialog.SelectedPath))
                {
                    if (File.Exists(Path.Combine(dialog.SelectedPath, GAME_EXE_FILENAME)))
                    {
                        Properties.Settings.Default.GameDirectory = dialog.SelectedPath;
                        Properties.Settings.Default.Save();
                        break;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(INVALID_GAME_DIRECTORY_MESSAGE, INVALID_DIRECTORY_CAPTION, MessageBoxButton.OK);
                    }
                }
            }
        }

        private void gameInitializeTimer_Callback(object state)
        {
            bool gameLoaded = iconInteractionUtility.IsGameLoaded();
            if (gameLoaded)
            {
                System.Threading.Thread.Sleep(1000);
                iconInteractionUtility.DoPostInitialization();
                DoPostGameLaunchOperations();
                gameInitializeTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.eventAggregator.PublishOnUIThread(new GameLaunchedEvent());
            }
            else
            {
                gameInitializeTimer.Change(100, System.Threading.Timeout.Infinite);
            }
        }

        private void DoPostGameLaunchOperations()
        {
            System.Action d = delegate () {
                LoadRequiredKeybinds();
                CreateCameraFilesIfNotExists();
                CreateAreaAttackPopupMenuIfNotExists();

                LoadModelsFile();

                LoadCostumeFiles();

                LoadSoundFiles();

                ClearTempFilesFromDataFolder();

                // Load camera on start
                camera.ActivateCameraIdentity();

                //LoadMainView();
            };
            Application.Current.Dispatcher.BeginInvoke(d);
        }

        private void LoadRequiredKeybinds()
        {
            CheckRequiredKeybindsFileExists();

            iconInteractionUtility.ExecuteCmd("bind_load_file required_keybinds.txt");
        }

        private void CheckRequiredKeybindsFileExists()
        {
            bool directoryExists = CheckGameDirectory();
            if (!directoryExists)
                SetGameDirectory();

            string dataDir = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME);
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            string filePath = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_KEYBINDS_FILENAME);
            if (!File.Exists(filePath))
            {
                ExtractRequiredKeybindsFile();
            }
        }

        private void ExtractRequiredKeybindsFile()
        {
            File.AppendAllText(
                Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_KEYBINDS_FILENAME),
                Properties.Resources.required_keybinds
                );
        }

        private void CreateCameraFilesIfNotExists()
        {
            string dirData = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME);

            string enableCameraFile = Path.Combine(dirData, GAME_ENABLE_CAMERA_FILENAME);
            string disableCameraFile = Path.Combine(dirData, GAME_DISABLE_CAMERA_FILENAME);

            var assembly = Assembly.GetExecutingAssembly();

            if (!File.Exists(enableCameraFile))
            {
                var resourceName = "Module.HeroVirtualTabletop.Resources.enable_camera.txt";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    File.AppendAllText(
                        enableCameraFile, result
                        );
                }
            }

            if (!File.Exists(disableCameraFile))
            {
                var resourceName = "Module.HeroVirtualTabletop.Resources.disable_camera.txt";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    File.AppendAllText(
                        disableCameraFile, result
                        );
                }
            }
        }

        private void CreateAreaAttackPopupMenuIfNotExists()
        {
            string dirTexts = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_TEXTS_FOLDERNAME);
            if (!Directory.Exists(dirTexts))
                Directory.CreateDirectory(dirTexts);
            string dirLanguage = Path.Combine(dirTexts, GAME_LANGUAGE_FOLDERNAME);
            if (!Directory.Exists(dirLanguage))
                Directory.CreateDirectory(dirLanguage);
            string dirMenus = Path.Combine(dirLanguage, GAME_MENUS_FOLDERNAME);
            if (!Directory.Exists(dirMenus))
                Directory.CreateDirectory(dirMenus);
            string fileAreaAttackMenu = Path.Combine(dirMenus, GAME_AREAATTACK_MENU_FILENAME);
            var assembly = Assembly.GetExecutingAssembly();

            if (!File.Exists(fileAreaAttackMenu))
            {
                var resourceName = "Module.HeroVirtualTabletop.Resources.areaattack.mnu";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    File.AppendAllText(
                        fileAreaAttackMenu, result
                        );
                }
            }
        }

        private void LoadModelsFile()
        {
            string filePath = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_MODELS_FILENAME);
            if (!File.Exists(filePath))
            {
                File.AppendAllText(
                filePath, Properties.Resources.Models
                );
            }
        }

        private void LoadSoundFiles()
        {
            string folderPath = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_SOUND_FOLDERNAME);
            if (Directory.Exists(folderPath))
            {
                return;
            }
        }

        private void LoadCostumeFiles()
        {
            string folderPath = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_COSTUMES_FOLDERNAME);
            if (Directory.Exists(folderPath))
            {
                return;
            }
            else
            {
                var resourceName = "HeroVirtualTabletop.Common.costumes.zip";
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    ZipArchive archive = new ZipArchive(reader.BaseStream);
                    archive.ExtractToDirectory(Properties.Settings.Default.GameDirectory);
                }


            }
        }

        private void ClearTempFilesFromDataFolder()
        {
            string dirPath = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME);
            System.IO.DirectoryInfo di = new DirectoryInfo(dirPath);

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name.Contains(DEFAULT_DELIMITING_CHARACTER_TRANSLATION))
                {
                    file.Delete();
                }
            }
        }

        #endregion

        #endregion
    }
} 
