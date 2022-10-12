using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using Org.BouncyCastle.Utilities.Zlib;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Ionic.Zlib;
using System.Net;
using DeflateStream = System.IO.Compression.DeflateStream;

namespace MapleShark
{
    public enum TransformLocale
    {
        SPECIAL,
        AES,
        AES_MCRYPTO,
        MCRYPTO,
        OLDEST_MCRYPTO,
        NONE,
    }

    public sealed class MapleStream
    {
        private const int DEFAULT_SIZE = 4096;

        private bool mOutbound = false;
        private MapleDES mDES = null;
        private byte[] mBuffer = new byte[DEFAULT_SIZE];
        private int mCursor = 0;

        public MapleStream(bool pOutbound, ushort pBuild, byte pLocale, byte[] pIV) { 
            mOutbound = pOutbound; 
            mDES = new MapleDES(pBuild, pLocale);
        }

        public void Append(byte[] packetBuffer, ref byte[] dataBuffer)
        {
            byte[] temp = new byte[dataBuffer.Length + packetBuffer.Length];
            dataBuffer.CopyTo(temp, 0);
            packetBuffer.CopyTo(temp, dataBuffer.Length);
            dataBuffer = new byte[temp.Length];
            temp.CopyTo(dataBuffer, 0);
//            Console.WriteLine("APPENDED: " + dataBuffer.Length);
        }
        
        public MaplePacket Read(DateTime pTransmitted, ushort pBuild, byte pLocale, ref bool firstPacket, ref byte[] dataBuffer, ref byte[] curIV)
        {
/*            Console.WriteLine(dataBuffer.Length);
            Console.WriteLine("BUFFER DATA:\n--------");
            for (int i = 0; i < dataBuffer.Length; i++)
            {
                Console.Write("0x");
                if (dataBuffer[i] < 22)
                {
                    Console.Write("0");
                    Console.Write(dataBuffer[i].ToString("X"));
                }
                else
                {
                    Console.Write(dataBuffer[i].ToString("X"));
                }
                Console.Write(" ");
            }
            Console.WriteLine("-------");*/

            if (dataBuffer.Length == 0)
            {
                return null;
            }
            int packetSize = mDES.GetHeaderLength(dataBuffer, 0);
            if (packetSize > dataBuffer.Length)
            {
                return null;
            }

            curIV = mDES.GetIV(dataBuffer);
            mDES.SetIV(curIV);

            byte[] packetBuffer = new byte[packetSize];
            Buffer.BlockCopy(dataBuffer, 22/*16*/, packetBuffer, 0, packetSize - 32/* - 26*/);

            //bool byteheader = false;

            byte[] decryptedBuffer = mDES.Decrypt(packetBuffer, firstPacket, true, null);
            if (firstPacket)
            {
                mDES.SetKey(decryptedBuffer);
                firstPacket = false;
            }

            bool a = decryptedBuffer[7] == 1;
            ushort opcode = (ushort) ( (decryptedBuffer[a ? 32 : 24] << 8) | (decryptedBuffer[a ? 33 : 25]) );
            uint size = (uint) ( (decryptedBuffer[a ? 34 : 26] << 24) | (decryptedBuffer[a ? 35 : 27] << 16) | (decryptedBuffer[a ? 36 : 28] << 8) | (decryptedBuffer[a ? 37 : 29]) );
            bool b = size > 0;
            bool compress = b && decryptedBuffer[a ? 38 : 30] == 1;

            byte[] decryptedBuffer_ = new byte[size];
            int start = (b ? 31 : 30);
            if (a) start += 8;
            Buffer.BlockCopy(decryptedBuffer, start, decryptedBuffer_, 0, (int) size);

//            byte[] c = compress ? ZlibStream.UncompressBuffer(decryptedBuffer_) : decryptedBuffer_;

            byte[] c;
            if (compress) {
                byte[] d = new byte[size - 4];
                Buffer.BlockCopy(decryptedBuffer_, 4, d, 0, (int) size - 4);
                uint original_size = (uint)((decryptedBuffer_[0]) | (decryptedBuffer_[1] << 8) | (decryptedBuffer_[2] << 16) | (decryptedBuffer_[3] << 24));

//                c = SharpZipLibDecompress(d);
                c = ZLibDotnetDecompress(d, (int) original_size);
            } else
            {
                c = decryptedBuffer_;
            }

            byte[] temp = new byte[dataBuffer.Length - packetSize];
            Buffer.BlockCopy(dataBuffer, packetSize, temp, 0, dataBuffer.Length - packetSize);
            dataBuffer = new byte[temp.Length];
            temp.CopyTo(dataBuffer, 0);

            Definition definition = Config.Instance.GetDefinition(pBuild, pLocale, mOutbound, opcode);
            return new MaplePacket(pTransmitted, mOutbound, pBuild, pLocale, opcode, definition == null ? "" : definition.Name, c);
//            return new MaplePacket(pTransmitted, mOutbound, pBuild, pLocale, opcode, definition == null ? "" : definition.Name, decryptedBuffer);
        }

        public static byte[] SharpZipLibDecompress(byte[] data)
        {
            MemoryStream compressed = new MemoryStream(data);
            MemoryStream decompressed = new MemoryStream();
            InflaterInputStream inputStream = new InflaterInputStream(compressed);
            inputStream.CopyTo(decompressed);
            return decompressed.ToArray();
        }

        public static byte[] ZLibDotnetDecompress(byte[] data, int size)
        {
            MemoryStream compressed = new MemoryStream(data);
            ZInputStream inputStream = new ZInputStream(compressed);
            byte[] result = new byte[size];   // 由於ZInputStream 繼承的是BinaryReader而不是Stream, 只能提前準備好輸出的 buffer 然後用 read 獲取定長資料。
            inputStream.Read(result, 0, result.Length);
            return result;
        }

    }
}