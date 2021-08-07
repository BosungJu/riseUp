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

    public class MapData : Message
    {
        public int[] map;

        public float superUserPos_x;
        public float userPos_x;

        public int superUserCount;
        public int userCount;
        
        public int superUserDirection;
        public int userDirection;

        public MapData(int[] map, float superUserPos_x, float userPos_x, int superUserCount, int userCount, int superUserDirection, int userDirection) : base(Type.MapData)
        {
            this.map = map;
            this.superUserPos_x = superUserPos_x;
            this.superUserCount = superUserCount;
            this.userPos_x = userPos_x;
            this.userCount = userCount;
            this.superUserDirection = superUserDirection;
            this.userDirection = userDirection;
        }
    }
}
