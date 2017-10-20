using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using System;

public class SceneModelMono : MonoBehaviour,IMouseCtrl
{
    #region Base Peroperty
    private string m_strGuid;
    //private Vector3 m_vDragBias;
    public string guid
    {
        get { return m_strGuid; }
        private set { m_strGuid = value; }
    }

    void Awake()
    {
        guid = Guid.NewGuid().ToString();
        //m_vDragBias = Vector3.zero;
    }    
    #endregion

    #region Mouse Event
    public void MouseClick(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
        //Debug.LogWarning(string.Format("Building:{0},Is Clicked!", name));
        Message msg = new Message(MsgType.ShopView_NewPoint,this);
        msg["guid"] = guid;
        msg["pos"] = worldPoint;
        msg.Send();
    }

    public void MouseDoubleClick(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {

        Debug.LogWarning(string.Format("Building:{0},Is DoubleClicked!", name));
    }

    public void MouseDown(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
    }

    public void MouseUp(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
    }

    public void OnDragEnd(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
        //m_vDragBias = Vector3.zero;
    }

    public void OnDraging(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
        //transform.position = Utils.GetPointOnLayer(Defines.MapsLayerName) + m_vDragBias;
    }

    public void OnDragStart(Vector2 screenPoint, Vector3 worldPoint, int mouse)
    {
        //m_vDragBias = transform.position - Utils.GetPointOnLayer(Defines.MapsLayerName);
    }
    #endregion
}
