using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using System.Drawing;
using System.IO;
using MyFrameWork;
using System.Text;

public class ShopModelMono : MonoBehaviour, IJsonData, IListData
{
    public string BuildingName;
    public List<Vector3> PointList;
    public List<ShopSignVO> ShopSignList;
    private ShopModule m_sm;

    /// <summary>
    /// 来个唯一编号
    /// </summary>
    private string m_strGUID;
    public string guid
    {
        private set { m_strGUID = value; }

        get { return m_strGUID; }
    }

    #region 计算用值 不保存
    public Vector3 worldCenter;//镜头焦点跳转
    public Vector3 worldNormal;//镜头焦点跳转
    float shopHeight;
    float shopTotalWidth;
    #endregion

    #region 面片transform

    void Awake()
    {
        m_sm = ModuleManager.Instance.Get<ShopModule>();
        if (string.IsNullOrEmpty(m_strGUID))
        {
            guid = Guid.NewGuid().ToString();
        }
    }

    public void Scale(Vector3 dir)
    {
        Vector3 moveDelta;
        if (dir == Vector3.up || dir == Vector3.down)
        {
            moveDelta = dir.GetScaledVector(0.1f);
            PointList[0] = PointList[0] + moveDelta;
            PointList[1] = PointList[1] - moveDelta;
            PointList[2] = PointList[2] - moveDelta;
            PointList[3] = PointList[3] + moveDelta;
        }
        else
        {
            float rotH = JHQCHelper.Instance.TheCameraCtrl.TheFocusCtrl.RotH / 180f * Mathf.PI;
            moveDelta = Utils.MoveTowards(rotH, dir);
            moveDelta = moveDelta.GetScaledVector(0.1f);
            PointList[0] = PointList[0] + moveDelta;
            PointList[1] = PointList[1] + moveDelta;
            PointList[2] = PointList[2] - moveDelta;
            PointList[3] = PointList[3] - moveDelta;
        }
        CreatePlane(PointList);
        CreateShops();
    }

    public void Move(Vector3 dir)
    {
        //Debug.Log("move");
        //transform.position += dir;
        Vector3 moveDelta;
        if (dir == Vector3.up || dir == Vector3.down)
        {
            moveDelta = dir.GetScaledVector(0.1f);
        }
        else
        {
            float rotH = JHQCHelper.Instance.TheCameraCtrl.TheFocusCtrl.RotH / 180f * Mathf.PI;
            moveDelta = Utils.MoveTowards(rotH, dir);
            moveDelta = moveDelta.GetScaledVector(0.1f);
        }
        for (int i = 0; i < PointList.Count; i++)
        {
            PointList[i] = PointList[i] + moveDelta;
        }
        CreatePlane(PointList);
        CreateShops();
    }

    public void Rotate(Vector3 dir)
    {
        float angle = dir == Vector3.up ? 1 : -1;
        List<GameObject> pointGOList = new List<GameObject>();
        GameObject centerGO = new GameObject();
        centerGO.transform.position = worldCenter;
        Vector3 rotateCenter = Vector3.zero;
        for (int i = 0; i < PointList.Count; i++)
        {
            GameObject pointGO = new GameObject();
            pointGO.transform.position = PointList[i];
            pointGO.transform.parent = centerGO.transform;
            pointGOList.Add(pointGO);
            rotateCenter += PointList[i];
        }

        centerGO.transform.Rotate(new Vector3(0, angle, 0), Space.Self);
        for (int i = 0; i < PointList.Count; i++)
        {
            GameObject pointGO = pointGOList[i];
            PointList[i] = pointGO.transform.position;
        }
        CreatePlane(PointList);

        Destroy(centerGO);
        Destroy(pointGOList[0]);
        Destroy(pointGOList[1]);
        Destroy(pointGOList[2]);
        Destroy(pointGOList[3]);
    }

    #endregion

    /// <summary>记录点, 并计算中间数据</summary>
    public void CreatePlane(List<Vector3> _worldPointList)
    {
        //mf = _mf;
        float _height = _worldPointList[1].y - _worldPointList[0].y;
        shopHeight = _height;
        Vector3 heightVec = new Vector3(0, _height, 0);
        PointList = new List<Vector3>();
        PointList.Add(_worldPointList[1] - heightVec);
        PointList.Add(_worldPointList[1]);
        PointList.Add(_worldPointList[2]);
        PointList.Add(_worldPointList[2] - heightVec);

        Vector3 oa = PointList[1] - PointList[0];
        Vector3 ob = PointList[2] - PointList[0];
        worldNormal = Vector3.Cross(oa, ob);
        worldNormal = worldNormal.normalized;

        worldCenter = (_worldPointList[2] + _worldPointList[1]).GetScaledVector(0.5f) - heightVec;
        shopTotalWidth = Vector3.Distance(PointList[0], PointList[3]);
    }

    /// <summary>一件商铺的模型的宽度 创建时赋予</summary>
    public float CellWidth;

    public void SetVisible(bool isVisible)
    {
        if (isVisible)
        {
            CreateShops(false);
        }
        gameObject.SetActive(isVisible);
    }

    public void DestroyShops()
    {
        for (int i = 0, length = transform.childCount; i < length; i++)
        {

            Material[] matArr = transform.GetChild(i).GetComponentInChildren<Renderer>().materials;
            for (int matIndex = 0, matNum = matArr.Length; matIndex < matNum; matIndex++)
            {
                Destroy(matArr[matIndex]);
            }
            Destroy(transform.GetChild(i).gameObject);
        }
        Model2ShopSignDic.Clear();
    }

    /// <summary>记录模型与shopSignVO的对应</summary>
    public Dictionary<GameObject, ShopSignVO> Model2ShopSignDic = new Dictionary<GameObject, ShopSignVO>();

    public ShopSignVO TryGetShopSignVO(GameObject tar)
    {
        if (tar.transform.parent == null)
            return null;

        ShopSignVO ssv;
        if (Model2ShopSignDic.TryGetValue(tar, out ssv))
        {
            Debug.Log("ShopSignVO ssv: " + ssv.ToJsonData().ToJson());
        }

        if (ssv == null)
            return TryGetShopSignVO(tar.transform.parent.gameObject);
        else
            return ssv;
    }

    bool _isShopCreated = false;
    /// <summary>创建商铺模型, 并加载招牌/铺面贴图</summary>
    public void CreateShops(bool isForceCreate = true)
    {
        if (!isForceCreate)
        {
            if (_isShopCreated)
            {
                return;
            }
        }
        //Debug.Log("CreateShops");
        DestroyShops();
        Model2ShopSignDic.Clear();
        //Resources.UnloadUnusedAssets();

        if (ShopSignList == null)
            ShopSignList = new List<ShopSignVO>();

        float curCellWidth = 0;
        if (ShopSignList.Count != 0)
        {
            for (int iSS = 0, lenSS = ShopSignList.Count; iSS < lenSS; iSS++)
            {
                ShopSignVO ss = ShopSignList[iSS];

                GameObject shop = m_sm.CreateShopModel(ss.CellNum, ss.PrefabType);
                
                Model2ShopSignDic.AddRep(shop, ss);

                shop.transform.parent = transform;
                shop.transform.localEulerAngles = new Vector3(-90, 0, 0);
                shop.transform.localPosition = new Vector3(-curCellWidth, 0, 0);
                shop.transform.localScale = new Vector3(1, 1, shopHeight / 3.35f);

                Utils.ForAllChildren(shop, child =>
                {
                    if (child.GetComponent<MeshFilter>() != null)
                    {
                        child.AddComponent<MeshCollider>();
                    }
                });


                curCellWidth += CellWidth * ss.CellNum;
                Material[] matArr = shop.GetComponentInChildren<Renderer>().materials;
                Material matShop = matArr.FindItem(mat => { return mat.name.Contains("mat_context"); });
                Material matSign = matArr.FindItem(mat => { return mat.name.Contains("mat_zp"); });

                m_sm.GetSignTexByCode(ss.SignCode, ss.RouteID, tex =>
                {
                    if (this != null && tex != null && matSign != null)
                        matSign.mainTexture = tex;
                });
                m_sm.GetShopTexByCode(ss.ShopCode, ss.CellNum, tex =>
                {
                    if (this != null && tex != null && matShop != null)
                        matShop.mainTexture = tex;
                });

            }
        }
        else
        {
            //没有商铺的也要创建一间空白的作为占位显示
            ShopSignVO ss = new ShopSignVO(1);
            ss.PrefabType = ShopSignVO.PrefabTypeShopSign;
            GameObject shop = m_sm.CreateShopModel(ss.CellNum, ss.PrefabType);
            Model2ShopSignDic.AddRep(shop, ss);
            shop.transform.parent = transform;
            shop.transform.localEulerAngles = new Vector3(-90, 0, 0);
            shop.transform.localPosition = new Vector3(-curCellWidth, 0, 0);
            shop.transform.localScale = new Vector3(1, 1, shopHeight / 3.35f);

            Utils.ForAllChildren(shop, child =>
            {
                if (child.GetComponent<MeshFilter>() != null)
                {
                    child.AddComponent<MeshCollider>();
                }
            });
            curCellWidth += CellWidth * ss.CellNum;
            Material[] matArr = shop.GetComponentInChildren<Renderer>().materials;
            Material matShop = matArr.FindItem(mat => { return mat.name.Contains("mat_context"); });
            Material matSign = matArr.FindItem(mat => { return mat.name.Contains("mat_zp"); });
            matSign.mainTexture = Texture2D.whiteTexture;
            matShop.mainTexture = Texture2D.whiteTexture;
        }

        transform.localScale = new Vector3(curCellWidth == 0 ? 1 : shopTotalWidth / curCellWidth, 1, 1);
        transform.eulerAngles = Vector3.up.GetScaledVector(Utils.GetRotateY(worldNormal));
        transform.position = PointList[0] + worldNormal.normalized.GetScaledVector(0.1f);
        transform.parent = m_sm.ShopModelFolder.transform;
        _isShopCreated = true;
    }

    public void setChildVisible(bool flag)
    {
        Utils.ForAllChildren(gameObject, tar => { tar.SetActive(flag); }, false);
    }

    #region JsonData

    public string GetLabelText()
    {
        return BuildingName;
    }

    /// <summary>
    /// 处理招牌图片Crc为空的数据
    /// </summary>
    public void UpdateSignPicCrc()
    {
        for (int i = 0; i < ShopSignList.Count; i++)
        {
            var voSign = ShopSignList[i];
            string strCrc = voSign.SignCrc;
            if (string.IsNullOrEmpty(strCrc) ||
                strCrc.Contains("null"))
            {
                voSign.SignCrc = m_sm.OnGetPicCrcByName(voSign.SignCode);
            }
        }
    }
 
    public JsonData ToJsonData()
    {
        JsonData jd = new JsonData();

        jd["buildName"] = BuildingName;

        // 处理Y值:不是所有的商铺变更的都是Y值
        //DealPointList();

        List<string> dataList = PointList.ConvertAll<string>((vec) => { return JsonUtils.vecToStr(vec); });
        //Debug.LogWarning("SMM To JsonData:PolintList:" + dataList);

        jd["pointList"] = JsonUtils.ToJsonData(dataList);
        if (ShopSignList != null)
            jd["shopSignList"] = ShopSignList.ToJsonDataList<ShopSignVO>();
        return jd;
    }

    private void DealPointList()
    {
        Vector3 vA = PointList[0];
        Vector3 vB = PointList[1];
        Vector3 vC = PointList[2];
        Vector3 vD = PointList[3];

        vC.y = vB.y;
        vD.y = vA.y;

        PointList[2] = vC;
        PointList[3] = vD;
    }

    public IJsonData ReadJsonData(JsonData jd)
    {
        if (jd.Keys.Contains("buildName"))
            BuildingName = jd["buildName"].ToString();
        if (jd.Keys.Contains("pointList"))
            PointList = jd["pointList"].ToVec3List();
        if (jd.Keys.Contains("shopSignList"))
            ShopSignList = jd["shopSignList"].ToItemVOList<ShopSignVO>();
        return this;
    }

    #endregion

    #region IListData
    bool isSelected;
    public bool IsSelect
    {
        get
        {
            return isSelected;
        }
        set
        {

            isSelected = value;
            if (OnDataChange != null)
                OnDataChange();
        }
    }

    public Action OnDataChange { get; set; }

    public string GetText(string paramName)
    {
        string val = "";
        if (ShopSignList != null)
        {
            //val += ShopSignList.ForeachToString(",", tar => { return tar.ShopName; });
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ShopSignList.Count; i++)
            {
                ShopSignVO ssv = ShopSignList[i];
                if (string.IsNullOrEmpty(ssv.m_strShopShowName))
                {
                    ssv.m_strShopShowName = m_sm.GetShopTypeByCode(ssv.ShopCode);
                }
                if (ssv.m_txtShop == null)
                {
                    m_sm.GetShopTexByCode(ssv.ShopCode, ssv.CellNum, (tx) =>
                    {
                        ssv.m_txtShop = tx;
                    });
                }
                sb.Append(ssv.m_strShopShowName);
                sb.Append(",");
            }
            val = sb.ToString();
        }
        return val;
    }

    public void GetTexture(string paramName, Action<Texture2D> onGet)
    {
        onGet(null);
    }

    public void ChangeData(string paramName, object value)
    {

    }
    #endregion
}


