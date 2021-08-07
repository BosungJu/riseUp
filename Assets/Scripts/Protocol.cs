using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    public enum Type : sbyte
    {
        Jump = 0,
        Collapse,
        UserData,
        Seed,

        GameStart,
        GameEnd
    }

    public class Message
    {
        public Type type;

        public Message(Type type)
        {
            this.type = type;
        }
    }

    public class JumpMessage : Message
    {
        public float userPos_x;
        public float userDirection;
        public int count;

        public JumpMessage(float userPos_x, float userDirection, int count) : base(Type.Jump)
        {
            this.userPos_x = userPos_x;
            this.userDirection = userDirection;
            this.count = count;
        }
    }

    public class Collapse : Message
    {
        public BackEnd.Tcp.SessionId mySessionID;
        public BackEnd.Tcp.SessionId otherSessionID;
        public int myCount;
        public int otherCount;

        public Collapse(BackEnd.Tcp.SessionId mySessionID, BackEnd.Tcp.SessionId otherSessionID, int myCount, int otherCount) : base(Type.Collapse)
        {
            this.mySessionID = mySessionID;
            this.otherSessionID = otherSessionID;
            this.myCount = myCount;
            this.otherCount = otherCount;
        }
    }


    public class UserMassage : Message
    {
        public float superUserPos_x;
        public float userPos_x;

        public int superUserCount;
        public int userCount;
        
        public int superUserDirection;
        public int userDirection;

        public UserMassage(float superUserPos_x, float userPos_x, int superUserCount, int userCount, int superUserDirection, int userDirection) : base(Type.UserData)
        {
            this.superUserPos_x = superUserPos_x;
            this.userPos_x = userPos_x;
            this.superUserCount = superUserCount;
            this.userCount = userCount;
            this.superUserDirection = superUserDirection;
            this.userDirection = userDirection;
        }
    }

    public class SeedMessage : Message
    {
        public int seed;

        public SeedMessage(int seed) : base(Type.Seed)
        {
            this.seed = seed;
        }
    }
}
