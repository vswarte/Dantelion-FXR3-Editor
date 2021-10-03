using System;
using System.Runtime.InteropServices;

namespace FFXPatchTest {
    public static class Kernel32 {
        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize,
            out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
            AllocationType flAllocationType, Protection flProtect);

        public enum AllocationType {
            COMMIT = 0x00001000,
        }

        public enum Protection {
            PAGE_EXECUTE_READWRITE = 0x40,
        }
    }
}