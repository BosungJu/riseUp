using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UserDatas
{
    static public Ref<string> ID = new Ref<string>("");
    static public Ref<string> PW = new Ref<string>("");
    static public Ref<string> NickName = new Ref<string>("");
    static public Ref<int> Gold = new Ref<int>(0);
    static public Ref<int> Score = new Ref<int>(0);
    static public Ref<bool> isLogin = new Ref<bool>(false);
}
