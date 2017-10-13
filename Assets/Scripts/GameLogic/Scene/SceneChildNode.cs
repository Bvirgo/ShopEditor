using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using LitJson;
using System;

public class SceneChildNode : MonoBehaviour,IJsonData, IMatEditable
{
    public List<SceneChildNode> children;
    public List<SceneChildNode> m_pChildrenSorted;

    JsonData material;
    public JsonData MatJD
    {
        get
        {
            return material;
        }

        set
        {
            material = value;
        }
    }

    public SceneChildNode()
    {
    }

    public SceneChildNode(JsonData jd)
    {
        ReadJsonData(jd);
    }

    public SceneChildNode ReadDataFromModel(GameObject go)
    {
        name = go.name;
        if (children == null)
            children = new List<SceneChildNode>();
        List<SceneChildNode> toDeleteList = children.Clone();

        foreach (Transform tr in go.transform)
        {
            SceneChildNode tarScn = children.Find((scn => {
                string goName = tr.name;
                return scn.name == tr.name;
            }));
            if (tarScn == null)
            {
                tarScn = new SceneChildNode();
                children.Add(tarScn);
            }
            else
                toDeleteList.Remove(tarScn);//能在新模型上找到的node不用删除
            tarScn.ReadDataFromModel(tr.gameObject);
        }

        //删除在新模型上已不存在的SceneChildNode
        children.RemoveFromList(removeNode => { return toDeleteList.Contains(removeNode); });

        // 按照子节点顺序存储
        m_pChildrenSorted = new List<SceneChildNode>();
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject cGo = go.transform.GetChild(i).gameObject;
            string strNodeName = cGo.name;
            SceneChildNode tarScn = children.Find(item =>
            {
                return item.name.Equals(strNodeName);
            });
            if (tarScn != null)
            {
                m_pChildrenSorted.Add(tarScn);
            }
        }
        return this;
    }

    public virtual JsonData ToJsonData()
    {
        JsonData node = new JsonData();
        node["name"] = name;
        if (material == null)
            material = new CityMaterial(null).ToJsonData();
        node["material"] = material;
        node["child"] = children.ToJsonDataList();
        return node;
    }

    public virtual IJsonData ReadJsonData(JsonData jd)
    {
        name = jd.ReadString("name");

        MatJD = jd.ReadJsonData("material");

        JsonData childJD = jd.ReadJsonData("child");
        if (childJD != null)
            children = childJD.ToItemVOList<SceneChildNode>();

        return this;
    }

    public IMatEditable GetChildMatEdit(GameObject childGO)
    {
        return null;//由于SceneChildNode不包含对GameObject的引用, 所以不能获取子集
    }
    /// <summary>只刷新对应模型的材质, 不刷新子物体的</summary>
    public void RefreshMaterial(GameObject go, bool rebuildImmediately, Action callback)
    {
        if (MatJD != null)
            CityMaterial.SetMaterial(MatJD, go, rebuildImmediately, callback);
    }
}
