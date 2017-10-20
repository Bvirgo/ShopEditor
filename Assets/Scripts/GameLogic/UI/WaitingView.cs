using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using UnityEngine.UI;
using System;

public class WaitingView : BaseUI {
    #region UI & Member
    [HideInInspector,AutoUGUI]
    public Slider sld_percent;
    [HideInInspector, AutoUGUI]
    public Text txt_percent;
    private Transform m_waitingImg;

    private bool m_bClock;
    private string m_strTips;
    private int m_nTotal;
    #endregion
    public override UIType GetUIType()
    {
        return UIType.Login;
    }
    
    protected override void OnStart()
    {
        base.OnStart();
        m_waitingImg = transform.Find("Panel/img_waiting");
        m_waitingImg.gameObject.SetActive(false);
        sld_percent.gameObject.SetActive(false);
        m_bClock = false;
        m_strTips = "加载进度";
        m_nTotal = -1;

        Message msg = uiParams[0] as Message;
        m_strTips = msg["tips"].ToString();
        string strType = msg["type"].ToString();
        m_nTotal = (int)msg["t"];
        Init(m_strTips,strType);

        MessageCenter.Instance.AddListener(MsgType.WV_UpdateWaiting, OnUpdate);
        MessageCenter.Instance.AddListener(MsgType.WV_NewWaiting, OnShowWaiting);
    }
    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if (m_bClock)
        {
            m_waitingImg.Rotate(new Vector3(0, 0, -100 * Time.deltaTime));
        }
    }
    
    protected override void OnRelease()
    {
        base.OnRelease();

        MessageCenter.Instance.RemoveListener(MsgType.WV_UpdateWaiting, OnUpdate);
        MessageCenter.Instance.RemoveListener(MsgType.WV_NewWaiting, OnShowWaiting);
    }

    private void Init(string _strTips,string _strType)
    {
        txt_percent.text = _strTips;
        m_bClock = _strType.Equals(Defines.WaitingType_Clock);
        m_waitingImg.gameObject.SetActive(m_bClock);
        sld_percent.gameObject.SetActive(!m_bClock);
    }

    private void OnShowWaiting(Message _msg)
    {
        string strType = _msg["type"].ToString();
        m_strTips = _msg["tips"].ToString();
        txt_percent.text = m_strTips;

        m_bClock = strType.Equals(Defines.WaitingType_Clock);
        m_waitingImg.gameObject.SetActive(m_bClock);
        sld_percent.gameObject.SetActive(!m_bClock);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="_msg"></param>
    private void OnUpdate(Message _msg)
    {
        if (!m_bClock)
        {
            m_nTotal = (int)_msg["t"];
            int nCurrent = (int)_msg["c"];
            nCurrent = nCurrent > 0 ? nCurrent : -999;

            if (m_nTotal <= nCurrent 
                || m_nTotal == 0 )
            {
                UIManager.Instance.CloseUI(UIType.Waiting);
                return;
            }

            float fPercent = (float)nCurrent / (float)m_nTotal;
            sld_percent.value = fPercent;
            txt_percent.text = string.Format("{0}：{1}/{2}", m_strTips, nCurrent, m_nTotal);
        }
    }
}
