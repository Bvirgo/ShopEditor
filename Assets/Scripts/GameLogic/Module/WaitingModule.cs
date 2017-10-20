using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
public class WaitingModule : BaseModule {

    private bool m_bShowWaiting;
    private Queue<int> m_qMsg;
    private int m_curMsgKey;
    private Dictionary<int, WaitingMsgInfo> key_msgs;
    private int m_nTotal;
    private int m_nCurrent;
    private class WaitingMsgInfo
    {
        public Message msg;
        public int nTotal;
        public int nCurrent;
        public bool bFinished;

        public WaitingMsgInfo(Message _msg)
        {
            msg = _msg;
            nTotal = 1;
            nCurrent = 0;
            bFinished = false;
        }
    }

    public WaitingModule()
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
        m_bShowWaiting = false;
        m_qMsg = new Queue<int>();
        key_msgs = new Dictionary<int, WaitingMsgInfo>();

        m_nTotal = 1;
        m_nCurrent = 0;
    }

    private void RegisterMessage()
    {
        MessageCenter.Instance.AddListener(MsgType.WV_PopWaiting, PopWaiting);
        MessageCenter.Instance.AddListener(MsgType.WV_PushWaiting, PushWaiting);
        MessageCenter.Instance.AddListener(MsgType.WV_ShowWaiting, ShowWaiting);
        MessageCenter.Instance.AddListener(MsgType.WV_HideWaiting, Hide);
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        MessageCenter.Instance.RemoveListener(MsgType.WV_ShowWaiting, ShowWaiting);
        MessageCenter.Instance.RemoveListener(MsgType.WV_HideWaiting, Hide);
        MessageCenter.Instance.RemoveListener(MsgType.WV_PopWaiting, PopWaiting);
        MessageCenter.Instance.RemoveListener(MsgType.WV_PushWaiting, PushWaiting);
    }

    private void SetMax(Message _msg)
    {
        if (_msg.IsKey("id") && _msg.IsKey("t"))
        {
            int nId = (int)_msg["id"];
            int nTotal = (int)_msg["t"];

            if (!key_msgs.ContainsKey(nId))
            {
                return;
            }
            WaitingMsgInfo mInfo = key_msgs[nId];
            mInfo.nTotal = nTotal;

            key_msgs[nId] = mInfo;
            m_nCurrent = mInfo.nCurrent;
        }

    }

    /// <summary>
    /// 当前进度+1
    /// </summary>
    /// <param name="_msg"></param>
    private void PopWaiting(Message _msg)
    {
        if (_msg.IsKey("id"))
        {
            int nId = (int)_msg["id"];
            if (!key_msgs.ContainsKey(nId))
            {
                return;
            }
            WaitingMsgInfo mInfo = key_msgs[nId];

            m_nCurrent = mInfo.nCurrent;

            mInfo.nCurrent = ++m_nCurrent;

            if (mInfo.nCurrent == mInfo.nTotal)
            {
                mInfo.bFinished = true;
            }

            key_msgs[nId] = mInfo;
            m_nTotal = mInfo.nTotal;

            if (nId == m_curMsgKey)
            {
                if (m_nCurrent >= m_nTotal)
                {
                    LoadWaitingView();
                }
                else
                {
                    Message msg = new Message(MsgType.WV_UpdateWaiting, this);
                    msg["t"] = m_nTotal;
                    msg["c"] = m_nCurrent;
                    msg.Send();
                }
            }
        }
   
    }

    /// <summary>
    /// 进度总数+1
    /// </summary>
    /// <param name="_msg"></param>
    private void PushWaiting(Message _msg)
    {
        if (_msg.IsKey("id"))
        {
            int nId = (int)_msg["id"];

            if (!key_msgs.ContainsKey(nId))
            {
                return;
            }
            WaitingMsgInfo mInfo = key_msgs[nId];
            m_nTotal = mInfo.nTotal;
            mInfo.nTotal = ++m_nTotal;

            key_msgs[nId] = mInfo;
            m_nCurrent = mInfo.nCurrent;

            if (nId == m_curMsgKey)
            {
                if (m_nCurrent >= m_nTotal)
                {
                    LoadWaitingView();
                }
                else
                {
                    Message msg = new Message(MsgType.WV_UpdateWaiting, this);
                    msg["t"] = m_nTotal;
                    msg["c"] = m_nCurrent;
                    msg.Send();
                }
            }
        }

    }

    /// <summary>
    /// 显示等待界面请求
    /// </summary>
    /// <param name="_msg"></param>
    private void ShowWaiting(Message _msg)
    {
        if (!_msg.IsKey("id"))
        {
            return;
        }

        int nId = _msg["id"] != null ? (int)_msg["id"] : 0;
        WaitingMsgInfo mInfo = new WaitingMsgInfo(_msg);

        key_msgs.AddOrReplace(nId, mInfo);

        SetMax(_msg);

        m_qMsg.Enqueue(nId);

        if (!m_bShowWaiting)
        {
            UIManager.Instance.OpenUI(UIType.Waiting, false,_msg);
            m_curMsgKey = nId;

            LoadWaitingView();
        }
        m_bShowWaiting = true;
    }

    /// <summary>
    /// 等待消息队列
    /// </summary>
    private void LoadWaitingView()
    {
        if (m_qMsg.Count > 0)
        {
            int nKey = (int)m_qMsg.Dequeue();
            WaitingMsgInfo mInfo = key_msgs[nKey];
            if (mInfo.bFinished)
            {
                LoadWaitingView();
            }
            else
            {
                Message msg = mInfo.msg;
                m_curMsgKey = nKey;
                Message newMsg = new Message(MsgType.WV_NewWaiting, msg);
                newMsg.Send();
            }
        }
        else
        {
            UIManager.Instance.CloseUI(UIType.Waiting);
            m_bShowWaiting = false;
        }
    }

    /// <summary>
    /// 关闭等待界面
    /// </summary>
    /// <param name="_msg"></param>
    private void Hide(Message _msg)
    {
        if (m_bShowWaiting)
        {
            InitData();
            LoadWaitingView();
        }
    }
}
