using UnityEngine;
using System.Collections;
using System;

namespace ZFrameWork
{
    /// <summary>
    /// 鼠标事件定义
    /// </summary>
    public class MouseEvent
    {
        public const string MouseDown = "MouseDown";
        public const string MouseUp = "MouseUp";
        public const string OnDraging = "OnDraging";
        public const string OnDragStart = "OnDragingStart";
        public const string OnMouseClick = "OnMouseClick";
        public const string OnDragEnd = "OnDragEnd";
        public const string OnMouseDoubleClick = "OnMouseDoubleClick";

        public string eventType;
        public IMouseCtrl tar;
        public Vector2 screenPoint;
        public Vector3 worldPoint;
        public int mouse;
        public MouseEvent(string eventType, IMouseCtrl tar, Vector2 screenPoint, Vector3 worldPoint, int mouse)
        {
            this.eventType = eventType;
            this.tar = tar;
            this.screenPoint = screenPoint;
            this.worldPoint = worldPoint;
            this.mouse = mouse;
        }
    }

    /// <summary>
    /// 鼠标事件
    /// </summary>
    public class MouseManager : DDOLSingleton<MouseManager>
    {
        public bool SelfMotivated = false;

        public bool RayCastAll = true;

        IMouseCtrl curMC = null;

        public void OnInit()
        {
            Debug.Log("Init Mouse Manager");
        }
        void OnMouseEvent(string eventType, IMouseCtrl tar, Vector2 screenPoint, Vector3 worldPoint, int mouse)
        {
            //Debug.Log("eventType : " + eventType + "   tar : " + tar);
            //Debug.Log("tar : " + tar);
            //Debug.Log("screenPoint : " + screenPoint);
            //Debug.Log("worldPoint : " + worldPoint);

            switch (eventType)
            {
                case MouseEvent.MouseDown:
                    if (tar != null)
                        tar.MouseDown(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.MouseUp:
                    if (tar != null)
                        tar.MouseUp(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.OnDragStart:
                    if (tar != null)
                        tar.OnDragStart(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.OnDraging:
                    if (curMC != null)
                        curMC.OnDraging(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.OnDragEnd:
                    if (curMC != null)
                        curMC.OnDragEnd(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.OnMouseClick:
                    if (tar != null)
                        tar.MouseClick(screenPoint, worldPoint, mouse);
                    break;
                case MouseEvent.OnMouseDoubleClick:
                    if (tar != null)
                        tar.MouseDoubleClick(screenPoint, worldPoint, mouse);
                    break;

            }

            MouseEvent me = new MouseEvent(eventType, tar, screenPoint, worldPoint, mouse);
            Message msg = new Message(MsgType.Com_MouseEvent, this);
            msg["event"] = me;
            msg.Send();
        }

        void Update()
        {
            if (!Utils.IsOnUI)
            {
                CheckMouse();
            }
            else
            {
                curMC = null;
            }
        }

        //CHECK 将时间记录 按 mouseBtn 区分开
        float doubleTimer0 = 0;
        float doubleTimer1 = 0;
        float doubleTimer2 = 0;
        const float intervalTime = 0.4f;
        bool isStartDrag = false;
        int num = 0;
        Vector3 lastScreenP = new Vector3(0, 0, 0);

        /// <summary>
        /// 鼠标操作检测
        /// </summary>
        private void CheckMouse()
        {
            float time = Time.unscaledTime;// RealTime.time;

            Vector3 screenP = Input.mousePosition;
            Vector3 worldP = Vector3.zero;
            Ray ray = Camera.main.ScreenPointToRay(screenP);
            RaycastHit tarRh = default(RaycastHit);
            IMouseCtrl mc = null;

            if (RayCastAll)
            {
                RaycastHit[] rhArr = Physics.RaycastAll(ray);
                float curDis = float.MaxValue;
                for (int i = 0, length = rhArr.Length; i < length; i++)
                {
                    IMouseCtrl tarMC = rhArr[i].collider.GetComponentInParent<IMouseCtrl>();
                    if (tarMC != null)
                    {

                        float dis = Vector3.Distance(ray.origin, rhArr[i].point);
                        if (dis < curDis)
                        {
                            curDis = dis;

                            mc = tarMC;
                            tarRh = rhArr[i];
                            worldP = tarRh.point;
                        }
                    }
                }
            }
            else
            {
                if (Physics.Raycast(ray, out tarRh, 99999f))
                {
                    mc = tarRh.collider.GetComponentInParent<IMouseCtrl>();

                    if (mc != null)
                    {
                        worldP = tarRh.point;
                    }
                }
            }

            //int mouseBtn = 0;
            CheckMouseBehavier(0, ref screenP, ref worldP, mc, ref time, ref doubleTimer0);
            CheckMouseBehavier(1, ref screenP, ref worldP, mc, ref time, ref doubleTimer1);
            CheckMouseBehavier(2, ref screenP, ref worldP, mc, ref time, ref doubleTimer2);
        }

        /// <summary>
        /// 判断鼠标是否在IMouseCtrl对象上
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public bool IsMousePointOn<T>(ref Vector3 worldPos) where T : IMouseCtrl
        {
            Vector3 screenP = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenP);
            RaycastHit[] rhArr = Physics.RaycastAll(ray);

            for (int i = 0, length = rhArr.Length; i < length; i++)
            {
                RaycastHit rh = rhArr[i];
                T tarMC = rh.collider.GetComponentInParent<T>();
                if (tarMC != null)
                {
                    worldPos = rh.point;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 鼠标按键操作检测
        /// </summary>
        /// <param name="mouseBtn"></param>
        /// <param name="screenP"></param>
        /// <param name="worldP"></param>
        /// <param name="mc"></param>
        /// <param name="time"></param>
        /// <param name="doubleTimer"></param>
        void CheckMouseBehavier(int mouseBtn, ref Vector3 screenP, ref Vector3 worldP, IMouseCtrl mc, ref float time, ref float doubleTimer)
        {
            string mouseEvent = null;
            if (Input.GetMouseButtonDown(mouseBtn))
            {
                lastScreenP = screenP;
                if (mc != null)
                {
                    //OnMouseDown
                    mouseEvent = MouseEvent.MouseDown;
                    curMC = mc;
                    if (curMC != null)
                        OnMouseEvent(mouseEvent, curMC, screenP, worldP, mouseBtn);

                    //OnMouseDoubleClick
                    num++;
                    if (1 == num)
                    {
                        doubleTimer = time;
                    }
                    if (2 == num && time - doubleTimer <= intervalTime)
                    {
                        mouseEvent = MouseEvent.OnMouseDoubleClick;
                        if (curMC != null)
                            OnMouseEvent(mouseEvent, curMC, screenP, worldP, mouseBtn);
                        num = 0;
                    }
                    if (time - doubleTimer > intervalTime)
                    {
                        num = 1;
                        doubleTimer = time;
                    }
                }
                else
                {
                    curMC = null;
                    OnMouseEvent(MouseEvent.MouseDown, null, screenP, worldP, mouseBtn);
                }

            }
            else if (Input.GetMouseButtonUp(mouseBtn))
            {
                //OnMouseClick
                if (lastScreenP == screenP)
                {
                    mouseEvent = MouseEvent.OnMouseClick;
                    if (mc != null)
                        OnMouseEvent(mouseEvent, mc, screenP, worldP, mouseBtn);
                }
                //OnDragEnd
                else
                {
                    if (isStartDrag)
                    {
                        isStartDrag = false;
                        mouseEvent = MouseEvent.OnDragEnd;
                        OnMouseEvent(mouseEvent, curMC, screenP, worldP, mouseBtn);
                    }
                }

                //OnMouseUp
                mouseEvent = MouseEvent.MouseUp;
                OnMouseEvent(mouseEvent, mc, screenP, worldP, mouseBtn);

            }
            else if (Input.GetMouseButton(mouseBtn))
            {
                //OnDragStart
                if (mc != null && lastScreenP != screenP && !isStartDrag)
                {
                    mouseEvent = MouseEvent.OnDragStart;

                    if (curMC != null)
                        OnMouseEvent(mouseEvent, curMC, screenP, worldP, mouseBtn);
                    num = 0;
                    isStartDrag = true;
                }
                //OnDraging
                else if (isStartDrag)
                {
                    mouseEvent = MouseEvent.OnDraging;
                    if (curMC != null)
                        OnMouseEvent(mouseEvent, curMC, screenP, worldP, mouseBtn);
                }
            }
        }
    }
}