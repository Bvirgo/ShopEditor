using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZFrameWork;
using System;

public class LoginView : BaseUI {
    #region UI
    [HideInInspector, AutoUGUI]
    public Text txt_user;
    [HideInInspector, AutoUGUI]
    public Text txt_psw;
    [HideInInspector, AutoUGUI]
    public Button btn_login;
    [HideInInspector, AutoUGUI]
    public InputField ipt_user;
    [HideInInspector, AutoUGUI]
    public InputField ipt_psw;
    #endregion
    public override UIType GetUIType()
    {
        return UIType.Login;
    }

    void Start()
    {
        ipt_user.text = UserCache.GetUserName();
        ipt_psw.text = UserCache.GetPassword();

        btn_login.onClick.AddListener(() => {
            Message msg = new Message(MsgType.LoginView_Login, this);
            msg["user"] = ipt_user.text;
            msg["psw"]  = ipt_psw.text;
            msg.Send();

        });
    }
    
}
