using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFrameWork;
using LitJson;

public class LoginModule : BaseModule {

    #region Base
    public LoginModule()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        MessageCenter.Instance.AddListener(MsgType.LoginView_Login, OnLogin);
    }

    protected override void OnRelease()
    {
        base.OnRelease();

        MessageCenter.Instance.RemoveListener(MsgType.LoginView_Login,OnLogin);
    }
    #endregion

    #region Actions
    private void OnLogin(Message _msg)
    {
        string strUser = _msg["user"].ToString();
        string strPsw = _msg["psw"].ToString();

        //Message msg;
        LogicUtils.Instance.OnShowWaiting(1, "Login...",true);

        HttpService.Login(strUser, strPsw, (success, resp) =>
        {       
            if (success)
            {
                UserCache.SetUserName(strUser);
                UserCache.SetPassword(strPsw);

                // 组件编辑器
                //LevelManager.Instance.ChangeSceneDirect(ScnType.CompEditor,UIType.CompEditor);

                // 商铺编辑器
                LevelManager.Instance.ChangeSceneDirect(ScnType.ShopEditor, UIType.ShopEditor);
            }
            else
            {
                Debug.LogError("LoginError："+resp.Error);
            }

            LogicUtils.Instance.OnHideWaiting();
        });
    }
    #endregion

}
