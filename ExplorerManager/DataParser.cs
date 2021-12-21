using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Fluorine;
using System.Net;
using Fluorine.AMF3;
using flex.messaging.messages;
using System.Threading;
using defaultGame.Communication.VO;
using System.Windows;

namespace ExplorerManager
{
    class DataParser
    {
        public int user;
        public string token;
        public string flex;
        private string dsId = string.Empty;
        public string validUrl;
        public delegate void OnLog(string message);
        public event OnLog OnLogHandler;
        public int playerLevel;
        public List<Explorer> validExpls = new List<Explorer>();
        public DataParser(int user, string token, string flex)
        {
            this.user = user;
            this.token = token;
            this.flex = flex;
            this.dsId = Guid.NewGuid().ToString("D").ToLower();
            Log.Write("Запускаем поиск сервера");
        }

        private DSKMessage sendRequest(byte[] StreamData, string Url, bool deser = true)
        {
            Log.Write("Запрос на - " + Url);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-amf";

                request.Timeout = 10000;
                CookieContainer Cookies = new CookieContainer();
                Cookies.Add(new Uri(Url), new Cookie("dsoAuthUser", this.user.ToString()));
                Cookies.Add(new Uri(Url), new Cookie("dsoAuthToken", this.token));
                request.CookieContainer = Cookies;
                request.ContentLength = StreamData.Length;
                request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(StreamData, 0, StreamData.Length);
                dataStream.Close();
                Log.Write("Запрос отправлен");
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                if (deser)
                {
                    AMFDes Deser = new AMFDes(dataStream);
                    Log.Write("Парсим ответ");
                    AMFMessage DAmfMessage = Deser.ReadAMFMessage();
                    dataStream.Close();
                    response.Close();
                    DSKMessage content = DAmfMessage.GetBodyAt(0).Content as DSKMessage;
                    return content;
                }
                else { return null; }
            } catch(Exception e)
            {
                File.WriteAllText("error.txt", e.Message + "\r\n" + e.StackTrace);
                MessageBox.Show("Критическая ошибка при отправке запроса! Лог ошибки в файле error.txt");
                Environment.Exit(1);
            }
            return null;
        }

        public bool getHeaders()
        {

            AMFMessage HeaderMessage = BuildMessage("ExecuteServerCall", "SMC", "com.bluebyte.game.servlet.EventHandler", getCall(1037, null));
            byte[] StreamData;
            using (MemoryStream Stream = new MemoryStream())
            {
                AMFSerializer Serializer = new AMFSerializer(Stream);
                Serializer.WriteMessage(HeaderMessage);
                StreamData = Stream.ToArray();
            }
            Log.Write("Запрос зоны");
            OnLogHandler("В клиенте ничего не нажимайте");
            OnLogHandler("Запрос зоны");
            DSKMessage content = sendRequest(StreamData, this.flex);
            dZoneVO mailheaders = content.body.data.data as dZoneVO;
            if (mailheaders != null)
            {
                // got server!
                Log.Write("Зона получена");
                OnLogHandler("Зона получена");
                validUrl = this.flex;
                return handleHeaders(mailheaders);
            }
            OnLogHandler("Не смогли получить данные.. устаревшие куки?");
            return false;
        }

        public void sendSpec(object Data)
        {
            List<ResultRow> ResData = Data as List<ResultRow>;
            int count = ResData.Count;
            int i = 0;
            foreach (ResultRow row in ResData)
            {
                i++;
                OnLogHandler(i + "/" + count + " Отправляем " + row.name + " " + (row.specType == 0 ? "в" : "искать") + " " + row.taskName);
                dServerAction action = new dServerAction()
                {
                    grid = 0,
                    type = row.type,
                    endGrid = 0,
                    data = new dStartSpecialistTaskVO()
                    {
                        subTaskID = row.task,
                        paramString = string.Empty,
                        uniqueID = new defaultGame.Communication.VO.dUniqueID() { uniqueID2 = row.id2, uniqueID1 = row.id }
                    }
                };
                AMFMessage HeaderMessage = BuildMessage("ExecuteServerCall", "SMC", "com.bluebyte.game.servlet.EventHandler", getCall(95, action));
                byte[] StreamData;
                using (MemoryStream Stream = new MemoryStream())
                {
                    AMFSerializer Serializer = new AMFSerializer(Stream);
                    Serializer.WriteMessage(HeaderMessage);
                    StreamData = Stream.ToArray();
                }
                DSKMessage content = sendRequest(StreamData, validUrl);
                
            }
            OnLogHandler("Шалость удалась, в клиенте можно нажать ОК :)");
            return;
        }

        private bool handleHeaders(dZoneVO headers)
        {
            Log.Write("Обрабатываем список специалистов");
            
            playerLevel = (headers.playersOnMap[0] as dPlayerVO).playerLevel;
            OnLogHandler("Уровень игрока - " + playerLevel.ToString());
            
            foreach (dSpecialistVO spec in headers.specialists_vector)
            {
                bool art = false, bea = false;
                foreach (SkillVO skill in spec.skills)
                {
                    if (skill.id == 39)
                        art = true;
                    if (skill.id == 40)
                        bea = true;
                }
                int specType = Loca.explorers.ContainsKey(spec.specialistType) ? 0 : Loca.geologists.ContainsKey(spec.specialistType) ? 1 : 0;
                if ((Loca.explorers.ContainsKey(spec.specialistType) || Loca.geologists.ContainsKey(spec.specialistType)) && spec.task == null)
                {
                    string name = Loca.explorers.ContainsKey(spec.specialistType) ? Loca.explorers[spec.specialistType] : Loca.geologists.ContainsKey(spec.specialistType) ? Loca.geologists[spec.specialistType] : "Unknown";
                    validExpls.Add(
                        new Explorer()
                        {
                            id = spec.uniqueID.uniqueID1,
                            id2 = spec.uniqueID.uniqueID2,
                            name = (!string.IsNullOrEmpty(spec.name_string) ? spec.name_string : Loca.locaData[Main.lang].ContainsKey(name) ? Loca.locaData[Main.lang][name] : name),
                            type = spec.specialistType,
                            specType = specType,
                            artefact = art,
                            beans = bea
                        }
                    );
                }
            }
            Log.Write("Нашли валидных специалистов - " + validExpls.Count);
            OnLogHandler("Специалистов - " + validExpls.Count);
            if (validExpls.Count > 0)
            {
                return true;
            }
            else
            {
                OnLogHandler("Нечего обрабатывать");
                OnLogHandler("Программа будет закрыта через 5 секунд.");
                Thread.Sleep(5000);
                Environment.Exit(1);
                return false;
            }
        }

        private AMFMessage BuildMessage(string operation, string destination, string source, dServerCall Call)
        {
            RemotingMessage Command = new RemotingMessage();
            Command.operation = operation;
            Command.destination = destination;
            Command.source = source;
            Command.SetHeader("DSEndpoint", "SMC-Endpoint");
            Command.SetHeader("DSId", this.dsId);
            Command.messageId = Guid.NewGuid().ToString("D");
            Command.clientId = Guid.NewGuid().ToString("D");
            Command.body = new object[] { Call };
            ResponseBody Body = new ResponseBody(new AMFBody(), new object[] { Command }) { Target = null };
            AMFMessage AmfMessage = new AMFMessage(3);
            AmfMessage.AddBody(Body);
            return AmfMessage;
        }

        private dServerCall getCall(int type, object data)
        {
            dServerCall Call = new dServerCall(user, token);
            Call.data = data;
            Call.type = type;
            return Call;
        }

        public string getResult(byte[] data)
        {
            List<string> res = new List<string>();
            foreach (byte b in data.Take(256))
                res.Add(b.ToString());
            return string.Join(".", res.ToArray());
        }
    }
    public class AMFDes : MyAmfReader
    {
        public AMFDes(Stream stream)
            : base(stream)
        {
        }

        public AMFMessage ReadAMFMessage()
        {
            return this.ReadAMFMessage((IApplicationContext)null);
        }

        public AMFMessage ReadAMFMessage(IApplicationContext applicationContext)
        {
            AMFMessage amfMessage = new AMFMessage(this.ReadUInt16());
            int num1 = (int)this.ReadUInt16();
            for (int index = 0; index < num1; ++index)
                amfMessage.AddHeader(this.ReadHeader(applicationContext));
            int num2 = (int)this.ReadUInt16();
            for (int index = 0; index < num2; ++index)
            {
                AMFBody body = this.ReadBody(applicationContext);
                if (body != null)
                    amfMessage.AddBody(body);
            }
            return amfMessage;
        }

        private AMFHeader ReadHeader(IApplicationContext applicationContext)
        {
            string name = this.ReadString();
            bool mustUnderstand = this.ReadBoolean();
            this.ReadInt32();
            object content = this.ReadData(applicationContext);
            return new AMFHeader(name, mustUnderstand, content);
        }

        private AMFBody ReadBody(IApplicationContext applicationContext)
        {
            this.Reset();
            string target = this.ReadString();
            string response = this.ReadString();
            int num = this.ReadInt32();
            if (this.BaseStream.CanSeek)
            {
                long position = this.BaseStream.Position;
                try
                {
                    object content = this.MyReadData(applicationContext);
                    return new AMFBody(target, response, content);
                }
                catch (Exception ex)
                {
                    this.BaseStream.Position = position + (long)num;
                    if (applicationContext != null)
                        applicationContext.Fail(new AMFBody(target, response, (object)null), ex);
                    return (AMFBody)null;
                }
            }
            else
            {
                object content = this.MyReadData(applicationContext);
                return new AMFBody(target, response, content);
            }
        }
    }
    public class Explorer
    {
        public int id { get; set; }
        public int id2 { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public bool beans { get; set; }
        public bool artefact { get; set; }
        public int specType { get; set; }
    }
}

namespace flex.messaging.messages
{
    class RemotingMessage : Fluorine.Messaging.Messages.RemotingMessage
    {

    }
}