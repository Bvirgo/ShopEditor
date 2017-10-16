using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFrameWork;
using System;
using UnityEngine.UI;
public class ShopView : BaseUI
{
    #region UI
    [HideInInspector,AutoUGUI]
    public Button btn_save;
    [HideInInspector, AutoUGUI]
    public Button btn_localSave;
    [HideInInspector, AutoUGUI]
    public Button btn_load;
    [HideInInspector, AutoUGUI]
    public Button btn_cancelPoint;
    [HideInInspector, AutoUGUI]
    public Button btn_onlyShop;
    [HideInInspector, AutoUGUI]
    public Button btn_onlyBoard;
    [HideInInspector, AutoUGUI]
    public Button btn_shopAndBoard;

    [HideInInspector, AutoUGUI]
    public Transform shopsGrid;
    [HideInInspector, AutoUGUI]
    public Transform boardGrid;
    [HideInInspector, AutoUGUI]
    public Transform sampleBoardGrid;

    private int m_nShopGrid;
    private int m_nBoardGrid;
    private int m_nSampleBoard;
    
    private string m_strSampleBoardItem;
    private string m_strShopItem;

    enum RefreshType
    {
        BoardList,
        SampleBoardList
    }
    #endregion

    #region I & R
    public override UIType GetUIType()
    {
        return UIType.CompEditor;
    }

    protected override void OnStart()
    {
        base.OnStart();

        InitData();

        RegisterMsg();

    }

    protected override void OnRelease()
    {
        base.OnRelease();
        UnRegister();
    }

    private void InitData()
    {
        m_nShopGrid = 83;
        m_nBoardGrid = 155;
        m_nSampleBoard = 155;

        m_strSampleBoardItem = "SampleBoardItem";
        m_strShopItem = "ShopItem";

        //m_objSampleBoard = ResManager.Instance.Load(UIPathDefines.UI_PREFAB + "SampleBoardItem") as GameObject;
        //m_objShopItem = ResManager.Instance.Load(UIPathDefines.UI_PREFAB + "ShopItem") as GameObject;

        //  UI Pool
        UIPoolManager.Instance.PushPrefab(m_strSampleBoardItem);
        UIPoolManager.Instance.PushPrefab(m_strShopItem);

        btn_load.onClick.AddListener(()=> 
        {
            Message msg = new Message(MsgType.ShopView_LoadRoute,this);
            // 测试用例
            msg["route"] = "03941001001";
            msg.Send();
        });

        btn_localSave.onClick.AddListener(()=> 
        {
            Message msg = new Message(MsgType.ShopView_LocalSave,this);
            msg.Send();
        });

        btn_cancelPoint.onClick.AddListener(()=> 
        {
            Message msg = new Message(MsgType.ShopView_CancelPoint,this);
            msg.Send();
        });

        btn_onlyBoard.onClick.AddListener(()=> 
        {
            Message msg = new Message(MsgType.ShopView_OnlyBoard,this);
            msg.Send();
        });

        btn_onlyShop.onClick.AddListener(() =>
        {
            Message msg = new Message(MsgType.ShopView_OnlyShop, this);
            msg.Send();
        });

        btn_shopAndBoard.onClick.AddListener(() =>
        {
            Message msg = new Message(MsgType.ShopView_ShopAndBoard, this);
            msg.Send();
        });
    }

    private void RegisterMsg()
    {
        MessageCenter.Instance.AddListener(MsgType.ShopView_RefreshBoardList, RefreshBoardList);
        MessageCenter.Instance.AddListener(MsgType.ShopView_RefreshSampleBoard, RefreshSampleBoardList);
        MessageCenter.Instance.AddListener(MsgType.ShopView_RefreshShopList, RefreshShopList);
    }

    private void UnRegister()
    {
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_RefreshBoardList, RefreshBoardList);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_RefreshSampleBoard, RefreshSampleBoardList);
        MessageCenter.Instance.RemoveListener(MsgType.ShopView_RefreshShopList, RefreshShopList);
    }
    #endregion

    #region Shop & Board 

    /// <summary>
    /// 右侧招牌模版列表
    /// </summary>
    /// <param name="_msg"></param>
    private void RefreshSampleBoardList(Message _msg)
    {
        List<ShopSignVO> pSsv = _msg["data"] as List<ShopSignVO>;
        if (pSsv != null)
        {
            MyFrameWork.MonoHelper.Instance.StartCoroutine(RefreshBoardItemList(pSsv, RefreshType.SampleBoardList));
        }
    }

    /// <summary>
    /// 底部招牌列表
    /// </summary>
    /// <param name="_msg"></param>
    private void RefreshBoardList(Message _msg)
    {
        List<ShopSignVO> pSsv = _msg["data"] as List<ShopSignVO>;
        if (pSsv != null)
        {
            MyFrameWork.MonoHelper.Instance.StartCoroutine(RefreshBoardItemList(pSsv, RefreshType.BoardList));
        }
        else
        {
            Utils.RemoveChildren(boardGrid);
        }
    }

    /// <summary>
    /// 创建招牌列表
    /// </summary>
    /// <param name="_pSss"></param>
    /// <param name="_objPrefab"></param>
    /// <param name="_tfGrid"></param>
    /// <param name="m_nGridHeight"></param>
    /// <returns></returns>
    private IEnumerator RefreshBoardItemList(List<ShopSignVO> _pSss,RefreshType _rType)
    {
        int nGridHeight = _rType == RefreshType.BoardList ? m_nBoardGrid : m_nSampleBoard;
        Transform tfGrid = _rType == RefreshType.BoardList ? boardGrid : sampleBoardGrid;

        int nLenth = nGridHeight * _pSss.Count;
        UIPoolManager.Instance.DeSpawnAll(tfGrid);

        if (_rType == RefreshType.SampleBoardList)
        {
            Utils.ResetRectTransform(tfGrid, -1, nLenth);
        }
        else
        {
            Utils.ResetRectTransform(tfGrid, nLenth,-1);
        }
        ToggleGroup tgg = tfGrid.GetComponent<ToggleGroup>();
        Utils.ResetScrollSensitivity(tfGrid.parent, nLenth);

        for (int i = 0; i < _pSss.Count; i++)
        {
            ShopSignVO ssv = _pSss[i];
            //Transform tf = GameObject.Instantiate(objPrefab).transform;
            Transform tf = UIPoolManager.Instance.OnGetItem(m_strSampleBoardItem);

            Image img_top = tf.Find("img_top").GetComponent<Image>();
            Image img_center = tf.Find("img_center").GetComponent<Image>();

            Text txt_index = tf.Find("txt_index").GetComponent<Text>();
            Text txt_shopCode = tf.Find("txt_shopCode").GetComponent<Text>();
            Text txt_prefabType = tf.Find("txt_prefabType").GetComponent<Text>();
            Text txt_shopId = tf.Find("txt_shopId").GetComponent<Text>();
            Text txt_cellNum = tf.Find("txt_cellNum").GetComponent<Text>();

            InputField ipt_shopName = tf.Find("ipt_shopName").GetComponent<InputField>();
            Toggle tg = tf.GetComponent<Toggle>();
            tg.group = tgg;

            Button btn_close = tf.Find("btn_close").GetComponent<Button>();
            btn_close.gameObject.SetActive(false);
            if (_rType == RefreshType.BoardList)
            {
                btn_close.gameObject.SetActive(true);
                btn_close.onClick.AddListener(()=> 
                {
                    Message msg = new Message(MsgType.ShopView_DeleteBoard, this);
                    msg["data"] = ssv;
                    msg.Send();
                });
            }

            ssv.GetTexture("Sign", (tx) => {
                img_top.sprite = Utils.GetSprite(tx);
            });

            ssv.GetTexture("Shop", (tx) =>
            {
                img_center.sprite = Utils.GetSprite(tx);
            });

            ipt_shopName.text = ssv.ShopName;
            if (_rType == RefreshType.SampleBoardList)
            {
                ipt_shopName.enabled = false;
                ipt_shopName.interactable = false;

                tg.onValueChanged.AddListener(bGo =>
                {
                    if (bGo)
                    {
                        Message msg = new Message(MsgType.ShopView_SampleBoardClicked, this);
                        msg["data"] = ssv;
                        msg.Send();
                        Debug.Log("Click SampleBoard:" + ssv.GetText("ShopCode"));
                    }
                });
            }
            txt_index.text = i.ToString();
            txt_shopCode.text = ssv.GetText("ShopCode");
            txt_prefabType.text = GetPrefabType(ssv.PrefabType);
            txt_shopId.text = ssv.ShopID;
            txt_cellNum.text = ssv.CellNum.ToString() + "间";
            tf.SetParent(tfGrid);

            yield return new WaitForEndOfFrame();
        }
    }

    private string GetPrefabType(string paramName)
    {
        string val = paramName;
  
        switch (paramName)
        {
            case "ShopSign":
                val = "店铺+招牌";
                break;
            case "OnlyShop":
                val = "只有店铺";
                break;
            case "OnlySign":
                val = "只有招牌";
                break;
        }

        return val;
    }
  

    /// <summary>
    /// 创建商店列表
    /// </summary>
    /// <param name="_msg"></param>
    private void RefreshShopList(Message _msg)
    {
        List<Message> pMsg = _msg["data"] as List<Message>;
        if (pMsg != null)
        {
            int nLenth = m_nShopGrid * pMsg.Count;

            UIPoolManager.Instance.DeSpawnAll(shopsGrid);
            Utils.ResetRectTransform(shopsGrid, -1, nLenth);
            ToggleGroup tgg = shopsGrid.GetComponent<ToggleGroup>();
            Utils.ResetScrollSensitivity(shopsGrid.parent, nLenth);

            for (int i = 0; i < pMsg.Count; i++)
            {
                Message smm = pMsg[i];
                //Transform tfShopItem = GameObject.Instantiate(m_objShopItem).transform;
                Transform tfShopItem = UIPoolManager.Instance.OnGetItem(m_strShopItem);
                Text txt_index = tfShopItem.Find("txt_index").GetComponent<Text>();
                Text txt_info = tfShopItem.Find("txt_info").GetComponent<Text>();
                Button btn_close = tfShopItem.Find("btn_close").GetComponent<Button>();
                btn_close.onClick.AddListener(() =>
                {
                    Message msg = new Message(MsgType.ShopView_DeleteShop, this);
                    msg["guid"] = smm["guid"] as string;
                    msg.Send();
                });

                txt_index.text = i.ToString();
                txt_info.text = smm["info"] as string;
                tfShopItem.SetParent(shopsGrid);

                Toggle tg = tfShopItem.GetComponent<Toggle>();
                tg.onValueChanged.AddListener((bGo) =>
                {
                    if (bGo)
                    {
                        Message msg = new Message(MsgType.ShopView_ShopItemClicked, this);
                        msg["guid"] = smm["guid"] as string;
                        msg.Send();
                        Debug.Log("Click ShopItem:" + txt_info.text);
                    }
                });

                if (0 == i)
                {
                    Message msg = new Message(MsgType.ShopView_ShopItemClicked, this);
                    msg["guid"] = smm["guid"] as string;
                    msg.Send();
                    tg.isOn = true;
                }
                tg.group = tgg;
            }
        }
    }

    #endregion
}
