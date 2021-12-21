using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace ExplorerManager
{
    class LocaParser
    {
        private string xmls_path = "e:\\MY\\Projects\\DSO.AutoParser\\DSO.AutoParser\\bin\\Debug\\live_24.07.2021\\";
        private Dictionary<string, Dictionary<string, string>> locaData = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<int, string> speExplorerData = new Dictionary<int, string>();
        private Dictionary<int, string> speGeologistData = new Dictionary<int, string>();

        public LocaParser()
        {
            Regex reg = new Regex(@"\\[a-z-]+\.xml");
            foreach (string file in Directory.GetFiles(xmls_path, "loca\\*.xml").Where(s => reg.IsMatch(s)))
            {
                string fileName = Path.GetFileName(file);
                string lang = fileName.Split(new[] { '.' }).First();
                locaData.Add(lang, parseLocaFile(fileName));
            }
            parseSettings();
            replaceLoca();
        }

        private void parseSettings()
        {
            XmlDocument XmlDocument = new XmlDocument();
            XmlDocument.Load(string.Format("{0}{1}", xmls_path, "game_settings.xml"));
            XmlNodeList t_list = XmlDocument.DocumentElement.SelectNodes("/Root/Specialists/Specialist");
            foreach (XmlNode t_item in t_list)
            {
                if (t_item.Attributes["baseType"].Value == "1")
                    speExplorerData.Add(int.Parse(t_item.Attributes["type"].Value), t_item.Attributes["name"].Value);
                if (t_item.Attributes["baseType"].Value == "2")
                    speGeologistData.Add(int.Parse(t_item.Attributes["type"].Value), t_item.Attributes["name"].Value);
            }
        }
        
        private void replaceLoca()
        {
            string fileContent = File.ReadAllText("../../Loca.cs");
            Match match = Regex.Match(fileContent, "(.*)//begin",  RegexOptions.Singleline);
            string begin = match.Value;
            match = Regex.Match(fileContent, "//end(.*)", RegexOptions.Singleline);
            string end = match.Value;
            string data = "        public static Dictionary<int, string> explorers = new Dictionary<int, string>()\r\n        {\r\n";
            foreach (KeyValuePair<int, string> item in speExplorerData)
                data += string.Format("            {{ {0}, \"{1}\" }},\r\n", item.Key.ToString(), item.Value);
            data += "        };\r\n        public static Dictionary<int, string> geologists = new Dictionary<int, string>()\r\n        {\r\n";
            foreach (KeyValuePair<int, string> item in speGeologistData)
                data += string.Format("            {{ {0}, \"{1}\" }},\r\n", item.Key.ToString(), item.Value);
            data += "        };\r\n        #region loca\r\n        public static Dictionary<string, Dictionary<string, string>> locaData = new Dictionary<string, Dictionary<string, string>>()\r\n        {\r\n";
            foreach ( KeyValuePair<string, Dictionary<string,string>> item in locaData)
            {
                data += "            { \"" + item.Key + "\", new Dictionary<string, string>(){\r\n";
                foreach(KeyValuePair<string,string> locItem in item.Value)
                {
                    data += string.Format("                {{ \"{0}\", \"{1}\" }},\r\n", locItem.Key, locItem.Value);
                }
                data += "              }\r\n            },\r\n";
            }
            File.WriteAllText("../../Loca.cs", string.Format("{0}\r\n{1}            {2}", begin, data, end));
        }

        private Dictionary<string, string> parseLocaFile(string filename)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            XmlDocument XmlDocument = new XmlDocument();
            XmlDocument.Load(string.Format("{0}loca\\{1}", xmls_path, filename));
            XmlNodeList t_list = XmlDocument.DocumentElement.SelectNodes("/oasis/translations/s[@name='SPE']/t");
            foreach (XmlNode t_item in t_list)
                result.Add(t_item.Attributes["id"].Value, t_item.Attributes["text"].Value);
            t_list = XmlDocument.DocumentElement.SelectNodes("/oasis/translations/s[@name='TOT']/t");
            foreach (XmlNode t_item in t_list)
                if(t_item.Attributes["id"].Value.StartsWith("Find"))
                    result.Add(t_item.Attributes["id"].Value, t_item.Attributes["text"].Value);
            XmlNode item = XmlDocument.DocumentElement.SelectSingleNode("/oasis/translations/s[@name='LAB']/t[@id='Cancel']");
            result.Add(item.Attributes["id"].Value, item.Attributes["text"].Value);
            item = XmlDocument.DocumentElement.SelectSingleNode("/oasis/translations/s[@name='LAB']/t[@id='UnitSkillIsSpecialist']");
            result.Add(item.Attributes["id"].Value, item.Attributes["text"].Value);
            item = XmlDocument.DocumentElement.SelectSingleNode("/oasis/translations/s[@name='LAB']/t[@id='Send']");
            result.Add(item.Attributes["id"].Value, item.Attributes["text"].Value);
            item = XmlDocument.DocumentElement.SelectSingleNode("/oasis/translations/s[@name='LAB']/t[@id='Commands']");
            result.Add(item.Attributes["id"].Value, item.Attributes["text"].Value);
            return result;
        }
        
    }
}
