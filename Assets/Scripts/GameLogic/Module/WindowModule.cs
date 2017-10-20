using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using System;

public class WindowModule : BaseModule {
    private bool m_bShow;
    private Queue<Message> m_qShowWin;
    public WindowModule()
    {
        this.AutoRegister = true;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        InitData();

        RegisterMessage();
    }

    private void InitData()
    {
        m_bShow = false;
        m_qShowWin = new Queue<Message>();
    }

    private void RegisterMessage()
    {
        MessageCenter.Instance.AddListener(MsgType.Win_Show, OnShowWindow);
        MessageCenter.Instance.AddListener(MsgType.Win_Finish, OnAffirm);
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        MessageCenter.Instance.RemoveListener(MsgType.Win_Show,OnShowWindow);
        MessageCenter.Instance.RemoveListener(MsgType.Win_Finish, OnAffirm);
    }

    private void OnShowWindow(Message _msg)
    {
        if (!m_bShow)
        {
            string strType = _msg["type"] as string;
            string strTitle = _msg["title"] as string;
            object data = _msg["data"];
            Action cb = _msg["cb"] as Action;
            Action fCb = _msg["fcb"] as Action;
            string strBtn1 = _msg["btn1"] as string;
            string strBtn2 = _msg["btn2"] as string;
            UIManager.Instance.OpenUI(UIType.AlertWindow, true, strType, strTitle, data, cb, fCb,strBtn1,strBtn2);
            m_bShow = true;
        }
        else
        {
            m_qShowWin.Enqueue(_msg);
        }
    }

    private void MsgLoop()
    {
        if (m_qShowWin.Count > 0)
        {
            Message msg = m_qShowWin.Dequeue() as Message;
            Message newMsg = new Message(MsgType.Win_Refresh, msg);
            newMsg.Send();
        }
        else
        {
            UIManager.Instance.CloseUI(UIType.AlertWindow);
            m_bShow = false;
        }
    }

    private void OnAffirm(Message _msg)
    {
        MsgLoop();
    }
}

/// <summary>
/// 提示框Item
/// </summary>
public class AlertInfo
{
    public int m_nId;
    public string m_strInfo;
    public Action<AlertInfo> m_cb;
}
