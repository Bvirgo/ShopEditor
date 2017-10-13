using UnityEngine;
using System;

namespace Jhqc.EditorCommon
{
    /// <summary>
    /// 主观判断不需要只有GUI没有DEBUG这个枚举类型，所以就不用flags了
    /// </summary>
    public enum LogType
    {
        None,
        Debug,
        DebugAndGUI
    }
}