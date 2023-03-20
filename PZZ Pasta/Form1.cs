using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static giogiogiogiogiogiogio.PZZManager;
using static giogiogiogiogiogiogio.TXBtool;

namespace PZZ_Pasta
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        readonly OpenFileDialog ofd = new OpenFileDialog();
        readonly SaveFileDialog sfd = new SaveFileDialog();
        readonly CommonOpenFileDialog dialog = new CommonOpenFileDialog();

        private void Button5_Click(object sender, EventArgs e)
        {
            help f = new help();
            f.Show();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true) checkBox2.Enabled = true;
            else if (checkBox1.Checked == false) checkBox2.Enabled = false;
        }

        public async void Button1_Click(object sender, EventArgs e)//extract
        {
            ofd.Title = "Select PZZ File";
            ofd.Filter = ".PZZ Files|*.pzz";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string infile = ofd.FileName;

                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    bool checkbox1stateOLD = checkBox1.Enabled;
                    bool checkbox2stateOLD = checkBox2.Enabled;
                    bool checkbox3stateOLD = checkBox3.Enabled;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;

                    string infileUnpack = Path.Combine(dialog.FileName, Path.GetFileNameWithoutExtension(ofd.FileName)) + "_unpack";
                    string infileTXB = Path.Combine(dialog.FileName, Path.GetFileNameWithoutExtension(ofd.FileName)) + "_textures";
                    bool texcheck = checkBox1.Checked;
                    bool clutcheck = checkBox2.Checked;
                    if (texcheck == false) clutcheck = false;
                    int tfunct = await PZZunpack(infileUnpack, infileTXB, infile, texcheck, clutcheck, checkBox3.Checked);
                    if (tfunct == 1) MessageBox.Show("Unpacked " + Path.GetFileName(ofd.FileName) + " to " + dialog.FileName, "PZZ-Manager", 0, MessageBoxIcon.Information);
                    if (tfunct == -1) MessageBox.Show("Unpack cancelled.", "PZZ-Manager", 0, MessageBoxIcon.Exclamation);
                    //await Task.Run(() => PZZunpack(infileUnpack, infileTXB, infile, extex));

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    checkBox1.Enabled = checkbox1stateOLD;
                    checkBox2.Enabled = checkbox2stateOLD;
                    checkBox3.Enabled = checkbox3stateOLD;
                }
            }
        }

        private async void Button2_Click(object sender, EventArgs e)//repack
        {
            ofd.Title = "Select PZZ Filelist";
            ofd.Filter = "PZZ Filelist|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sfd.Filter = ".PZZ File|*.pzz";
                sfd.Title = "Save as PZZ";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    bool checkbox1stateOLD = checkBox1.Enabled;
                    bool checkbox2stateOLD = checkBox2.Enabled;
                    bool checkbox3stateOLD = checkBox3.Enabled;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;

                    string testfilename = ofd.FileName.Replace("_filelist.txt", "");
                    string inputlist = ofd.FileName;
                    string infileUnpack = testfilename + "_unpack";
                    string infileTXB = testfilename + "_textures";
                    string outfile = Path.GetFileName(sfd.FileName);
                    List<string> filelist = new List<string>();
                    FileStream fs = (FileStream)sfd.OpenFile();

                    string[] readText = File.ReadAllLines(inputlist);
                    foreach (string s in readText)
                    {
                        filelist.Add(s);
                    }

                    if (checkBox1.Checked == true && Directory.Exists(infileTXB)) foreach (string fileee in Directory.EnumerateFiles(infileTXB, "*.txb"))
                        {
                            byte[] filebytes = File.ReadAllBytes(fileee);
                            string expath = infileTXB + "\\" + Path.GetFileNameWithoutExtension(fileee) + "_textures";
                            TXBre(expath + "\\" + Path.GetFileName(fileee), filebytes, checkBox2.Checked);
                            File.Copy(expath + "\\" + Path.GetFileName(fileee), infileUnpack + "\\" + Path.GetFileName(fileee), true);
                            File.Copy(expath + "\\" + Path.GetFileName(fileee), infileTXB + "\\" + Path.GetFileName(fileee), true);
                            File.Delete(expath + "\\" + Path.GetFileName(fileee));
                        }

                    int tfunct = await PZZrepack(infileUnpack, filelist, fs);
                    if (tfunct == 1) MessageBox.Show("Repacked to " + outfile, "PZZ-Manager", 0, MessageBoxIcon.Information);

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    checkBox1.Enabled = checkbox1stateOLD;
                    checkBox2.Enabled = checkbox2stateOLD;
                    checkBox3.Enabled = checkbox3stateOLD;
                }
            }
        }

        private async void Button4_Click(object sender, EventArgs e)//batchex
        {
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string inpath = dialog.FileName;
                int fcount = 0;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    int pzcount = Directory.EnumerateFiles(inpath, "*.pzz").Count();
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    bool checkbox1stateOLD = checkBox1.Enabled;
                    bool checkbox2stateOLD = checkBox2.Enabled;
                    bool checkbox3stateOLD = checkBox3.Enabled;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;

                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = pzcount;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;
                    foreach (string infile in Directory.EnumerateFiles(inpath, "*.pzz"))
                    {
                        string infileUnpack = Path.Combine(dialog.FileName, Path.GetFileNameWithoutExtension(infile)) + "_unpack";
                        string infileTXB = Path.Combine(dialog.FileName, Path.GetFileNameWithoutExtension(infile)) + "_textures";
                        bool texcheck = checkBox1.Checked;
                        bool clutcheck = checkBox2.Checked;
                        if (texcheck == false) clutcheck = false;
                        int tfunct = await PZZunpack(infileUnpack, infileTXB, infile, texcheck, clutcheck, checkBox3.Checked);
                        if (tfunct == 1)
                        {
                            fcount++;
                            progressBar1.PerformStep();
                        }
                    }
                    MessageBox.Show("Unpacked " + fcount + " files.", "PZZ-Manager", 0, MessageBoxIcon.Information);
                    progressBar1.Value = 1;

                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    checkBox1.Enabled = checkbox1stateOLD;
                    checkBox2.Enabled = checkbox2stateOLD;
                    checkBox3.Enabled = checkbox3stateOLD;
                }
            }
        }
        private async void Button3_Click(object sender, EventArgs e)//batchre
        {
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string inpath = dialog.FileName;
                int fcount = 0;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    int txcount = Directory.EnumerateFiles(inpath, "*.txt").Count();
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    bool checkbox1stateOLD = checkBox1.Enabled;
                    bool checkbox2stateOLD = checkBox2.Enabled;
                    bool checkbox3stateOLD = checkBox3.Enabled;
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    checkBox3.Enabled = false;

                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = txcount;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;
                    foreach (string infile in Directory.EnumerateFiles(inpath, "*.txt"))
                    {
                        string testfilename = infile.Replace("_filelist.txt", "");
                        string inputlist = infile;
                        string infileUnpack = testfilename + "_unpack";
                        string infileTXB = testfilename + "_textures";
                        string infileRepack = testfilename + "_newfiles";
                        string outfile = Path.Combine(dialog.FileName, Path.GetFileName(testfilename)) + "_modded.pzz";
                        List<string> filelist = new List<string>();
                        if (File.Exists(outfile) == true)
                        {
                            string message = Path.GetFileName(outfile) + " already exists!\nDo you want to overwrite?";
                            string caption = "Confirm Overwrite";
                            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                            DialogResult result;
                            result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                            if (result == DialogResult.No) continue;
                        }

                        FileStream fs = new FileStream(outfile, FileMode.Create);

                        string[] readText = File.ReadAllLines(inputlist);
                        foreach (string s in readText)
                        {
                            filelist.Add(s);
                        }

                        if (checkBox1.Checked == true && Directory.Exists(infileTXB)) foreach (string fileee in Directory.EnumerateFiles(infileTXB, "*.txb"))
                            {
                                byte[] filebytes = File.ReadAllBytes(fileee);
                                string expath = infileTXB + "\\" + Path.GetFileNameWithoutExtension(fileee) + "_textures";
                                TXBre(expath + "\\" + Path.GetFileName(fileee), filebytes, checkBox2.Checked);
                                File.Copy(expath + "\\" + Path.GetFileName(fileee), infileUnpack + "\\" + Path.GetFileName(fileee), true);
                                File.Copy(expath + "\\" + Path.GetFileName(fileee), infileTXB + "\\" + Path.GetFileName(fileee), true);
                                File.Delete(expath + "\\" + Path.GetFileName(fileee));
                            }
                        int tfunct = await PZZrepack(infileUnpack, filelist, fs);
                        if (tfunct == 1)
                        {
                            fcount++;
                            progressBar1.PerformStep();
                        }
                        
                    }
                    MessageBox.Show("Repacked " + fcount + " files.", "PZZ-Manager", 0, MessageBoxIcon.Information);
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    checkBox1.Enabled = checkbox1stateOLD;
                    checkBox2.Enabled = checkbox2stateOLD;
                    checkBox3.Enabled = checkbox3stateOLD;
                    progressBar1.Value = 1;
                }
            }
        }

        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
