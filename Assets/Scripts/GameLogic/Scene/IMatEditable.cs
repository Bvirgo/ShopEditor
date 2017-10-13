using UnityEngine;
using System.Collections;
using LitJson;
using System;

/// <summary>可编辑材质的(输入Json格式的PM材质数据)</summary>
public interface IMatEditable
{
    JsonData MatJD { get; set; }
    /// <summary>针对SceneNodeMono等含有多层IMatEditable子项, 并且只有顶级才挂有MonoBehavior脚本的实体, 
    /// 必须先获取顶层Mono对象, 再通过此方法获取目标GameObject对应的IMatEditable对象</summary>
    IMatEditable GetChildMatEdit(GameObject childGO);
    void RefreshMaterial(GameObject go, bool rebuildImmediately, Action callback);
}
