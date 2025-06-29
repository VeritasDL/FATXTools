using FATX;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace FATXTools.DiskTypes
{
    public class PhysicalDisk : DriveReader
    {
        private long _length;
        private long _sectorLength;
        private long _position;
        public PhysicalDisk(SafeFileHandle handle, long length, long sectorLength)
            : base(new FileStream(handle, FileAccess.ReadWrite))
        {
            this._length = length;
            this._sectorLength = sectorLength;
            this.Initialize();
        }

        public override long Length => _length;

        public override long Position => _position;

        public override long Seek(long offset)
        {
            _position = offset;
            return _position;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = _length - offset;
                    break;
            }
            return _position;
        }

        public override void Read(byte[] buffer, int count)
        {
            // Align position down to nearest sector
            var offset = _position;
            if (_position % _sectorLength != 0)
            {
                offset -= _position % _sectorLength;
            }

            // Then seek to the sector aligned offset
            BaseStream.Seek(offset, SeekOrigin.Begin);

            // Now read bytes of sector size
            var alignedCount = (int)(((count + (_sectorLength - 1)) & ~(_sectorLength - 1)));
            var tempBuf = new byte[alignedCount];
            BaseStream.Read(tempBuf, 0, alignedCount);

            // Only copy the bytes we need
            Buffer.BlockCopy(tempBuf, (int)(_position % _sectorLength), buffer, 0, count);

            // Increment the position by how much we took
            _position += count;
        }

        public override byte[] ReadBytes(int count)
        {
            byte[] buf = new byte[count];
            Read(buf, count);
            return buf;
        }

        public override byte ReadByte()
        {
            byte[] buf = new byte[1];
            Read(buf, 1);
            return buf[0];
        }
        public virtual void Write(byte[] buffer, long offset, int count)
        {
            BaseStream.Seek(offset, SeekOrigin.Begin);

            //// Read the full sector into a temp buffer (for partial sector writes)
            //var alignedCount = (int)(((count + (_sectorLength - 1)) & ~(_sectorLength - 1)));
            //var tempBuf = new byte[alignedCount];

            //// Read existing data (in case of partial sector write)
            //BaseStream.Read(tempBuf, 0, alignedCount);

            //// Copy new data into the temp buffer at the correct offset
            //Buffer.BlockCopy(buffer, 0, tempBuf, (int)(_position % _sectorLength), count);

            //// Seek back to the aligned offset for writing
            BaseStream.Seek(offset, SeekOrigin.Begin);

            //// Write the modified buffer back
            BaseStream.Write(buffer, 0, count);
            BaseStream.Flush();

            // Increment position by how much was written
            _position += count;
        }

        //public virtual void WriteBytes(byte[] buffer)
        //{
        //    Write(buffer, buffer.Length);
        //}

        //public virtual void WriteByte(byte value)
        //{
        //    Write(new byte[] { value }, 1);
        //}
       // public void Write(byte[] buffer, int offset, int count)
       // {
       //     Align position down to nearest sector
       //     var offseta = _position;
       //     if (_position % _sectorLength != 0)
       //     {
       //         offseta -= _position % _sectorLength;
       //     }

       //     Seek to the sector aligned offset
       //     BaseStream.Seek(offseta, SeekOrigin.Begin);

       //     Read the full sector into a temp buffer(for partial sector writes)
       //     var alignedCount = (int)(((count + (_sectorLength - 1)) & ~(_sectorLength - 1)));
       // var tempBuf = new byte[alignedCount];

       // Read existing data(in case of partial sector write)
       //     BaseStream.Read(tempBuf, 0, alignedCount);

       //      Copy new data into the temp buffer at the correct offset
       //     Buffer.BlockCopy(buffer, 0, tempBuf, (int)(_position % _sectorLength), count);

       //      Seek back to the aligned offset for writing
       //     BaseStream.Seek(offseta, SeekOrigin.Begin);

       // Write the modified buffer back
       //BaseStream.Write(tempBuf, 0, alignedCount);
       // BaseStream.Flush();

       //      Increment position by how much was written
       //     _position += count;
       // }

       // public virtual void WriteBytes(byte[] buffer)
       // {
       //     int offset = ((int)this.BaseStream.Position);
       //     Write(buffer, offset, buffer.Length);
       // }

       // public virtual void WriteByte(byte value)
       // {
       //     int offset = ((int)this.BaseStream.Position);
       //     Write(new byte[] { value }, offset, 1);
       // }
    }
}
