
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extensions
{
    public static void BindEvent(this GameObject go, Action<PointerEventData> action = null, Define.ETouchEvent type = Define.ETouchEvent.Click) 
    {
        UI_Base.BindEvent(go, action, type);    
    }
}
