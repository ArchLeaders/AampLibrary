using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using Syroot.Maths;

namespace AampV2Library
{
    //Thanks to Syroot for the IO and methods
    public class FileReader : BinaryDataReader
    {
        public FileReader(Stream stream, bool leaveOpen = false)
    : base(stream, Encoding.ASCII, leaveOpen)
        {
            this.Position = 0;
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

        internal T LoadCustom<T>(Func<T> callback, long? offset = null)
        {
            offset = offset ?? ReadOffset();
            if (offset == 0) return default(T);

            using (this.TemporarySeek(offset.Value, SeekOrigin.Begin))
            {
                return callback.Invoke();
            }
        }

        internal string LoadString(Encoding encoding = null)
        {
            long offset = ReadInt64();
            if (offset == 0) return null;

            encoding = encoding ?? Encoding;
            using (this.TemporarySeek(offset, SeekOrigin.Begin))
            {
                ushort count = ReadUInt16();
                return this.ReadString(count, encoding: encoding);
            }
        }
        internal Vector4F ReadVector4F()
        {
            return new Vector4F(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
        internal Vector3F ReadVector3F()
        {
            return new Vector3F(ReadSingle(), ReadSingle(), ReadSingle());
        }
        internal Vector2F ReadVector2F()
        {
            return new Vector2F(ReadSingle(), ReadSingle());
        }
        internal void LoadBlockHeader()
        {
            uint offset = ReadUInt32();
            ulong size = ReadUInt64();
        }

        internal long ReadOffset()
        {
            long offset = ReadInt64();
            return offset == 0 ? 0 : offset;
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
    }
}
