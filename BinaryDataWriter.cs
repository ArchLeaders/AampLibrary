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
    public class FileWriter : BinaryDataWriter
    {
        public FileWriter(Stream stream, bool leaveOpen = false)
        : base(stream, Encoding.ASCII, leaveOpen)
        {
            this.Position = 0;
            ByteOrder = ByteOrder.BigEndian;
        }

        public void WriteSize(long EndPosition, long startPosition)
        {
            using (TemporarySeek(startPosition, SeekOrigin.Begin))
            {
                Write((uint)(EndPosition - startPosition));
            }
        }

        public void WriteBoolean(bool boolean) => Write(boolean == false ? 0 : 1);
        
        public void WriteVector2F(Vector2F vector2F)
        {
            Write(vector2F.X);
            Write(vector2F.Y);
        }

        public void WriteVector3F(Vector3F vector3F)
        {
            Write(vector3F.X);
            Write(vector3F.Y);
            Write(vector3F.Z);
        }

        public void WriteVector4F(Vector4F vector4F)
        {
            Write(vector4F.X);
            Write(vector4F.Y);
            Write(vector4F.Z);
            Write(vector4F.W);
        }

        /// <summary>
        /// Checks the byte order mark to determine the endianness of the writer.
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
    }
}
