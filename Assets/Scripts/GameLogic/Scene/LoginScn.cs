using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFrameWork;

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
    }
    #endregion
}
