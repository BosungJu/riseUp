using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Data", order = 1)]
public class Data : ScriptableObject
{
    public int _level;
    public int _count;

    public int level
    {
        get
        {
            return _level;
        }
        set
        {
            _level = value;

            if (levelUpEvent != null) 
            { 
                levelUpEvent(value); 
            }
        }
    }

    public int count
    {
        get
        {
            return _count;
        }
        set
        {
            _count = value;

            if (countUpEvent != null) 
            { 
                countUpEvent(value); 
            }

            
            if (_count % 20 == 0 && _count != 0)
            {
                level += 1;
            }
        }
    }

    public Action<int> levelUpEvent;
    public Action<int> countUpEvent;

    public Action jumpStartEvent;
    public Action jumpEndEvent;
}
