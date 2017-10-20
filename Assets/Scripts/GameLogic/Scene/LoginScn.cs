using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;

/// <summary>
/// Login Scene
/// </summary>
public class LoginScn : BaseScene
{
    #region Base
    public LoginScn()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        RegisterModule();
    }

    protected override void OnRelease()
    {
        base.OnRelease();
    }

    /// <summary>
    /// Registe Child Module
    /// </summary>
    private void RegisterModule()
    {
        ModuleManager.Instance.RegisterModule(typeof(LoginModule));
        ModuleManager.Instance.RegisterModule(typeof(WindowModule));
        ModuleManager.Instance.RegisterModule(typeof(WaitingModule));
    }
    #endregion
}
