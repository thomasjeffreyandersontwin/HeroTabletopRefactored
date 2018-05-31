using Caliburn.Micro;
using HeroVirtualTabletop.AnimatedAbility;
using HeroVirtualTabletop.Attack;
using HeroVirtualTabletop.Crowd;
using HeroVirtualTabletop.Desktop;
using HeroVirtualTabletop.ManagedCharacter;
using HeroVirtualTabletop.Movement;
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
using System.Windows.Media;

namespace HeroUI
{
    public class HeroVirtualTabletopMainViewModelImpl : PropertyChangedBase, HeroVirtualTabletopMainViewModel, IShell,
        IHandle<EditIdentityEvent>, IHandle<EditCharacterEvent>, IHandle<EditAnimatedAbilityEvent>, IHandle<EditCharacterMovementEvent>,
        IHandle<ActivateCharacterEvent>, IHandle<ActivateGangEvent>, IHandle<DeActivateCharacterEvent>, IHandle<DeactivateGangEvent>,
        IHandle<ConfigureAttackEvent>, IHandle<CancelAttackEvent>, IHandle<CloseAttackConfigurationWidgetEvent>, IHandle<RepositoryLoadedEvent>, IHandle<WindowClosedEvent>
    {
        #region Private Members
        private IEventAggregator eventAggregator;
        private IconInteractionUtility iconInteractionUtility;
        private Camera camera;
        private PopupService popupService;
        private DesktopContextMenu desktopContextMenu;
        System.Threading.Timer gameInitializeTimer;

        private const string GAME_EXE_FILENAME = "cityofheroes.exe";
        private const string GAME_DATA_FOLDERNAME = "data";
        private const string GAME_DATA_BACKUP_FOLDERNAME = "Backup";
        private const string GAME_KEYBINDS_FILENAME = "required_keybinds.txt";
        private const string GAME_KEYBINDS_ALT_FILENAME = "required_keybinds_alt.txt";
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
                if (value)
                    ActivateWindow(ActiveWindow.CHARACTERS_AND_CROWDS);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.CHARACTERS_AND_CROWDS);
                NotifyOfPropertyChange(() => IsCharacterExplorerExpanded);
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
                if (value)
                    ActivateWindow(ActiveWindow.ROSTER);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.ROSTER);
                NotifyOfPropertyChange(() => IsRosterExplorerExpanded);
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
                if (value)
                    ActivateWindow(ActiveWindow.CHARACTER_ACTION_GROUPS);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.CHARACTER_ACTION_GROUPS);
                NotifyOfPropertyChange(() => IsCharacterEditorExpanded);
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
                if (value)
                    ActivateWindow(ActiveWindow.ROSTER);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.ROSTER);
                NotifyOfPropertyChange(() => IsIdentityEditorExpanded);
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
                if (value)
                    ActivateWindow(ActiveWindow.ABILITIES);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.ABILITIES);
                NotifyOfPropertyChange(() => IsAbilityEditorExpanded);
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
                if (value)
                    ActivateWindow(ActiveWindow.MOVEMENTS);
                else
                    ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.MOVEMENTS);
                NotifyOfPropertyChange(() => IsMovementEditorExpanded);
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

        private CharacterEditorViewModel characterEditorViewModel;
        public CharacterEditorViewModel CharacterEditorViewModel
        {
            get
            {
                return characterEditorViewModel;
            }
            set
            {
                characterEditorViewModel = value;
                NotifyOfPropertyChange(() => CharacterEditorViewModel);
            }
        }

        private IdentityEditorViewModel identityEditorViewModel;
        public IdentityEditorViewModel IdentityEditorViewModel
        {
            get
            {
                return identityEditorViewModel;
            }
            set
            {
                identityEditorViewModel = value;
                NotifyOfPropertyChange(() => IdentityEditorViewModel);
            }
        }

        private AbilityEditorViewModel abilityEditorViewModel;
        public AbilityEditorViewModel AbilityEditorViewModel
        {
            get
            {
                return abilityEditorViewModel;
            }
            set
            {
                abilityEditorViewModel = value;
                NotifyOfPropertyChange(() => AbilityEditorViewModel);
            }
        }

        private MovementEditorViewModel movementEditorViewModel;
        public MovementEditorViewModel MovementEditorViewModel
        {
            get
            {
                return movementEditorViewModel;
            }
            set
            {
                movementEditorViewModel = value;
                NotifyOfPropertyChange(() => MovementEditorViewModel);
            }
        }

        private ActiveCharacterWidgetViewModel activeCharacterWidgetViewModel;
        public ActiveCharacterWidgetViewModel ActiveCharacterWidgetViewModel
        {
            get
            {
                return activeCharacterWidgetViewModel;
            }
            set
            {
                activeCharacterWidgetViewModel = value;
                NotifyOfPropertyChange(() => ActiveCharacterWidgetViewModel);
            }
        }

        private AttackConfigurationWidgetViewModel attackConfigurationWidgetViewModel;
        public AttackConfigurationWidgetViewModel AttackConfigurationWidgetViewModel
        {
            get
            {
                return attackConfigurationWidgetViewModel;
            }
            set
            {
                attackConfigurationWidgetViewModel = value;
                NotifyOfPropertyChange(() => AttackConfigurationWidgetViewModel);
            }
        }

        #endregion

        #region Constructor
        public HeroVirtualTabletopMainViewModelImpl(IEventAggregator eventAggregator, CrowdMemberExplorerViewModel crowdMemberExplorerViewModel, 
            RosterExplorerViewModel rosterExplorerViewModel, CharacterEditorViewModel characterEditorViewModel, IdentityEditorViewModel identityEditorViewModel,
            AbilityEditorViewModel abilityEditorViewModel, MovementEditorViewModel movementEditorViewModel, ActiveCharacterWidgetViewModel activeCharacterWidgetViewModel, 
            AttackConfigurationWidgetViewModel attackConfigurationWidgetViewModel, PopupService popupService,
            IconInteractionUtility iconInteractionUtility, DesktopContextMenu desktopContextMenu, Camera camera)
        {
            this.eventAggregator = eventAggregator;
            this.CrowdMemberExplorerViewModel = crowdMemberExplorerViewModel;
            this.RosterExplorerViewModel = rosterExplorerViewModel;
            this.CharacterEditorViewModel = characterEditorViewModel;
            this.IdentityEditorViewModel = identityEditorViewModel;
            this.AbilityEditorViewModel = abilityEditorViewModel;
            this.MovementEditorViewModel = movementEditorViewModel;
            this.ActiveCharacterWidgetViewModel = activeCharacterWidgetViewModel;
            this.AttackConfigurationWidgetViewModel = attackConfigurationWidgetViewModel;
            this.iconInteractionUtility = iconInteractionUtility;
            this.camera = camera;
            this.popupService = popupService;
            this.desktopContextMenu = desktopContextMenu;
            RegisterPopups();
            gameInitializeTimer = new System.Threading.Timer(gameInitializeTimer_Callback, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            LaunchGame();
            this.eventAggregator.Subscribe(this);
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

        private void RegisterPopups()
        {
            this.popupService.Register("ActiveCharacterWidgetView", typeof(ActiveCharacterWidgetView));
            this.popupService.Register("AttackConfigurationWidgetView", typeof(AttackConfigurationWidgetView));
        }
       
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

        private void SetHeroVirtualTabletopGameRunningDirectory()
        {
            HeroVirtualTabletop.Common.HeroVirtualTabletopGame.RunningDirectory = Properties.Settings.Default.GameDirectory;
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
                DeleteOldBackupFiles();

                // Load camera on start
                camera.ActivateCameraIdentity();

                SetHeroVirtualTabletopGameRunningDirectory();

                ConfigureDesktopContextMenu();
                //LoadMainView();
            };
            Application.Current.Dispatcher.BeginInvoke(d);
        }

        private void ConfigureDesktopContextMenu()
        {
            DesktopContextMenuImpl.GameDirectoryPath = Properties.Settings.Default.GameDirectory;
            this.desktopContextMenu.Configure();
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
            File.AppendAllText(
                Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_KEYBINDS_ALT_FILENAME),
                Properties.Resources.required_keybinds_alt
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
                var resourceName = "HeroVirtualTabletop.ManagedCharacter.enable_camera.txt";

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
                var resourceName = "HeroVirtualTabletop.ManagedCharacter.disable_camera.txt";

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
                var resourceName = "HeroVirtualTabletop.Desktop.areaattack.mnu";

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
                var resourceName = "HeroVirtualTabletop.ManagedCharacter.costumes.zip";
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

        public void Handle(RepositoryLoadedEvent message)
        {
            //this.TakeWorkingRepoBackup(message.RepositoryPath);
        }
        private void TakeWorkingRepoBackup(string crowdRepositoryPath)
        {
            string backupDir = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_DATA_BACKUP_FOLDERNAME);
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);
            string backupFilePath = Path.Combine(backupDir, "Refactored_CrowdRepository_Backup" + String.Format("{0:MMddyyyy}", DateTime.Today) + ".data");
            if (!File.Exists(backupFilePath))
            {
                File.Copy(crowdRepositoryPath, backupFilePath, true);
            }

        }
        private void DeleteOldBackupFiles()
        {
            string backupDir = Path.Combine(Properties.Settings.Default.GameDirectory, GAME_DATA_FOLDERNAME, GAME_DATA_BACKUP_FOLDERNAME);
            string[] files = Directory.GetFiles(backupDir);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
                    fi.Delete();
            }
        }

        #endregion

        #region Open Editors

        public void Handle(EditIdentityEvent message)
        {
            this.IsIdentityEditorExpanded = true;
        }

        public void Handle(EditCharacterEvent message)
        {
            this.IsCharacterEditorExpanded = true;
        }

        public void Handle(EditAnimatedAbilityEvent message)
        {
            this.IsAbilityEditorExpanded = true;
        }
        public void Handle(EditCharacterMovementEvent message)
        {
            this.IsMovementEditorExpanded = true;
        }
        #endregion

        #region Open/Close Popups

        public void Handle(ActivateCharacterEvent message)
        {
            ShowActivateCharacterWidgetPopup(message.ActivatedCharacter, message.SelectedActionGroupName, message.SelectedActionName);
        }

        public void Handle(ActivateGangEvent message)
        {
            ShowActivateGangWidgetPopup(message.GangMembers);
        }

        public void Handle(ConfigureAttackEvent message)
         {
            ShowAttackConfigurationWidgetPopup();
        }

        public void Handle(CancelAttackEvent message)
        {
            this.CloseAttackConfigurationWidgetPopup();
        }

        public void Handle(CloseAttackConfigurationWidgetEvent message)
        {
            this.CloseAttackConfigurationWidgetPopup();
        }

        public void Handle(DeActivateCharacterEvent message)
        {
            this.CloseActiveCharacterWidgetPopup(message.DeActivatedCharacter);
        }
        public void Handle(DeactivateGangEvent message)
        {
            this.CloseActiveCharacterWidgetPopup(message.DeactivatedGangLeader);
        }
        private void ShowActivateCharacterWidgetPopup(ManagedCharacter character, string optionGroupName, string optionName)
        {
            if (character != null && character.IsActive)
            {
                OpenActivateCharacterWidgetPopup(character);
                this.eventAggregator.PublishOnUIThread(new ShowActivateCharacterWidgetEvent(character, optionGroupName, optionName));
            }
            else if ((character != null && !character.IsActive) && popupService.IsOpen("ActiveCharacterWidgetView"))
            {
                this.CloseActiveCharacterWidgetPopup(character);
            }
        }

        private void OpenActivateCharacterWidgetPopup(ManagedCharacter activatedCharacter)
        {
            if (!popupService.IsOpen("ActiveCharacterWidgetView"))
            {
                System.Windows.Style style = ControlUtilities.GetCustomWindowStyle();
                double minwidth = 80;
                style.Setters.Add(new Setter(Window.MinWidthProperty, minwidth));
                var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
                double left = desktopWorkingArea.Right - 500;
                double top = desktopWorkingArea.Bottom - 80 * activatedCharacter.CharacterActionGroups.Count;
                object savedPos = popupService.GetPosition("ActiveCharacterWidgetView", activatedCharacter.Name);
                if (savedPos != null)
                {
                    double[] posArray = (double[])savedPos;
                    left = posArray[0];
                    top = posArray[1];
                }
                style.Setters.Add(new Setter(Window.LeftProperty, left));
                style.Setters.Add(new Setter(Window.TopProperty, top));
                popupService.ShowDialog("ActiveCharacterWidgetView", ActiveCharacterWidgetViewModel, "", false, null, new SolidColorBrush(Colors.Transparent), style, WindowStartupLocation.Manual);
                ActivateWindow(ActiveWindow.ACTIVE_CHARACTER);
            }
        }
        private void ShowActivateGangWidgetPopup(List<ManagedCharacter> gangMembers)
        {
            ManagedCharacter gangLeader = gangMembers.FirstOrDefault(gm => gm.IsGangLeader);
            OpenActivateCharacterWidgetPopup(gangLeader);
            this.eventAggregator.PublishOnUIThread(new ShowActivateGangWidgetEvent(gangMembers));
        }
        private void ShowAttackConfigurationWidgetPopup()
        {
            if (!popupService.IsOpen("AttackConfigurationWidgetView"))
            {
                System.Windows.Style style = ControlUtilities.GetCustomWindowStyle();
                popupService.ShowDialog("AttackConfigurationWidgetView", AttackConfigurationWidgetViewModel, "", false, null, new SolidColorBrush(Colors.Transparent), style);
                ActivateWindow(ActiveWindow.ATTACK);
            }
        }

        private void CloseActiveCharacterWidgetPopup(ManagedCharacter character)
        {
            popupService.SavePosition("ActiveCharacterWidgetView", character != null ? character.Name : null);
            popupService.CloseDialog("ActiveCharacterWidgetView");
            ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.ACTIVE_CHARACTER);
        }

        private void CloseAttackConfigurationWidgetPopup()
        {
            popupService.CloseDialog("AttackConfigurationWidgetView");
            ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow.ATTACK);
        }

        #endregion

        #region Set Active Window

        public void Handle(WindowClosedEvent message)
        {
            ActivateAnotherWindowAfterCollapsingCurrentOne(message.ClosedWindow);
        }

        public void ActivateWindow(string windowName)
        {
            ActiveWindow window = (ActiveWindow)Enum.Parse(typeof(ActiveWindow), windowName);
            ActivateWindow(window);
        }
        public void ActivateWindow(ActiveWindow window)
        {
            DesktopFocusManager.CurrentActiveWindow = window;
        }

        private void ActivateAnotherWindowAfterCollapsingCurrentOne(ActiveWindow collapsingWIndow)
        {
            if (popupService.IsOpen("AttackConfigurationWidgetView"))
            {
                DesktopFocusManager.CurrentActiveWindow = ActiveWindow.ATTACK;
            }
            else if (popupService.IsOpen("ActiveCharacterWidgetView"))
            {
                DesktopFocusManager.CurrentActiveWindow = ActiveWindow.ACTIVE_CHARACTER;
            }
            else if (IsRosterExplorerExpanded)
            {
                DesktopFocusManager.CurrentActiveWindow = ActiveWindow.ROSTER;
            }
            else if (IsCharacterEditorExpanded)
            {
                DesktopFocusManager.CurrentActiveWindow = ActiveWindow.CHARACTER_ACTION_GROUPS;
            }
            else if (IsCharacterExplorerExpanded)
            {
                DesktopFocusManager.CurrentActiveWindow = ActiveWindow.CHARACTERS_AND_CROWDS;
            }
            else if (IsAbilityEditorExpanded)
            {
                var abilityEditorVM = IoC.Get<AbilityEditorViewModel>();
                if (abilityEditorVM.IsShowingAbilityEditor)
                    DesktopFocusManager.CurrentActiveWindow = ActiveWindow.ABILITIES;
            }
            else if (IsMovementEditorExpanded)
            {
                var movementEditorVM = IoC.Get<MovementEditorViewModel>();
                if (movementEditorVM.IsShowingMovementEditor)
                    DesktopFocusManager.CurrentActiveWindow = ActiveWindow.MOVEMENTS;
            }
        }

        #endregion

        #endregion
    }
} 
