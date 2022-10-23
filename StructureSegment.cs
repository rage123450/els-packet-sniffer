using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleShark
{
    public sealed class StructureSegment
    {
        private byte[] mBuffer;

        public StructureSegment(byte[] pBuffer, int pStart, int pLength)
        {
            mBuffer = new byte[pLength];
            Buffer.BlockCopy(pBuffer, pStart, mBuffer, 0, pLength);
        }

        public byte? Byte { get { if (mBuffer.Length == 1) return mBuffer[0]; return null; } }
        //public sbyte? SByte { get { if (mBuffer.Length == 1) return (sbyte)mBuffer[0]; return null; } }
        //public ushort? SShort
        //{
        //    get {
        //        if (mBuffer.Length == 2) {
        //            Array.Reverse(mBuffer, 0, 2);
        //            return BitConverter.ToUInt16(mBuffer, 0);
        //        } else {
        //            return null;
        //        }
        //    }
        //}
        public short? Short
        {
            get
            {
                if (mBuffer.Length == 2)
                {
                    Array.Reverse(mBuffer, 0, 2);
                    return BitConverter.ToInt16(mBuffer, 0);
                }
                else
                {
                    return null;
                }
            }
        }
        //public uint? UInt
        //{
        //    get
        //    {
        //        if (mBuffer.Length == 4)
        //        {
        //            Array.Reverse(mBuffer, 0, 4);
        //            return BitConverter.ToUInt32(mBuffer, 0);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}
        public int? Int
        {
            get
            {
                if (mBuffer.Length == 4)
                {
                    Array.Reverse(mBuffer, 0, 4);
                    return BitConverter.ToInt32(mBuffer, 0);
                }
                else
                {
                    return null;
                }
            }
        }
        //public ulong? ULong
        //{
        //    get
        //    {
        //        if (mBuffer.Length == 8)
        //        {
        //            Array.Reverse(mBuffer, 0, 8);
        //            return BitConverter.ToUInt64(mBuffer, 0);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}
        public long? Long
        {
            get
            {
                if (mBuffer.Length == 8)
                {
                    Array.Reverse(mBuffer, 0, 8);
                    return BitConverter.ToInt64(mBuffer, 0);
                }
                else
                {
                    return null;
                }
            }
        }

        public float? Float
        {
            get
            {
                if (mBuffer.Length == 4)
                {
                    return BitConverter.ToSingle(mBuffer, 0);
                }
                else
                {
                    return null;
                }
            }
        }

        public double? Double
        {
            get
            {
                if (mBuffer.Length == 8)
                {
                    return BitConverter.ToDouble(mBuffer, 0);
                }
                else
                {
                    return null;
                }
            }
        }

        public string UnicodeString
        {
            get
            {
                if (mBuffer.Length == 0) return null;
                if (mBuffer[0] == 0x00) return "";

                return Encoding.Unicode.GetString(mBuffer, 0, mBuffer.Length);
//                for (int index = 0; index < mBuffer.Length; ++index) if (mBuffer[index] == 0x00) return Encoding.ASCII.GetString(mBuffer, 0, index);
//                return Encoding.ASCII.GetString(mBuffer, 0, mBuffer.Length);
            }
        }

        public DateTime? Date
        {
            get
            {
                try
                {
                    if (mBuffer.Length >= 8)
                        return DateTime.FromFileTimeUtc(BitConverter.ToInt64(mBuffer, 0));
                }
                catch { }
                return null;
            }
        }

        public DateTime? FlippedDate
        {
            get
            {
                try
                {
                    if (mBuffer.Length >= 8)
                    {
                        long time = BitConverter.ToInt64(mBuffer, 0);
                        time = (long)(
                            ((time << 32) & 0xFFFFFFFF) |
                            (time & 0xFFFFFFFF)
                            );

                        return DateTime.FromFileTimeUtc(time);
                    }
                }
                catch { }
                return null;
            }
        }
        public string Length
        {
            get
            {
                return mBuffer.Length + (mBuffer.Length != 1 ? " bytes" : " byte");
            }
        }
    }
}
