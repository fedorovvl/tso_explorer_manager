using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fluorine.AMF3;

namespace ExplorerManager
{
    class DSKMessage : IExternalizable
    {
        public dServerResponse body;
        public void ReadExternal(IDataInput input)
        {
            this.body = input.ReadObject() as dServerResponse;
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    class dServerResponse : IExternalizable
    {
        public int type;
        public int zoneId;
        public dServerActionResult data;
        public void ReadExternal(IDataInput input)
        {
            this.type = input.ReadInt();
            this.zoneId = input.ReadInt();
            this.data = input.ReadObject() as dServerActionResult;
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    class dServerActionResult : IExternalizable
    {
        public double clientTime;
        public int errorCode;
        public object data;
        public void ReadExternal(IDataInput input)
        {
            this.clientTime = input.ReadDouble();
            this.errorCode = input.ReadInt();
            this.data = input.ReadObject();
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    class dMailHeaderResponseVO : IExternalizable
    {
        public bool isinbox;
        public object headers_collection;
        public void ReadExternal(IDataInput input)
        {
            this.isinbox = input.ReadBoolean();
            this.headers_collection = input.ReadObject();
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    class dMailVO : IExternalizable
    {
        public string subject;
        public int type;
        public int senderId;
        public object attachments;
        public object recipientIds_collection;
        public object recipientNames_collection;
        public int id;
        public string senderName;
        public string body;
        public int reciepientId;
        public int deletedAt;
        public double timestamp;
        public double expirationTime;
        public void ReadExternal(IDataInput input)
        {
            this.type = input.ReadInt();
            this.senderId = input.ReadInt();
            this.id = input.ReadInt();
            this.reciepientId = input.ReadInt();
            this.deletedAt = input.ReadInt();
            this.timestamp = input.ReadDouble();
            this.expirationTime = input.ReadDouble();
            this.subject = input.ReadUTF();
            this.senderName = input.ReadUTF();
            this.subject = input.ReadUTF();
            this.body = input.ReadUTF();
            this.attachments = input.ReadObject();
            this.recipientIds_collection = input.ReadObject();
            this.recipientNames_collection = input.ReadObject();
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dLootItemsVO : IExternalizable
    {
        public ArrayCollection items { get; set; }
        public int shopItemId { get; set; }
        public ArrayCollection uniqueIDs { get; set; }
        public ArrayCollection premiumItems { get; set; }
        public int mailId { get; set; }
        public dUniqueID uniqueID { get; set; }
        public ArrayCollection premiumUniqueIDs { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.items = input.ReadObject() as ArrayCollection;
            this.shopItemId = input.ReadInt();
            this.uniqueIDs = input.ReadObject() as ArrayCollection;
            this.premiumItems = input.ReadObject() as ArrayCollection;
            this.mailId = input.ReadInt();
            this.uniqueID = input.ReadObject() as dUniqueID;
            this.premiumUniqueIDs = input.ReadObject() as ArrayCollection;
        }

        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dBuffVO
    {
        public int uniqueId1 { get; set; }
        public int uniqueId2 { get; set; }
        public int count { get; set; }
        public int recurringChance { get; set; }
        public string buffName_string { get; set; }
        public int remaining { get; set; }
        public string resourceName_string { get; set; }
        public int amount { get; set; }
        public void ReadExternal(IDataInput input)
        {
            this.uniqueId1 = input.ReadInt();
            this.uniqueId2 = input.ReadInt();
            this.count = input.ReadInt();
            this.recurringChance = input.ReadInt();
            this.buffName_string = input.ReadUTF();
            this.remaining = input.ReadInt();
            this.resourceName_string = input.ReadUTF();
            this.amount = input.ReadInt();
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dUniqueID
    {
        public int uniqueID1 { get; set; }
        public int uniqueID2 { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.uniqueID1 = input.ReadInt();
            this.uniqueID2 = input.ReadInt();
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }
    //
    public class dZoneVO
    {
        public ArrayCollection specialists_vector { get; set; }
        public ArrayCollection playersOnMap { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.specialists_vector = input.ReadObject() as ArrayCollection;
            this.playersOnMap = input.ReadObject() as ArrayCollection;
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dPlayerVO
    {
        public int playerLevel { get; set; }
        public string username_string { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.username_string = input.ReadUTF();
            this.playerLevel = input.ReadInt();
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }

    public class SkillVO
    {
        public int level { get; set; }
        public int id { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.level = input.ReadInt();
            this.id = input.ReadInt();
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dSpecialistTaskVO
    {
        public int subTaskID{ get; set; }
        public int phase{ get; set; }
        public int bonusTime{ get; set; }
        public int type{ get; set; }
        public int collectedTime { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.subTaskID = input.ReadInt();
            this.phase = input.ReadInt();
            this.bonusTime = input.ReadInt();
            this.type = input.ReadInt();
            this.collectedTime = input.ReadInt();
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }
    public class dSpecialistVO
    {
        public int specialistType { get; set; }
        public string name_string { get; set; }
        public dSpecialistTaskVO task { get; set; }
        public ArrayCollection skills { get; set; }
        public dUniqueID uniqueID { get; set; }

        public void ReadExternal(IDataInput input)
        {
            this.specialistType = input.ReadInt();
            this.name_string = input.ReadUTF();
            this.task = input.ReadObject() as dSpecialistTaskVO;
            this.skills = input.ReadObject() as ArrayCollection;
            this.uniqueID = input.ReadObject() as dUniqueID;
        }
        public void WriteExternal(IDataOutput output)
        {

        }
    }

}
