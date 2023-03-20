using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static giogiogiogiogiogiogio.TXBtool;
using static GioGio_Khnum.HexClass;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using static System.Net.Mime.MediaTypeNames;

namespace giogiogiogiogiogiogio
{
    class PZZManager
    {
        public static async Task<int> PZZrepack(string filefolder, List<string> filelist, FileStream outfile)
        {
            if (Directory.Exists(filefolder))
            {
                outfile.Close();
                byte[] newpzz = File.ReadAllBytes(outfile.Name);
                bool compressed;
                int filecount = 0;
                Array.Resize(ref newpzz, 2048);
                foreach (string file in filelist)
                {
                    filecount++;
                    string testname = file;
                    testname = testname.Replace("(compress)", "");
                    string filename = Path.GetFileName(file);

                    if (filename.Contains("(compress)"))
                    {
                        compressed = true;
                        await Task.Run(() => PZZcompress(testname, file));
                    }
                    else compressed = false;
                    byte[] filedata = File.ReadAllBytes(file);
                    int filesize = filedata.Length;
                    int sectorsize = 2048;
                    int sectorcount = 0;

                    string fileID = Path.GetFileNameWithoutExtension(filename).Split('_').Last(); //get file id from filename

                    for (int i = 0; filesize > i; i += 2048) //calculate filesize sectors
                    {
                        sectorcount = i / 2048;
                        sectorsize = i;
                    }

                    int finalsectorcount = sectorcount + 1;
                    int finalsectorsize = sectorsize + 2048; //ending pad
                    byte[] secsizebytes = BitConverter.GetBytes(finalsectorcount);

                    if (compressed == true)
                    {
                        byte tmpfilecountcom = Convert.ToByte(Int32.Parse(fileID) + 1);
                        //Console.WriteLine("ID:" + fileID);
                        Buffer.SetByte(newpzz, 0, tmpfilecountcom); //update file count
                        Buffer.BlockCopy(secsizebytes, 0, newpzz, 0x0 + 0x4 * Int32.Parse(fileID) + 0x4, 2); // 0x800 byte sector count
                        Buffer.SetByte(newpzz, 0x4 * Int32.Parse(fileID) + 0x3 + 0x4, 0x80); //set compression flag
                    }
                    if (compressed == false)
                    {
                        byte tmpfilecountdec = Convert.ToByte(Int32.Parse(fileID) + 1);
                        //Console.WriteLine("ID:" + fideID);
                        Buffer.SetByte(newpzz, 0, tmpfilecountdec); //update file count
                        Buffer.BlockCopy(secsizebytes, 0, newpzz, 0x0 + 0x4 * Int32.Parse(fileID) + 0x4, 2); // 0x800 byte sector count
                        Buffer.SetByte(newpzz, 0x4 * Int32.Parse(fileID) + 0x3 + 0x4, 0x00); //remove compression flag
                    }

                    //Console.WriteLine("Sectors:" + finalsectorcount + "\nsectorsize:" + finalsectorsize + "\nfilesize:" + filesize);
                    Array.Resize(ref filedata, finalsectorsize);
                    int oldsize = (newpzz.Length);
                    Array.Resize(ref newpzz, newpzz.Length + finalsectorsize);
                    Buffer.BlockCopy(filedata, 0, newpzz, oldsize, finalsectorsize);
                    if (compressed == true) File.Delete(file);
                }
                File.WriteAllBytes(outfile.Name, newpzz);
                return 1;
            }
            else { MessageBox.Show("The repack directory doesn't exist."); return -1; };
        }
        public static string GetFileType(byte[] infile)
        {
            int magic0x0 = Buffer.GetByte(infile, 0x0);
            int magic0x1 = Buffer.GetByte(infile, 0x1);
            int magic0x2 = Buffer.GetByte(infile, 0x2);
            int magic0x3 = Buffer.GetByte(infile, 0x3);
            int magic0x4 = Buffer.GetByte(infile, 0x4);
            int magic0x5 = Buffer.GetByte(infile, 0x5);
            int magic0x6 = Buffer.GetByte(infile, 0x6);
            int magic0x7 = Buffer.GetByte(infile, 0x7);
            int commacount = 0;
            int breakcount = 0;
            if (magic0x3 == 00 && magic0x4 == 04 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) return ".amo";
            else if (magic0x0 == 00 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 192) return ".ahi";
            else if (magic0x4 == 0 && magic0x5 == 0 && magic0x6 == 0 && magic0x7 == 0 && infile.Length >= 544)
            {
                int TXBmagic1 = Buffer.GetByte(infile, 0x200);
                int TXBmagic2 = Buffer.GetByte(infile, 0x201);
                int TXBmagic3 = Buffer.GetByte(infile, 0x202);
                int TXBmagic4 = Buffer.GetByte(infile, 0x203);
                if (magic0x4 == 00 && TXBmagic1 == 84 && TXBmagic2 == 73 && TXBmagic3 == 77 && TXBmagic4 == 50) return ".txb";
            }
            else if (magic0x4 == 64 && magic0x5 == 00 && magic0x6 == 00 && magic0x7 == 00) return ".aan";
            else if (magic0x4 == 00 && magic0x5 == 00 && magic0x6 == 05 && magic0x7 == 00) return ".sdt";
            else if (magic0x0 == 32 && magic0x1 == 00 && magic0x2 == 00 && magic0x3 == 00) return ".hit";
            else if (magic0x0 == 72 && magic0x1 == 73 && magic0x2 == 84 && magic0x3 == 83) return ".hits";
            //check for text 
            for (int i = 0; i < infile.Length; i++)
            {
                if (infile[i] == 0x2C)
                {
                    commacount++;
                }
                if (i < infile.Length - 1 && infile[i] == 0x0D && infile[i + 1] == 0x0A)
                {
                    breakcount++;
                }
            }
            if (commacount >= 4 && breakcount >= 1) return ".txt";
            else return ".bin";
        }
        public static void PZZrename(string args, bool nullterm) 
        {
            string filefolder = args;
            if (filefolder.Contains("_unpack")) 
            {
                foreach (string file in Directory.EnumerateFiles(filefolder, "*.dat"))
                {
                    byte[] contents = File.ReadAllBytes(file);
                    string newdir = Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file);
                    string type = GetFileType(contents);
                    File.Copy(file, newdir + type, true);
                    File.Delete(file);
                    if (nullterm == true && type == ".txt")
                    {
                        for (int i = 0; i < contents.Length; i++)
                        {
                            if (contents[i] == 0x00)
                            {
                                using (FileStream stream = new FileStream(newdir + ".txt", FileMode.Open)) {
                                    stream.SetLength(i);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }


        public static async Task<int> PZZunpack(string unpackfolder, string texturefolder, string infile, bool txbEx, bool clutfix, bool nultext)
        {

            if (Directory.Exists(unpackfolder) == true)
            {
                string message = "Extracted files for " + Path.GetFileName(infile) + " already exist!\nDo you want to overwrite?";
                string caption = "Confirm Overwrite";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No) return -1;
                if (result == DialogResult.Yes && Directory.EnumerateFiles(unpackfolder).Count() > 0 )
                { 
                    foreach (string file in Directory.EnumerateFiles(unpackfolder)){
                        File.Delete(file);
                    }
                }
            }
            else if (Directory.Exists(unpackfolder) == false) Directory.CreateDirectory(unpackfolder);
            string infilename = Path.GetFileNameWithoutExtension(infile);
            byte[] inPZZ = File.ReadAllBytes(infile);
            int filecount = Buffer.GetByte(inPZZ, 0x00);
            int sbf = 0x800;
            for (int x = 0; filecount > x; x++) //unpack
            {
                int fsize = ReadUInt16(inPZZ, 0x04 + x * 4);
                int cflag = ReadUInt16(inPZZ, 0x06 + x * 4);
                if (fsize == 0) continue; 
                string outoutout = unpackfolder + "\\" + infilename + "_" + x.ToString("D3") + ".dat";
                if (cflag == 32768)
                {
                    outoutout = unpackfolder + "\\" + infilename + "_" + x.ToString("D3") + "_compressed.dat";
                    using (var stream = File.Create(outoutout))
                    {
                        //stream.Write(inPZZ, sbf, fsize * 0x800);
                        byte[] compbytes = new byte[fsize * 0x800];
                        Array.Copy(inPZZ, sbf, compbytes, 0, fsize * 0x800);
                        await Task.Run(() => PZZdecompress(compbytes, stream));
                    }
                    sbf += fsize * 0x800;
                    continue;
                }
                else if (cflag == 0)
                {
                    byte[] testbuf = new byte[fsize * 0x800];
                    Array.Copy(inPZZ, sbf, testbuf, 0, fsize * 0x800);
                    outoutout = unpackfolder + "\\" + infilename + "_" + x.ToString("D3") + GetFileType(testbuf);
                    using (var stream = File.Create(outoutout)) {
                        stream.Write(inPZZ, sbf, fsize * 0x800);
                    }
                    sbf += fsize * 0x800;
                }
            }

            PZZrename(unpackfolder, nultext);//adds file extensions

            string filelistdir = Path.Combine(Directory.GetParent(unpackfolder).FullName, Path.GetFileNameWithoutExtension(infile)) + "_filelist.txt";
            if (File.Exists(filelistdir) == true) File.Delete(filelistdir);
            string[] filelist = Directory.GetFiles(unpackfolder);
            Array.Sort(filelist);
            using (StreamWriter sw = new StreamWriter(filelistdir))
            {
                foreach (string file in filelist)
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    if (filename.Contains("_compressed"))
                    {
                        string outfn = file;
                        outfn = outfn.Replace("_compressed", "");
                        outfn += "(compress)";
                        sw.WriteLine(outfn);
                    }
                    else sw.WriteLine(file);
                }
            }

            foreach (string fileee in Directory.EnumerateFiles(unpackfolder, "*_compressed*"))
            {
                string newname = Path.GetFileName(fileee).Replace("_compressed", "");
                string newdir = Path.GetDirectoryName(fileee) + "/" + newname;
                File.Copy(fileee, newdir, true);
                File.Delete(fileee);
            }

            if (txbEx == true)
            {
                if (Directory.EnumerateFiles(unpackfolder, "*.txb").Count() > 0)
                {
                    if (Directory.Exists(texturefolder) == false) Directory.CreateDirectory(texturefolder);
                    foreach (string fileee in Directory.EnumerateFiles(unpackfolder, "*.txb"))
                    {
                        string txbcopy = texturefolder + "\\" + Path.GetFileName(fileee);
                        File.Copy(fileee, txbcopy, true);
                    }
                    foreach (string fileee in Directory.EnumerateFiles(texturefolder, "*.txb"))
                    {
                        byte[] filebytes = File.ReadAllBytes(fileee);
                        string expath = texturefolder + "\\" + Path.GetFileNameWithoutExtension(fileee) + "_textures";
                        Directory.CreateDirectory(expath);
                        TXBex(fileee, filebytes, clutfix, expath);
                    }
                }
            }
            return 1;
        }
        private static void PZZdecompress(byte[] infile, FileStream output) 
        {
            //---------------------------------------------------------------------//
            //originally written in C by infval
            //https://github.com/infval/pzzcompressor_jojo/blob/master/pzzcomp_jojo.c
            //---------------------------------------------------------------------//
            byte[] outfile = { };
            int size_b = infile.Length;
            int cb = 0;  // Control bytes
            int cb_bit = -1;
            int count;
            int offset;
            int dsize = PZZ_GetDecompressedSize(infile, infile.Length);
            if (dsize == -1)
            {
                Console.WriteLine("Bad PZZ file");
                MessageBox.Show("Bad PZZ file.\nOutFile: " + output.Name);
                return;
            }
            int dpos = 0;
            int spos = 0;
            Array.Resize(ref outfile, dsize);
            while (spos < size_b)
            {
                if (cb_bit < 0)
                {
                    cb = infile[spos++] << 0;
                    cb |= infile[spos++] << 8;
                    cb_bit = 15;
                    //continue;
                }

                int compress_flag = cb & (1 << cb_bit);
                cb_bit--;

                if (compress_flag != 0)
                {
                    count = infile[spos++] << 0;
                    count |= infile[spos++] << 8;
                    offset = (count & 0x7FF) * 2;
                    if (offset == 0)
                    {
                        break; // End of the compressed data
                    }
                    count >>= 11;
                    if (count == 0)
                    {
                        count = infile[spos++] << 0;
                        count |= infile[spos++] << 8;
                    }
                    count *= 2;
                    for (int j = 0; j < count; j++)
                    {
                        //Array.Resize(ref outfile, dpos + 1);
                        outfile[dpos] = outfile[dpos - offset];
                        dpos++;
                    }
                }
                else
                {
                    //Array.Resize(ref outfile, dpos + 2);
                    outfile[dpos++] = infile[spos++];
                    outfile[dpos++] = infile[spos++];
                }
            }
            //byte[] outdecomp = outfile.ToArray();
            //output.Write(outdecomp, 0, outdecomp.Length);
            output.Write(outfile, 0, outfile.Length);
        }
        public static int PZZ_GetDecompressedSize(byte[] src, int src_len)
        {
            //---------------------------------------------------------------------//
            //originally written in C by infval
            //https://github.com/infval/pzzcompressor_jojo/blob/master/pzzcomp_jojo.c
            //---------------------------------------------------------------------//
            int offset;
            int count;
            int spos = 0;
            int dpos = 0;
            if (spos >= src_len) return -1;
            int cb = 0;
            int cb_bit = -1;
            src_len = src_len / 2 * 2;
            while (spos < src_len)
            {
                if (cb_bit < 0)
                {
                    if (spos >= src_len) return -1; //check spos
                    cb = src[spos++] << 0;
                    if (spos >= src_len) return -1; //check spos
                    cb |= src[spos++] << 8;
                    cb_bit = 15;
                    //continue;
                }

                int compress_flag = cb & (1 << cb_bit);
                cb_bit--;

                if (compress_flag != 0)
                {
                    if (spos >= src_len) return -1; //check spos
                    count = src[spos++] << 0;
                    if (spos >= src_len) return -1; //check spos
                    count |= src[spos++] << 8;
                    offset = (count & 0x7FF) * 2;
                    if (offset == 0)
                    {
                        break; // End of the compressed data
                    }
                    count >>= 11;
                    if (count == 0)
                    {
                        count = src[spos++] << 0;
                        if (spos >= src_len) return -1; //check spos
                        count |= src[spos++] << 8;
                        if (spos >= src_len) return -1; //check spos
                    }
                    count *= 2;
                    if (dpos < offset) return -1;//check dpos
                    for (int j = 0; j < count; j++)
                    {
                        //Array.Resize(ref outfile, dpos + 1);
                        dpos++;
                    }
                }
                else
                {
                    //Array.Resize(ref outfile, dpos + 2);
                    if (spos >= src_len) return -1; //check spos
                    dpos++;
                    spos++;
                    if (spos >= src_len) return -1; //check spos
                    dpos++;
                    spos++;
                }
            }
            // CHECK SPOS: if (spos >= src_len) return -1;
            return dpos;
        }
        private static void PZZcompress(string input, string output)
        {
            //---------------------------------------------------------------------//
            //originally written in C by infval
            //https://github.com/infval/pzzcompressor_jojo/blob/master/pzzcomp_jojo.c
            //---------------------------------------------------------------------//
            using (FileStream fs = new FileStream(input, FileMode.Open, FileAccess.ReadWrite))
            {
                byte[] infile = new byte[fs.Length];
                fs.Read(infile, 0, (int)fs.Length);
                byte[] outfile = { };
                int size_b = infile.Length;
                int cb = 0;  // Control bytes
                int cb_bit = 15;
                int cb_pos = 0;

                int dpos = 0;
                int spos = 0;

                int outsize = PZZ_GetCompressedSize(infile);
                if (outsize >= infile.Length) MessageBox.Show("Warning: " + input + " compressed size is equal or bigger than the uncompressed size.");
                Array.Resize(ref outfile, outsize);
                outfile[dpos++] = 0x00;
                outfile[dpos++] = 0x00;

                while (spos < size_b)
                {
                    int offset = 0;
                    int length = 0;

                    for (int i = (spos >= 0x7FF * 2 ? spos - 0x7FF * 2 : 0); i < spos; i += 2)
                    {
                        if (infile[i] == infile[spos] && infile[i + 1] == infile[spos + 1])
                        {
                            int cur_len = 0;
                            do
                            {
                                cur_len += 2;
                            } while ((cur_len < 0xFFFF * 2)
                                && (spos + cur_len < size_b)
                                && infile[i + cur_len] == infile[spos + cur_len]
                                && infile[i + 1 + cur_len] == infile[spos + 1 + cur_len]);

                            if (cur_len > length)
                            {
                                offset = spos - i;
                                length = cur_len;
                                if (length >= 0xFFFF * 2)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    int compress_flag = 0;
                    if (length >= 4)
                    {
                        compress_flag = 1;
                        offset /= 2;
                        length /= 2;
                        int c = offset;
                        if (length <= 0x1F)
                        {
                            c |= length << 11;
                            outfile[dpos++] = (byte)(c & 0xFF);
                            outfile[dpos++] = (byte)(c >> 8);
                        }
                        else
                        {
                            outfile[dpos++] = (byte)(c & 0xFF);
                            outfile[dpos++] = (byte)(c >> 8);
                            outfile[dpos++] = (byte)(length & 0xFF);
                            outfile[dpos++] = (byte)(length >> 8);
                        }
                        spos += length * 2;
                    }
                    else
                    {
                        outfile[dpos++] = infile[spos++];
                        outfile[dpos++] = infile[spos++];
                    }

                    cb |= compress_flag << cb_bit;
                    cb_bit--;

                    if (cb_bit < 0)
                    {
                        outfile[cb_pos + 0] = (byte)(cb & 0xFF);
                        outfile[cb_pos + 1] = (byte)(cb >> 8);
                        cb = 0x0000;
                        cb_bit = 15;
                        cb_pos = dpos;
                        outfile[dpos++] = 0x00;
                        outfile[dpos++] = 0x00;
                    }
                }
                cb |= 1 << cb_bit;
                outfile[cb_pos + 0] = (byte)(cb & 0xFF);
                outfile[cb_pos + 1] = (byte)(cb >> 8);
                outfile[dpos++] = 0x00;
                outfile[dpos++] = 0x00;

                using (FileStream outfs = new FileStream(output, FileMode.Create, FileAccess.ReadWrite))
                {
                    outfs.Write(outfile, 0, outsize);
                }
            }
        }
        public static int PZZ_GetCompressedSize(byte[] src)
        {
            //---------------------------------------------------------------------//
            //originally written in C by infval
            //https://github.com/infval/pzzcompressor_jojo/blob/master/pzzcomp_jojo.c
            //---------------------------------------------------------------------//
            byte[] infile = src;
            int size_b = infile.Length;
            int cb = 0;  // Control bytes
            int cb_bit = 15;
            int cb_pos = 0;

            int dpos = 0;
            int spos = 0;

            dpos++;
            dpos++;

            while (spos < size_b)
            {
                int offset = 0;
                int length = 0;

                for (int i = (spos >= 0x7FF * 2 ? spos - 0x7FF * 2 : 0); i < spos; i += 2)
                {
                    if (infile[i] == infile[spos] && infile[i + 1] == infile[spos + 1])
                    {
                        int cur_len = 0;
                        do
                        {
                            cur_len += 2;
                        } while ((cur_len < 0xFFFF * 2)
                            && (spos + cur_len < size_b)
                            && infile[i + cur_len] == infile[spos + cur_len]
                            && infile[i + 1 + cur_len] == infile[spos + 1 + cur_len]);

                        if (cur_len > length)
                        {
                            offset = spos - i;
                            length = cur_len;
                            if (length >= 0xFFFF * 2)
                            {
                                break;
                            }
                        }
                    }
                }

                int compress_flag = 0;
                if (length >= 4)
                {
                    compress_flag = 1;
                    offset /= 2;
                    length /= 2;
                    int c = offset;
                    if (length <= 0x1F)
                    {
                        c |= length << 11;
                        dpos++;
                        dpos++;
                    }
                    else
                    {
                        dpos++;
                        dpos++;
                        dpos++;
                        dpos++;
                    }
                    spos += length * 2;
                }
                else
                {
                    dpos++;
                    spos++;
                    dpos++;
                    spos++;
                }

                cb |= compress_flag << cb_bit;
                cb_bit--;

                if (cb_bit < 0)
                {
                    //outfile[cb_pos + 0] = (byte)(cb & 0xFF);
                    //outfile[cb_pos + 1] = (byte)(cb >> 8);
                    cb = 0x0000;
                    cb_bit = 15;
                    cb_pos = dpos;
                    dpos++;
                    dpos++;
                }
            }
            cb |= 1 << cb_bit;
            //outfile[cb_pos + 0] = (byte)(cb & 0xFF);
            //outfile[cb_pos + 1] = (byte)(cb >> 8);
            dpos++;
            dpos++;

            return dpos;
        }
    }
}