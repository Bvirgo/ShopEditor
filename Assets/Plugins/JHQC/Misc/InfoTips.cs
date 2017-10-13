using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jhqc.EditorCommon
{
    public class InfoTips : MonoBehaviour
    {
        private static List<string> infoList = new List<string>();
        private static InfoTips onlyOne = null;

        private const int MaxLineNumberPerPage = 26;
        private const int LineHeight = 20;
        private const int LineWidth = 500;
        private const int MaxLineChar = 70;
        const int MaxSaveLengthPerInfo = 10000;

        public LogType logType;
        static int curPageIndex;
        static int maxPageCount = 1;
        
        public static bool IsVisible { get; set; }
        // Use this for initialization
        public static bool IsOnlySaveOnePage { get; set; }
        public static bool IsEnable { get; set; }
        void Awake()
        {
            IsOnlySaveOnePage = IsEnable = true;
            IsVisible = true;
            onlyOne = this;
        }

        public static InfoTips GetInsTance()
        {
            return onlyOne;
        }

        static int GetNextPageListNum(int curIndex)
        {
            var nextPageListNum = ++curIndex * MaxLineNumberPerPage;
            return nextPageListNum < infoList.Count ? nextPageListNum : infoList.Count;
        }

        static int GetNextPageIndex(int curIndex)
        {
            if (++curIndex >= maxPageCount)
            {
                curIndex = maxPageCount - 1;
            }
            return curIndex;
        }

        public static void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        public static void GotoNextPage()
        {
            curPageIndex = GetNextPageIndex(curPageIndex);
        }

        public static void GotoLastPage()
        {
            if(--curPageIndex < 0)
            {
                curPageIndex = 0;
            }
        }

        public static void GotoCurrentPage()
        {
            curPageIndex = maxPageCount - 1;
        }

        public static void ClearLogs()
        {
            ResetPageIndex();
            infoList.Clear();
        }

        static void ResetPageIndex()
        {
            curPageIndex = 0;
            maxPageCount = 1;
        }

        // Update is called once per frame
        void Update()
        {
        }

        StringBuilder sb = new StringBuilder();
        void OnGUI()
        {
            if (!IsVisible)
            {
                return;
            }
            if (logType != LogType.DebugAndGUI)
            {
                return;
            }

            GUI.contentColor = Color.red;
            sb.Length = 0;
            sb.AppendFormat("Page {0} / {1}", curPageIndex + 1, maxPageCount);
            GUI.Label(new Rect(0, 0, LineWidth, LineHeight), sb.ToString());
            for (int i = curPageIndex * MaxLineNumberPerPage; i < GetNextPageListNum(curPageIndex); ++i)
            {
                GUI.Label(new Rect(0, (1 + i - curPageIndex * MaxLineNumberPerPage) * LineHeight, LineWidth, LineHeight), infoList[i]);
            }
        }

        public static void LogInfo(object info)
        {
            DoLog(info, Debug.Log);
        }

        public static void LogWarning(object sth)
        {
            DoLog(sth, Debug.LogWarning);
        }

        private static void DoLog(object info, Action<object> logMethod)
        {
            if (onlyOne.logType == LogType.None)
            {
                return;
            }
            else if (onlyOne.logType == LogType.Debug)
            {
                logMethod(info);
            }
            else if (onlyOne.logType == LogType.DebugAndGUI)
            {
                logMethod(info);

                if (!IsEnable)
                {
                    ClearLogs();
                    return;
                }
                string infoStr = "null";
                if (info != null)
                {
                    infoStr = info.ToString();
                }
                if (infoStr.Length > MaxSaveLengthPerInfo)
                {
                    infoList.Add("InfoLength(" + infoStr.Length + ") is Too Long To Save");
                }
                else
                {
                    while (infoStr.Length > MaxLineChar)
                    {
                        infoList.Add(infoStr.Substring(0, MaxLineChar));
                        infoStr = infoStr.Substring(MaxLineChar, infoStr.Length - MaxLineChar);
                    }
                    infoList.Add(infoStr);
                }


                int count = infoList.Count;
                if (IsOnlySaveOnePage)
                {
                    ResetPageIndex();
                    if (count > MaxLineNumberPerPage)
                    {
                        infoList.RemoveRange(0, count - MaxLineNumberPerPage);
                    }
                }
                else
                {
                    while (count > MaxLineNumberPerPage * maxPageCount)
                    {
                        maxPageCount++;
                        // 最低则自动更新
                        if (curPageIndex == maxPageCount - 1 - 1)
                        {
                            curPageIndex = maxPageCount - 1;
                        }
                    }
                }
            }
        }
    }
}
