using System;
using System.IO;
using System.Reflection;

namespace BabyCLARA.FlowAnalyzerInterface
{
    class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream stream) : base(stream) { }

        public override short ReadInt16()
        {
            byte[] data = base.ReadBytes(sizeof(short));
            Array.Reverse(data);

            return BitConverter.ToInt16(data, 0);
        }

        public override int ReadInt32()
        {
            byte[] data = base.ReadBytes(sizeof(int));
            Array.Reverse(data);

            return BitConverter.ToInt32(data, 0);
        }

        public override long ReadInt64()
        {
            byte[] data = base.ReadBytes(sizeof(long));
            Array.Reverse(data);

            return BitConverter.ToInt64(data, 0);
        }

        public override ushort ReadUInt16()
        {
            byte[] data = base.ReadBytes(sizeof(ushort));
            Array.Reverse(data);

            return BitConverter.ToUInt16(data, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] data = base.ReadBytes(sizeof(uint));
            Array.Reverse(data);

            return BitConverter.ToUInt32(data, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] data = base.ReadBytes(sizeof(ulong));
            Array.Reverse(data);

            return BitConverter.ToUInt64(data, 0);
        }

        public override float ReadSingle()
        {
            byte[] data = base.ReadBytes(sizeof(float));
            Array.Reverse(data);

            return BitConverter.ToSingle(data, 0);
        }

        public override double ReadDouble()
        {
            byte[] data = base.ReadBytes(sizeof(double));
            Array.Reverse(data);

            return BitConverter.ToDouble(data, 0);
        }
    }

    class BigEndianBinaryWriter : BinaryWriter
    {
        public BigEndianBinaryWriter(Stream stream) : base(stream) { }

        public override void Write(short value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }

        public override void Write(double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);

            base.Write(data);
        }
    }

    static class Util
    {
        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);

            return new FileInfo(location.AbsolutePath).Directory.FullName;
        }
    }
}
