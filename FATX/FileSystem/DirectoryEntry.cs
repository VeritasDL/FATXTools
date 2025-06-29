using System;
using System.Collections.Generic;
using System.Text;

namespace FATX.FileSystem
{
    public class DirectoryEntry
    {
        private byte _fileNameLength;
        private byte _fileAttributes;
        private byte[] _fileNameBytes;
        private uint _firstCluster;
        private uint _fileSize;
        private uint _creationTimeAsInt;
        private uint _lastWriteTimeAsInt;
        private uint _lastAccessTimeAsInt;
        private TimeStamp _creationTime;
        private TimeStamp _lastWriteTime;
        private TimeStamp _lastAccessTime;
        private string _fileName;

        private DirectoryEntry _parent;
        private List<DirectoryEntry> _children = new List<DirectoryEntry>();
        private uint _cluster;
        private long _offset;

        public DirectoryEntry(Platform platform, byte[] data, int offset)
        {
            this._parent = null;

            ReadDirectoryEntry(platform, data, offset); //offset is the offset in the data array where this dirent starts 
        }

        private void ReadDirectoryEntry(Platform platform, byte[] data, int offset)
        {
            this._fileNameLength = data[offset + 0];
            this._fileAttributes = data[offset + 1];

            this._fileNameBytes = new byte[42];
            Buffer.BlockCopy(data, offset + 2, this._fileNameBytes, 0, 42);

            if (platform == Platform.Xbox)
            {
                this._firstCluster = BitConverter.ToUInt32(data, offset + 0x2C);
                this._fileSize = BitConverter.ToUInt32(data, offset + 0x30);
                this._creationTimeAsInt = BitConverter.ToUInt32(data, offset + 0x34);
                this._lastWriteTimeAsInt = BitConverter.ToUInt32(data, offset + 0x38);
                this._lastAccessTimeAsInt = BitConverter.ToUInt32(data, offset + 0x3C);
                this._creationTime = new XTimeStamp(this._creationTimeAsInt);
                this._lastWriteTime = new XTimeStamp(this._lastWriteTimeAsInt);
                this._lastAccessTime = new XTimeStamp(this._lastAccessTimeAsInt);
            }
            else if (platform == Platform.X360)
            {
                Array.Reverse(data, offset + 0x2C, 4);
                this._firstCluster = BitConverter.ToUInt32(data, offset + 0x2C);
                Array.Reverse(data, offset + 0x30, 4);
                this._fileSize = BitConverter.ToUInt32(data, offset + 0x30);
                Array.Reverse(data, offset + 0x34, 4);
                this._creationTimeAsInt = BitConverter.ToUInt32(data, offset + 0x34);
                Array.Reverse(data, offset + 0x38, 4);
                this._lastWriteTimeAsInt = BitConverter.ToUInt32(data, offset + 0x38);
                Array.Reverse(data, offset + 0x3C, 4);
                this._lastAccessTimeAsInt = BitConverter.ToUInt32(data, offset + 0x3C);
                this._creationTime = new X360TimeStamp(this._creationTimeAsInt);
                this._lastWriteTime = new X360TimeStamp(this._lastWriteTimeAsInt);
                this._lastAccessTime = new X360TimeStamp(this._lastAccessTimeAsInt);
            }
        }

        public uint Cluster { get => _cluster; set => _cluster = value; }

        public long Offset { get => _offset; set => _offset = value; }

        public uint FileNameLength
        {
            get => _fileNameLength;
            set => _fileNameLength = (byte)value; // Explicit cast to byte to resolve CS0266  
        }

        public FileAttribute FileAttributes
        {
            get { return (FileAttribute)_fileAttributes; }
            set { _fileAttributes = (byte)value; }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                {
                    if (_fileNameLength == Constants.DirentDeleted)
                    {
                        var trueFileNameLength = Array.IndexOf(_fileNameBytes, (byte)0xff);
                        if (trueFileNameLength == -1)
                        {
                            trueFileNameLength = 42;
                        }
                        _fileName = Encoding.ASCII.GetString(_fileNameBytes, 0, trueFileNameLength);
                    }
                    else
                    {
                        if (_fileNameLength > 42)
                        {
                            _fileNameLength = 42;
                        }

                        _fileName = Encoding.ASCII.GetString(_fileNameBytes, 0, _fileNameLength);
                    }
                }

                return _fileName;
            }
            set
            {
                if (_fileNameLength == Constants.DirentDeleted)
                {
                    var trueFileNameLength = Array.IndexOf(_fileNameBytes, (byte)0xff);
                    if (trueFileNameLength == -1)
                    {
                        trueFileNameLength = 42;
                    }
                    _fileName = Encoding.ASCII.GetString(_fileNameBytes, 0, trueFileNameLength);
                }
                else
                {
                    if (_fileNameLength > 42)
                    {
                        _fileNameLength = 42;
                    }

                    _fileName = Encoding.ASCII.GetString(_fileNameBytes, 0, _fileNameLength);
                }
            }
        }

        public byte[] FileNameBytes { get => _fileNameBytes; set => _fileNameBytes = (byte[])value; }

        public uint FirstCluster
        {
            get => _firstCluster;
            set => _firstCluster = value;
        }

        public uint FileSize
        {
            get => _fileSize;
            set => _fileSize = value;
        }

        public TimeStamp CreationTime
        {
            get => _creationTime;
            set => _creationTime = value;
        }

        public TimeStamp LastWriteTime
        {
            get => _lastWriteTime;
            set => _lastWriteTime = value;
        }

        public TimeStamp LastAccessTime
        {
            get => _lastAccessTime;
            set => _lastAccessTime = value;
        }

        /// <summary>
        /// Get all dirents from this directory.
        /// </summary>
        /// <returns></returns>
        public List<DirectoryEntry> Children
        {
            get
            {
                if (!this.IsDirectory())
                {
                    Console.WriteLine("Trying to get children from non directory.");
                }

                return _children;
            }
        }

        /// <summary>
        /// Add a single dirent to this directory.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(DirectoryEntry child)
        {
            if (!this.IsDirectory())
            {
                Console.WriteLine("Trying to add child to non directory.");
            }

            _children.Add(child);
        }

        /// <summary>
        /// Add list of dirents to this directory.
        /// </summary>
        /// <param name="children">List of dirents</param>
        public void AddChildren(List<DirectoryEntry> children)
        {
            if (!IsDirectory())
            {
                // TODO: warn user
                return;
            }

            foreach (DirectoryEntry dirent in children)
            {
                dirent.SetParent(this);
                //Console.WriteLine($"Adding child: {dirent.FileName} {dirent.Offset} {dirent.FirstCluster}  to parent: {this.FileName} {this.Offset} {this.FirstCluster}");
                _children.Add(dirent);
            }
        }

        public void SetParent(DirectoryEntry parent)
        {
            this._parent = parent;
        }

        public DirectoryEntry GetParent()
        {
            return this._parent;
        }

        public bool HasParent()
        {
            return this._parent != null;
        }

        public bool IsDirectory()
        {
            return FileAttributes.HasFlag(FileAttribute.Directory);
        }

        public bool IsDeleted()
        {
            return _fileNameLength == Constants.DirentDeleted;
        }

        /// <summary>
        /// Get only the path of this dirent.
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            List<string> ancestry = new List<string>();

            for (DirectoryEntry parent = _parent; parent != null; parent = parent._parent)
            {
                ancestry.Add(parent.FileName);
            }

            ancestry.Reverse();
            return String.Join("/", ancestry.ToArray());
        }

        /// <summary>
        /// Get full path including file name.
        /// </summary>
        /// <returns></returns>
        public string GetFullPath()
        {
            return GetPath() + "/" + FileName;
        }

        public DirectoryEntry GetRootDirectoryEntry()
        {
            DirectoryEntry parent = GetParent();

            if (parent == null)
            {
                return this;
            }

            while (parent.GetParent() != null)
            {
                parent = parent.GetParent();
            }

            return parent;
        }

        public long CountFiles()
        {
            if (IsDeleted())
            {
                return 0;
            }

            if (IsDirectory())
            {
                long numFiles = 1;

                foreach (var dirent in Children)
                {
                    numFiles += dirent.CountFiles();
                }

                return numFiles;
            }
            else
            {
                return 1;
            }
        }
    }
}
