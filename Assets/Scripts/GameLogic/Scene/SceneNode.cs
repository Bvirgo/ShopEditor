using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.IO;
using System;
using UnityEngine.EventSystems;
using LitJson;
using System.Windows.Forms;

public class SceneNode : SceneChildNode
{
    public string guid;
    public string fileName;
    public string fileCrc;
    public string postion;
    public string rotation;
    public string scale;
    /// <summary>文件类型, 用于解析</summary>
    public string resType = "";
    /// <summary>记录文件在服务器命名规则, 用于获取文件</summary>
    public int resTag;
    public string picRouteID;

    public JsonData shopJD;
    public JsonData atlasJD;

    public SceneNode()
    {
        guid = System.Guid.NewGuid().ToString();
    }

    public SceneNode(JsonData jd)
    {
        ReadJsonData(jd);
    }

    public SceneNode Clone()
    {
        return new SceneNode(ToJsonData());
    }

    private void UpdatePos()
    {
       postion = gameObject.transform.position.ToString();
       rotation = gameObject.transform.rotation.ToString();
       scale = gameObject.transform.localScale.ToString();
    }

    public override JsonData ToJsonData()
    {
        UpdatePos();

        JsonData node = new JsonData();
        node["guid"] = guid;
        node["fileCrc"] = fileCrc;
        node["fileName"] = fileName.ToLower();
        if (!string.IsNullOrEmpty(resType))
        {
            node["type"] = resType.ToLower();
        }
        node["tag"] = resTag;
        node["postion"] = postion;
        node["rotation"] = rotation;
        node["scale"] = scale;
        node["name"] = name;
        if (MatJD == null)
            MatJD = new CityMaterial(null).ToJsonData();
        node["material"] = MatJD;

        if(children != null)
            node["children"] = children.ToJsonDataList();
        return node;
    }

    public override IJsonData ReadJsonData(JsonData jd)
    {
        guid = jd.ReadString("guid");
        fileName = jd.ReadString("fileName","null").ToLower();
        fileCrc = jd.ReadString("fileCrc");
        postion = jd.ReadString("postion");
        rotation = jd.ReadString("rotation");
        scale = jd.ReadString("scale");
        name = jd.ReadString("name");
        resType = jd.ReadString("type");
        resTag = jd.ReadInt("tag");

        MatJD = jd.ReadJsonData("material");

        JsonData childrenJD = jd.ReadJsonData("children");
        if (childrenJD != null)
            this.children = childrenJD.ToItemVOList<SceneChildNode>();
        else
            this.children = new List<SceneChildNode>();

        return this;
    }
}
