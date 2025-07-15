﻿using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FATXTools.Utilities
{
    public static class WinApi
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string FileName,
            FileAccess DesiredAccess,
            FileShare ShareMode,
            IntPtr SecurityAttributes,
            FileMode CreationDisposition,
            int FlagsAndAttributes,
            IntPtr Template);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object lpInBuffer,
            int nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object lpOutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr lpOverlapped
            );

        public static long GetDiskCapactity(SafeFileHandle diskHandle)
        {
            byte[] sizeBytes = new byte[8];
            int bytesRet = sizeBytes.Length;
            if (!DeviceIoControl(diskHandle, 0x00000007405C, null, 0, sizeBytes, bytesRet, ref bytesRet, IntPtr.Zero))
            {
                throw new Exception("Failed to get disk size!");
            }
            return BitConverter.ToInt64(sizeBytes, 0);
        }

        public static long GetSectorSize(SafeFileHandle diskHandle)
        {
            byte[] buf = new byte[0x18];
            int bytesRet = buf.Length;
            if (!DeviceIoControl(diskHandle, 0x000000070000, null, 0, buf, bytesRet, ref bytesRet, IntPtr.Zero))
            {
                throw new Exception("Failed to get disk geometry!");
            }
            return BitConverter.ToInt32(buf, 0x14);
        }
    }
}
