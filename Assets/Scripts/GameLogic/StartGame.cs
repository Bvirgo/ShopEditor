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
        // Server Net
        InitNet();

        // Mouse Manager
        MouseManager.Instance.OnInit();

        // Level Manager
        LevelManager.Instance.OnInit();

        // UI Pool
        UIPoolManager.Instance.OnInit();

        // Load First Scene
        LoadFirstScn();
    }

    /// <summary>
    /// Init Net
    /// </summary>
    private void InitNet()
    {
        WWWManager.Instance.Init(Defines.ServerAddress, Jhqc.EditorCommon.LogType.None);// 外网
        WWWManager.Instance.TimeOut = 600f;
    }

    /// <summary>
    /// Load First Scene
    /// </summary>
    private void LoadFirstScn()
    {

        // Register Login Scene Module
        ModuleManager.Instance.RegisterModule(typeof(LoginScn));

        // Open Login View
        UIManager.Instance.OpenUI(UIType.Login, true);
    }

}
