using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FFXPatchTest {
    public class FFXReloader {
        public static Process GetGameHandle() {
            // Find the fucken game
            var processArray = Process.GetProcessesByName("DarkSoulsIII");
            return processArray.Single();
        }

        // IMPORTANT: this scan doesn't reliably find the result because I'm lazy and didn't want to wait minutes
        // for the AOB to found. So the starting pointer might need some tweaking (find the appropiate spot with CE)
        // if this AOB fails to find the FXR header.
        // TODO: find static pointer to optimize this shit
        public static IntPtr FindBasePointer(Process process, int fxrId) {
            var headerAob = new byte[16];
            Buffer.BlockCopy(new byte[] { 0x46, 0x58, 0x52, 0x00, 0x00, 0x00, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00 }, 0, headerAob, 0, 12);
            Buffer.BlockCopy(BitConverter.GetBytes(fxrId), 0, headerAob, 12, 4);

            var ffxScanner = new MemoryScanner(process, new IntPtr(0x7FF431000000), 0xfffffff);
            var ffxPtr = ffxScanner.FindPattern(headerAob, "xxxxxxxxxxxxxxxx");
            if (ffxPtr == IntPtr.Zero) {
                throw new Exception("Could not find FFX AOB");
            }
            return ffxPtr;
        }

        public static IntPtr FindTablePointer(Process process, IntPtr ffxPtr) {
            // Find the reference to this in-memory FXR file
            var pointerBytes = BitConverter.GetBytes(ffxPtr.ToInt64());
            var tableScanner = new MemoryScanner(process, new IntPtr(0x7FF432407280), 0xfffffff);
            var ffxTablePtr = tableScanner.FindPattern(pointerBytes, "xxxxxxxx");
            if (ffxTablePtr == IntPtr.Zero) {
                throw new Exception("Could not find FFX table pointer AOB");
            }
            return ffxTablePtr;
        }

        public static void SwapPointers(IntPtr process, IntPtr dest, IntPtr replacement) {
            WriteBytes(process, dest, BitConverter.GetBytes(replacement.ToInt64()));
        }

        // public static async Task Reload(IntPtr process, IntPtr ffxPtr, byte[] originalFxrByteArray, byte[] changedFxrByteArray) {
        //     var originalFxr = originalFxrByteArray;
        //     var patchedFxr = changedFxrByteArray;
        //
        //     // Sanity checks
        //     if (originalFxr.Length != patchedFxr.Length) {
        //         throw new NotImplementedException("Bad human! No changing file sizes!");
        //     }
        //
        //     await File.WriteAllBytesAsync("in-memory.fxr", debugDumpFxr(process, ffxPtr));
        //     await File.WriteAllBytesAsync("patched.fxr", patchedFxr);
        //
        //     writeBytes(process, ffxPtr, patchedFxr);
        // }

        // private static byte[] debugDumpFxr(IntPtr process, IntPtr ffxPtr) {
        //     // Grab the end of the in-memory FFX file
        //     // In memory, the 8 bytes preceeding the actual FXR seems to indicate the end of the FXR file
        //     var ffxEndPtr = readPointer(process, ffxPtr - 8);
        //     var inMemoryFxrLength = (long) ffxEndPtr - (long) ffxPtr;
        //     var memoryFxrContents = readBytes(process, ffxPtr, inMemoryFxrLength);
        //     return memoryFxrContents;
        // }

        // private static IntPtr readPointer(IntPtr process, IntPtr ptr) {
        //     return new IntPtr(BitConverter.ToInt64(readBytes(process, ptr, 8)));
        // }

        // private static byte[] readBytes(IntPtr process, IntPtr ptr, long length) {
        //     var bytesRead = 0;
        //     var buffer = new byte[length];
        //     var success = Kernel32.ReadProcessMemory(process, ptr, buffer, buffer.Length, ref bytesRead);
        //     if (!success || bytesRead != buffer.Length) {
        //         throw new Exception("Could not read pointer");
        //     }
        //     return buffer;
        // }

        public static void WriteBytes(IntPtr process, IntPtr ptr, byte[] buffer) {
            Kernel32.WriteProcessMemory(process, ptr, buffer, (int) buffer.Length, out var numWrite);
            if (numWrite != buffer.Length) {
                throw new Exception("Couldn't write bytes");
            }
        }

        public static IntPtr Allocate(IntPtr process, uint size) {
            return Kernel32.VirtualAllocEx(process, IntPtr.Zero, size, Kernel32.AllocationType.COMMIT, Kernel32.Protection.PAGE_EXECUTE_READWRITE);
        }
    }
}

