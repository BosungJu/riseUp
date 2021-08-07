using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    public enum Type : sbyte
    {
        Jump = 0,
        Collapse,
        MapData,

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

        public JumpMessage(float userPos_x) : base(Type.Jump)
        {
            this.userPos_x = userPos_x;
        }
    }

    public class UserMapData
    {
        public byte[] map;

        public float superUserPos_x;
        public float userPos_x;

        public int superUserCount;
        public int userCount;

        public int superUserDirection;
        public int userDirection;

        public UserMapData()
        {

        }

        public UserMapData(byte[] map, float superUserPos_x, float userPos_x, int superUserCount, int userCount, int superUserDirection, int userDirection)
        {
            this.map = map;
            this.superUserPos_x = superUserPos_x;
            this.userPos_x = userPos_x;
            this.superUserCount = superUserCount;
            this.userCount = userCount;
            this.superUserDirection = superUserDirection;
            this.userDirection = userDirection;
        }
    }

    public class MapData : Message
    {
        public byte[] map;

        public float superUserPos_x;
        public float userPos_x;

        public int superUserCount;
        public int userCount;
        
        public int superUserDirection;
        public int userDirection;

        public MapData(UserMapData userMapData) : base(Type.MapData)
        {
            this.map = userMapData.map;
            this.superUserPos_x = userMapData.superUserPos_x;
            this.superUserCount = userMapData.superUserCount;
            this.userPos_x = userMapData.userPos_x;
            this.userCount = userMapData.userCount;
            this.superUserDirection = userMapData.superUserDirection;
            this.userDirection = userMapData.userDirection;
        }
    }
}
