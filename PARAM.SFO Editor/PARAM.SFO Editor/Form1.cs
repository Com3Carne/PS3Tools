﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Deployment.Application;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace PARAM.SFO_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string FileLocation)
        {
            InitializeComponent();

            //disable everything that is not needed
            gbxSoundFormat.Enabled = false;
            gbxVideo.Enabled = false;
            tbControl.TabPages.Remove(tbPS4);
            tbControl.TabPages.Remove(tbPS3);
            tbControl.TabPages.Add(tbPS4);
            tbControl.TabPages.Add(tbPS3);

            //clear pannels
            cbxAddon.Items.Clear();
            cbVersion.Items.Clear();
            cbSystemVersion.Items.Clear();
            cbxAppVersion.Items.Clear();

            chbBoot.Enabled = false;
            chbBoot.Text = "Bootable";


            txtSFOpath.Text = FileLocation;
            pbLogo.Image = null;
            using (FileStream str = File.OpenRead(FileLocation))
            {

                psfo = new Param_SFO.PARAM_SFO(FileLocation);

                //WriteToXmlFile(Application.StartupPath + @"\testing.xml", psfo.Tables, true);


                MainPath = System.IO.Path.GetDirectoryName(FileLocation);

                //Check MAGIC
                if (psfo != null)
                    ReloadSFO();
            }
        }
        

        #region << Error Code >>

        //----The Point of this is to simulate ps3 data corrupt for whatever reason
        int errorcount = 0;
        bool errors = false;
        public void errormessage(int errorcount)
        {
            string message = "The Parameters of the System File Object has errors they have been marked\n\n\r\t Total Errors " + errorcount + "\n\n\r\t";
            MessageBox.Show(message, "Errors Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion << Error Code >>

        private System.Windows.Forms.Timer timer1;

        int i = 0;
        string MainPath;
        bool InitialLoad, CheckBoxBusy = false;

        Param_SFO.PARAM_SFO psfo;

        Playstation version;

        System.Timers.Timer timer;

        public enum Playstation
        {
            ps3 = 0,
            psvita = 1,
            ps4 = 2,
            psp = 3,
        }

        #region << For The Image Loop >>

        public void RunTimer()
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = Convert.ToInt32(TimeSpan.FromSeconds(5).TotalMilliseconds); // in milliseconds
            timer1.Start();
            if (backgroundWorker1.IsBusy == false)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        public void Stoptimer()
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy == false && backgroundWorker1.CancellationPending == false)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        #endregion << For The Image Loop >>


        #region << Methods >>

        #region << Swapping and bytes >>


        public static string ByteArrayToHexString(byte[] ByteArray)
        {
            string HexString = "";
            for (int i = 0; i < ByteArray.Length; ++i)
                HexString += ByteArray[i].ToString("X2"); // +" ";
            return HexString;
        }

        public static string HexStringToAscii(string HexString, bool cleanEndOfString)
        {
            try
            {
                string StrValue = "";
                // While there's still something to convert in the hex string
                while (HexString.Length > 0)
                {
                    // Use ToChar() to convert each ASCII value (two hex digits) to the actual character
                    StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexString.Substring(0, 2), 16)).ToString();

                    // Remove from the hex object the converted value
                    HexString = HexString.Substring(2, HexString.Length - 2);
                }
                // Clean String
                if (cleanEndOfString)
                    StrValue = StrValue.Replace("\0", "");

                return StrValue;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string ByteArrayToAscii(byte[] ByteArray, int startPos, int length, bool cleanEndOfString)
        {
            byte[] byteArrayPhrase = new byte[length];
            Array.Copy(ByteArray, startPos, byteArrayPhrase, 0, byteArrayPhrase.Length);
            string hexPhrase = ByteArrayToHexString(byteArrayPhrase);
            return HexStringToAscii(hexPhrase, true);
        }

        #endregion << Swapping and bytes >>


        /// <summary>
        /// Uncheck all Resolution boxes (PS3)
        /// </summary>
        private void Uncheck_Resolution_All()
        {
            chb720.Checked = false;
            chbx1080.Checked = false;
            chbx480.Checked = false;
            chbx480Wide.Checked = false;
            chbx576.Checked = false;
            chbx576Wide.Checked = false;
        }

        /// <summary>
        /// Gets the common path for PS4Tools
        /// I like to put it inside AppData/Roaming
        /// </summary>
        /// <returns></returns>
        private string AppCommonPath()
        {
            string returnstring = "";

            returnstring = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Ps4Tools\";

            return returnstring;
        }

        /// <summary>
        /// This will extract all resources for the solution
        /// v0.2- This Extracts SCE tools to the appdata folder
        /// V1.1+ - This only creates the working directory as we no longer need sce tools
        /// </summary>
        public void ExtractAllResources()
        {
            if (!Directory.Exists(AppCommonPath()))
            {
                Directory.CreateDirectory(AppCommonPath());
            }


            #region << (NO LONGER REQUIRED)>>
            //if (!Directory.Exists(AppCommonPath() + @"\ext\"))
            //{
            //    Directory.CreateDirectory(AppCommonPath() + @"\ext\");
            //}
            ////We will replace every file each time we call any toolkit to stop issues with different versions ext ext

            //////SCE Files 
            ////copy byte files

            ////ext
            //System.IO.File.WriteAllBytes(AppCommonPath() + @"\ext\" + "di.exe", Properties.Resources.di);
            //System.IO.File.WriteAllBytes(AppCommonPath() + @"\ext\" + "sc.exe", Properties.Resources.sc);
            //System.IO.File.WriteAllBytes(AppCommonPath() + @"\ext\" + "libatrac9.dll", Properties.Resources.libatrac9);
            ////orbis
            //System.IO.File.WriteAllBytes(AppCommonPath() + "orbis-pub-cmd.exe", Properties.Resources.orbis_pub_cmd);
            //System.IO.File.WriteAllBytes(AppCommonPath() + "orbis-pub-rx.dll", Properties.Resources.orbis_pub_prx);

            ////copy text files
            //System.IO.File.WriteAllText(AppCommonPath() + @"\ext\" + "trp_compare_default.css", Properties.Resources.trp_compare_default);

            #endregion << (NO LONGER REQUIRED)>>

            //Delete Working Directory and re-create it
            if (Directory.Exists(AppCommonPath() + @"\Working\"))
            {
                DeleteDirectory(AppCommonPath() + @"\Working\");
            }
            Directory.CreateDirectory(AppCommonPath() + @"\Working\");

        }

        /// <summary>
        /// Recursively delete directory
        /// </summary>
        /// <param name="target_dir">The Main Target Directory</param>
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }


        #region << SFX/SFO As XML >>

        public void CreateSFX(Param_SFO.PARAM_SFO psfo, SaveFileDialog dlg)
        {
            string FileHeader;
            if (version == Form1.Playstation.ps4)
            {
                //list items we don't want to see in the SFX
                List<string> Blockeditems = new List<string>();
                Blockeditems.Add("PUBTOOLINFO");
                Blockeditems.Add("DEV_FLAG");
                Blockeditems.Add("PUBTOOLVER");

                //table items
                FileHeader = CreateSFXHeader();
                string XMLItem = FileHeader + "\n<paramsfo>";//begin the tag
                foreach (var item in psfo.Tables)
                {
                    if (!Blockeditems.Contains(item.Name))
                        XMLItem += "\n\t<param key=\"" + item.Name + "\">" + item.Value + "</param>";
                }
                XMLItem += "\n</paramsfo>";//close the tag
                //we dont aks user where he wants to save the file 



                System.IO.File.WriteAllText(AppCommonPath() + @"\Working\param.sfx", XMLItem);

                System.IO.File.Copy(AppCommonPath() + @"\Working\param.sfx", dlg.FileName, true);//overwrite the file if it already exists

                System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(dlg.FileName));//start explorer
                //System.IO.File.Delete(AppCommonPath() + @"\Working\param.sfx");//remove the SFX

                MessageBox.Show("SFX Created", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                psfo.SaveSFO(psfo, dlg.FileName);

                MessageBox.Show("SFX Created", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }


        }

        public string CreateSFXHeader()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>";
        }

        #endregion << SFX/SFO As XML >>


        #endregion << Methods >>

        #region << Events >>

        private void button3_Click_1(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(Application.StartupPath));
        }

        private void txtTitleId_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "TITLE_ID")
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = txtTitleId.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "TITLE")
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = txtTitle.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void cbSystemVersion_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "SYSTEM_VER" || psfo.Tables[i].Name == "PS3_SYSTEM_VER")
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbSystemVersion.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void cbxParent_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxParent.Tag.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbxParent.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void cbxParent_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxParent.Tag.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbxParent.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void cbVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbVersion.Tag.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbVersion.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void cbxAppVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxAppVersion.Tag.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbxAppVersion.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
        /// <summary>
        /// Load Button Event ( Loads a psfo from a location )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //disable everything that is not needed
            gbxSoundFormat.Enabled = false;
            gbxVideo.Enabled = false;
            tbControl.TabPages.Remove(tbPS4);
            tbControl.TabPages.Remove(tbPS3);
            tbControl.TabPages.Add(tbPS4);
            tbControl.TabPages.Add(tbPS3);

            //clear pannels
            cbxAddon.Items.Clear();
            cbVersion.Items.Clear();
            cbSystemVersion.Items.Clear();
            cbxAppVersion.Items.Clear();

            chbBoot.Enabled = false;
            chbBoot.Text = "Bootable";

            OpenFileDialog thedialog = new OpenFileDialog();
            thedialog.Title = "PARAM.SFO";
            thedialog.Filter = ".SFO|*.SFO";
            thedialog.InitialDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
            if (thedialog.ShowDialog() == DialogResult.OK)
            {
                button5.Visible = true;
                txtSFOpath.Text = thedialog.FileName.ToString();
                timer.Stop();
                pbLogo.Image = null;
                using (FileStream str = File.OpenRead(thedialog.FileName.ToString()))
                {

                    psfo = new Param_SFO.PARAM_SFO(thedialog.FileName.ToString());

                    //WriteToXmlFile(Application.StartupPath + @"\testing.xml", psfo.Tables, true);


                    MainPath = System.IO.Path.GetDirectoryName(thedialog.FileName.ToString());

                    //Check MAGIC
                    if (psfo != null)
                    {
                        //set initial load too true so we dont do anything unnasasary 
                        InitialLoad = true;

                        List<string> AlreadyAdded = new List<string>();
                        foreach (Param_SFO.PARAM_SFO.Table t in psfo.Tables.ToList())
                        {
                            if (t.Name == "TITLE_ID")
                            {
                                txtTitleId.Text = t.Value.Trim();
                                txtTitleId.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                //and here we have it now we can add max lengths so users can't break anything
                                txtTitleId.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "PUBTOOLINFO")
                            {
                                txtPS4Pub.Text = t.Value.Trim();
                                txtPS4Pub.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                //and here we have it now we can add max lengths so users can't break anything
                                txtPS4Pub.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "PUBTOOLVER")
                            {
                                //cbxPS4Pub
                                int value = Convert.ToInt32(t.Value);
                                string hexOutput = String.Format("{0:X}", value);
                                cbxPS4Pub.Items.Add(hexOutput);
                                cbxPS4Pub.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                cbxPS4Pub.SelectedIndex = 0;
                                cbxPS4Pub.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "CONTENT_ID")
                            {
                                txtContentId.Text = t.Value.Trim();
                                txtContentId.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                txtContentId.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "TITLE")
                            {
                                txtTitle.Text = t.Value.Trim();
                                txtTitle.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                txtTitle.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "CATEGORY")
                            {
                                //we need to know what version of the sfo this is either ps3 / psvita(ps4)
                                //this id use for ps4 i geus vita can work 2
                                var hex = (BitConverter.ToString(t.ValueBuffer, 0, Convert.ToInt32(t.Indextable.param_data_max_len))).ToString().Replace("-", string.Empty);
                                var temp = Convert.ToInt32(hex).ToString("X4");
                                txtCATEGORY.Text = ((Param_SFO.PARAM_SFO.DataTypes)Convert.ToInt32(hex)).ToString();

                                //ps3
                                txtCATEGORY.Text = ((Param_SFO.PARAM_SFO.DataTypes)BitConverter.ToUInt16(Encoding.UTF8.GetBytes(t.Value), 0)).ToString();

                                //for now straight 
                                txtCATEGORY.Text = t.Value;
                                var remp = BitConverter.ToUInt16(Encoding.Default.GetBytes(hex), 0);
                                txtCATEGORY.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                txtCATEGORY.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "APP_VER")
                            {
                                cbxAppVersion.Items.Add(t.Value.Trim());
                                cbxAppVersion.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                cbxAppVersion.SelectedIndex = 0;
                                cbxAppVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "APP_TYPE")
                            {
                                cbxPS4AppVersion.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                cbxPS4AppVersion.SelectedIndex = Convert.ToInt32(t.Value);
                            }
                            if (t.Name == "VERSION")
                            {
                                cbVersion.Items.Add(t.Value.Trim());
                                cbVersion.Tag = t.Name;
                                AlreadyAdded.Add(t.Name);
                                cbVersion.SelectedIndex = 0;
                                cbVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "PARENTAL_LEVEL")
                            {
                                if (t.Value == "")
                                {
                                    cbxParent.SelectedIndex = 0;
                                }
                                else
                                {
                                    cbxParent.Tag = t.Name;
                                    cbxParent.SelectedIndex = Convert.ToInt32(t.Value);
                                    AlreadyAdded.Add(t.Name);
                                    cbxParent.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                                }
                            }
                            if (t.Name == "PS3_SYSTEM_VER")
                            {
                                cbSystemVersion.Tag = t.Name;
                                //we know its PS3
                                cbSystemVersion.Items.Add(t.Value);
                                pbLogo.Image = Properties.Resources.images;
                                AlreadyAdded.Add(t.Name);
                                cbSystemVersion.SelectedIndex = 0;
                                version = Playstation.ps3;
                                tbControl.TabPages.Remove(tbPS4);
                                cbSystemVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                            }
                            if (t.Name == "SYSTEM_VER")
                            {
                                cbSystemVersion.Tag = t.Name;
                                int value = Convert.ToInt32(t.Value);
                                string hexOutput = String.Format("{0:X}", value);
                                cbSystemVersion.Items.Add(hexOutput);
                                pbLogo.Image = Properties.Resources.ps4_logo_white1;
                                AlreadyAdded.Add(t.Name);
                                cbSystemVersion.SelectedIndex = 0;
                                version = Playstation.ps4;
                                tbControl.TabPages.Remove(tbPS3);
                                cbSystemVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                                
                            }
                            if (t.Name == "RESOLUTION")
                            {
                                gbxVideo.Enabled = true;
                                #region << PS3 Resolution >>
                                int Val = 0;
                                int.TryParse(t.Value.Trim(), out Val);
                                switch (Val)
                                {
                                    case 1:
                                        chbx480.Checked = true;
                                        break;
                                    case 2:
                                        chbx576.Checked = true;
                                        break;
                                    case 3:
                                        chbx480.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 4:
                                        chb720.Checked = true;
                                        break;
                                    case 5:
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 6:
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 7:
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 8:
                                        chbx1080.Checked = true;
                                        break;
                                    case 9:
                                        chbx1080.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 10:
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 11:
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 12:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 13:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 14:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 15:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 16:
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 17:
                                        chbx480Wide.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 18:
                                        chbx480Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 19:
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 20:
                                        chbx480Wide.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 21:
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 22:
                                        chbx480Wide.Checked = true;
                                        chbx576.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 23:
                                        chbx480Wide.Checked = true;
                                        chbx480.Checked = true;
                                        chbx576.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 24:
                                        chbx1080.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 25:
                                        chbx1080.Checked = true;
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 26:
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 27:
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 28:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 29:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 30:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480Wide.Checked = true;
                                        break;
                                    case 31:
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        chbx480Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 32:
                                        chbx576Wide.Checked = true;
                                        break;
                                    case 33:
                                        chbx576Wide.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 34:
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 35:
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 36:
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 37:
                                        chb720.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 38:
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx576Wide.Checked = true;
                                        break;
                                    case 39:
                                        chb720.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 40:
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        break;
                                    case 41:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 42:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 43:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 44:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 45:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 46:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 47:
                                        chbx1080.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 48:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        break;
                                    case 49:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 50:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 51:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 52:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 53:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 54:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 55:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 56:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        break;
                                    case 57:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 58:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 59:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 60:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        break;
                                    case 61:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    case 62:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        break;
                                    case 63:
                                        chbx480Wide.Checked = true;
                                        chbx576Wide.Checked = true;
                                        chbx1080.Checked = true;
                                        chb720.Checked = true;
                                        chbx576.Checked = true;
                                        chbx480.Checked = true;
                                        break;
                                    default:
                                        break;
                                }
                                #endregion << PS3 Resolution >>
                            }
                            if (t.Name == "BOOTABLE")
                            {
                                chbBoot.Enabled = true;
                                if (t.Value.ToString() == "0")
                                {
                                    chbBoot.Checked = false;
                                }
                                if (t.Value == "1")
                                {
                                    chbBoot.Checked = true;
                                    chbBoot.Text = "Bootable (Mode 1)";
                                }
                                if (t.Value == "2")
                                {
                                    chbBoot.Checked = true;
                                    chbBoot.Text = "Bootable (Mode 2)";
                                }
                            }
                            if (t.Name == "SOUND_FORMAT")
                            {
                                gbxSoundFormat.Enabled = true;
                                #region << PS3 Sound Format >>
                                int Val = 0;
                                int.TryParse(t.Value.Trim(), out Val);
                                switch (Val)
                                {
                                    case 1:
                                        chbxLPCM2.Checked = true;
                                        break;
                                    default:
                                        break;
                                }
                                #endregion << PS3 Sound Format >>
                            }

                            if (!AlreadyAdded.Contains(t.Name))
                            {
                                cbxAddon.Items.Add(t.Name);
                            }


                        }

                        //after loading we need to spesify some things
                        cbxAddon.SelectedIndex = 0;
                        if (backgroundWorker1.IsBusy == true)
                        {
                            Stoptimer();
                            backgroundWorker1.CancelAsync();
                        }

                        RunTimer();
                        btnRaw.Enabled = true;
                        InitialLoad = false;
                        button2.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("The file selected isn't a valid SFO", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
            else
            {
                tbControl.TabPages.Remove(tbPS4);
                tbControl.TabPages.Remove(tbPS3);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            RawView raw = new RawView(psfo, version);
            raw.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //check for updates on start up 
            //no longer required as we no use click once

            ExtractAllResources();

            timer = new System.Timers.Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
            LoadRanomImage();


            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            //Check to see if we are ClickOnce Deployed.
            //i.e. the executing code was installed via ClickOnce
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                //Collect the ClickOnce Current Version
                v = ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }

            //Show the version in a simple manner
            this.Text = string.Format("PARAM.SFO EDITOR : {0}", v);
        }

        public void LoadRanomImage()
        {
            Random r = new Random();
            int switcher = r.Next(0, 3);

            switch (switcher)
            {
                case 0:
                    { pbLogo.Image = Properties.Resources.ps_vita_logo; } break;
                case 1:
                    { pbLogo.Image = Properties.Resources.ps4_logo_white1; } break;
                case 2:
                    { pbLogo.Image = Properties.Resources.images; } break;
            }
        }
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LoadRanomImage();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorker1.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                while (i == 0)
                {
                    #region << PS3 >>
                    if (version == Playstation.ps3)
                    {

                        if (Directory.Exists(MainPath + @"\C00\") && i == 0)
                        {
                            pbLogoAndBackground.ImageLocation = MainPath + @"\C00\ICON0.PNG";
                            i = 1;
                            Thread.Sleep(1500);
                        }
                        else
                        {
                            pbLogoAndBackground.ImageLocation = MainPath + @"\ICON0.PNG";
                            i = 1;
                            Thread.Sleep(1500);
                        }
                    }
                    #endregion << PS3 >>
                    else
                    {
                        pbLogoAndBackground.ImageLocation = MainPath + @"\ICON0.PNG";
                        i = 1;
                        Thread.Sleep(1500);
                    }
                }
                while (i == 1)
                {
                    try
                    {
                        #region << PS3 >>
                        if (version == Playstation.ps3)
                        {
                            if (Directory.Exists(MainPath + @"\C00\") && i == 1)
                            {
                                pbLogoAndBackground.ImageLocation = MainPath + @"\C00\PIC1.PNG";
                                i = 0;
                                Thread.Sleep(1500);
                            }
                            else
                            {
                                pbLogoAndBackground.ImageLocation = MainPath + @"\PIC1.PNG";
                                i = 0;
                                Thread.Sleep(1500);
                            }
                        }
                        #endregion << PS3 >>

                        else
                        {
                            Random rnd = new Random();
                            int ran = rnd.Next(1, 3);
                            if (ran == 1)
                            {
                                pbLogoAndBackground.ImageLocation = MainPath + @"\PIC1.PNG";
                            }
                            else
                            {
                                if (File.Exists(MainPath + @"\PIC0.PNG"))
                                {
                                    pbLogoAndBackground.ImageLocation = MainPath + @"\PIC0.PNG";
                                }
                            }
                            i = 0;
                            Thread.Sleep(1500);
                        }
                    }
                    catch (Exception ee)
                    {
                        string test = ee.Message;
                    }
                }
            }
        }

        private void cbxAddon_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Param_SFO.PARAM_SFO.Table item in psfo.Tables)
            {
                if (item.Name == cbxAddon.SelectedItem.ToString().Trim() && item.Name != string.Empty)
                {
                    CheckBoxBusy = true;
                    txtAddonData.Text = item.Value.ToString();
                    CheckBoxBusy = false;
                    txtAddonData.MaxLength = Convert.ToInt32(item.Indextable.param_data_max_len);
                }
            }
        }

        private void txtAddonData_Leave(object sender, EventArgs e)
        {
            //on leave save the info to the table
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxAddon.SelectedItem.ToString().Trim())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = txtAddonData.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                //item has been cancled
                MessageBox.Show("Prog Cancled");
            }

        }

        private void txtAddonData_TextChanged(object sender, EventArgs e)
        {
            if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
            {
                Uncheck_Resolution_All();
                #region << PS3 Resolution >>
                int Val = 0;
                int.TryParse(txtAddonData.Text.Trim(), out Val);
                switch (Val)
                {
                    case 1:
                        chbx480.Checked = true;
                        break;
                    case 2:
                        chbx576.Checked = true;
                        break;
                    case 3:
                        chbx480.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 4:
                        chb720.Checked = true;
                        break;
                    case 5:
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 6:
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 7:
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 8:
                        chbx1080.Checked = true;
                        break;
                    case 9:
                        chbx1080.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 10:
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 11:
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 12:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 13:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 14:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 15:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 16:
                        chbx480Wide.Checked = true;
                        break;
                    case 17:
                        chbx480Wide.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 18:
                        chbx480Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 19:
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 20:
                        chbx480Wide.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 21:
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 22:
                        chbx480Wide.Checked = true;
                        chbx576.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 23:
                        chbx480Wide.Checked = true;
                        chbx480.Checked = true;
                        chbx576.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 24:
                        chbx1080.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 25:
                        chbx1080.Checked = true;
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 26:
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 27:
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 28:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 29:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 30:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480Wide.Checked = true;
                        break;
                    case 31:
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        chbx480Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 32:
                        chbx576Wide.Checked = true;
                        break;
                    case 33:
                        chbx576Wide.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 34:
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 35:
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 36:
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 37:
                        chb720.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 38:
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx576Wide.Checked = true;
                        break;
                    case 39:
                        chb720.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 40:
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        break;
                    case 41:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 42:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 43:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 44:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 45:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 46:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 47:
                        chbx1080.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 48:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        break;
                    case 49:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 50:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 51:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 52:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 53:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 54:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 55:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 56:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        break;
                    case 57:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 58:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 59:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 60:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        break;
                    case 61:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx480.Checked = true;
                        break;
                    case 62:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        break;
                    case 63:
                        chbx480Wide.Checked = true;
                        chbx576Wide.Checked = true;
                        chbx1080.Checked = true;
                        chb720.Checked = true;
                        chbx576.Checked = true;
                        chbx480.Checked = true;
                        break;
                    default:
                        break;
                }
                #endregion << PS3 Resolution >>
            }
            //set all other data
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxAddon.SelectedText.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = txtAddonData.Text.Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        #region << Resolutions >>

        private void chb720_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chb720.Checked == true)
            {

                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 4).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 4).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        private void chbx576Wide_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chbx576Wide.Checked == true)
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 32).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 32).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        private void chbx576_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chbx576.Checked == true)
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 2).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 2).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        private void chbx480Wide_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chbx480Wide.Checked == true)
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 16).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 16).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        private void chbx480_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chbx480.Checked == true)
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 1).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 1).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        private void chbx1080_CheckedChanged(object sender, EventArgs e)
        {
            if (InitialLoad == true || CheckBoxBusy == true)
            {
                return;
            }

            CheckBoxBusy = true;

            //first get the value from param table
            int iValue = 0, psfoValue = 0;

            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == "RESOLUTION")
                {
                    int.TryParse(psfo.Tables[i].Value, out psfoValue);
                    iValue = i;
                    break;
                }
            }
            //now we add or subtract
            if (chbx1080.Checked == true)
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue + 8).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
            else
            {
                var tempitem = psfo.Tables[iValue];
                tempitem.Value = (psfoValue - 8).ToString();
                psfo.Tables[iValue] = tempitem;
                if (cbxAddon.SelectedItem.ToString() == "RESOLUTION")
                {
                    txtAddonData.Text = psfo.Tables[iValue].Value.ToString();
                    CheckBoxBusy = false;
                }
            }
        }

        #endregion << Resolutions >>

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PARAM.SFO (PARAM.SFO)|PARAM.SFO";
            dlg.DefaultExt = "SFO";
            dlg.AddExtension = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //user wants to save in a new location or whatever
                psfo.SaveSFO(psfo, dlg.FileName);
                //CreateSFX(psfo, dlg);/*Old Method using CMD*/
                MessageBox.Show("SFO Created", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cbxPS4AppVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < psfo.Tables.Count; i++)
            {
                if (psfo.Tables[i].Name == cbxPS4AppVersion.Tag.ToString())
                {
                    var tempitem = psfo.Tables[i];
                    tempitem.Value = cbxPS4AppVersion.SelectedIndex.ToString().Trim();
                    psfo.Tables[i] = tempitem;
                }
            }
        }

        #endregion << Events >>

      

        public List<Param_SFO.PARAM_SFO.Table> AddNewItem(int Index,string Name,string Value,Param_SFO.PARAM_SFO.FMT format,int lenght,int maxlength,List<Param_SFO.PARAM_SFO.Table> xtable)
        {
            Param_SFO.PARAM_SFO.index_table indextable = new Param_SFO.PARAM_SFO.index_table();

            Param_SFO.PARAM_SFO.Table tableitem = new Param_SFO.PARAM_SFO.Table();

            indextable.param_data_fmt = format;
            indextable.param_data_len = Convert.ToUInt32(lenght);
            indextable.param_data_max_len = Convert.ToUInt32(maxlength);
            tableitem.index = Index;
            tableitem.Indextable = indextable;
            tableitem.Name = Name;
            tableitem.Value = Value;
            xtable.Add(tableitem);

            return xtable;
        }

        int NewItemIndex = 0; //this var is used for item indexes
        List<Param_SFO.PARAM_SFO.Table> xtables = new List<Param_SFO.PARAM_SFO.Table>();
        private void button4_Click(object sender, EventArgs e)
        {
            NewSFO newsfo = new NewSFO();
            newsfo.ShowDialog();
            //vita and ps3 seem to be relatively similar 
            if (newsfo._SfoToMake == NewSFO.SFOToMake.PS4 || newsfo._SfoToMake == NewSFO.SFOToMake.PSVita)
            {
                button5.Visible = true;

                xtables = new List<Param_SFO.PARAM_SFO.Table>();

                AddNewItem(NewItemIndex++, "APP_TYPE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "ATTRIBUTE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "APP_VER", "01.00", Param_SFO.PARAM_SFO.FMT.Utf8Null, 5, 8, xtables);

                AddNewItem(NewItemIndex++, "CATEGORY", "gd", Param_SFO.PARAM_SFO.FMT.Utf8Null, 3, 4, xtables);

                AddNewItem(NewItemIndex++, "CONTENT_ID", "XXYYYY-XXXXYYYYY_00-ZZZZZZZZZZZZZZZZ", Param_SFO.PARAM_SFO.FMT.Utf8Null, 37, 48, xtables);

                AddNewItem(NewItemIndex++, "DOWNLOAD_DATA_SIZE", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "FORMAT", "obs", Param_SFO.PARAM_SFO.FMT.Utf8Null, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "PARENTAL_LEVEL", "1", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "REMOTE_PLAY_KEY_ASSIGN", "1", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                for (int i = 1; i < 8; i++)
                {
                    AddNewItem(NewItemIndex++, "SERVICE_ID_ADDCONT_ADD_" + i, "", Param_SFO.PARAM_SFO.FMT.Utf8Null, 1, 20, xtables);
                }

                AddNewItem(NewItemIndex++, "SYSTEM_VER", "0", Param_SFO.PARAM_SFO.FMT.UINT32, 4, 4, xtables);

                AddNewItem(NewItemIndex++, "TITLE", "Your Game Title ID", Param_SFO.PARAM_SFO.FMT.Utf8Null, 19, 128, xtables);

                AddNewItem(NewItemIndex++, "TITLE_ID", "XXXXYYYYY", Param_SFO.PARAM_SFO.FMT.Utf8Null, 10, 12, xtables);

                AddNewItem(NewItemIndex++, "VERSION", "01.00", Param_SFO.PARAM_SFO.FMT.Utf8Null, 6, 8, xtables);


                psfo = new Param_SFO.PARAM_SFO();


                Param_SFO.PARAM_SFO.Header.IndexTableEntries = Convert.ToUInt32(NewItemIndex);

                psfo.Tables = xtables;
            }

            ReloadSFO();
          
           
        }

        public void ReloadSFO()
        {
            if (psfo != null)
            {
                //set initial load too true so we dont do anything unnasasary 
                InitialLoad = true;

                cbxAddon.Items.Clear();
                cbSystemVersion.Items.Clear();
                cbVersion.Items.Clear();
                cbxAppVersion.Items.Clear();
                //cbxParent.Items.Clear();
                //cbxPS4AppVersion.Items.Clear();
                cbxPS4Pub.Items.Clear();
                

                List<string> AlreadyAdded = new List<string>();
                foreach (Param_SFO.PARAM_SFO.Table t in psfo.Tables.ToList())
                {
                    if (t.Name == "TITLE_ID")
                    {
                        txtTitleId.Text = t.Value.Trim();
                        txtTitleId.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        //and here we have it now we can add max lengths so users can't break anything
                        txtTitleId.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "PUBTOOLINFO")
                    {
                        txtPS4Pub.Text = t.Value.Trim();
                        txtPS4Pub.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        //and here we have it now we can add max lengths so users can't break anything
                        txtPS4Pub.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "PUBTOOLVER")
                    {
                        //cbxPS4Pub
                        int value = Convert.ToInt32(t.Value);
                        string hexOutput = String.Format("{0:X}", value);
                        cbxPS4Pub.Items.Add(hexOutput);
                        cbxPS4Pub.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        cbxPS4Pub.SelectedIndex = 0;
                        cbxPS4Pub.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "CONTENT_ID")
                    {
                        txtContentId.Text = t.Value.Trim();
                        txtContentId.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        txtContentId.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "TITLE")
                    {
                        txtTitle.Text = t.Value.Trim();
                        txtTitle.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        txtTitle.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "CATEGORY")
                    {
                        //we need to know what version of the sfo this is either ps3 / psvita(ps4)
                        //this id use for ps4 i geus vita can work 2
                        var hex = (BitConverter.ToString(t.ValueBuffer, 0, Convert.ToInt32(t.Indextable.param_data_max_len))).ToString().Replace("-", string.Empty);
                        var temp = Convert.ToInt32(hex).ToString("X4");
                        txtCATEGORY.Text = ((Param_SFO.PARAM_SFO.DataTypes)Convert.ToInt32(hex)).ToString();

                        //ps3
                        txtCATEGORY.Text = ((Param_SFO.PARAM_SFO.DataTypes)BitConverter.ToUInt16(Encoding.UTF8.GetBytes(t.Value), 0)).ToString();

                        //for now straight 
                        txtCATEGORY.Text = t.Value;
                        var remp = BitConverter.ToUInt16(Encoding.Default.GetBytes(hex), 0);
                        txtCATEGORY.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        txtCATEGORY.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "APP_VER")
                    {
                        cbxAppVersion.Items.Add(t.Value.Trim());
                        cbxAppVersion.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        cbxAppVersion.SelectedIndex = 0;
                        cbxAppVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "APP_TYPE")
                    {
                        cbxPS4AppVersion.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        cbxPS4AppVersion.SelectedIndex = Convert.ToInt32(t.Value);
                    }
                    if (t.Name == "VERSION")
                    {
                        cbVersion.Items.Add(t.Value.Trim());
                        cbVersion.Tag = t.Name;
                        AlreadyAdded.Add(t.Name);
                        cbVersion.SelectedIndex = 0;
                        cbVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "PARENTAL_LEVEL")
                    {
                        if (t.Value == "")
                        {
                            cbxParent.SelectedIndex = 0;
                        }
                        else
                        {
                            cbxParent.Tag = t.Name;
                            cbxParent.SelectedIndex = Convert.ToInt32(t.Value);
                            AlreadyAdded.Add(t.Name);
                            cbxParent.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                        }
                    }
                    if (t.Name == "PS3_SYSTEM_VER")
                    {
                        cbSystemVersion.Tag = t.Name;
                        //we know its PS3
                        cbSystemVersion.Items.Add(t.Value);
                        pbLogo.Image = Properties.Resources.images;
                        AlreadyAdded.Add(t.Name);
                        cbSystemVersion.SelectedIndex = 0;
                        version = Playstation.ps3;
                        tbControl.TabPages.Remove(tbPS4);
                        cbSystemVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);
                    }
                    if (t.Name == "SYSTEM_VER")
                    {
                        cbSystemVersion.Tag = t.Name;
                        int value = Convert.ToInt32(t.Value);
                        string hexOutput = String.Format("{0:X}", value);
                        cbSystemVersion.Items.Add(hexOutput);
                        pbLogo.Image = Properties.Resources.ps4_logo_white1;
                        AlreadyAdded.Add(t.Name);
                        cbSystemVersion.SelectedIndex = 0;
                        version = Playstation.ps4;
                        tbControl.TabPages.Remove(tbPS3);
                        cbSystemVersion.MaxLength = Convert.ToInt32(t.Indextable.param_data_max_len);

                    }
                    if (t.Name == "RESOLUTION")
                    {
                        gbxVideo.Enabled = true;
                        #region << PS3 Resolution >>
                        int Val = 0;
                        int.TryParse(t.Value.Trim(), out Val);
                        switch (Val)
                        {
                            case 1:
                                chbx480.Checked = true;
                                break;
                            case 2:
                                chbx576.Checked = true;
                                break;
                            case 3:
                                chbx480.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 4:
                                chb720.Checked = true;
                                break;
                            case 5:
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 6:
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 7:
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 8:
                                chbx1080.Checked = true;
                                break;
                            case 9:
                                chbx1080.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 10:
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 11:
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 12:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 13:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 14:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 15:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 16:
                                chbx480Wide.Checked = true;
                                break;
                            case 17:
                                chbx480Wide.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 18:
                                chbx480Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 19:
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 20:
                                chbx480Wide.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 21:
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 22:
                                chbx480Wide.Checked = true;
                                chbx576.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 23:
                                chbx480Wide.Checked = true;
                                chbx480.Checked = true;
                                chbx576.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 24:
                                chbx1080.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 25:
                                chbx1080.Checked = true;
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 26:
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 27:
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 28:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 29:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 30:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480Wide.Checked = true;
                                break;
                            case 31:
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                chbx480Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 32:
                                chbx576Wide.Checked = true;
                                break;
                            case 33:
                                chbx576Wide.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 34:
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 35:
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 36:
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 37:
                                chb720.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 38:
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx576Wide.Checked = true;
                                break;
                            case 39:
                                chb720.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 40:
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                break;
                            case 41:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 42:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 43:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 44:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 45:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 46:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 47:
                                chbx1080.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 48:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                break;
                            case 49:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 50:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 51:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 52:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 53:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 54:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 55:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 56:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                break;
                            case 57:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 58:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 59:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 60:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                break;
                            case 61:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx480.Checked = true;
                                break;
                            case 62:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                break;
                            case 63:
                                chbx480Wide.Checked = true;
                                chbx576Wide.Checked = true;
                                chbx1080.Checked = true;
                                chb720.Checked = true;
                                chbx576.Checked = true;
                                chbx480.Checked = true;
                                break;
                            default:
                                break;
                        }
                        #endregion << PS3 Resolution >>
                    }
                    if (t.Name == "BOOTABLE")
                    {
                        chbBoot.Enabled = true;
                        if (t.Value.ToString() == "0")
                        {
                            chbBoot.Checked = false;
                        }
                        if (t.Value == "1")
                        {
                            chbBoot.Checked = true;
                            chbBoot.Text = "Bootable (Mode 1)";
                        }
                        if (t.Value == "2")
                        {
                            chbBoot.Checked = true;
                            chbBoot.Text = "Bootable (Mode 2)";
                        }
                    }
                    if (t.Name == "SOUND_FORMAT")
                    {
                        gbxSoundFormat.Enabled = true;
                        #region << PS3 Sound Format >>
                        int Val = 0;
                        int.TryParse(t.Value.Trim(), out Val);
                        switch (Val)
                        {
                            case 1:
                                chbxLPCM2.Checked = true;
                                break;
                            default:
                                break;
                        }
                        #endregion << PS3 Sound Format >>
                    }

                    if (!AlreadyAdded.Contains(t.Name))
                    {
                        cbxAddon.Items.Add(t.Name);
                    }


                }

                //after loading we need to spesify some things
                cbxAddon.SelectedIndex = 0;
                if (backgroundWorker1.IsBusy == true)
                {
                    Stoptimer();
                    backgroundWorker1.CancelAsync();
                }

                RunTimer();
                btnRaw.Enabled = true;
                InitialLoad = false;
                button2.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            NewSFOItem newitem = new NewSFOItem();
            if(newitem.ShowDialog() == DialogResult.OK)
            {
                //for saftey
                xtables = psfo.Tables;

                if (NewItemIndex != 0)
                {
                    AddNewItem(NewItemIndex++, newitem.tableItemAdded.Name, newitem.tableItemAdded.Value, newitem.Indextableadded.param_data_fmt, Convert.ToInt32(newitem.Indextableadded.param_data_len), Convert.ToInt32(newitem.Indextableadded.param_data_max_len), xtables);
                }
                else
                {
                    NewItemIndex = Convert.ToInt32(Param_SFO.PARAM_SFO.Header.IndexTableEntries) + 1;
                    
                    AddNewItem(NewItemIndex++, newitem.tableItemAdded.Name, newitem.tableItemAdded.Value, newitem.Indextableadded.param_data_fmt, Convert.ToInt32(newitem.Indextableadded.param_data_len), Convert.ToInt32(newitem.Indextableadded.param_data_max_len), xtables);
                }
                psfo = new Param_SFO.PARAM_SFO();

                Param_SFO.PARAM_SFO.Header.IndexTableEntries = Convert.ToUInt32(NewItemIndex);


                psfo.Tables = xtables;
                ReloadSFO();

                tbControl.TabPages.Remove(tbPS4);
                tbControl.TabPages.Remove(tbPS3);
                tbControl.TabPages.Add(tbPS4);
                tbControl.TabPages.Add(tbPS3);

                tbControl.TabPages.Remove(tbPS3);

                btnRaw.Enabled = true;
                InitialLoad = false;
                button2.Enabled = true;
            }
        }

    }
}
