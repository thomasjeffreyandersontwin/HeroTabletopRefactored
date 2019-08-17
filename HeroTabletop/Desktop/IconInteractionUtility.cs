using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;

namespace HeroVirtualTabletop.Desktop
{
    public class IconInteractionUtilityImpl : IconInteractionUtility
    {
        private static IntPtr dllHandle;
        private static InitGame initGame;
        private static CloseGame closeGame;
        private static SetUserHWND setUserHWnd;
        private static ExecuteCommand executeCmd;
        private static GetHoveredNPCInfo getHoveredNPCInfo;
        private static GetMouseXYZInGame getMouseXYZInGame;
        private static CheckIfGameLoaded checkIfGameLoaded;
        private static DetectCollision detectCollision;

        public Vector3 Start { get; set; }
        public Vector3 Destination { get; set; }
        public Vector3 Collision
        {
            get
            {
                return GetCollision(this.Start, this.Destination);
            }
            set { throw new NotImplementedException(); }
        }

        public virtual Vector3 GetCollision(Vector3 start, Vector3 destination)
        {
            string collisionInfo = Marshal.PtrToStringAnsi(detectCollision(start.X, start.Y, start.Z, destination.X, destination.Y, destination.Z));

            float X = 0f, Y = 0f, Z = 0f;
            try
            {
                int indexXStart = collisionInfo.IndexOf("[");
                int indexXEnd = collisionInfo.IndexOf("]");
                string xStr = collisionInfo.Substring(indexXStart + 1, indexXEnd - indexXStart - 1);
                X = float.Parse(xStr);

                int indexYStart = collisionInfo.IndexOf("[", indexXEnd);
                int indexYEnd = collisionInfo.IndexOf("]", indexYStart);
                string yStr = collisionInfo.Substring(indexYStart + 1, indexYEnd - indexYStart - 1);
                Y = float.Parse(yStr);

                int indexZStart = collisionInfo.IndexOf("[", indexYEnd);
                int indexZEnd = collisionInfo.IndexOf("]", indexZStart);
                string zStr = collisionInfo.Substring(indexZStart + 1, indexZEnd - indexZStart - 1);
                Z = float.Parse(zStr);
            }
            catch (Exception ex)
            {
            }

            return new Vector3(X, Y, Z);
        }

        public void InitializeDlll(string path)
        {
            //TO DO FIX 
            dllHandle = WindowsUtilities.LoadLibrary(Path.Combine(path, "HookCostume.dll"));
            if (dllHandle != null)
            {
                var initGameAddress = WindowsUtilities.GetProcAddress(dllHandle, "InitGame");
                if (initGameAddress != IntPtr.Zero)
                    initGame = (InitGame) Marshal.GetDelegateForFunctionPointer(initGameAddress, typeof(InitGame));

                var closeGameAddress = WindowsUtilities.GetProcAddress(dllHandle, "CloseGame");
                if (closeGameAddress != IntPtr.Zero)
                    closeGame = (CloseGame) Marshal.GetDelegateForFunctionPointer(closeGameAddress, typeof(CloseGame));

                var setUserHWndAddress = WindowsUtilities.GetProcAddress(dllHandle, "SetUserHWND");
                if (setUserHWndAddress != IntPtr.Zero)
                    setUserHWnd =
                        (SetUserHWND) Marshal.GetDelegateForFunctionPointer(setUserHWndAddress, typeof(SetUserHWND));

                var executeCmdAddress = WindowsUtilities.GetProcAddress(dllHandle, "ExecuteCommand");
                if (executeCmdAddress != IntPtr.Zero)
                    executeCmd =
                        (ExecuteCommand)
                        Marshal.GetDelegateForFunctionPointer(executeCmdAddress, typeof(ExecuteCommand));

                var getHoveredNPCInfoAddress = WindowsUtilities.GetProcAddress(dllHandle, "GetHoveredNPCInfo");
                if (getHoveredNPCInfoAddress != IntPtr.Zero)
                    getHoveredNPCInfo =
                        (GetHoveredNPCInfo)
                        Marshal.GetDelegateForFunctionPointer(getHoveredNPCInfoAddress, typeof(GetHoveredNPCInfo));

                var getMouseXYZInGameAddress = WindowsUtilities.GetProcAddress(dllHandle, "GetMouseXYZInGame");
                if (getMouseXYZInGameAddress != IntPtr.Zero)
                    getMouseXYZInGame =
                        (GetMouseXYZInGame)
                        Marshal.GetDelegateForFunctionPointer(getMouseXYZInGameAddress, typeof(GetMouseXYZInGame));

                var checkGameDoneAddress = WindowsUtilities.GetProcAddress(dllHandle, "CheckGameDone");
                if (checkGameDoneAddress != IntPtr.Zero)
                    checkIfGameLoaded =
                        (CheckIfGameLoaded)
                        Marshal.GetDelegateForFunctionPointer(checkGameDoneAddress, typeof(CheckIfGameLoaded));

                var detectCollisionAddress = WindowsUtilities.GetProcAddress(dllHandle, "CollisionDetection");
                if (checkGameDoneAddress != IntPtr.Zero)
                    detectCollision =
                        (DetectCollision)
                        Marshal.GetDelegateForFunctionPointer(detectCollisionAddress, typeof(DetectCollision));
            }
        }

        public void RunCOHAndLoadDLL(string path)
        {
            initGame(1, path);
            while (true)
            {
                var gameLoaded = checkIfGameLoaded();
                if (gameLoaded)
                    break;
                Thread.Sleep(1000);
            }
            Thread.Sleep(1500);
            setUserHWnd(IntPtr.Zero);
        }

        public void ExecuteCmd(string command)
        {
            if (command.Length > 254)
            {
                var parsedCmd = command;
                var position = 0;
                while (parsedCmd.Length > 254)
                {
                    parsedCmd = parsedCmd.Substring(0, parsedCmd.LastIndexOf("$$", 254));
                    executeCmd("/" + parsedCmd);
                    Thread.Sleep(20); // Sleep a while after executing a command
                    position += parsedCmd.Length + 2;
                    parsedCmd = command.Substring(position);
                }
                executeCmd("/" + parsedCmd);
            }
            else
            {
                executeCmd("/" + command);
            }
        }

        public string GeInfoFromNpcMouseIsHoveringOver()
        {
            return Marshal.PtrToStringAnsi(getHoveredNPCInfo());
        }

        public string GetMouseXYZString()
        {
            Thread.Sleep(100);
            return Marshal.PtrToStringAnsi(getMouseXYZInGame());
        }

        public void CloseCOH()
        {
            closeGame(IntPtr.Zero);
        }

        public void InitializeGame(string path)
        {
            InitializeDlll(path);
            initGame(1, path);
        }

        public bool IsGameLoaded()
        {
            bool gameLoaded = checkIfGameLoaded();
            return gameLoaded;
        }

        public void DoPostInitialization()
        {
            System.Threading.Thread.Sleep(1500);
            setUserHWnd(IntPtr.Zero);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //private delegate bool InitGame(IntPtr hWnd);
        private delegate bool InitGame(int x, [MarshalAs(UnmanagedType.LPStr)] string gamePath);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CloseGame(IntPtr hWnd);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool SetUserHWND(IntPtr hWnd);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ExecuteCommand([MarshalAs(UnmanagedType.LPStr)] string commandline);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetCOHPath([MarshalAs(UnmanagedType.LPStr)] string path);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetHoveredNPCInfo();

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetMouseXYZInGame();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CheckIfGameLoaded();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate IntPtr DetectCollision(
            float sourceX, float sourceY, float sourceZ, float destX, float destY, float destZ);
    }
}