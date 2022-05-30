using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;

namespace AampLibrary.IO
{
    // Thanks to Syroot for the IO and methods
    public class FileReader : BinaryDataReader
    {
        public FileReader(Stream stream, bool leaveOpen = false) : base(stream, Encoding.ASCII, leaveOpen)
        {
            Position = 0;
            ByteOrder = ByteOrder.BigEndian;
        }

        /// <summary>
        /// Checks the byte order mark to determine the endianness of the reader.
        /// </summary>
        /// <param name="ByteOrderMark">The byte order value being read. 0x01000000 = Little, 0x00000000 = Big. </param>
        /// <returns></returns>
        public void CheckByteOrderMark(uint ByteOrderMark)
        {
            if (ByteOrderMark == 0x01000000)
                ByteOrder = ByteOrder.LittleEndian;
            else
                ByteOrder = ByteOrder.BigEndian;
        }

        internal T? LoadCustom<T>(Func<T> callback, long? offset = null)
        {
            offset ??= ReadOffset();
            if (offset == 0) return default;

            using (TemporarySeek(offset.Value, SeekOrigin.Begin))
                return callback.Invoke();
        }

        internal string? LoadString(Encoding? encoding = null)
        {
            long offset = ReadInt64();
            if (offset == 0) return null;

            encoding = encoding ?? Encoding;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                ushort count = ReadUInt16();
                return ReadString(count, encoding: encoding);
            }
        }

        internal void LoadBlockHeader()
        {
            uint offset = ReadUInt32();
            ulong size = ReadUInt64();
        }

        internal void CheckSignature(string validSignature)
        {
            // Read the actual signature and compare it.
            string signature = this.ReadString(sizeof(uint), Encoding.ASCII);
            if (signature != validSignature)
            {
                throw new Exception($"Invalid signature, expected '{validSignature}' but got '{signature}'.");
            }
        }

        internal long ReadOffset()
        {
            long offset = ReadInt64();
            return offset == 0 ? 0 : offset;
        }

        internal bool ReadIntBoolean() => ReadInt32() == 0 ? false : true;
        internal Vector4F ReadVector4F() => new Vector4F(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        internal Vector3F ReadVector3F() => new Vector3F(ReadSingle(), ReadSingle(), ReadSingle());
        internal Vector2F ReadVector2F() => new Vector2F(ReadSingle(), ReadSingle());
    }
}
