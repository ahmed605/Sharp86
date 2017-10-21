﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp86;

namespace Sharp86UnitTests
{
    public class CPUUnitTests : CPU, IMemoryBus, IPortBus
    {
        public CPUUnitTests()
        {
            MemoryBus = this;
            PortBus = this;
        }

        byte[] _mem;
        ushort _emitLocation;
        StringBuilder _emitBuffer = new StringBuilder();

        [TestInitialize]
        public override void Reset()
        {
            base.Reset();

            _emitLocation = 0x100;
            _emitBuffer.Length = 0;
            _emitBuffer.Append("ORG 0x100\n");
            ip = 0x100;
            _mem = new byte[0x20000];
        }

        public bool IsExecutableSelector(ushort seg)
        {
            return true;
        }

        public byte ReadByte(ushort seg, ushort offset)
        {
            var addr = (seg << 4) + offset;
            var b = _mem[addr];
            return b;
        }

        public void WriteByte(ushort seg, ushort offset, byte value)
        {
            var addr = (seg << 4) + offset;
            _mem[addr] = value;
        }

        protected void emit(string asm)
        {
            _emitBuffer.Append(asm);
            _emitBuffer.Append("\n");
        }

        void flushEmit()
        {
            if (_emitBuffer.Length == 0)
                return;

            // write the text
            System.IO.File.WriteAllText("temp.asm", _emitBuffer.ToString(), Encoding.ASCII);

            // Execute the assembler
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\users\brad\dropbox\wintools\yasm.exe",
                Arguments = "temp.asm -o temp.bin -l temp.lst",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            var process = Process.Start(processStartInfo);
            //var output = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.Format("YASM failed"));


            /*
            0 -> 0
            1 -> 3
            2 -> 2
            3 -> 1
            */

            // Read the generated bytes
            var bytes = System.IO.File.ReadAllBytes("temp.bin");
            for (int i=(4-(_emitLocation % 4))%4; i<bytes.Length; i++)
            {
                WriteByte(0, _emitLocation++, bytes[i]);
            }

            // Clear the emit buffer
            _emitBuffer.Length = 0;
            _emitBuffer.AppendFormat("ORG {0:X}H\n", _emitLocation);
        }

        protected void run()
        {
            flushEmit();
            while (ip != _emitLocation)
            {
                Step();
            }
        }
        protected void step()
        {
            flushEmit();
            Step();
        }

        public virtual ushort ReadPortWord(ushort port)
        {
            return 0;
        }

        public virtual void WritePortWord(ushort port, ushort value)
        {
        }

        public ushort ReadWord(ushort seg, ushort offset)
        {
            return (ushort)(ReadByte(seg, offset) | ReadByte(seg, (ushort)(offset + 1)) << 8);
        }

        public void WriteWord(ushort seg, ushort offset, ushort value)
        {
            WriteByte(seg, offset, (byte)(value & 0xFF));
            WriteByte(seg, (ushort)(offset + 1), (byte)(value >> 8));
        }

    }
}
