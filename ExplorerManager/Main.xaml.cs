using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Reflection;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Web;

namespace ExplorerManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Main : Window
    {
        public static string server = string.Empty;
        public static Cookie session = new Cookie();
        public static string filename = "explorers.xml";
        public static bool autostart = false;
        public static string lang = "ru-ru";
        public static bool genloca = false;
        private XmlSerializer _serializer = new XmlSerializer(typeof(SavedItem[]), new XmlRootAttribute("Dictionary"));
        public Main()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Log.Write("Старт");
            if (App.cmd["config"] != null)
                filename = App.cmd["config"].Trim();
            if (App.cmd["autostart"] != null)
                autostart = true;
            if (genloca)
            {
                new LocaParser();
                Application.Current.Shutdown();
            }
            if (App.cmd["lang"] != null)
            {
                if (Loca.locaData.ContainsKey(App.cmd["lang"]))
                    lang = App.cmd["lang"];
            }
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionDumper.DumpException(e.ExceptionObject as Exception);
        }

        Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Fluorine"))
            {
                WriteLog("Загружаем библиотеку Fluorine..");
                return LoadFluorine();
            }
            return null;
        }

        public static Assembly LoadFluorine()
        {
            byte[] ba = null;

            string resource = "ExplorerManager.Fluorine.dll";
            Assembly curAsm = Assembly.GetExecutingAssembly();
            using (Stream stm = curAsm.GetManifestResourceStream(resource))
            {
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
            }
            bool fileOk = false;
            string tempFile = "";

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                string fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty);
                tempFile = System.IO.Path.GetTempPath() + "Fluorine.dll";
                if (File.Exists(tempFile))
                {
                    byte[] bb = File.ReadAllBytes(tempFile);
                    string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);
                    fileOk = (fileHash == fileHash2) ? true : false;
                }
            }
            if (!fileOk)
                System.IO.File.WriteAllBytes(tempFile, ba);
            return Assembly.LoadFile(tempFile);
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as RichTextBox).ScrollToEnd();
        }

        public void WriteLog(string Message)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                richTextBox1.AppendText(string.Format("{0}\r", Message));
            }));
        }
        public static double GetFlashTime()
        {
            return Math.Round((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds, 0);
        }
        public string getFlexByZoneId(string _authUser, string _authToken, string _bbServer)
        {
            int _endpointWait = 0;
            PostSubmitter post;
            CookieCollection _cookies = new CookieCollection();
            while (_endpointWait < 4)
            {
                _endpointWait++;
                post = new PostSubmitter
                {
                    Url = _bbServer + "Z" + GetFlashTime(),
                    Type = PostSubmitter.PostTypeEnum.Post
                };
                post.PostItems.Add("DSOAUTHUSER", _authUser);
                post.PostItems.Add("DSOAUTHTOKEN", _authToken);
                post.PostItems.Add("zoneID", _authUser);
                string res = post.Post(ref _cookies);
                if (res.Contains("GameServer"))
                {
                    return res;
                }
                else
                {
                    WriteLog("#" + _endpointWait.ToString() + " Неудачная попытка получить адрес..");
                }
                Thread.Sleep(3000);
                continue;
            }
            return string.Empty;
        }

        private void NewLogic()
        {
            try
            {
                Process[] processlist = Process.GetProcesses();
                bool found = false;
                foreach (Process theprocess in processlist)
                {
                    if (theprocess.MainWindowTitle.Contains("The Settlers Online"))
                    {
                        found = true;
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + theprocess.Id))
                        using (ManagementObjectCollection objects = searcher.Get())
                        {
                            string cmd = objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                            var tsoUrl = HttpUtility.ParseQueryString(cmd);

                            Match match = Regex.Match(cmd, "dsoAuthToken=([a-zA-Z0-9]+)", RegexOptions.IgnoreCase);
                            string _authToken = tsoUrl.Get("dsoAuthToken");
                            string _authUser = tsoUrl.Get("dsoAuthUser");
                            string _bbServer = tsoUrl.Get("bb");
                            WriteLog("Запрос адреса геймсервера");
                            string _flexEndpoint = getFlexByZoneId(_authUser, _authToken, _bbServer);
                            if(string.IsNullOrEmpty(_flexEndpoint))
                            {
                                WriteLog("Не смогли получить адрес :(");
                                return;
                            }
                            startDP(int.Parse(_authUser), _authToken, _flexEndpoint.Replace("https", "http").Replace(":443", ""));
                        }
                    }
                }
                if (!found)
                {
                    WriteLog("Не смогли найти запущенный клиент");
                    return;
                }
            }
            catch (Exception e)
            {
                File.WriteAllText("error.txt", e.Message + "\n" + e.StackTrace);
                MessageBox.Show("Критическая ошибка! Лог ошибки в файле error.txt");
                Environment.Exit(1);
            }
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            new Thread(NewLogic) { IsBackground = true }.Start();
            return;
        }


        [STAThread]
        private void startDP(int user, string token, string flex)
        {
            DataParser dp = new DataParser(user, token, flex);
            dp.OnLogHandler += WriteLog;
            bool status = dp.getHeaders();
            if(!status)
            {
                return;
            }
            Log.Write("Обработка закончена");
            List<SavedItem> savedExplorers = new List<SavedItem>();
            if (File.Exists(filename))
            {
                try
                {
                    savedExplorers = Deserialize(filename);
                } catch (Exception e)
                {
                    WriteLog("Файл сохранения поврежден");
                }
            }
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                Result result = new Result() { Owner = this, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner, playerLevel = dp.playerLevel };
                result.setData(dp.validExpls, savedExplorers);
                result.ShowDialog();
                if(result.DialogResult == true)
                {
                    foreach (SavedItem item in result.SaveResults)
                    {
                        if (savedExplorers.Any(x => x.Key == item.Key)) {
                            savedExplorers.Single(x => x.Key == item.Key).Value = item.Value;
                            savedExplorers.Single(x => x.Key == item.Key).Name = item.Name;
                        }
                        else
                            savedExplorers.Add(new SavedItem() { Key = item.Key, Value = item.Value, Name = item.Name });
                    }
                    Serialize(filename, savedExplorers);
                    new Thread(new ParameterizedThreadStart(dp.sendSpec)) { IsBackground = true }.Start(result.Results);
                }
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Click(null, null);
        }
        public void Serialize(string filename, List<SavedItem> data)
        {
            using (var writer = new StreamWriter(filename))
            {
                _serializer.Serialize(writer, data.ToArray());
            }
        }
        public List<SavedItem> Deserialize(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                return ((SavedItem[])_serializer.Deserialize(reader)).ToList();
            }
        }
    }

    public static class Log
    {
        public static string file = "debug.log";
        public static string datePatt = @"[M/d/yyyy hh:mm:ss]";
        public static bool debug = false;

        public static void Write(string message)
        {
            if (!debug)
                return;
            using(FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write))
            {
                byte[] msg = UTF8Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", getDate(), message));
                fs.Write(msg, 0, msg.Length);
            }
        }

        public static string getDate()
        {
            return DateTime.Now.ToString(datePatt);
        }
    }
    
    public class DictionarySerializer<TKey, TValue>
    {
        [XmlType(TypeName = "Item")]
        public class Item
        {
            [XmlAttribute("key")]
            public TKey Key;
            [XmlAttribute("value")]
            public TValue Value;
        }

        private XmlSerializer _serializer = new XmlSerializer(typeof(Item[]), new XmlRootAttribute("Dictionary"));

        public Dictionary<TKey, TValue> Deserialize(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                return ((Item[])_serializer.Deserialize(reader)).ToDictionary(p => p.Key, p => p.Value);
            }
        }

        public void Serialize(string filename, Dictionary<TKey, TValue> dictionary)
        {
            using (var writer = new StreamWriter(filename))
            {
                _serializer.Serialize(writer, dictionary.Select(p => new Item() { Key = p.Key, Value = p.Value }).ToArray());
            }
        }
    }

    [XmlType(TypeName = "Item")]
    public class SavedItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("value")]
        public int Value { get; set; }

    }
}
