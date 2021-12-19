using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace defaultGame.Communication.VO
{
    public class dServerCall
    {
        public string dsoAuthToken { get; set; }
        public int type { get; set; }
        public int zoneID { get; set; }
        public int dsoAuthUser { get; set; }
        public object data { get; set; }
        public int dsoAuthRandomClientID { get; set; }

        public dServerCall(int user, string token)
        {
            zoneID = user;
            dsoAuthUser = user;
            dsoAuthToken = token;
            int rand = new Random().Next() * 2147483646;
            if (rand < 0) rand *= -1;
            dsoAuthRandomClientID = rand;
        }
    }
    public class dStartSpecialistTaskVO
    {
        public int subTaskID { get; set; }
        public string paramString { get; set; }
        public defaultGame.Communication.VO.dUniqueID uniqueID { get; set; }

    }
    public class dServerAction
    {
        public int grid { get; set; }
        public int type { get; set; }
        public int endGrid { get; set; }
        public object data { get; set; }

    }
    public class dUniqueID
    {
        public int uniqueID1 { get; set; }
        public int uniqueID2 { get; set; }

    }

    public class dIntegerVO
    {
        public int value { get; set; }
    }
}