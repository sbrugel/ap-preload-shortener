using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace PreloadShortener
{
    public partial class Form1 : Form
    {
        string gameDirectory;
        readonly string startupPath = Directory.GetCurrentDirectory();
        readonly string dirfile;

        string[] subd = null;

        List<string> assetFolders = new List<string>(); // all the user's asset providers that have preloads

        // for these, refer to addKeys()
        List<string> liveriesKey = new List<string>();
        List<string> liveriesNew = new List<string>();
        List<string> keywordsKey = new List<string>();
        List<string> keywordsNew = new List<string>();

        public Form1()
        {
            InitializeComponent();

            //get TS directory through registry
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Railsimulator.com\RailWorks");
                object objRegisteredValue = key.GetValue("EXE_Path");
                gameDirectory = objRegisteredValue.ToString().Substring(0, objRegisteredValue.ToString().Length - 13); //remove executable name
            }
            catch (Exception ex)
            {
                gameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\RailWorks";
            }

            label3.Text = "PLEASE NOTE: The more AP products you have installed, the longer this program will take.";
            dirfile = startupPath + "\\local\\dir.txt"; // read the first line of the dir.txt file
            string line1 = File.ReadLines(dirfile).First();
            if (!line1.Equals(null)) // set directory if one is saved
            {
                gameDirectory = line1;
                textBox1.Text = gameDirectory;
            }
            findFolders();
        }
        private void button2_Click(object sender, System.EventArgs e) // select folder
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                gameDirectory = @folderBrowserDialog1.SelectedPath;
                textBox1.Text = gameDirectory;
                File.WriteAllText(dirfile, String.Empty);
                TextWriter tw = new StreamWriter(dirfile, true);
                tw.WriteLine(gameDirectory); // save the directory in a .txt
                tw.Close();
            }
            findFolders();
        }

        private void findFolders()
        {
            try // try to find AP folders if this is a valid game directory
            {
                subd = Directory.GetDirectories(gameDirectory + "\\Assets\\AP");
                foreach (string subdirectory in subd) // loop through all AP folders and find preloads
                {
                    if (Directory.Exists(subdirectory + "\\PreLoad") && !subdirectory.Contains("WherryLines"))
                    {
                        listBox1.Items.Add(subdirectory);
                        assetFolders.Add(subdirectory);
                    }
                }
                subd = Directory.GetDirectories(gameDirectory + "\\Assets\\AP_Waggonz");
                foreach (string subdirectory in subd)
                {
                    if (Directory.Exists(subdirectory + "\\PreLoad") && !subdirectory.Contains("Class90Pack") && !subdirectory.Contains("MK3DVT"))
                    {
                        listBox1.Items.Add(subdirectory);
                        assetFolders.Add(subdirectory);
                    }
                }
                //additional directories not located in Assets/AP or APW
                if (Directory.Exists(gameDirectory + "\\Assets\\RSC\\Class91Addon\\PreLoad"))
                {
                    listBox1.Items.Add(gameDirectory + "\\Assets\\RSC\\Class91Addon");
                    assetFolders.Add(gameDirectory + "\\Assets\\RSC\\Class91Addon");
                }
                if (Directory.Exists(gameDirectory + "\\Assets\\RSC\\ECMLS\\PreLoad"))
                {
                    listBox1.Items.Add(gameDirectory + "\\Assets\\RSC\\ECMLS");
                    assetFolders.Add(gameDirectory + "\\Assets\\RSC\\ECMLS");
                }
            }
            catch (Exception ec) // if not a valid directory, do not scan and set directory to null
            {
                gameDirectory = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            addKeys();
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            for (int i = 0; i < assetFolders.Count; i++) //look through all valid folders and convert all preload files to .xml
                //xml is a readable format for the program, .bin is only readable by TS2021 and certain parsers (like TS-Tools)
            {
                var files = Directory.GetFiles(assetFolders[i] + "\\Preload", "*.bin", SearchOption.AllDirectories);
                progressBar1.Maximum += files.Length;
                foreach (var file in files)
                {
                    //opens up serz.exe, does not show a window but rather writes the output to the program console instead
                    //converts bin file to xml for editing
                    Process p = new Process();
                    p.StartInfo.FileName = gameDirectory + "\\serz.exe";
                    p.StartInfo.Arguments = "\"" + file + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.Start();
                    label3.Text = "Serzing " + file + "...";
                    this.Refresh();
                    Application.DoEvents();
                }
            }
            for (int i = 0; i < assetFolders.Count; i++) //once all converted, change elements and convert back to .bin
            {
                var files = Directory.GetFiles(assetFolders[i] + "\\Preload", "*.bin", SearchOption.AllDirectories); //get all .bin files
                foreach (var file in files)
                {
                    var toLoad = file.Replace(".bin", ".xml"); //to find the .xml file, just replace the extension
                    doc.Load(toLoad);
                    XmlNodeList elemList = doc.GetElementsByTagName("English"); //find both English tags which house the display names
                    for (int j = 0; j < elemList.Count; j++)
                    {
                        var element = elemList[j].InnerXml; //get the innerXML of the tags
                        //now replace the keywords according to addKeys()
                        for (int k = 0; k < keywordsKey.Count; k++)
                        {
                            element = element.Replace(keywordsKey[k], keywordsNew[k]);
                        }
                        for (int k = 0; k < liveriesKey.Count; k++)
                        {
                            element = element.Replace(liveriesKey[k], liveriesNew[k]);
                        }
                        label3.Text = "Changing elements of  " + file + "...";

                        //this is just to refresh the display, i.e. to progress the loading bar
                        this.Refresh();
                        Application.DoEvents();
                        Thread.Sleep(10);

                        // properly change the element
                        elemList[j].InnerXml = element;
                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Encoding = new UTF8Encoding(false); //false means do not emit the BOM - emitting it will make the .bin files completely empty
                        using (TextWriter sw = new StreamWriter(@toLoad, false, new UTF8Encoding(false)))
                        {
                            doc.Save(sw);
                        }
                    }

                    //opens up serz.exe, does not show a window but rather writes the output to the program console instead
                    //converts modified xml back to bin
                    Process p = new Process();
                    p.StartInfo.FileName = gameDirectory + "\\serz.exe";
                    p.StartInfo.Arguments = "\"" + toLoad + "\"";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.Start();
                    progressBar1.PerformStep();
                    label3.Text = "Re-Serzing " + file + "...";
                    this.Refresh();
                    Application.DoEvents();
                }

                // once everything is serz'd, delete all the unneeded .xml files
                var files2 = Directory.GetFiles(assetFolders[i] + "\\Preload", "*.xml", SearchOption.AllDirectories);
                foreach (var file in files2)
                {
                    File.Delete(file);
                    label3.Text = "Clearing of garbage...";
                    this.Refresh();
                    Application.DoEvents();
                }
            }
            label3.Text = "Done!";
            MessageBox.Show("Done shortening preload names! Woot!", "N!");
            progressBar1.Value = 0;
        }

        private void addKeys()
        {
            /* This portion is purely for the program's reference.
             * If anything found in keywordsKey or liveriesKey is found in the preload name,
             * the string is replaced by its corresponding entry in keywordsNew / liveriesNew
             * E.g. Unbranded is at the same index as UB in keywordsNew. As such, Unbranded -> UB
             * in all preload names that have it.
             * ***These strings are case-sensitive***
             */
            keywordsKey.Add("AP ");
            keywordsNew.Add("AP");
            keywordsKey.Add(" (EP)");
            keywordsNew.Add("");
            keywordsKey.Add("APW ");
            keywordsNew.Add("");
            keywordsKey.Add(" (AP)");
            keywordsNew.Add("");
            keywordsKey.Add("Class ");
            keywordsNew.Add("C");
            keywordsKey.Add("Unbranded");
            keywordsNew.Add("UB");
            keywordsKey.Add("168/170/171");
            keywordsNew.Add("170");
            keywordsKey.Add("465/466");
            keywordsNew.Add("465");
            keywordsKey.Add("375/377");
            keywordsNew.Add("375");
            keywordsKey.Add("158/159");
            keywordsNew.Add("158");
            keywordsKey.Add("High Speed Train");
            keywordsNew.Add("HST");
            keywordsKey.Add("New Lights"); 
            keywordsNew.Add("NL");
            keywordsKey.Add("Old Lights");
            keywordsNew.Add("OL");
            keywordsKey.Add("(Cummins)");
            keywordsNew.Add("(C)");
            keywordsKey.Add("(Perkins)");
            keywordsNew.Add("(P)");
            keywordsKey.Add("Revised");
            keywordsNew.Add("Rev.");
            keywordsKey.Add("Transport for Wales");
            keywordsNew.Add("TfW");
            keywordsKey.Add("Saltire");
            keywordsNew.Add("Salt");
            keywordsKey.Add("Large Logo");
            keywordsNew.Add("LL");
            keywordsKey.Add("Express");
            keywordsNew.Add("Exp.");
            keywordsKey.Add("Weathered");
            keywordsNew.Add("W");
            keywordsKey.Add("Low Panto");
            keywordsNew.Add("LP");

            liveriesKey.Add("Great Northern");
            liveriesNew.Add("GN");
            liveriesKey.Add("National Express East Anglia");
            liveriesNew.Add("NXEA");
            liveriesKey.Add("Stansted Express");
            liveriesNew.Add("SX");
            liveriesKey.Add("Network South Central");
            liveriesNew.Add("NSC");
            liveriesKey.Add("First Great Eastern");
            liveriesNew.Add("FGE");
            liveriesKey.Add("East Anglia");
            liveriesNew.Add("EA");
            liveriesKey.Add("CrossCountry");
            liveriesNew.Add("XC");
            liveriesKey.Add("London North Eastern Railway");
            liveriesNew.Add("LNER");
            liveriesKey.Add("London Northeastern Railway");
            liveriesNew.Add("LNER");
            liveriesKey.Add("Virgin Trains");
            liveriesNew.Add("VT");
            liveriesKey.Add("Central Trains");      
            liveriesNew.Add("CT");
            liveriesKey.Add("Northern Rail");
            liveriesNew.Add("NR");
            liveriesKey.Add("First North Western");
            liveriesNew.Add("FNW");
            liveriesKey.Add("London Midland");
            liveriesNew.Add("LM");
            liveriesKey.Add("Arriva Northern");
            liveriesNew.Add("NT");
            liveriesKey.Add("Arriva Trains Northern");
            liveriesNew.Add("ATN");
            liveriesKey.Add("Northern Spirit");
            liveriesNew.Add("NR SP");
            liveriesKey.Add("Northern");
            liveriesNew.Add("NT");
            liveriesKey.Add("Network West Midlands");
            liveriesNew.Add("NWM");
            liveriesKey.Add("East Coast");
            liveriesNew.Add("EC");
            liveriesKey.Add("East Midlands Trains");
            liveriesNew.Add("EMT");
            liveriesKey.Add("East Midlands Railway");
            liveriesNew.Add("EMR");
            liveriesKey.Add("First Great Western");
            liveriesNew.Add("FGW");
            liveriesKey.Add("GW Railway");
            liveriesNew.Add("GWR");
            liveriesKey.Add("Great Western Railway");
            liveriesNew.Add("GWR");
            liveriesKey.Add("Great Western");
            liveriesNew.Add("GW");
            liveriesKey.Add("Grand Central");
            liveriesNew.Add("GC");
            liveriesKey.Add("InterCity Swallow");
            liveriesNew.Add("ICS");
            liveriesKey.Add("InterCity");
            liveriesNew.Add("IC");
            liveriesKey.Add("Executive");
            liveriesNew.Add("Exec");
            liveriesKey.Add("Midland Mainline");
            liveriesNew.Add("MML");
            liveriesKey.Add("Mainline");
            liveriesNew.Add("ML");
            liveriesKey.Add("Network Rail");
            liveriesNew.Add("NetR");
            liveriesKey.Add("Wessex Trains");
            liveriesNew.Add("WT");
            liveriesKey.Add("Regional Railways");
            liveriesNew.Add("RR");
            liveriesKey.Add("ScotRail");
            liveriesNew.Add("SR");
            liveriesKey.Add("Scotrail");
            liveriesNew.Add("SR");
            liveriesKey.Add("DB Schenker");
            liveriesNew.Add("DBS");
            liveriesKey.Add("Freightliner");
            liveriesNew.Add("FL");
            liveriesKey.Add("GB Railfreight");
            liveriesNew.Add("GBRf");
            liveriesKey.Add("Railfreight");
            liveriesNew.Add("RF");
            liveriesKey.Add("Arriva Trains Wales");
            liveriesNew.Add("ATW");
            liveriesKey.Add("Chiltern Railways");
            liveriesNew.Add("CR");
            liveriesKey.Add("DB Cargo");
            liveriesNew.Add("DBC");
            liveriesKey.Add("Caledonian Sleeper");
            liveriesNew.Add("CS");
            liveriesKey.Add("Direct Rail Services");
            liveriesNew.Add("DRS");
            liveriesKey.Add("Transpennine Exp.");
            liveriesNew.Add("TPE");
            liveriesKey.Add("Transpennine Express");
            liveriesNew.Add("TPE");
            liveriesKey.Add("TransPennine Express");
            liveriesNew.Add("TPE");
            liveriesKey.Add("Anglia Railways");
            liveriesNew.Add("AR");
            liveriesKey.Add("Powerhaul");
            liveriesNew.Add("PH");
            liveriesKey.Add("Network SouthEast");
            liveriesNew.Add("NSE");
            liveriesKey.Add("Rail Express Systems");
            liveriesNew.Add("RES");
            liveriesKey.Add("Provincial");
            liveriesNew.Add("Prov.");
            liveriesKey.Add("London North Western Railway");
            liveriesNew.Add("LNWR");
            liveriesKey.Add("London Northwestern Railway");
            liveriesNew.Add("LNWR");
            liveriesKey.Add("North West");
            liveriesNew.Add("NW");
            liveriesKey.Add("Silverlink");
            liveriesNew.Add("SL");
            liveriesKey.Add("London Overground");
            liveriesNew.Add("LO");
            liveriesKey.Add("Wales and Borders");
            liveriesNew.Add("WB");
            liveriesKey.Add("Wales and West");
            liveriesNew.Add("WW");
            liveriesKey.Add("South West Trains");
            liveriesNew.Add("SWT");
            liveriesKey.Add("South Western Railway");
            liveriesNew.Add("SWR");
            liveriesKey.Add("Hull Trains");
            liveriesNew.Add("HT");
            liveriesKey.Add("Greater Anglia");
            liveriesNew.Add("GA");
            liveriesKey.Add("West Midlands Trains");
            liveriesNew.Add("WMT");
            liveriesKey.Add("Southern");
            liveriesNew.Add("SN");
            liveriesKey.Add("South Central");
            liveriesNew.Add("SC");
            liveriesKey.Add("SouthCentral");
            liveriesNew.Add("SC");
            liveriesKey.Add("South Eastern");
            liveriesNew.Add("SE");
            liveriesKey.Add("Connex");
            liveriesNew.Add("CX");
            liveriesKey.Add("First Capital Connect");
            liveriesNew.Add("FCC");
            liveriesKey.Add("Thameslink");
            liveriesNew.Add("TL");
            liveriesKey.Add("Europhoenix");
            liveriesNew.Add("EP");
            liveriesKey.Add("Rail Operations Group");
            liveriesNew.Add("ROG");
            liveriesKey.Add("Loadhaul");
            liveriesNew.Add("LH");
            liveriesKey.Add("Royal Scotsman");
            liveriesNew.Add("RS");
            liveriesKey.Add("West Coast Railway Company");
            liveriesNew.Add("WCRC");
            liveriesKey.Add("Civil Engineers");
            liveriesNew.Add("CE");
            liveriesKey.Add("Merseyrail");
            liveriesNew.Add("MR");
            liveriesKey.Add("Merseytravel");
            liveriesNew.Add("MT");
            liveriesKey.Add("Valley Lines");
            liveriesNew.Add("VL");
            liveriesKey.Add("Stansted Express");
            liveriesNew.Add("SX");
            liveriesKey.Add("Continental Airlines");
            liveriesNew.Add("Cont. AL");
            liveriesKey.Add("London and South East");
            liveriesNew.Add("LSE");
            liveriesKey.Add("APC");
            liveriesNew.Add("C");
            liveriesKey.Add("APW Class");
            liveriesNew.Add("C");
        }
    }
}