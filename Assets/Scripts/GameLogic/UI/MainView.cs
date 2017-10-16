using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFrameWork;
using UnityEngine.UI;
using System;

public class MainView : BaseUI {
    #region UI
    [HideInInspector,AutoUGUI]
    public Button btn_load;
    [HideInInspector, AutoUGUI]
    public Button btn_save;
    [HideInInspector, AutoUGUI]
    public Button btn_mutiReplace;
    [HideInInspector, AutoUGUI]
    public ScrollRect list_tags;
    [HideInInspector, AutoUGUI]
    public ScrollRect list_coms;
    [HideInInspector, AutoUGUI]
    public Transform tagGrid;
    [HideInInspector, AutoUGUI]
    public Transform comGrid;
    [HideInInspector, AutoUGUI]
    public Text txt_id;
    [HideInInspector, AutoUGUI]
    public Text txt_imgName;
    [HideInInspector, AutoUGUI]
    public InputField ipt_tag;
    [HideInInspector, AutoUGUI]
    public InputField ipt_comName;
    [HideInInspector, AutoUGUI]
    public Image img_com;
    [HideInInspector, AutoUGUI]
    public Text txt_file;
    [HideInInspector, AutoUGUI]
    public Button btn_new;
    [HideInInspector, AutoUGUI]
    public Button btn_ok;

    private int m_nTag = 105;
    private int m_nComp = 140;
    private GameObject m_objComPrefabs;
    private GameObject m_objTagPrefabs;
    private List<string> m_pTags;
    private List<ComProperty> m_pComProperty;
    private string m_strCurTag;
    private string m_strCurCom;
    #endregion

    #region Base
    public override UIType GetUIType()
    {
        return UIType.CompEditor;
    }

    void Start()
    {
        InitData();

        RegisterMessage();
    }

    private void RegisterMessage()
    {
        MessageCenter.Instance.AddListener(MsgType.MainView_RefreshTag, CreateTagList);
        MessageCenter.Instance.AddListener(MsgType.MainView_RefreshCom, RefreshComList);
    }

    private void InitData()
    {
        
        m_objComPrefabs = ResManager.Instance.Load(UIPathDefines.UI_PREFAB  + "CompItem") as GameObject;
        m_objTagPrefabs = ResManager.Instance.Load(UIPathDefines.UI_PREFAB  + "TagItem") as GameObject;

        btn_load.onClick.AddListener(() =>
        {
            Message msg = new Message(MsgType.MainView_LoadRes, this);
            msg.Send();
        });
        btn_save.onClick.AddListener(() => {
            Message msg = new Message(MsgType.MainView_Save, this);
            msg["tag"] = ipt_tag.text;
            msg["name"] = ipt_comName.text;
            msg.Send();
        });
        btn_mutiReplace.onClick.AddListener(() => {
            Message msg = new Message(MsgType.MainView_ReplaceAll, this);
            msg.Send();
        });
        btn_new.onClick.AddListener(() =>
        {
            Message msg = new Message(MsgType.MainView_NewComp, this);
            msg.Send();
            NewCompUI();
        });

        btn_ok.onClick.AddListener(()=> {

            Message msg = new Message(MsgType.MainView_Affirm, this);
            msg["tag"] = ipt_tag.text;
            msg["name"] = ipt_comName.text;
            msg.Send();
        });

        CleanUI();

    }

    protected override void OnRelease()
    {
        base.OnRelease();
        MessageCenter.Instance.RemoveListener(MsgType.MainView_RefreshTag, CreateTagList);
        MessageCenter.Instance.RemoveListener(MsgType.MainView_RefreshCom, RefreshComList);
    }

    #endregion

    #region Actions

    /// <summary>
    /// 刷新分类
    /// </summary>
    /// <param name="_msg"></param>
    private void CreateTagList(Message _msg)
    {
        m_pTags = _msg["tags"] as List<string>;
        int nLength = m_nTag * m_pTags.Count;

        Utils.RemoveChildren(tagGrid);
        Utils.RemoveChildren(comGrid);
        CleanUI();

        RectTransform rtf = tagGrid.GetComponent<RectTransform>();
        rtf.sizeDelta =new Vector2(nLength,rtf.sizeDelta.y);

        for (int i = 0; i < m_pTags.Count; i++)
        {
            GameObject tagItem = GameObject.Instantiate(m_objTagPrefabs);
            string strTag = m_pTags[i];
            Button btn = tagItem.GetComponent<Button>();
            btn.onClick.AddListener(()=> {
                //Debug.Log(string.Format("点击Tag：{0},当前Tag：{1}",strTag,m_strCurTag));
                if (strTag.Equals(m_strCurTag))
                {
                    return;
                }
                m_strCurTag = strTag;
                Message msg = new Message(MsgType.MainView_TagItemClick, this);
                msg["tag"] = strTag;
                msg.Send();
            });
            btn.Select();
            Text txt = tagItem.GetComponentInChildren<Text>();
            txt.text = strTag;

            tagItem.transform.SetParent(tagGrid);
        }
    }

    /// <summary>
    /// 刷新组件
    /// </summary>
    /// <param name="_msg"></param>
    private void RefreshComList(Message _msg)
    {
        CleanUI();

        m_pComProperty = _msg["coms"] as List<ComProperty>;

        int nLength = m_nComp * m_pComProperty.Count;

        Utils.RemoveChildren(comGrid);

        RectTransform rtf = comGrid.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(nLength, rtf.sizeDelta.y);

        for (int i = 0; i < m_pComProperty.Count; i++)
        {
            GameObject comItem = GameObject.Instantiate(m_objComPrefabs);
            ComProperty cpp = m_pComProperty[i];
            Button btn = comItem.GetComponent<Button>();
            Image img = comItem.GetComponent<Image>();
            btn.onClick.AddListener(() => {
                if (m_strCurCom.Equals(cpp.m_strCode))
                {
                    return;
                }
                m_strCurCom = cpp.m_strCode;

                ComItemClick(cpp);

                Message msg = new Message(MsgType.MainView_ComItemClick, this);
                msg["com"] = cpp;
                msg.Send();
            });
            string strTips = string.Format("Code:{0}", cpp.m_strCode);
            Text txt = comItem.GetComponentInChildren<Text>();
            txt.text = strTips;
            ResManager.Instance.OnLoadServerTexure(cpp.m_strImg, cpp.m_strImgCrc, (_tx) => {
                if (img != null)
                {
                    img.sprite = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), Vector2.zero);
                }
            });
            comItem.transform.SetParent(comGrid);
        }
    }

    /// <summary>
    /// 选择组件
    /// </summary>
    /// <param name="_cpp"></param>
    private void ComItemClick(ComProperty _cpp)
    {
        txt_id.text = _cpp.m_strCode;
        ipt_comName.text = _cpp.m_strShowName;
        ipt_tag.text = _cpp.m_strTag;
        txt_imgName.text = _cpp.m_strImg;
        txt_file.text = _cpp.m_strModelName;
        ResManager.Instance.OnLoadServerTexure(_cpp.m_strImg, _cpp.m_strImgCrc, (_tx) => {
            img_com.sprite = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), Vector2.zero);
        });
    }

    /// <summary>
    /// 新组件
    /// </summary>
    private void NewCompUI()
    {
        CleanUI();

        ipt_comName.text = "新组件";
        ipt_tag.text = "NewTest";
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void CleanUI()
    {
        txt_id.text = "";
        ipt_comName.text = "";
        ipt_tag.text = "";
        txt_imgName.text = "";
        txt_file.text = "";
        m_strCurCom = "";
        m_strCurTag = "";
        img_com.sprite = null;
    }

    /// <summary>
    /// 导入新缩率图
    /// </summary>
    /// <param name="_tx"></param>
    public void OnRefreshImg(Texture2D _tx,string _strImgName)
    {
        img_com.sprite = Sprite.Create(_tx, new Rect(0, 0, _tx.width, _tx.height), Vector2.zero);
        txt_imgName.text = _strImgName;
    }

    /// <summary>
    /// 导入模型
    /// </summary>
    /// <param name="_strModelName"></param>
    public void OnRefreshModel(string _strModelName,bool _isNewCom = false)
    {
        if (_isNewCom)
        {
            txt_id.text = Utils.GetFilePrefix(_strModelName);
        }
        txt_file.text = _strModelName;
    }
    #endregion

}
