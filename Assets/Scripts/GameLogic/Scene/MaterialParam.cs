using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using Jhqc.EditorCommon;
using System.IO;
using System;
using LitJson;

public class CityMaterialParam : IJsonData
{
    public string name;
    public string type;
    public string value;

    public JsonData ToJsonData()
    {
        JsonData jd = new JsonData();
        jd["name"] = name;
        jd["type"] = type;
        jd["value"] = value;
        return jd;
    }

    public IJsonData ReadJsonData(JsonData jd)
    {
        name = jd.ReadString("name");
        type = jd.ReadString("type");
        value = jd.ReadString("value");
        return this;
    }

    static private CityMaterialParam CreateFromString(string json)
    {
        return CreateFromJson(JsonMapper.ToObject(json));
    }

    static public CityMaterialParam CreateFromJson(JsonData jd)
    {
        return new CityMaterialParam().ReadJsonData(jd) as CityMaterialParam;
    }

    public override string ToString()
    {
        string str = "--Name:" + name + "--Type:" + type + "--Value:" + value;
        return str;
    }
}


public class CityMaterialParamList :IJsonData
{
    public List<CityMaterialParam> paramList;
    public CityMaterialParamList(ProceduralMaterial pm)
    {
        SetValue(pm);
    }

    public CityMaterialParamList()
    {
        paramList = new List<CityMaterialParam>();
    }


    public static CityMaterialParamList CreateFromJson(JsonData jdArr)
    {
        CityMaterialParamList paramList = new CityMaterialParamList();

        paramList.paramList = jdArr.ToItemVOList<CityMaterialParam>();

        return paramList;
    }

    public static CityMaterialParamList CreateFromMaterialEntity()
    {
        CityMaterialParamList paramList = new CityMaterialParamList();


        return paramList;
    }

    public void SetValue(ProceduralMaterial pm)
    {
        if (pm != null)
        {
            paramList = new List<CityMaterialParam>();

            var des = pm.GetProceduralPropertyDescriptions();
            for (int i = 0; i < des.Length; ++i)
            {
                var param = new CityMaterialParam();
                param.name = des[i].name;
                param.type = des[i].type.ToString();
                param.value = GetValueStringByName(pm, des[i], des[i].name);
                //InfoTips.LogInfo(string.Format("{0}   {1}   {2}", param.name, param.type, param.value));
                paramList.Add(param);
            }
        }
    }


    //level 是压缩等级 1 表示r g b 各有256阶
    //                2 表示r g b 各有128阶
    //                3 表示r g b 各有 64阶
    public bool BeSamiliar(ProceduralMaterial pm, int level)
    {
        foreach (var param in paramList)
        {
            if ((param.type == "Color4") || (param.type == "Color3"))
            {
                if (!BeSamiliarColor(StringUtil.StringToColor4(param.value),
                    pm.GetProceduralColor(param.name), level))
                    return false;
            }
        }
        return true;
    }

    //只比较rgb，忽略a
    private bool BeSamiliarColor(Color a, Color b, int level)
    {
        var va = LevelDown(ColorToVec3(a), level);
        var vb = LevelDown(ColorToVec3(b), level);
        return SameVector(va, vb);
    }

    private bool SameVector(int[] a, int[] b)
    {
        if (a == null || b == null)
            return false;

        if (a.Length == 3 && b.Length == 3)
        {
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                    return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }



    private int[] LevelDown(int[] vec, int level)
    {
        for (int i = 0; i < vec.Length; ++i)
        {
            vec[i] = (int)(vec[i] / (2 ^ (level - 1))) * level;
        }
        return vec;
    }

    private int[] ColorToVec3(Color color)
    {
        int[] vec = new int[3];
        vec[0] = (int)(color.r * 255);
        vec[1] = (int)(color.g * 255);
        vec[2] = (int)(color.b * 255);

        return vec;
    }


    public void SetMaterial(ProceduralMaterial pm, bool rebuildImmediately = false)
    {
        foreach (var param in paramList)
        {
            SetValueByType(pm, param);
        }
    }


    public JsonData ToJsonData()
    {  
        return paramList.ToJsonDataList();
    }

    public IJsonData ReadJsonData(JsonData jd)
    {
        paramList = jd.ToItemVOList<CityMaterialParam>();
        return this;
    }

    private void SetValueByType(ProceduralMaterial pm, CityMaterialParam param)
    {
        if (param.type == "Boolean")
        {
            pm.SetProceduralBoolean(param.name,
                (param.value.ToLower() == "true") ? true : false);
        }
        else if (param.type == "Float")
        {
            pm.SetProceduralFloat(param.name, float.Parse(param.value));
        }
        else if (param.type == "Vector4" ||
            param.type == "Vector3" ||
            param.type == "Vector2")
        {
            pm.SetProceduralVector(param.name, StringUtil.StringToVector4(param.value));
        }
        else if (param.type == "Color4" ||
            param.type == "Color3")
        {
            pm.SetProceduralColor(param.name, StringUtil.StringToColor4(param.value));
        }
    }

    private string GetValueStringByName(ProceduralMaterial pm,
                    ProceduralPropertyDescription des, string name)
    {
        if (des.type == ProceduralPropertyType.Boolean)
        {
            return pm.GetProceduralBoolean(name).ToString();
        }
        else if (des.type == ProceduralPropertyType.Float)
        {
            return pm.GetProceduralFloat(name).ToString();
        }
        else if (des.type == ProceduralPropertyType.Vector4 ||
            des.type == ProceduralPropertyType.Vector3 ||
            des.type == ProceduralPropertyType.Vector2)
        {
            return pm.GetProceduralVector(name).ToString();
        }
        else if (des.type == ProceduralPropertyType.Color4 ||
            des.type == ProceduralPropertyType.Color3)
        {
            return pm.GetProceduralColor(name).ToString();
        }

        return "";
    }
}


public class CityMaterial :IJsonData
{
    public static string UnsetName = "__Unset";
    public string matName;
    public JsonData matParams;
    public CityMaterial(ProceduralMaterial pm)
    {
        if (pm != null)
        {
            matName = Utils.RemovePostfix_Instance(pm.name);
            matParams = new CityMaterialParamList(pm).ToJsonData();
        }
        else
        {
            matName = UnsetName;
            matParams = new List<IJsonData>().ToJsonDataList();
        }
    }

    public CityMaterial()
    {
    }

    public JsonData ToJsonData()
    {
        //UpdateParam();
        var node = new JsonData();
        node["name"] = matName;
        node["params"] = matParams;
        return node;
    }
    public IJsonData ReadJsonData(JsonData jd)
    {
        matName = jd.ReadString("name");
        matParams = jd.ReadJsonData("params");
        return this;
    }

    //static public CityMaterial CreateFromJClass(JSONClass json)
    //{
    //    var cityMat = new CityMaterial();
    //    cityMat.matName = json["name"];
    //    cityMat.matParams = json["params"] as JSONArray;
    //    return cityMat;
    //}
    static public CityMaterial CreateFromJClass(JsonData jd)
    {
        var cityMat = new CityMaterial();
        cityMat.matName = jd.ReadString("name");
        cityMat.matParams = jd.ReadJsonData("params");
        return cityMat;
    }

    static public void SetMaterial(JsonData js, GameObject go, bool rebuildImmediately, Action onDone = null)
    {
        if (go == null)
            return;

        if (js == null || !js.Keys.Contains("name") || !js.Keys.Contains("params"))
        {
            if (onDone != null)
                onDone();
            return;
        }

        var cityMat = new CityMaterial();
        cityMat.ReadJsonData(js);
        if (cityMat.matName == UnsetName || cityMat.matName == null)
        {
            if (onDone != null)
                onDone();
        }
        else
        {
            //cityMat.matName = cityMat.matName.Substring(0, cityMat.matName.Length - backstr.Length);
            cityMat.matName = Utils.RemovePostfix_Instance(cityMat.matName);
            //CityMaterialParamList paramList = CityMaterialParamList.CreateFromJson(cityMat.matParams);

            string strName = cityMat.matName;
            if (strName.IndexOf("(") > -1)
            {
                strName = strName.Substring(0, strName.IndexOf("("));
                strName = strName.TrimEnd();
            }

            JHQCHelper.Instance.GetMatByName(strName, (mat) =>
                {
                    if (go == null)
                        return;
                    var render = go.GetComponent<MeshRenderer>();

            
                    if (render != null && mat != null)
                    {
                        render.material = mat.Material;

                        JHQCHelper.Instance.AddObjAndMat(go, mat);

                        if (onDone != null)
                            onDone();

                    }

                });
        }
    }
}
