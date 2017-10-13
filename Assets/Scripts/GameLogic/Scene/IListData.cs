using UnityEngine;
using System.Collections;
using System;

public interface IListData
{
    bool IsSelect {get;set; }
    string GetText(string paramName);
    void GetTexture(string paramName, Action<Texture2D> onGet);
    void ChangeData(string paramName, object value);
    Action OnDataChange { get; set; }
}
