using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyFrameWork;
using System;
using Jhqc.EditorCommon;

public class StartGame : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        // 初始化网络
        InitNet();

        // 鼠标管理器
        MouseManager.Instance.OnInit();

        // 加载数据模块
        ModuleManager.Instance.RegisterAllModules();

        // 打开指定UI
		UIManager.Instance.OpenUI(UIType.Login,true);
	}

    /// <summary>
    /// 初始化网络
    /// </summary>
    private void InitNet()
    {
        WWWManager.Instance.Init(Defines.ServerAddress, Jhqc.EditorCommon.LogType.None);// 外网
        WWWManager.Instance.TimeOut = 600f;
    }

}
