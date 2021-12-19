using Fluorine.Activation;
using Fluorine;
using Fluorine.AMF3;
using Fluorine.Exceptions;
using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Collections.Specialized;
using System.Configuration;

using System.Globalization;
using System.Runtime.InteropServices;

namespace ExplorerManager
{
    public class MyAmfReader : AMFReader
    {
        private bool _useLegacyCollection = true;
        private Hashtable _amf0ObjectReferences;
        private Hashtable _objectReferences;
        private Hashtable _stringReferences;
        private Hashtable _classDefinitions;
        public ClassMappings _map;

        public MyAmfReader(Stream stream)
            : base(stream)
        {
            this.Reset();
        }

        public void Reset()
        {
            this._amf0ObjectReferences = new Hashtable(5);
            this._objectReferences = new Hashtable(15);
            this._stringReferences = new Hashtable(15);
            this._classDefinitions = new Hashtable(2);
            this._map = new ClassMappings();
        }

        public object MyReadData()
        {
            return this.MyReadData((IApplicationContext)null);
        }

        public object MyReadData(IApplicationContext applicationContext)
        {
            return this.MyReadData(this.BaseStream.ReadByte(), applicationContext);
        }
        private IList ReadArray(IApplicationContext applicationContext)
        {
            int capacity = this.ReadInt32();
            ArrayList arrayList = new ArrayList(capacity);
            this._amf0ObjectReferences.Add((object)this._amf0ObjectReferences.Count, (object)arrayList);
            for (int index = 0; index < capacity; ++index)
                arrayList.Add(this.ReadData(applicationContext));
            return (IList)arrayList;
        }

        private DateTime ReadDateValue()
        {
            DateTime dateTime = new DateTime(1970, 1, 1).AddMilliseconds(this.ReadDouble());
            int num = (int)this.ReadUInt16();
            if (num > 720)
                num = 65536 - num;
            int timezone = num / 60;
            return dateTime;
        }
        private Hashtable ReadAssociativeArray(IApplicationContext applicationContext)
        {
            Hashtable hashtable = new Hashtable(this.ReadInt32());
            this._amf0ObjectReferences.Add((object)this._amf0ObjectReferences.Count, (object)hashtable);
            string str = this.ReadString();
            for (int typeCode = this.BaseStream.ReadByte(); typeCode != 9; typeCode = this.BaseStream.ReadByte())
            {
                object obj = this.MyReadData(typeCode, applicationContext);
                hashtable.Add((object)str, obj);
                str = this.ReadString();
            }
            return hashtable;
        }

        internal object MyReadData(int typeCode, IApplicationContext applicationContext)
        {
            object obj = (object)null;
            switch (typeCode)
            {
                case 0:
                    return (object)this.ReadDouble();
                case 1:
                    return (object)(this.ReadBoolean());
                case 2:
                    return (object)this.ReadString();
                case 3:
                    return (object)this.ReadASObject(applicationContext);
                case 4:
                    throw new UnexpectedAMF();
                case 5:
                    return obj;
                case 6:
                    return obj;
                case 7:
                    return this._amf0ObjectReferences[(object)(int)this.ReadUInt16()];
                case 8:
                    return (object)this.ReadAssociativeArray(applicationContext);
                case 9:
                    throw new UnexpectedAMF();
                case 10:
                    return (object)this.ReadArray(applicationContext);
                case 11:
                    return (object)this.ReadDateValue();
                case 12:
                    return (object)this.ReadUTF(this.ReadInt32());
                case 13:
                    throw new UnexpectedAMF();
                case 14:
                    throw new UnexpectedAMF();
                case 15:
                    return (object)this.ReadXmlDocument();
                case 16:
                    return this.ReadObject(applicationContext);
                case 17:
                    object obj1 = this.MyReadAMF3Data(applicationContext);
                    return obj1;
                default:
                    throw new UnexpectedAMF();
            }
        }
        public object MyReadAMF3Data(IApplicationContext applicationContext)
        {
            return this.MyReadAMF3Data(this.ReadByte(), applicationContext);
        }
        internal object MyReadAMF3Data(byte typeCode, IApplicationContext applicationContext)
        {
            switch (typeCode)
            {
                case (byte)0:
                    return (object)null;
                case (byte)1:
                    return (object)null;
                case (byte)2:
                    return (object)false;
                case (byte)3:
                    return (object)true;
                case (byte)4:
                    return (object)this.ReadAMF3Int();
                case (byte)5:
                    return (object)this.ReadDouble();
                case (byte)6:
                    return (object)this.ReadAMF3String();
                case (byte)7:
                case (byte)11:
                    return (object)this.ReadAMF3XmlDocument();
                case (byte)8:
                    return (object)this.ReadAMF3Date();
                case (byte)9:
                    return this.MyReadAMF3Array(applicationContext);
                case (byte)10:
                    object obj = this.MyReadAMF3Object(this.ReadAMF3IntegerData(), applicationContext);
                    return obj;
                case (byte)12:
                    return (object)this.ReadAMF3ByteArray(applicationContext);
                default:
                    throw new UnexpectedAMF();
            }
        }
        public object MyReadAMF3Array(IApplicationContext applicationContext)
        {
            int num = this.ReadAMF3IntegerData();
            bool flag = (num & 1) != 0;
            int capacity = num >> 1;
            if (!flag)
                return this._objectReferences[(object)capacity];
            Hashtable hashtable = (Hashtable)null;
            for (string str = this.ReadAMF3String(); str != string.Empty; str = this.ReadAMF3String())
            {
                if (hashtable == null)
                {
                    hashtable = new Hashtable();
                    this._objectReferences.Add((object)this._objectReferences.Count, (object)hashtable);
                }
                object obj = this.ReadAMF3Data(applicationContext);
                hashtable.Add((object)str, obj);
            }
            if (hashtable == null)
            {
                IList list = this._useLegacyCollection ? (IList)new ArrayList(capacity) : (IList)new object[capacity];
                this._objectReferences.Add((object)this._objectReferences.Count, (object)list);
                for (int index = 0; index < capacity; ++index)
                {
                    object obj = this.MyReadAMF3Data(this.ReadByte(), applicationContext);
                    if (list is ArrayList)
                        list.Add(obj);
                    else
                        list[index] = obj;
                }
                return (object)list;
            }
            for (int index = 0; index < capacity; ++index)
            {
                object obj = this.ReadAMF3Data(applicationContext);
                hashtable.Add((object)index.ToString(), obj);
            }
            return (object)hashtable;
        }
        public object MyReadAMF3Object(int handle, IApplicationContext applicationContext)
        {
            bool flag1 = (handle & 1) != 0;
            handle >>= 1;
            if (!flag1)
                return this._objectReferences[(object)handle];
            bool flag2 = (handle & 1) != 0;
            handle >>= 1;
            ClassDefinition classDefinition;
            if (flag2)
            {
                string str = this.ReadAMF3String();
                bool flag3 = str != null && str != string.Empty;
                bool externalizable = (handle & 1) != 0;
                handle >>= 1;
                bool dynamic = (handle & 1) != 0;
                handle >>= 1;
                int memberCount = handle;
                ClassMemberDefinition[] classMemberDefinitions = new ClassMemberDefinition[memberCount];
                for (int index = 0; index < memberCount; ++index)
                {
                    string classMember = this.ReadAMF3String();
                    classMemberDefinitions[index] = new ClassMemberDefinition(classMember);
                }
                string mappedClassName = str;
                //if (applicationContext != null)
                //    mappedClassName = applicationContext.GetMappedTypeName(str);
                //
                mappedClassName = this._map.GetType(str);
                classDefinition = new ClassDefinition(str, mappedClassName, memberCount, classMemberDefinitions, externalizable, dynamic);
                this._classDefinitions.Add((object)this._classDefinitions.Count, (object)classDefinition);
            }
            else
                classDefinition = this._classDefinitions[(object)handle] as ClassDefinition;
            Type type;
            object obj1;
            if (classDefinition.IsTypedObject)
            {
                switch (classDefinition.ClassName)
                {
                    case "flex.messaging.messages.AcknowledgeMessage":
                        type = typeof(DSKMessage);
                        obj1 = (object)new DSKMessage();
                        break;
                    case "defaultGame.Communication.VO.dServerResponse":
                        type = typeof(dServerResponse);
                        obj1 = (object)new dServerResponse();
                        break;
                    case "defaultGame.Communication.VO.dServerActionResult":
                        type = typeof(dServerActionResult);
                        obj1 = (object)new dServerActionResult();
                        break;
                    case "defaultGame.Communication.VO.Mail.dMailHeaderResponseVO":
                        type = typeof(dMailHeaderResponseVO);
                        obj1 = (object)new dMailHeaderResponseVO();
                        break;
                    case "defaultGame.Communication.VO.Mail.dMailVO":
                        type = typeof(dMailVO);
                        obj1 = (object)new dMailVO();
                        break;
                    case "defaultGame.Communication.VO.UpdateVO.dLootItemsVO":
                        type = typeof(dLootItemsVO);
                        obj1 = (object)new dLootItemsVO();
                        break;
                    case "defaultGame.Communication.VO.dBuffVO":
                        type = typeof(dBuffVO);
                        obj1 = (object)new dBuffVO();
                        break;
                    case "defaultGame.Communication.VO.dUniqueID":
                        type = typeof(dUniqueID);
                        obj1 = (object)new dUniqueID();
                        break;
                    case "defaultGame.Communication.VO.dZoneVO":
                        type = typeof(dZoneVO);
                        obj1 = (object)new dZoneVO();
                        break;
                    case "defaultGame.Communication.VO.dPlayerVO":
                        type = typeof(dPlayerVO);
                        obj1 = (object)new dPlayerVO();
                        break;
                    case "defaultGame.Communication.VO.dSpecialistVO":
                        type = typeof(dSpecialistVO);
                        obj1 = (object)new dSpecialistVO();
                        break;
                    case "defaultGame.Communication.VO.Skill.SkillVO":
                        type = typeof(SkillVO);
                        obj1 = (object)new SkillVO();
                        break;
                    case "defaultGame.Communication.VO.dSpecialistTaskVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_FindTreasureVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_AttackBuildingVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_ExploreSectorVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_FindDepositVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_FindEventZoneVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_MoveVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_RecoverVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_TravelToZoneVO":
                    case "defaultGame.Communication.VO.dSpecialistTask_WaitForConfirmationVO":
                        type = typeof(dSpecialistTaskVO);
                        obj1 = (object)new dSpecialistTaskVO();
                        break;
                    case "flex.messaging.io.ArrayCollection":
                        type = typeof(ArrayCollection);
                        obj1 = (object)new ArrayCollection();
                        break;
                    default:
                        type = typeof(ASObject);
                        obj1 = (object)new ASObject();
                        break;

                }
            }
            else
            {
                type = typeof(ASObject);
                obj1 = (object)new ASObject();
            }
            this._objectReferences.Add((object)this._objectReferences.Count, obj1);
            if (classDefinition.IsExternalizable)
            {

                if (!(obj1 is IExternalizable))
                {
                    object instance = null;
                    if (classDefinition.ClassName != null && classDefinition.ClassName != string.Empty)
                        instance = ObjectFactory.CreateInstance(applicationContext, classDefinition.ClassName);
                    else
                        instance = new ASObject();
                    if (instance == null)
                    {
                        instance = new ASObject(classDefinition.ClassName);
                    }
                    this._objectReferences.Add((object)this._objectReferences.Count, instance);
                    string msg = __Res.GetString(__Res.Externalizable_CastFail, instance.GetType().FullName);
                    //throw new FluorineException("Object " + classDefinition.ClassName + " does not implement IExternalizable.");
                }
                (obj1 as IExternalizable).ReadExternal((IDataInput)new DataInput(applicationContext, this));
            }
            else
            {
                for (int index = 0; index < classDefinition.MemberCount; ++index)
                {
                    string classMember = classDefinition.ClassMemberDefinitions[index].ClassMember;
                    object obj2 = this.MyReadAMF3Data(applicationContext);
                    PropertyInfo property = type.GetProperty(classMember);
                    if (property != null)
                    {
                        try
                        {
                            object obj3 = TypeHelper.ChangeType(applicationContext, obj2, property.PropertyType);
                            property.SetValue(obj1, obj3, (object[])null);
                        }
                        catch (Exception ex)
                        {
                            throw new FluorineException("Custom object " + classDefinition.ClassName + " setting property " + classMember + " failed. " + ex.Message, ex);
                        }
                    }
                    else
                    {
                        FieldInfo field = type.GetField(classMember, BindingFlags.Instance | BindingFlags.Public);
                        try
                        {
                            if (field != null)
                            {
                                object obj3 = TypeHelper.ChangeType(applicationContext, obj2, field.FieldType);
                                field.SetValue(obj1, obj3);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new FluorineException("Custom object " + classDefinition.ClassName + " setting field " + classMember + " failed. " + ex.Message, ex);
                        }
                    }
                }
                if (classDefinition.IsDynamic && obj1 is ASObject)
                {
                    ASObject asObject = obj1 as ASObject;
                    for (string str = this.ReadAMF3String(); str != string.Empty; str = this.ReadAMF3String())
                    {
                        object obj2 = this.MyReadAMF3Data(applicationContext);
                        asObject.Add((object)str, obj2);
                    }
                }
            }
            return obj1;
        }
    }
    internal class ClassDefinition
    {
        private string _className;
        private string _mappedClassName;
        private int _memberCount;
        private ClassMemberDefinition[] _classMemberDefinitions;
        private bool _externalizable;
        private bool _dynamic;

        public string ClassName
        {
            get
            {
                return this._className;
            }
        }

        public int MemberCount
        {
            get
            {
                return this._memberCount;
            }
        }

        public ClassMemberDefinition[] ClassMemberDefinitions
        {
            get
            {
                return this._classMemberDefinitions;
            }
        }

        public bool IsExternalizable
        {
            get
            {
                return this._externalizable;
            }
        }

        public bool IsDynamic
        {
            get
            {
                return this._dynamic;
            }
        }

        public bool IsTypedObject
        {
            get
            {
                return this._className != null && this._className != string.Empty;
            }
        }

        public ClassDefinition(string className, string mappedClassName, int memberCount, ClassMemberDefinition[] classMemberDefinitions, bool externalizable, bool dynamic)
        {
            this._className = className;
            this._memberCount = memberCount;
            this._classMemberDefinitions = classMemberDefinitions;
            this._externalizable = externalizable;
            this._mappedClassName = mappedClassName;
            this._dynamic = dynamic;
        }

        public object GetValue(ClassMemberDefinition classMemberDefinition, object obj)
        {
            return this.GetValue(classMemberDefinition.ClassMember, obj);
        }

        public object GetValue(string member, object obj)
        {
            if (obj is IDictionary)
            {
                IDictionary dictionary = obj as IDictionary;
                if (dictionary.Contains((object)member))
                    return dictionary[(object)member];
            }
            PropertyInfo property = obj.GetType().GetProperty(member);
            if (property != null)
                return property.GetValue(obj, (object[])null);
            FieldInfo field = obj.GetType().GetField(member);
            if (field != null)
                return field.GetValue(obj);
            throw new FluorineException("Class member " + member + " not found.");
        }
    }

    internal class ClassMemberDefinition
    {
        private string _classMember;

        public string ClassMember
        {
            get
            {
                return this._classMember;
            }
        }

        public ClassMemberDefinition(string classMember)
        {
            this._classMember = classMember;
        }
    }
    internal class DataInput : IDataInput
    {
        private MyAmfReader _amfReader;
        private IApplicationContext _applicationContext;

        public DataInput(IApplicationContext applicationContext, MyAmfReader amfReader)
        {
            this._amfReader = amfReader;
            this._applicationContext = applicationContext;
        }

        public bool ReadBoolean()
        {
            return this._amfReader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return this._amfReader.ReadByte();
        }

        public void ReadBytes(byte[] bytes, uint offset, uint length)
        {
            byte[] numArray = this._amfReader.ReadBytes((int)length);
            for (int index = 0; index < numArray.Length; ++index)
                bytes[(long)index + (long)offset] = numArray[index];
        }

        public double ReadDouble()
        {
            return this._amfReader.ReadDouble();
        }

        public float ReadFloat()
        {
            return this._amfReader.ReadFloat();
        }

        public int ReadInt()
        {
            return this._amfReader.ReadInt32();
        }

        public object ReadObject()
        {
            return this._amfReader.MyReadAMF3Data(this._applicationContext);
        }

        public short ReadShort()
        {
            return this._amfReader.ReadInt16();
        }

        public byte ReadUnsignedByte()
        {
            return this._amfReader.ReadByte();
        }

        public uint ReadUnsignedInt()
        {
            return (uint)this._amfReader.ReadInt32();
        }

        public ushort ReadUnsignedShort()
        {
            return this._amfReader.ReadUInt16();
        }

        public string ReadUTF()
        {
            return this._amfReader.ReadString();
        }

        public string ReadUTFBytes(uint length)
        {
            return this._amfReader.ReadUTF((int)length);
        }
    }
    internal class ObjectFactory
    {
        private static Hashtable _typeCache = new Hashtable();
        private static Hashtable _activationModeCache = new Hashtable();
        private static Hashtable _activatorsCache = new Hashtable();

        static ObjectFactory()
        {
            ObjectFactory._activatorsCache.Add((object)"application", (object)new ApplicationActivator());
            ObjectFactory._activatorsCache.Add((object)"request", (object)new RequestActivator());
            ObjectFactory._activatorsCache.Add((object)"session", (object)new SessionActivator());
            try
            {
                NameValueCollection nameValueCollection = (NameValueCollection)ConfigurationSettings.GetConfig("fluorine/activators");
                if (nameValueCollection == null)
                    return;
                foreach (string index in (NameObjectCollectionBase)nameValueCollection)
                {
                    Type type = ObjectFactory.Locate((IApplicationContext)null, nameValueCollection[index]);
                    if (type != null && !ObjectFactory._activatorsCache.Contains((object)index))
                        ObjectFactory._activatorsCache[(object)index] = Activator.CreateInstance(type);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void AddActivator(IApplicationContext applicationContext, string activationMode, string typeName)
        {
            Type type = ObjectFactory.Locate(applicationContext, typeName);
            if (type != null)
            {
                ObjectFactory._activationModeCache[(object)type] = (object)activationMode;
            }
        }

        public static Type Locate(IApplicationContext applicationContext, string typeName)
        {
            if (typeName == null || typeName == string.Empty)
                return (Type)null;
            string typeName1 = typeName;
            if (applicationContext != null)
                typeName1 = applicationContext.GetMappedTypeName(typeName);
            lock (typeof(Type))
            {
                Type local_1 = ObjectFactory._typeCache[(object)typeName1] as Type;
                if (local_1 == null)
                {
                    local_1 = TypeHelper.Locate(typeName1);
                    if (local_1 != null)
                    {
                        ObjectFactory._typeCache[(object)typeName1] = (object)local_1;
                        return local_1;
                    }
                    if (applicationContext != null)
                        local_1 = ObjectFactory.LocateInLac(applicationContext, typeName1);
                }
                return local_1;
            }
        }

        public static Type LocateInLac(IApplicationContext applicationContext, string typeName)
        {
            if (typeName == null || typeName == string.Empty)
                return (Type)null;
            string mappedTypeName = applicationContext.GetMappedTypeName(typeName);
            lock (typeof(Type))
            {
                Type local_1 = ObjectFactory._typeCache[(object)mappedTypeName] as Type;
                if (local_1 == null && applicationContext != null)
                {
                    foreach (string item_0 in new string[4]
          {
            applicationContext.ApplicationPath,
            applicationContext.DynamicDirectory,
            Path.Combine(applicationContext.PhysicalApplicationPath, "bin"),
            Path.Combine(applicationContext.PhysicalApplicationPath, "Bin")
          })
                    {
                        local_1 = TypeHelper.Locate(mappedTypeName, item_0);
                        if (local_1 != null)
                        {
                            ObjectFactory._typeCache[(object)mappedTypeName] = (object)local_1;
                            return local_1;
                        }
                    }
                }
                return local_1;
            }
        }

        public static void AddTypeToCache(Type type)
        {
            if (type == null)
                return;
            lock (typeof(Type))
                ObjectFactory._typeCache[(object)type.FullName] = (object)type;
        }

        public static bool ContainsType(string typeName)
        {
            if (typeName == null)
                return false;
            lock (typeof(Type))
                return ObjectFactory._typeCache.Contains((object)typeName);
        }

        public static object CreateInstance(IApplicationContext applicationContext, Type type)
        {
            return ObjectFactory.CreateInstance(applicationContext, type, (object[])null);
        }

        public static object CreateInstance(IApplicationContext applicationContext, Type type, object[] args)
        {
            if (type == null)
                return (object)null;
            lock (typeof(Type))
            {
                string local_0 = ObjectFactory._activationModeCache[(object)type] as string;
                if (local_0 == null)
                {
                    ActivationAttribute local_1 = (ActivationAttribute)null;
                    object[] local_7 = type.GetCustomAttributes(typeof(ActivationAttribute), true);
                    int local_8 = 0;
                    if (local_8 < local_7.Length)
                        local_1 = (Attribute)local_7[local_8] as ActivationAttribute;
                    if (local_1 != null)
                    {
                        local_0 = local_1.ActivationMode;
                        ObjectFactory._activationModeCache[(object)type] = (object)local_0;
                    }
                }
                if (local_0 == null)
                {
                    local_0 = "request";
                    ObjectFactory._activationModeCache[(object)type] = (object)local_0;
                }
                if (applicationContext.ActivationMode != null)
                    local_0 = applicationContext.ActivationMode;
                IActivator local_3 = ObjectFactory._activatorsCache[(object)local_0] as IActivator;
                if (local_3 != null)
                    return local_3.Activate(applicationContext, type, args);
                string local_4 = string.Format("The requested activator [{0}] was not found.", (object)local_0);
                throw new FluorineException(local_4);
            }
        }

        public static object CreateInstance(IApplicationContext applicationContext, string typeName)
        {
            return ObjectFactory.CreateInstance(applicationContext, typeName, (object[])null);
        }

        public static object CreateInstance(IApplicationContext applicationContext, string typeName, object[] args)
        {
            Type type = ObjectFactory.Locate(applicationContext, typeName);
            return ObjectFactory.CreateInstance(applicationContext, type, args);
        }
    }
    public sealed class ClassMappings
    {
        private Hashtable _typeToCustomClass;
        private Hashtable _customClassToType;

        public ClassMappings()
        {
            this._typeToCustomClass = new Hashtable();
            this._customClassToType = new Hashtable();
            this.Add("Fluorine.AMF3.ArrayCollection", "flex.messaging.io.ArrayCollection");
            this.Add("Fluorine.AMF3.ByteArray", "flex.messaging.io.ByteArray");
            this.Add("Fluorine.AMF3.ObjectProxy", "flex.messaging.io.ObjectProxy");
            this.Add("Fluorine.Messaging.Messages.CommandMessage", "flex.messaging.messages.CommandMessage");
            this.Add("Fluorine.Messaging.Messages.RemotingMessage", "flex.messaging.messages.RemotingMessage");
            this.Add("Fluorine.Messaging.Messages.AsyncMessage", "flex.messaging.messages.AsyncMessage");
            this.Add("Fluorine.Messaging.Messages.AcknowledgeMessage", "flex.messaging.messages.AcknowledgeMessage");
            this.Add("Fluorine.Data.Messages.DataMessage", "flex.data.messages.DataMessage");
            this.Add("Fluorine.Data.Messages.PagedMessage", "flex.data.messages.PagedMessage");
            this.Add("Fluorine.Data.Messages.UpdateCollectionMessage", "flex.data.messages.UpdateCollectionMessage");
            this.Add("Fluorine.Data.Messages.SequencedMessage", "flex.data.messages.SequencedMessage");
            this.Add("Fluorine.Messaging.Messages.ErrorMessage", "flex.messaging.messages.ErrorMessage");
            this.Add("Fluorine.Messaging.Messages.RemotingMessage", "flex.messaging.messages.RemotingMessage");
            this.Add("Fluorine.Messaging.Messages.RPCMessage", "flex.messaging.messages.RPCMessage");
            this.Add("Fluorine.Data.UpdateCollectionRange", "flex.data.UpdateCollectionRange");
            this.Add("Fluorine.Messaging.Services.RemotingService", "flex.messaging.services.RemotingService");
            this.Add("Fluorine.Messaging.Services.MessageService", "flex.messaging.services.MessageService");
            this.Add("Fluorine.Data.DataService", "flex.data.DataService");
            this.Add("Fluorine.Messaging.Services.Remoting.DotNetAdapter", "flex.messaging.services.remoting.adapters.JavaAdapter");
        }

        public void Add(string type, string customClass)
        {
            this._typeToCustomClass[(object)type] = (object)customClass;
            this._customClassToType[(object)customClass] = (object)type;
        }

        public string GetCustomClass(string type)
        {
            if (this._typeToCustomClass.Contains((object)type))
                return this._typeToCustomClass[(object)type] as string;
            return type;
        }

        public string GetType(string customClass)
        {
            if (customClass == null)
                return (string)null;
            if (this._customClassToType.Contains((object)customClass))
                return this._customClassToType[(object)customClass] as string;
            return customClass;
        }
    }
}
namespace ExplorerManager
{
    internal class __Res
    {
        private static System.Resources.ResourceManager _resMgr;

        internal const string Amf_Begin = "Amf_Begin";
        internal const string Amf_End = "Amf_End";
        internal const string Amf_Fatal = "Amf_Fatal";
        internal const string Amf_Fatal404 = "Amf_Fatal404";
        internal const string Amf_ReadBodyFail = "Amf_ReadBodyFail";
        internal const string Amf_SerializationFail = "Amf_SerializationFail";
        internal const string Amf_ResponseFail = "Amf_ResponseFail";

        internal const string Rtmpt_Begin = "Rtmpt_Begin";
        internal const string Rtmpt_End = "Rtmpt_End";
        internal const string Rtmpt_Fatal = "Rtmpt_Fatal";
        internal const string Rtmpt_Fatal404 = "Rtmpt_Fatal404";
        internal const string Rtmpt_CommandBadRequest = "Rtmpt_CommandBadRequest";
        internal const string Rtmpt_CommandNotSupported = "Rtmpt_CommandNotSupported";
        internal const string Rtmpt_CommandOpen = "Rtmpt_CommandOpen";
        internal const string Rtmpt_CommandSend = "Rtmpt_CommandSend";
        internal const string Rtmpt_CommandIdle = "Rtmpt_CommandIdle";
        internal const string Rtmpt_CommandClose = "Rtmpt_CommandClose";
        internal const string Rtmpt_ReturningMessages = "Rtmpt_ReturningMessages";
        internal const string Rtmpt_NotifyError = "Rtmpt_NotifyError";
        internal const string Rtmpt_UnknownClient = "Rtmpt_UnknownClient";

        internal const string Swx_Begin = "Swx_Begin";
        internal const string Swx_End = "Swx_End";
        internal const string Swx_Fatal = "Swx_Fatal";
        internal const string Swx_Fatal404 = "Swx_Fatal404";
        internal const string Swx_InvalidCrossDomainUrl = "Swx_InvalidCrossDomainUrl";

        internal const string Json_Begin = "Json_Begin";
        internal const string Json_End = "Json_End";
        internal const string Json_Fatal = "Json_Fatal";
        internal const string Json_Fatal404 = "Json_Fatal404";

        internal const string Rtmp_HSInitBuffering = "Rtmp_HSInitBuffering";
        internal const string Rtmp_HSReplyBuffering = "Rtmp_HSReplyBuffering";
        internal const string Rtmp_HeaderBuffering = "Rtmp_HeaderBuffering";
        internal const string Rtmp_ChunkSmall = "Rtmp_ChunkSmall";
        internal const string Rtmp_DecodeHeader = "Rtmp_DecodeHeader";
        internal const string Rtmp_ServerAddMapping = "Rtmp_ServerAddMapping";
        internal const string Rtmp_ServerRemoveMapping = "Rtmp_ServerRemoveMapping";
        internal const string Rtmp_SocketListenerAccept = "Rtmp_SocketListenerAccept";
        internal const string Rtmp_SocketBeginReceive = "Rtmp_SocketBeginReceive";
        internal const string Rtmp_SocketReceiveProcessing = "Rtmp_SocketReceiveProcessing";
        internal const string Rtmp_SocketBeginRead = "Rtmp_SocketBeginRead";
        internal const string Rtmp_SocketReadProcessing = "Rtmp_SocketReadProcessing";
        internal const string Rtmp_SocketBeginSend = "Rtmp_SocketBeginSend";
        internal const string Rtmp_SocketSendProcessing = "Rtmp_SocketSendProcessing";
        internal const string Rtmp_SocketConnectionReset = "Rtmp_SocketConnectionReset";
        internal const string Rtmp_SocketDisconnectProcessing = "Rtmp_SocketDisconnectProcessing";
        internal const string Rtmp_ConnectionClose = "Rtmp_ConnectionClose";
        internal const string Rtmp_CouldNotProcessMessage = "Rtmp_CouldNotProcessMessage";

        internal const string Arg_Mismatch = "Arg_Mismatch";

        internal const string Cache_Hit = "Cache_Hit";
        internal const string Cache_HitKey = "Cache_HitKey";

        internal const string Compiler_Error = "Compiler_Error";

        internal const string ClassDefinition_Loaded = "ClassDefinition_Loaded";
        internal const string ClassDefinition_LoadedUntyped = "ClassDefinition_LoadedUntyped";
        internal const string Externalizable_CastFail = "Externalizable_CastFail";
        internal const string TypeIdentifier_Loaded = "TypeIdentifier_Loaded";
        internal const string TypeLoad_ASO = "TypeLoad_ASO";
        internal const string TypeMapping_Write = "TypeMapping_Write";
        internal const string TypeSerializer_NotFound = "TypeSerializer_NotFound";

        internal const string Endpoint_BindFail = "Endpoint_BindFail";
        internal const string Endpoint_Bind = "Endpoint_Bind";

        internal const string Type_InitError = "Type_InitError";
        internal const string Type_Mismatch = "Type_Mismatch";
        internal const string Type_MismatchMissingSource = "Type_MismatchMissingSource";

        internal const string Wsdl_ProxyGen = "Wsdl_ProxyGen";
        internal const string Wsdl_ProxyGenFail = "Wsdl_ProxyGenFail";

        internal const string Destination_NotFound = "Destination_NotFound";

        internal const string MessageBroker_NotAvailable = "MessageBroker_NotAvailable";
        internal const string MessageBroker_RegisterError = "MessageBroker_RegisterError";
        internal const string MessageBroker_RoutingError = "MessageBroker_RoutingError";

        internal const string MessageServer_LoadingConfig = "MessageServer_LoadingConfig";
        internal const string MessageServer_LoadingConfigDefault = "MessageServer_LoadingConfigDefault";
        internal const string MessageServer_LoadingServiceConfig = "MessageServer_LoadingServiceConfig";
        internal const string MessageServer_Start = "MessageServer_Start";
        internal const string MessageServer_Started = "MessageServer_Started";
        internal const string MessageServer_StartError = "MessageServer_StartError";
        internal const string MessageServer_Stop = "MessageServer_Stop";
        internal const string MessageServer_AccessFail = "MessageServer_AccessFail";
        internal const string MessageServer_Create = "MessageServer_Create";

        internal const string MessageClient_Disconnect = "MessageClient_Disconnect";
        internal const string MessageClient_Unsubscribe = "MessageClient_Unsubscribe";
        internal const string MessageClient_Timeout = "MessageClient_Timeout";

        internal const string MessageDestination_RemoveSubscriber = "MessageDestination_RemoveSubscriber";

        internal const string MessageServiceSubscribe = "MessageServiceSubscribe";
        internal const string MessageServiceUnsubscribe = "MessageServiceUnsubscribe";
        internal const string MessageServiceUnknown = "MessageServiceUnknown";
        internal const string MessageServiceRoute = "MessageServiceRoute";
        internal const string MessageServicePush = "MessageServicePush";
        internal const string MessageServicePushBinary = "MessageServicePushBinary";

        internal const string Subtopic_Invalid = "Subtopic_Invalid";
        internal const string Selector_InvalidResult = "Selector_InvalidResult";

        internal const string SubscriptionManager_Remove = "SubscriptionManager_Remove";
        internal const string SubscriptionManager_CacheExpired = "SubscriptionManager_CacheExpired";

        internal const string Invalid_Destination = "Invalid_Destination";

        internal const string Security_AccessNotAllowed = "Security_AccessNotAllowed";
        internal const string Security_LoginMissing = "Security_LoginMissing";
        internal const string Security_ConstraintRefNotFound = "Security_ConstraintRefNotFound";
        internal const string Security_ConstraintSectionNotFound = "Security_ConstraintSectionNotFound";
        internal const string Security_AuthenticationFailed = "Security_AuthenticationFailed";

        internal const string SocketServer_Start = "SocketServer_Start";
        internal const string SocketServer_Started = "SocketServer_Started";
        internal const string SocketServer_Stopping = "SocketServer_Stopping";
        internal const string SocketServer_Stopped = "SocketServer_Stopped";
        internal const string SocketServer_Failed = "SocketServer_Failed";
        internal const string SocketServer_ListenerFail = "SocketServer_ListenerFail";
        internal const string SocketServer_SocketOptionFail = "SocketServer_SocketOptionFail";

        internal const string RtmpEndpoint_Start = "RtmpEndpoint_Start";
        internal const string RtmpEndpoint_Starting = "RtmpEndpoint_Starting";
        internal const string RtmpEndpoint_Started = "RtmpEndpoint_Started";
        internal const string RtmpEndpoint_Stopping = "RtmpEndpoint_Stopping";
        internal const string RtmpEndpoint_Stopped = "RtmpEndpoint_Stopped";
        internal const string RtmpEndpoint_Failed = "RtmpEndpoint_Failed";
        internal const string RtmpEndpoint_Error = "RtmpEndpoint_Error";

        internal const string Scope_Connect = "Scope_Connect";
        internal const string Scope_NotFound = "Scope_NotFound";
        internal const string Scope_ChildNotFound = "Scope_ChildNotFound";
        internal const string Scope_Check = "Scope_Check";
        internal const string Scope_CheckHostPath = "Scope_CheckHostPath";
        internal const string Scope_CheckWildcardHostPath = "Scope_CheckWildcardHostPath";
        internal const string Scope_CheckHostNoPath = "Scope_CheckHostNoPath";
        internal const string Scope_CheckDefaultHostPath = "Scope_CheckDefaultHostPath";
        internal const string Scope_UnregisterError = "Scope_UnregisterError";
        internal const string Scope_DisconnectError = "Scope_DisconnectError";

        internal const string SharedObject_Delete = "SharedObject_Delete";
        internal const string SharedObject_DeleteError = "SharedObject_DeleteError";
        internal const string SharedObject_StoreError = "SharedObject_StoreError";
        internal const string SharedObject_Sync = "SharedObject_Sync";
        internal const string SharedObject_SyncConnError = "SharedObject_SyncConnError";

        internal const string SharedObjectService_CreateStore = "SharedObjectService_CreateStore";
        internal const string SharedObjectService_CreateStoreError = "SharedObjectService_CreateStoreError";

        internal const string DataDestination_RemoveSubscriber = "DataDestination_RemoveSubscriber";

        internal const string DataService_Unknown = "DataService_Unknown";

        internal const string Sequence_AddSubscriber = "Sequence_AddSubscriber";
        internal const string Sequence_RemoveSubscriber = "Sequence_RemoveSubscriber";

        internal const string SequenceManager_CreateSeq = "SequenceManager_CreateSeq";
        internal const string SequenceManager_Remove = "SequenceManager_Remove";
        internal const string SequenceManager_RemoveStatus = "SequenceManager_RemoveStatus";
        internal const string SequenceManager_Unknown = "SequenceManager_Unknown";
        internal const string SequenceManager_ReleaseCollection = "SequenceManager_ReleaseCollection";
        internal const string SequenceManager_RemoveSubscriber = "SequenceManager_RemoveSubscriber";
        internal const string SequenceManager_RemoveEmptySeq = "SequenceManager_RemoveEmptySeq";
        internal const string SequenceManager_RemoveSubscriberSeq = "SequenceManager_RemoveSubscriberSeq";

        internal const string Service_NotFound = "Service_NotFound";

        internal const string Identity_Failed = "Identity_Failed";

        internal const string Invoke_Method = "Invoke_Method";

        internal const string Channel_NotFound = "Channel_NotFound";

        internal const string Service_Mapping = "Service_Mapping";

        internal const string TypeHelper_Probing = "TypeHelper_Probing";
        internal const string TypeHelper_LoadDllFail = "TypeHelper_LoadDllFail";
        internal const string TypeHelper_ConversionFail = "TypeHelper_ConversionFail";

        internal const string Invocation_NoSuitableMethod = "Invocation_NoSuitableMethod";
        internal const string Invocation_Ambiguity = "Invocation_Ambiguity";
        internal const string Invocation_ParameterType = "Invocation_ParameterType";

        internal const string Reflection_MemberNotFound = "Reflection_MemberNotFound";
        internal const string Reflection_PropertyReadOnly = "Reflection_PropertyReadOnly";
        internal const string Reflection_PropertySetFail = "Reflection_PropertySetFail";
        internal const string Reflection_PropertyIndexFail = "Reflection_PropertyIndexFail";
        internal const string Reflection_FieldSetFail = "Reflection_FieldSetFail";

        internal const string AppAdapter_AppConnect = "AppAdapter_AppConnect";
        internal const string AppAdapter_AppDisconnect = "AppAdapter_AppDisconnect";
        internal const string AppAdapter_RoomConnect = "AppAdapter_RoomConnect";
        internal const string AppAdapter_RoomDisconnect = "AppAdapter_RoomDisconnect";
        internal const string AppAdapter_AppStart = "AppAdapter_AppStart";
        internal const string AppAdapter_RoomStart = "AppAdapter_RoomStart";
        internal const string AppAdapter_AppStop = "AppAdapter_AppStop";
        internal const string AppAdapter_RoomStop = "AppAdapter_RoomStop";
        internal const string AppAdapter_AppJoin = "AppAdapter_AppJoin";
        internal const string AppAdapter_AppLeave = "AppAdapter_AppLeave";
        internal const string AppAdapter_RoomJoin = "AppAdapter_RoomJoin";
        internal const string AppAdapter_RoomLeave = "AppAdapter_RoomLeave";

        internal const string Compress_Info = "Compress_Info";

        internal const string Fluorine_InitModule = "Fluorine_InitModule";
        internal const string Fluorine_Start = "Fluorine_Start";
        internal const string Fluorine_Version = "Fluorine_Version";

        internal const string ServiceBrowser_Aquire = "ServiceBrowser_Aquire";
        internal const string ServiceBrowser_Aquired = "ServiceBrowser_Aquired";
        internal const string ServiceBrowser_AquireFail = "ServiceBrowser_AquireFail";

        internal const string ServiceAdapter_MissingSettings = "ServiceAdapter_MissingSettings";
        internal const string ServiceAdapter_Stop = "ServiceAdapter_Stop";

        internal const string Msmq_StartQueue = "Msmq_StartQueue";
        internal const string Msmq_InitFormatter = "Msmq_InitFormatter";
        internal const string Msmq_Receive = "Msmq_Receive";
        internal const string Msmq_Send = "Msmq_Send";
        internal const string Msmq_Fail = "Msmq_Fail";
        internal const string Msmq_Enable = "Msmq_Enable";
        internal const string Msmq_Poison = "Msmq_Poison";

        internal const string Silverlight_StartPS = "Silverlight_StartPS";
        internal const string Silverlight_PSError = "Silverlight_PSError";

        internal static string GetString(string key)
        {
            if (_resMgr == null)
            {
                _resMgr = new System.Resources.ResourceManager("FluorineFx.Resources.Resource", typeof(__Res).Assembly);
            }
            string text = _resMgr.GetString(key);
            if (text == null)
            {
                throw new ApplicationException("Missing resource from FluorineFx library!  Key: " + key);
            }
            return text;
        }

        internal static string GetString(string key, params object[] inserts)
        {
            return string.Format(GetString(key), inserts);
        }
    }
}
