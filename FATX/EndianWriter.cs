using System;
using System.IO;

namespace FATX
{
    public class EndianWriter : BinaryWriter
    {
        private ByteOrder byteOrder;
        public EndianWriter(Stream stream, ByteOrder byteOrder)
            : base(stream)
        {
            this.byteOrder = byteOrder;
        }
        public EndianWriter(Stream stream)
            : base(stream)
        {
            this.byteOrder = ByteOrder.Little;
        }
        public ByteOrder ByteOrder
        {
            get { return this.byteOrder; }
            set { this.byteOrder = value; }
        }
        public virtual long Length
        {
            get { return BaseStream.Length; }
        }
        public virtual long Position
        {
            get { return BaseStream.Position; }
        }
        public virtual long Seek(long offset)
        {
            BaseStream.Position = offset;
            return BaseStream.Position;
        }
        public virtual long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }
        public virtual void Write(byte[] buffer, int count)
        {
            BaseStream.Write(buffer, 0, count);
        }
        public override void Write(short value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 2);
        }
        public override void Write(ushort value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 2);
        }
        public override void Write(int value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 4);
        }
        public override void Write(uint value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 4);
        }
        public override void Write(long value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 8);
        }
        public override void Write(ulong value)
        {
            var temp = BitConverter.GetBytes(value);
            if (byteOrder == ByteOrder.Big)
            {
                Array.Reverse(temp);
            }
            base.Write(temp, 0, 8);
        }
    }
}