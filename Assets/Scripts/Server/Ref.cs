using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ref <T>
{
    public System.Action<T> OnChange;

    public Ref(T value)
    {
        this.value = value;
    }

    private T value;
    public T Value
    {
        set
        {
            this.value = value;
            if(OnChange != null)
                OnChange(value);
        }
        get
        {
            return value;
        }
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
