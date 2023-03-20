using System;
using System.IO;

namespace giogiogiogiogiogiogio
{
    class TXBtool
    {
        public static void TXBex(string TXBpath, byte[] TXBin, bool clutfix, string outpath)
        {
            int texcount = Buffer.GetByte(TXBin, 0x00);
            //Console.WriteLine("Texture Count: " + texcount);
            for (int k = 0; k < texcount; k++)
            {
                byte[] IDArray = { Buffer.GetByte(TXBin, 0x08 + k * 8), Buffer.GetByte(TXBin, 0x09 + k * 8), Buffer.GetByte(TXBin, 0x0A + k * 8), Buffer.GetByte(TXBin, 0x0B + k * 8) };
                byte[] OffArray = { Buffer.GetByte(TXBin, 0x0C + k * 8), Buffer.GetByte(TXBin, 0x0D + k * 8), Buffer.GetByte(TXBin, 0x0E + k * 8), Buffer.GetByte(TXBin, 0x0F + k * 8) };
                int texID = BitConverter.ToInt32(IDArray, 0);                 //internal image ID
                int texOffset = BitConverter.ToInt32(OffArray, 0);            //where the image is in the TXB
                int alignment = Buffer.GetByte(TXBin, 0x05 + texOffset);      //what byte alignment the image is using 
                int shortclutcount = Buffer.GetByte(TXBin, 0x14 + texOffset); //the color count on a 16 byte aligned image
                int longclutcount = Buffer.GetByte(TXBin, 0x8E + texOffset);  //the color count on a 128 byte aligned image
                byte[] shortsize = { Buffer.GetByte(TXBin, 0x10 + texOffset), Buffer.GetByte(TXBin, 0x11 + texOffset), Buffer.GetByte(TXBin, 0x12 + texOffset), Buffer.GetByte(TXBin, 0x13 + texOffset) };//16  byte clut size
                byte[] longsize = { Buffer.GetByte(TXBin, 0x80 + texOffset), Buffer.GetByte(TXBin, 0x81 + texOffset), Buffer.GetByte(TXBin, 0x82 + texOffset), Buffer.GetByte(TXBin, 0x83 + texOffset) }; //128 byte clut size

                //Console.WriteLine("Texture " + k + " ID: " + texID + "\nTexture " + k + " Offset: " + texOffset);
                //string pathnoex = 
                //string path 

                string outoutout = outpath + "\\" + Path.ChangeExtension(Path.GetFileName(TXBpath), null) + "_img" + texID + ".tm2";
                using (var stream = File.Create(outoutout))
                {
                    stream.Write(TXBin, texOffset, TXBin.Length - texOffset);
                    if (alignment == 0)
                    {
                        stream.SetLength(BitConverter.ToInt32(shortsize, 0) + 16);
                        if (clutfix == true && shortclutcount == 16) //fixes clut size on 16 color 16 byte images
                        {
                            stream.Seek(0x14, 0x0);
                            stream.WriteByte(0x40);
                        }
                    }
                    if (alignment == 1)
                    {
                        stream.SetLength(BitConverter.ToInt32(longsize, 0) + 128);
                        if (clutfix == true && longclutcount == 16) //fixes clut size on 16 color 128 byte images
                        {
                            stream.Seek(0x84, 0x0);
                            if (texID == 591) stream.WriteByte(0x20);//Oh the Misery
                            else if (texID == 1113) stream.WriteByte(0x20);//Capcom why
                            else stream.WriteByte(0x40);
                        }
                    }
                }

                //Console.WriteLine("Extracted: " + Path.GetFileName(Path.ChangeExtension(TXBpath, null)) + "_img" + texID + ".tm2");
            }
        }
        public static void TXBre(string TXBpath, byte[] TXBin, bool clutfix)
        {
            int texcount = Buffer.GetByte(TXBin, 0x00);
            //Console.WriteLine("Texture Count:" + texcount);

            //var newTXB = File.Create(Path.ChangeExtension(TXBpath, null) + "_repack.txb");
            //newTXB.Close();

            for (int k = 0; k < texcount; k++)
            {
                byte[] IDArray = { Buffer.GetByte(TXBin, 0x08 + k * 8), Buffer.GetByte(TXBin, 0x09 + k * 8), Buffer.GetByte(TXBin, 0x0A + k * 8), Buffer.GetByte(TXBin, 0x0B + k * 8) };
                int texID = BitConverter.ToInt32(IDArray, 0);		//internal image ID
                byte[] OffArray = { Buffer.GetByte(TXBin, 0x0C + k * 8), Buffer.GetByte(TXBin, 0x0D + k * 8), Buffer.GetByte(TXBin, 0x0E + k * 8), Buffer.GetByte(TXBin, 0x0F + k * 8) };
                int texOffset = BitConverter.ToInt32(OffArray, 0);	//where the image is in the TXB

                byte[] TM2in = File.ReadAllBytes(Path.ChangeExtension(TXBpath, null) + "_img" + texID + ".tm2");

                int TM2alignment = Buffer.GetByte(TM2in, 0x05);		//what byte alignment the image is using 
                int TM2sclutcount = Buffer.GetByte(TM2in, 0x14);	//the color count is on a 16 byte aligned image
                int TM2lclutcount = Buffer.GetByte(TM2in, 0x8E);	//the color count is on a 128 byte aligned image

                if (TM2alignment == 0 && clutfix == true && TM2sclutcount == 16) Buffer.SetByte(TM2in, 0x14, 0x80);
                //reverts clut size on 16 color 16 byte images
                if (TM2alignment == 1 && clutfix == true && TM2lclutcount == 16) Buffer.SetByte(TM2in, 0x84, 0x80);
                //reverts clut size on 16 color 128 byte images

                Buffer.BlockCopy(TM2in, 0x0, TXBin, texOffset, TM2in.Length);
                //Console.WriteLine(Path.GetFileName(Path.ChangeExtension(TXBpath, null)) + "_img" + texID + ".tm2" + " has been inserted at offset " + texOffset);
            }

            File.WriteAllBytes(TXBpath, TXBin);
            //Console.WriteLine("Saved as: " + Path.ChangeExtension(TXBpath, null) + "_repack.txb");
        }
    }
}
