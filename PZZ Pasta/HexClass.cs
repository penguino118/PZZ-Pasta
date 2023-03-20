using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GioGio_Khnum
{
    class HexClass
    {
        public static string DisplayHex(int integer)
        {
            byte[] tmpbyteint = BitConverter.GetBytes(integer).ToArray();
            string tmpprint = BitConverter.ToString(tmpbyteint);
            return String.Join("", tmpprint.Split('-'));
        }
        public static string DisplayHexBigEn(int integer)
        {
            byte[] tmpbyteint = BitConverter.GetBytes(integer).Reverse().ToArray();
            string tmpprint = BitConverter.ToString(tmpbyteint);
            return String.Join("", tmpprint.Split('-'));
        }
        public static string HexString(byte[] data)
        {
            string hex = BitConverter.ToString(data).Replace("-", string.Empty);
            return hex;
        }
        public static int ReadUInt32(byte[] array, int index)
        {
            byte[] tmpArray = { Buffer.GetByte(array, index), Buffer.GetByte(array, index + 1), Buffer.GetByte(array, index + 2), Buffer.GetByte(array, index + 3) };
            int tmpnumb = BitConverter.ToInt32(tmpArray, 0);

            return tmpnumb;
        }
        public static int GetInt(string filepath, int off)
        {
            byte[] contents = File.ReadAllBytes(filepath);
            int int0x0 = Buffer.GetByte(contents, off);
            return int0x0;
        }
        public void WriteUInt32(byte[] array, int index, int value)
        {
            byte[] tmpArray = BitConverter.GetBytes(value);
            Buffer.SetByte(array, index, tmpArray[0]);
            Buffer.SetByte(array, index +1, tmpArray[1]);
            Buffer.SetByte(array, index +2, tmpArray[2]);
            Buffer.SetByte(array, index +3, tmpArray[3]);
        }

        public static int ReadUInt16(byte[] array, int index)
        {
            byte[] tmpArray = { Buffer.GetByte(array, index), Buffer.GetByte(array, index + 1) };
            int tmpnumb = BitConverter.ToUInt16(tmpArray, 0);
            return tmpnumb;
        }
        public void WriteUInt16(byte[] array, int index, int value)
        {
            byte[] tmpArray = BitConverter.GetBytes(value);
            Buffer.SetByte(array, index, tmpArray[0]);
            Buffer.SetByte(array, index + 1, tmpArray[1]);
        }

        public void FixClut(byte[] TIM2, bool fix)
        {
            int alignment = Buffer.GetByte(TIM2, 0x05);
            int newsize = 0x40;
            if (fix == false) newsize = 0x80;


            if (alignment == 0)
            {
                int clutcount = Buffer.GetByte(TIM2, 0x14);
                if (clutcount == 16) WriteUInt16(TIM2, 0x14, newsize);
            }
            
            if (alignment == 1) 
            {
                int clutcount = Buffer.GetByte(TIM2, 0x8E);
                if (clutcount == 16) WriteUInt16(TIM2, 0x8E, newsize);
            }
        }
    }
}
