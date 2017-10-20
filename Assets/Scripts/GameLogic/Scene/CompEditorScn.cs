using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;

public class CompEditorScn : BaseScene
{
    #region Init & Regster
    public CompEditorScn()
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
        ModuleManager.Instance.RegisterModule(typeof(CompModule));
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
    }
    #endregion
}
