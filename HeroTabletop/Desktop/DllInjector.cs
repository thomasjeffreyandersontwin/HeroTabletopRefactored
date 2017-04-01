using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HeroVirtualTableTop.Desktop
{
    public enum DllInjectionResult
    {
        DllNotFound,
        GameProcessNotFound,
        InjectionFailed,
        Success
    }

    public sealed class DllInjector
    {
        private static readonly IntPtr INTPTR_ZERO = (IntPtr) 0;

        private static DllInjector _instance;

        private DllInjector()
        {
        }

        public static DllInjector GetInstance => _instance ?? (_instance = new DllInjector());

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize,
            uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size,
            int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);


        public DllInjectionResult Inject(string sProcName, string sDllPath)
        {
            if (!File.Exists(sDllPath))
            {
                return DllInjectionResult.DllNotFound;
            }

            uint _procId = 0;

            Process[] _procs = Process.GetProcesses();
            foreach (Process t in _procs)
            {
                if (t.ProcessName == sProcName)
                {
                    _procId = (uint)t.Id;
                    break;
                }
            }

            if (_procId == 0)
            {
                return DllInjectionResult.GameProcessNotFound;
            }

            if (!bInject(_procId, sDllPath))
            {
                return DllInjectionResult.InjectionFailed;
            }

            return DllInjectionResult.Success;
        }

        

        private bool bInject(uint pToBeInjected, string sDllPath)
        {
            var hndProc = OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, 1, pToBeInjected);

            if (hndProc == INTPTR_ZERO)
                return false;

            var lpLLAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (lpLLAddress == INTPTR_ZERO)
                return false;

            var lpAddress = VirtualAllocEx(hndProc, (IntPtr) null, (IntPtr) sDllPath.Length, 0x1000 | 0x2000, 0X40);

            if (lpAddress == INTPTR_ZERO)
                return false;

            var bytes = Encoding.ASCII.GetBytes(sDllPath);

            if (WriteProcessMemory(hndProc, lpAddress, bytes, (uint) bytes.Length, 0) == 0)
                return false;

            if (CreateRemoteThread(hndProc, (IntPtr) null, INTPTR_ZERO, lpLLAddress, lpAddress, 0, (IntPtr) null) ==
                INTPTR_ZERO)
                return false;

            CloseHandle(hndProc);

            return true;
        }
    }
}