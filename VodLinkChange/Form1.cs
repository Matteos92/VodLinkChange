using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace VodLinkChange
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            startProgram();
        }
        
        public string path, filesnumber;
        public string file, xmlFile, file2;
        public bool isChosen11 = false;//dla buttona 1
        public bool isChosen12 = false;//dla buttona 13
        public bool isChosen2 = false;//dla buttona 2
        public bool isChosen3 = false;//dla buttona 9
        public bool isChosen4 = false;//dla buttona 16

        public void startProgram()
        {
            label3.Text = " ";//sciezka do xml z vod
            label4.Text = " ";//sciezka do csv z vod
            label5.Text = " ";//sciezka do podmiany tekstu
            label6.Text = " ";//sciezka do zmian image'ow
            label7.Text = " ";//sciezka do short descriptor checher
            label8.Text = "Postęp";
            label13.Text = " ";
            label14.Text = " ";
            label15.Text = " ";
        }
        //podmiana linków vod --------------------------------------------
        //folder xml
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                label3.Text = fbd.SelectedPath;
                isChosen11 = true;
            }
        }
        //do pojedynczego pliku xml
        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlFile = ofd.FileName.ToString();
                string ext = Path.GetExtension(xmlFile);
                if (ext == ".xml")
                {
                    label3.Text = ofd.FileName.ToString();
                    isChosen12 = true;
                }
                else
                {
                    MessageBox.Show("To nie jest plik .xml");
                    xmlFile = null;
                }
            }
        }
        // csv
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file = ofd.FileName.ToString();
                string ext = Path.GetExtension(file);
                if (ext == ".csv")
                {
                    label4.Text = ofd.FileName.ToString();
                    isChosen2 = true;
                }
                else
                {
                    file = null;
                    MessageBox.Show("To nie jest plik .csv");
                }   
            }
        }
        //znajdz i wstaw
        //path dla sciezki, file dla pliku csv
        private void Button18_Click(object sender, EventArgs e)
        {
            if (isChosen11 == false && isChosen12 == false)
            {
                MessageBox.Show("Nie wybrano folderu z plikami xml bądź samego pliku");
            }
            else
            {
                string[] files = Directory.GetFiles(path);
                string line;
                int nr = 1;
                StreamReader csvreader = new StreamReader(file, Encoding.UTF8);

                while ((line = csvreader.ReadLine()) != null)
                {
                    var lastPart = line.Split(';').ToList();

                    string value = lastPart[1].ToString();//vod link
                    string filmName = lastPart[2].ToString();// nazwa filmu

                    Debug.WriteLine(nr.ToString() + ": " + filmName);
                    nr++;
                    for (int i = 0; i < files.Length; i++)
                    {
                        
                        try
                        {
                            XDocument xmlFile = XDocument.Load(files[i]);
                            var events = xmlFile.Root.Descendants("event").ToList();

                            foreach (XElement eventt in events)
                            {
                                var items = eventt.Element("short-event-descriptor");
                                var bets = eventt.Descendants("event-name").ToList();
                                var eventName = bets.First().Value.ToString();
                                //Tu dac checkseasonforhbo
                                //eventName = CheckSeasonForHbo(filmName);
                                if (string.Equals(filmName, eventName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (eventt.Element("custom-items-descriptor") != null)
                                    {
                                        eventt.Element("custom-items-descriptor").Element("item").Attribute("value").SetValue(value);
                                    }
                                    else
                                    {
                                        eventt.Element("parental-rating-descriptor").AddAfterSelf(new XElement("custom-items-descriptor", new XElement("item", new XAttribute("type", "Vod_Link"), new XAttribute("value", value))));
                                        //eventt.Element("resource-descriptor").AddBeforeSelf(new XElement("custom-items-descriptor", new XElement("item", new XAttribute("type", "Vod_Link"), new XAttribute("value", value))));
                                        //eventt.Add(new XElement("custom-items-descriptor", new XElement("item", new XAttribute("type", "Vod_Link"), new XAttribute("value", value))));
                                    }
                                }
                            }
                            xmlFile.Save(files[i]);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
                MessageBox.Show("Koniec");
            }
        }
        // dla hbo
        private string CheckEpisodeForHbo(string episode)
        {

            return episode;
        }

        private string CheckSeasonForHbo(string season)
        {
             if (season.Contains("II,"))
              {
                String matchpattern = @" II[,].+";
                String replacementpattern = @"";
                season = Regex.Replace(season, matchpattern, replacementpattern);
                seasonNumber = "2";
                }               
            return season;
        }
        // podmień
        private void button5_Click(object sender, EventArgs e)
        {
            if (isChosen11 == false && isChosen12 == false)
            {
                MessageBox.Show("Nie wybrano folderu z plikami xml bądź samego pliku");
            }
            else
            {
                if (isChosen2 == true)
                {
                    string line, line2, line3, replace = ";";
                    StreamReader reader = new StreamReader(file, Encoding.Default);
                    line = reader.ReadToEnd();
                    reader.Close();
                    line2 = line;
                    line3 = line;

                    Regex reg = new Regex(@"\d{1,6}\;");//numer vod 
                    Regex regvod2 = new Regex(@"\w{8}-.+-.+-.+-\w{12}");//vod link
                    Regex regimdb = new Regex(@";tt\d{7};");//numer imdb
                    string rege = @"\;\w{8}-.+-.+-.+-\w{12}\;.+";
                    //plik .bak
                    string file2 = file + ".bak";
                    if (File.Exists(file2))
                    { 
                        File.Delete(file2);
                    }
                    line2 = Regex.Replace(line2, rege, replace);
                    FileStream fs = new FileStream(file2, FileMode.CreateNew, FileAccess.ReadWrite);
                    using (StreamWriter writer = new StreamWriter(fs, Encoding.Default))
                    {
                        writer.Write(line2);
                        writer.Close();
                    }

                    StreamReader reader2 = new StreamReader(file2, Encoding.Default);
                    string line1 = reader2.ReadToEnd();
                    reader2.Close();

                    MatchCollection mc = reg.Matches(line1);//kopia
                    MatchCollection mc2 = regvod2.Matches(line3);//oryginał
                    MatchCollection mcm = regimdb.Matches(line3);//imdb

                    var arr = mc.OfType<Match>().Select(m => m.Groups[0]).ToArray();//kopia
                    var arr2 = mc2.OfType<Match>().Select(m => m.Groups[0]).ToArray();//oryginał
                    var arrm = mcm.OfType<Match>().Select(m => m.Groups[0]).ToArray();//imdb

                    string linia1 = "arr: " + arr.Length + " arr2: " + arr2.Length + " arrm: " + arrm.Length;
                    Debug.WriteLine(linia1);

                    StringBuilder obiekt2 = new StringBuilder();
                    string number, link;
                    progressBar1.Minimum = 0;
                    progressBar5.Minimum = 0;

                    if (checkBox2.Checked == false && checkBox3.Checked == false)
                                {
                                    MessageBox.Show("Wybierz podmianę HBO i/lub IMDB");
                                }
                    if (checkBox2.Checked == true)//hbo
                    {
                        label8.Text = "";
                        if (arr.Length == arr2.Length)
                        {
                            progressBar1.Maximum = arr.Length;
                            for (int Idx = 0; Idx < arr.Length; Idx++)
                            {
                                number = arr[Idx].Value.Remove(arr[Idx].Value.Length - 1);
                                link = arr2[Idx].Value;
                                
                                label8.Text = label8.Text + ": " + (Idx + 1) + " / " + arr.Length;
                                progressBar1.Value = Idx + 1;
                                if (isChosen11 == true)//dla folderu
                                {
                                    Debug.WriteLine("zmiana "+ Idx +": " + number + " na " + link);
                                    makeChange(path, number, link);
                                    //-----------------------------------------------
                                     obiekt2.Append("-").Append(number);
                                    number = obiekt2.ToString();
                                    //-----------------------------------------------
                                    obiekt2.Clear();
                                    Debug.WriteLine("zmiana " + Idx + ": " + number + " na " + link);
                                    makeChange(path, number, link);
                                }
                                else if (isChosen12 == true)//dla jednego pliku xml
                                {
                                    makeChangeForOneXml(xmlFile, number, link);
                                    obiekt2.Append("-").Append(number);
                                    number = obiekt2.ToString();
                                    obiekt2.Clear();
                                    makeChangeForOneXml(xmlFile, number, link);
                                }
                                else if (isChosen2 == true)//dla jednego pliku csv
                                {
                                    makeChangeForOneXml(xmlFile, number, link);
                                    obiekt2.Append("-").Append(number);
                                    number = obiekt2.ToString();
                                    obiekt2.Clear();
                                    makeChangeForOneXml(xmlFile, number, link);
                                }
                                else
                                {
                                    MessageBox.Show("Nie wykonano żadnych działań na plikach");
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine(linia1);
                            MessageBox.Show("Nie takie rozmiary");
                        }
                    }
                    if (checkBox3.Checked == true)//imdb
                    {
                        if (arr.Length == arrm.Length)
                        {
                            progressBar5.Maximum = arr.Length;
                            for (int Idx = 0; Idx < arr.Length; Idx++)
                            {
                                number = arrm[Idx].Value.Remove(arrm[Idx].Value.Length - 1);
                                number = number.Substring(1);
                                link = arr2[Idx].Value;
                                string liniam = (Idx + 1) + ": " + "zmiana z " + number + " na " + link;
                                Debug.WriteLine(liniam);
                                label12.Text = label12.Text + ": " + (Idx + 1) + " / " + arr.Length;
                                progressBar5.Value = Idx + 1;
                                if (isChosen11 == true)
                                {
                                    makeChange(path, number, link);
                                }
                                else if (isChosen12 == true)
                                {
                                    makeChangeForOneXml(xmlFile, number, link);
                                }
                                else
                                {
                                    MessageBox.Show("Nie wykonano żadnych działań na plikach");
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine(linia1);
                            MessageBox.Show("Nie takie rozmiary");
                        }
                    }

                    if (File.Exists(file2))
                    {
                        File.Delete(file2);
                    }
                    MessageBox.Show("Koniec. Było: " + filesnumber + "plików");
                }
                else
                {
                    MessageBox.Show("Nie wybrano pliku .csv");
                }
            }
        }
        // metoda do podmiany dla folderów
        private void makeChange(string paths, string numbers, string vodlink)
        {
            if (paths == null)
            {
                MessageBox.Show("Brak ścieżki do folderu z plikami");
            }
            else if (numbers == null)
            {
                MessageBox.Show("Brak numeru do podmiany");
            }
            else if (vodlink == null)
            {
                MessageBox.Show("Brak vod linku do podmiany");
            }
            else
            {
                string[] files = Directory.GetFiles(paths);
                String lineToCheck = numbers;
                String lineToReplace = vodlink;

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        XDocument xmlFile = XDocument.Load(files[i]);

                        var events = xmlFile.Root.Descendants("event").ToList();
                        foreach (XElement eventt in events)
                        {
                            var items = eventt.Element("extended-event-descriptor");
                            var bets = eventt.Descendants("custom-items-descriptor").Descendants("item").ToList();
                            var query = from c in bets.Where(c => (string)c.Attribute("type") == "VoD_Link") select c;

                            foreach (XElement link in query)
                            {
                                if (link.Attribute("value").Value == lineToCheck)
                                {
                                    link.Attribute("value").Value = lineToReplace;
                                    Debug.WriteLine(lineToCheck + " zmiana na " + lineToReplace);
                                    //<items item-text="HBO On Demand. Wciśnij ‘OK’ na pilocie i wybierz ‘Oglądaj w VOD’" item-description="Dostępne również w "/>
                                    items.Add(new XElement("items", new XAttribute("item-text", "HBO On Demand. Wciśnij ‘OK’ na pilocie i wybierz ‘Oglądaj w VOD’"), new XAttribute("item-description", "Dostępne również w")));
                                }
                            }
                        }
                        xmlFile.Save(files[i]);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                filesnumber = files.Length.ToString();
            }
        }
        // metoda do podmiany dla jednego pliku
        private void makeChangeForOneXml(string xfile, string numbers, string vodlink)
        {
            if (xfile == null)
            {
                MessageBox.Show("Brak pliku xml");
            }
            else if (numbers == null)
            {
                MessageBox.Show("Brak numeru do podmiany");
            }
            else if (vodlink == null)
            {
                MessageBox.Show("Brak vod linku do podmiany");
            }
            else
            {
                String lineToCheck = numbers;
                String lineToReplace = vodlink;
                try
                {
                    XDocument xmlFile = XDocument.Load(xfile);

                    var events = xmlFile.Root.Descendants("event").ToList();
                    foreach (XElement eventt in events)
                    {
                        var items = eventt.Element("extended-event-descriptor");
                        var bets = eventt.Descendants("custom-items-descriptor").Descendants("item").ToList();
                        var query = from c in bets.Where(c => (string)c.Attribute("type") == "VoD_Link") select c;

                        foreach (XElement link in query)
                        {
                            if (link.Attribute("value").Value == lineToCheck)
                            {
                                link.Attribute("value").Value = lineToReplace;
                                Debug.WriteLine(lineToCheck + " zmiana na " + lineToReplace);
                                //<items item-text="HBO On Demand. Wciśnij ‘OK’ na pilocie i wybierz ‘Oglądaj w VOD’" item-description="Dostępne również w "/>
                                items.Add(new XElement("items", new XAttribute("item-text", "HBO On Demand. Wciśnij ‘OK’ na pilocie i wybierz ‘Oglądaj w VOD’"), new XAttribute("item-description", "Dostępne również w")));
                            }
                        }
                    }
                    xmlFile.Save(xfile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
        
        // podmiana image'ów ---------------------------------------------
        public bool isChosen = false;//dla buttona 3 i 6, 4
        // wybierz folder z xml
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                label6.Text = fbd.SelectedPath;
                isChosen = true;
            }
        }
        // podmiana
        private void button6_Click(object sender, EventArgs e)
        {
            if (isChosen == true)
            {
                string[] files = Directory.GetFiles(path);
                for (int k = 0; k < files.Length; k++)
                {
                    try
                    {
                        string ext = Path.GetExtension(files[k]);
                        if (ext == ".xml")
                        {
                            XDocument xdoc = XDocument.Load(files[k]);
                            var bets = xdoc.Root.Descendants("resource-descriptor").ToList();
                            for (int i = 0; i < bets.Count(); i++)
                            {
                                int countbets = bets[i].Descendants("image").Count();
                                Debug.WriteLine(bets[i].Descendants("image").ToString() + " " + countbets);
                                for (int j = countbets; j > 1; j--)
                                {
                                    bets[i].Descendants("image").Last().Remove();
                                }
                            }
                            xdoc.Save(files[k]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                MessageBox.Show("Koniec pracy nad plikami");
                isChosen = false;
            }
            else if (isChosen12 == true)
            {
                try
                {
                    string ext = Path.GetExtension(xmlFile);
                    if (ext == ".xml")
                    {
                        XDocument xdoc = XDocument.Load(xmlFile);
                        var bets = xdoc.Root.Descendants("resource-descriptor").ToList();
                        for (int i = 0; i < bets.Count(); i++)
                        {
                            int countbets = bets[i].Descendants("image").Count();
                            Debug.WriteLine(bets[i].Descendants("image").ToString() + " " + countbets);
                            for (int j = countbets; j > 1; j--)
                            {
                                bets[i].Descendants("image").Last().Remove();
                            }
                        }
                        xdoc.Save(xmlFile);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano folderu lub pliku");
            }
        }
        //do pojedynczego pliku xml
        private void button11_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlFile = ofd.FileName.ToString();
                string ext = Path.GetExtension(xmlFile);
                if (ext == ".xml")
                {
                    label6.Text = ofd.FileName.ToString();
                    isChosen12 = true;
                }
                else
                {
                    MessageBox.Show("To nie jest plik .xml");
                    xmlFile = null;
                }
            }
        }

        // podmiana ciagu znakow ------------------------------------------
        // wybierz folder
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                label5.Text = fbd.SelectedPath;
                isChosen = true;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlFile = ofd.FileName.ToString();
                string ext = Path.GetExtension(xmlFile);
                if (ext == ".xml")
                {
                    label5.Text = ofd.FileName.ToString();
                    isChosen12 = true;
                }
                else
                {
                    MessageBox.Show("To nie jest plik .xml");
                    xmlFile = null;
                }
            }
        }
        // podmien
        private void button7_Click(object sender, EventArgs e)
        {
            if (isChosen == false && isChosen12 == false)
            {
                MessageBox.Show("Nie wybrano folderu z plikami lub samego pliku do podmiany");
            }
            else
            {
                if (textBox1.Text == null)
                {
                    MessageBox.Show("Brak tekstu do znalezienia");
                }
                else
                {
                    if (textBox2.Text == null)
                    {
                        MessageBox.Show("Brak tekstu do podmiany");
                    }
                    else
                    {
                        if (isChosen == true)//dla folderu
                        {
                            string[] files = Directory.GetFiles(path);
                            for (int i = 0; i < files.Length; i++)
                            {
                                try
                                {
                                    XDocument xmlFile = XDocument.Load(files[i]);
                                    string line2 = xmlFile.ToString();
                                    string findString = textBox1.Text;
                                    string replaceString = textBox2.Text;
                                    line2 = Regex.Replace(line2, findString, replaceString);
                                    FileStream fs = new FileStream(files[i], FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                    using (StreamWriter writer = new StreamWriter(fs, Encoding.Default))
                                    {
                                        writer.Write(line2);
                                        writer.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                }
                            }
                            path = null;
                            isChosen = false;
                        }
                        if(isChosen12 == true)// dla jednego pliku
                        {
                            try
                            {
                                XDocument xmlPlik = XDocument.Load(xmlFile);
                                string line2 = xmlPlik.ToString();
                                string findString = textBox1.Text;
                                string replaceString = textBox2.Text;
                                line2 = Regex.Replace(line2, findString, replaceString);
                                FileStream fs = new FileStream(xmlFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                using (StreamWriter writer = new StreamWriter(fs, Encoding.Default))
                                {
                                    writer.Write(line2);
                                    writer.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                            xmlFile = null;
                        }
                    }
                    MessageBox.Show("Zakończono podmianę.");
                }
            }   
        }

        //Short Descriptor Checker ----------------------------------------
        //do wyboru folderu z plikami
        private void button8_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
                label7.Text = fbd.SelectedPath;
                isChosen3 = true;
            }
        }
        //wykonaj sprawdzenie
        private void button9_Click(object sender, EventArgs e)
        {
            if (isChosen3 == true)
            {
                string[] files = Directory.GetFiles(path);
                for (int k = 0; k < files.Length; k++)
                {
                    try
                        {
                            string ext = Path.GetExtension(files[k]);
                            if (ext == ".xml")
                            {
                                XDocument xdoc = XDocument.Load(files[k]);
                                var bets = xdoc.Root.Descendants("short-event-descriptor").ToList();
                                for (int i = 0; i < bets.Count(); i++)
                                {
                                    bool b = bets[i].Descendants("short-description").Any();
                                    if(b == false)
                                    {
                                        bets[i].Add(new XElement("short-description", ""));
                                    }
                                }
                                xdoc.Save(files[k]);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("Brak plików w folderze");
                            isChosen = false;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                }
                
                isChosen = false;
                path = null;
                MessageBox.Show("Koniec pracy nad plikami");
            }
            else if (isChosen12 == true)
            {
                try
                {
                    string ext = Path.GetExtension(xmlFile);
                    if (ext == ".xml")
                    {
                        XDocument xdoc = XDocument.Load(xmlFile);
                        var bets = xdoc.Root.Descendants("short-event-descriptor").ToList();
                        for (int i = 0; i < bets.Count(); i++)
                        {
                            bool b = bets[i].Descendants("short-description").Any();
                            if (b == false)
                            {
                                bets[i].Add(new XElement("short-description", ""));
                            }
                        }
                        xdoc.Save(xmlFile);
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Brak plików w folderze");
                    isChosen = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                isChosen12 = false;
                xmlFile = null;
                MessageBox.Show("Koniec pracy nad plikiem");
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku lub folderu z plikami do sprawdzenia");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlFile = ofd.FileName.ToString();
                string ext = Path.GetExtension(xmlFile);
                if (ext == ".xml")
                {
                    label7.Text = ofd.FileName.ToString();
                    isChosen12 = true;
                }
                else
                {
                    MessageBox.Show("To nie jest plik .xml");
                    xmlFile = null;
                }
            }
        }

        //IMDB get info ---------------------------------------------------
        //IMDB get info - wybranie csv
        private void button14_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                xmlFile = ofd.FileName.ToString();
                string ext = Path.GetExtension(xmlFile);
                if (ext == ".csv")
                {
                    label14.Text = ofd.FileName.ToString();
                    isChosen12 = true;
                }
                else
                {
                    xmlFile = null;
                    MessageBox.Show("To nie jest plik .csv");  
                }
            }
        }
        //IMDB get info - wykonaj
        private void button15_Click(object sender, EventArgs e)
        {//xmlFile
            string file2 = xmlFile + ".bak";
            if (File.Exists(file2))
            {
                File.Delete(file2);
            }
            string fil3 = xmlFile + ".csv";
            if (File.Exists(fil3))
            {
                File.Delete(fil3);
            }
            string line;
            StreamReader reader = new StreamReader(xmlFile, Encoding.Default);
            label13.Text = "Pracuję nad plikami ...";
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    string lastPart = line.Split(';').Last();
                    int filter = 2;

                    if (lastPart.Contains("("))
                    {
                        checkEpisode(lastPart);
                        lastPart = lastPart.Split('(').First();
                        season = 1;
                        isTV = true;
                    }
                    else
                    {
                        isTV = false;
                    }

                    lastPart = checkSeason(lastPart);

                    if (lastPart.Substring(lastPart.Length - 1) == " ")
                    {
                        lastPart = lastPart.TrimEnd(' ');
                    }
                    //-----------------------------------------
                    Debug.WriteLine(no + ": " + lastPart);
                    no++;
                    string lastPartForSave = lastPart;
                    //-----------------------------------------
                    lastPart = lastPart.Replace(" ", "+");

                    if (lastPart.Contains(":"))
                    {
                        lastPart = lastPart.Replace(":", "%3A");
                    }
                    if (lastPart.Contains(","))
                    {
                        lastPart = lastPart.Replace(",", "%2C");
                    }

                    if (lastPart.Substring(lastPart.Length - 1) == "+")
                    {
                        lastPart = lastPart.TrimEnd('+');
                    }
                    else
                    {
                        lastPart = lastPart.TrimEnd(' ');
                        lastPart = lastPart.TrimEnd('+');
                    }

                    string url = @"http://api.myapifilms.com/imdb/idIMDB?title=" + lastPart + "&token=855ca1b9-9ea2-4b49-a705-968510bd8f03&format=xml&language=en-us&aka=0&business=0&seasons=" + season + "&seasonYear=0&technical=0&filter=" + filter + "&exactFilter=0&limit=1&forceYear=0&trailers=0&movieTrivia=0&awards=0&moviePhotos=0&movieVideos=0&actors=0&biography=0&uniqueName=0&filmography=0&bornAndDead=0&starSign=0&actorActress=0&actorTrivia=0&similarMovies=0&adultSearch=0&goofs=0&keyword=0&quotes=0&fullSize=0&companyCredits=0&filmingLocations=0";

                    GetFullResponse(url, lastPartForSave, seasonNumber);

                    writeBackup(line + ";" + movieIdIMDBstr + ";" + movieRatingstr + ";" + urlIMDBstr + ";" + urlPosterstr, fil3);

                }
                Debug.Write("Koniec !");
                MessageBox.Show("Koniec !");
                label13.Text = " ";
            }
            catch (Exception ex)
            {
                writeBackup(ex.ToString(), xmlFile + "_log.txt");
            }
        }

        public string movieIdIMDBstr, urlIMDBstr, movieRatingstr, urlPosterstr; bool isTV = false;

        public int season = 0; public string seasonNumber = "1", episodeNumber = "";

        private void Button17_Click(object sender, EventArgs e)
        {
            string fileo = file + ".bak";
            if (File.Exists(fileo))
            {
                File.Delete(fileo);
            }
            string filet = file2 + ".bak";
            if (File.Exists(filet))
            {
                File.Delete(filet);
            }
            string line, error;
            StreamReader reader = new StreamReader(file, Encoding.Default);
            StreamReader readerr = new StreamReader(file2, Encoding.Default);
            int number = 0;
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (File.ReadAllLines(file2).Contains(line))
                    {
                        Debug.Write(number + " : Linię " + line + " z pliku " + file + " znaleziono w pliku " + file2 + "\n");
                        number++;
                    }
                    else
                    {
                        error = "Brak linii " + line + " z pliku " + file + " w pliku " + file2 + "\n";
                        Debug.Write(error);
                    }
                }
            }
            catch (Exception ex)
            {
                //writeBackup(ex.ToString(), file + "_log.txt");
                Debug.Write(ex.ToString());
            }
            MessageBox.Show("Koniec !");
        }

        //csv2
        private void Button16_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file2 = ofd.FileName.ToString();
                string ext = Path.GetExtension(file2);
                if (ext == ".csv")
                {
                    label15.Text = ofd.FileName.ToString();
                    isChosen4 = true;
                }
                else
                {
                    file = null;
                    MessageBox.Show("To nie jest plik .csv");
                }
            }
        }

        public int no = 1;

        public void writeBackup(string line, string fileToSave)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n" + line);
            File.AppendAllText(fileToSave, sb.ToString());
            sb.Clear();
        }

        public void checkEpisode(string epi)
        {
            string line = epi;
            int count = 0;
            string temp = "";
            string str = "";
            foreach (char ch in line)
            {
                if (ch == '(')
                {
                    count++;
                }
                else if (ch == ')')
                {
                    str += temp;
                    count = 0;
                }
                else if (count >= 1)
                {
                    count++;
                    temp += ch.ToString();
                }
                else
                {
                    count = 0;
                }
            }
            episodeNumber = str;
        }

        public string checkSeason(string sea)
        { 
            if (sea.Contains("III."))
            {
                String matchpattern = @"III[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "3";
                season = 1;
                isTV = true;
            }else if (sea.Contains("II."))
            {
                String matchpattern = @"II[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "2";
                season = 1;
                isTV = true;
            }
            else if (sea.Contains("IV."))
            {
                String matchpattern = @"IV[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "4";
                season = 1;
                isTV = true;
            }
            else if (sea.Contains("V."))
            {
                String matchpattern = @"V[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "5";
                season = 1;
                isTV = true;
            }
            else if (sea.Contains("VI."))
            {
                String matchpattern = @"VI[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "6";
                season = 1;
                isTV = true;
            }
            else if (sea.Contains("VII."))
            {
                String matchpattern = @"VII[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "7";
                season = 1;
                isTV = true;
            }
            else if (sea.Contains("VIII."))
            {
                String matchpattern = @"VIII[.].+";
                String replacementpattern = @" ";
                sea = Regex.Replace(sea, matchpattern, replacementpattern);
                seasonNumber = "8";
                season = 1;
                isTV = true;
            }
            return sea;
        }

        public void GetFullResponse(string url, string title1, string seasonNumber1)
        {
            string file2 = xmlFile + ".bak";
            string fil3 = xmlFile + ".csv";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(url);
                XmlNodeList listOfMovies = doc.SelectNodes("/results/data/movie");
                string numSeason;
                foreach (XmlNode Movie in listOfMovies)
                {
                    movieIdIMDBstr = Movie.SelectSingleNode("idIMDB").InnerText;
                    movieRatingstr = Movie.SelectSingleNode("rating").InnerText;
                    urlIMDBstr = Movie.SelectSingleNode("urlIMDB").InnerText;
                    urlPosterstr = Movie.SelectSingleNode("urlPoster").InnerText;

                    if (isTV == true)
                    {
                        XmlNodeList listOfSeasons = doc.SelectNodes("/results/data/movie/seasons");
                        foreach (XmlNode Seasons in listOfSeasons)
                        {
                            numSeason = Seasons.SelectSingleNode("numSeason").InnerText;
                            if (numSeason.Equals(seasonNumber1))
                            {
                                XmlNodeList listOfEpisodes = Seasons.SelectNodes("episodes");
                                foreach (XmlNode Episodes in listOfEpisodes)
                                {
                                    string numEpisode = Episodes.SelectSingleNode("episode").InnerText;
                                    if (numEpisode.Equals(episodeNumber))
                                    {
                                        string idIMDBEpisode = Episodes.SelectSingleNode("idIMDB").InnerText;
                                        string urlPosterEpisode = Episodes.SelectSingleNode("urlPoster").InnerText;
                                        urlIMDBstr = @"http://www.imdb.com/title/" + idIMDBEpisode.ToString();
                                        urlPosterstr = urlPosterEpisode.ToString();
                                        writeBackup(title1 + " sezon: " + numSeason.ToString() + " odcinek: " + episodeNumber + " " + movieRatingstr + "\n" + urlIMDBstr + "\n" + urlPosterEpisode, file2);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        writeBackup(title1 + " " + movieIdIMDBstr + " " + movieRatingstr + "\n" + urlIMDBstr + "\n" + urlPosterstr, file2);
                    }
                    seasonNumber = "1";
                    episodeNumber = "";
                    writeBackup("//---------------------------------------------------------------------------------", file2);
                }
            }
            catch (Exception ex)
            {
                writeBackup(title1 + " " + ex, file2);
                writeBackup("//---------------------------------------------------------------------------------", file2);
            }
        }
    }
}