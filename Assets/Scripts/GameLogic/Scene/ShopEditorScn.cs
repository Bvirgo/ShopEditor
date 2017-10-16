using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFrameWork;

/// <summary>
/// Shop Eidtor Scene
/// </summary>
public class ShopEditorScn : BaseScene
{
    #region Init & Register
    public ShopEditorScn()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        RegisterModule();

        UnRegisterMsg();

        InitData();
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        UnRegisterMsg();
    }

    private void RegisterModule()
    {
        ModuleManager.Instance.RegisterModule(typeof(ShopModule));
    }

    private void ReigsterMsg()
    {
    }

    private void UnRegisterMsg()
    {
    }
    #endregion

    #region Init

    private void InitData()
    {
        JHQCHelper.Instance.OnInitScene();

        LoadTestBuilding();
    }
    private void LoadTestBuilding()
    {
        // 加载一个测试建筑
        GameObject objBuilding = ResManager.Instance.LoadInstance(Defines.WhiteHousePath) as GameObject;
        Utils.AddMeshCollider(objBuilding);
        objBuilding.AddComponent<SceneModelMono>();
    }
    #endregion

}
