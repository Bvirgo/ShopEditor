using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using UnityEngine.UI;
using System;

public class AlertWindowView : BaseUI {
    #region UI & Member
    [HideInInspector,AutoUGUI]
    public Button btn_close;
    [HideInInspector, AutoUGUI]
    public Text txt_title;
    [HideInInspector, AutoUGUI]
    public Button btn_cancel;
    [HideInInspector, AutoUGUI]
    public Button btn_ok;
    [HideInInspector, AutoUGUI]
    public RectTransform grid;
    [HideInInspector, AutoUGUI]
    public Text txt_info;
    [HideInInspector,AutoUGUI]
    public ScrollRect sc_list;

    private string m_strTitle;
    private string m_strType;
    private List<AlertInfo> m_pAlertItem;
    private string m_strInfo;
    private GameObject m_objItemPrefab;
    private Action m_okCb;
    private Action m_cancelCb;
    #endregion
    public override UIType GetUIType()
    {
        return UIType.AlertWindow;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        UnRegister();
    }

    protected override void OnStart()
    {
        base.OnStart();

        RegisterMessage();

        InitData();

        ShowInfo();
    }

    private void InitData()
    {
        m_objItemPrefab = ResManager.Instance.Load("Prefabs/UI/InfoItem") as GameObject;

        btn_cancel.onClick.AddListener(() => {
            Close();
            if (m_cancelCb != null)
            {
                m_cancelCb();
            }
        });

        btn_close.onClick.AddListener(() =>
        {
            Close();
        });

        btn_ok.onClick.AddListener(() => {
            if (m_okCb != null)
            {
                m_okCb();
                Close();
            }
        });

        grid.gameObject.SetActive(false);
        txt_info.gameObject.SetActive(false);

        m_strType = this.uiParams[0] as string;
        m_strTitle = this.uiParams[1] as string;
        txt_title.text = m_strTitle;

        if (m_strType.Equals(Defines.AlertType_List))
        {
            m_pAlertItem = this.uiParams[2] as List<AlertInfo>;
        }
        else
        {
            m_strInfo = this.uiParams[2] as string;
        }

        m_okCb = this.uiParams[3] as Action;

        m_cancelCb = this.uiParams[4] as Action;
        if (m_okCb == null)
        {
            m_okCb = Close;
        }

        string strBtn1 = this.uiParams[5] as string;
        string strBtn2 = this.uiParams[6] as string;

        btn_ok.GetComponentInChildren<Text>().text = strBtn1;
        btn_cancel.GetComponentInChildren<Text>().text = strBtn2;
    }

    private void RegisterMessage()
    {
        MessageCenter.Instance.AddListener(MsgType.Win_Refresh,RefreshInfo);
    }

    private void UnRegister()
    {
        MessageCenter.Instance.RemoveListener(MsgType.Win_Refresh, RefreshInfo);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="_msg"></param>
    private void RefreshInfo(Message _msg)
    {

        m_strType = _msg["type"] as string;
        m_strTitle = _msg["title"] as string;
        txt_title.text = m_strTitle;

        if (m_strType.Equals(Defines.AlertType_List))
        {
            m_pAlertItem = _msg["data"] as List<AlertInfo>;
        }
        else
        {
            m_strInfo = _msg["data"] as string;
        }

        m_okCb = _msg["cb"] as Action;

        m_cancelCb = _msg["fcb"] as Action;

        if (m_okCb == null)
        {
            m_okCb = Close;
        }

        ShowInfo();
    }

    /// <summary>
    /// 显示信息
    /// </summary>
    private void ShowInfo()
    {
        if (m_strType.Equals(Defines.AlertType_Single))
        {
            txt_info.text = m_strInfo;
            txt_info.gameObject.SetActive(true);
            sc_list.content = txt_info.rectTransform;

            int nCount = m_strInfo.Length / 36;
            nCount = nCount > 0 ? nCount : 1;
            
            int nLength = 20 * nCount;

            RectTransform rtf = txt_info.GetComponent<RectTransform>();
            rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, nLength);
        }
        else
        {
            txt_info.gameObject.SetActive(false);
            grid.gameObject.SetActive(true);
            sc_list.content = grid;
            int nLength = 40 * m_pAlertItem.Count;

            RectTransform rtf = grid.GetComponent<RectTransform>();
            rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, nLength);

            for (int i = 0; i < m_pAlertItem.Count; i++)
            {
                AlertInfo aif = m_pAlertItem[i];
                GameObject objItem = GameObject.Instantiate(m_objItemPrefab);
                Button btn = objItem.GetComponent<Button>();
                Text txt = objItem.GetComponent<Text>();
                txt.text = aif.m_strInfo;
                btn.onClick.AddListener(()=> {
                    aif.m_cb(aif);
                });
            }
        }
    }

    private void Close()
    {
        //UIManager.Instance.CloseUI(UIType.AlertWindow);
        Message msg = new Message(MsgType.Win_Finish,this);
        msg.Send();
    }
}
