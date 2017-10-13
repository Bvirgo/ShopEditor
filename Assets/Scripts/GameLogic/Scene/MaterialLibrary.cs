using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using LitJson;

public class MaterialList
{
    public MaterialList() {
        matList = new List<ProceduralMaterial>();
    }

    //level 是压缩等级 1 表示r g b 各有256阶
    //                2 表示r g b 各有128阶
    //                3 表示r g b 各有 64阶
    // 如果没有会返回NULL
    public bool GetSamiliarMaterial(CityMaterialParamList paramList,int level,out ProceduralMaterial samiliarMat)
    {
        samiliarMat = null;
        if (matList.Count == 0)
        {        
            return false;
        }

        for (int i = 0; i < matList.Count; ++i)
        {
            if (paramList.BeSamiliar(matList[i],level))
            {
                samiliarMat = matList[i];
                return true;
            }
        }   
        return false;
    }

    public void AddMaterial(ProceduralMaterial pm)
    {
        matList.Add(pm);
    }


    private List<ProceduralMaterial> matList;
}



public class MaterialLibrary {
    static MaterialLibrary instance;
    //level 是压缩等级 1 表示r g b 各有256阶
    //                2 表示r g b 各有128阶
    //                3 表示r g b 各有 64阶
    //                4 .......
    private int Matlevel;

    private Dictionary<string, MaterialList> matLibrary = new Dictionary<string, MaterialList>();

    public static MaterialLibrary Instance
    {
        get
        {
            if (instance == null)
                instance = new MaterialLibrary();
            return instance;
        }
    }
    private MaterialLibrary()
    {
        Matlevel = 1;
    }

    public void SetLevel(int l)
    {
        Matlevel = l;
    }

    public int GetLevel()
    {
        return Matlevel;
    }

    public bool FindMaterial(JsonData cityMatJson, out ProceduralMaterial outMat)
    {
        return FindMaterial(CityMaterial.CreateFromJClass(cityMatJson), out outMat);
    }

    public bool FindMaterial(CityMaterial cityMat, out ProceduralMaterial outMat)
    {
        outMat = null;
        string targetMatName = Utils.RemovePostfix_Instance(cityMat.matName);


        if (matLibrary.ContainsKey(targetMatName))
        {
            var mlist = matLibrary[targetMatName];
            return mlist.GetSamiliarMaterial(CityMaterialParamList.CreateFromJson(cityMat.matParams), 
                Matlevel, out outMat);
        }
        return false;
    }


    public void AddMaterial(ProceduralMaterial pm)
    {

        string targetMatName = Utils.RemovePostfix_Instance(pm.name);

        if (!matLibrary.ContainsKey(targetMatName))
            matLibrary.Add(targetMatName, new MaterialList());

        matLibrary[targetMatName].AddMaterial(pm);
    }
}
