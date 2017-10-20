using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrameWork
{
    /// <summary>
    /// Queue Manager
    /// </summary>
    public  class QueueManager: Singleton<QueueManager>
    {
        /// <summary>
        /// 运行窗阈值
        /// </summary>
        public int m_nMax;

        public Action m_actAllDone;

        List<AsyncTask> m_pRuningList;

        List<AsyncTask> m_pWaitingList;

        Queue<AsyncTask> m_qTask;

        public QueueManager()
        {
            m_nMax = 5;
        }
        public override void Init()
        {
            base.Init();
            m_pWaitingList = new List<AsyncTask>();
            m_pRuningList = new List<AsyncTask>();
        }

        /// <summary>
        /// New Task
        /// </summary>
        /// <param name="_task"></param>
        /// <returns></returns>
        public AsyncTask Add(AsyncTask _task)
        {
            Insert(_task);

            TaskLoop();

            return _task;
        }

        /// <summary>
        /// New Task
        /// </summary>
        /// <param name="loadFunc">回调</param>
        /// <param name="priority">权重</param>
        /// <param name="label"></param>
        /// <returns></returns>
        public AsyncTask Add(Action<Action> loadFunc, int priority = 3, string label = "")
        {
            AsyncTask _task = new AsyncTask(loadFunc, priority, label);
            return Add(_task);
        }

        /// <summary>
        /// 新任务，插入队列
        /// </summary>
        /// <param name="_task"></param>
        public void Insert(AsyncTask _task)
        {
            if (!m_pRuningList.Contains(_task))
            {
                if (m_pWaitingList.Contains(_task))
                {
                    m_pWaitingList.Remove(_task);
                }
                m_pWaitingList.Add(_task);
                m_pWaitingList.Sort(SortTask);               
            }
        }

        /// <summary>
        /// 任务按权重排序
        /// </summary>
        /// <param name="_q1"></param>
        /// <param name="_q2"></param>
        /// <returns></returns>
        private int SortTask(AsyncTask _q1,AsyncTask _q2)
        {
            if (_q1.Priority > _q2.Priority)
            {
                return -1;
            }
            return 1;
        }

        public bool Cancel(AsyncTask _task)
        {
            if (m_pWaitingList.Contains(_task))
            {
                m_pWaitingList.Remove(_task);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 清除等待队列
        /// </summary>
        public void ClearWaitList()
        {
            m_pWaitingList.Clear();
            m_pRuningList.Clear();
        }

        /// <summary>
        /// 获取等待队列
        /// </summary>
        /// <returns></returns>
        public List<AsyncTask> GetWaitList()
        {
            return m_pWaitingList;
        }

        /// <summary>
        /// 等待队列长度
        /// </summary>
        /// <returns></returns>
        public int GetWaitLenth()
        {
            return m_pWaitingList.Count + m_pRuningList.Count;
        }
        
        /// <summary>
        /// Go
        /// </summary>
        void TaskLoop()
        {
            if (m_pRuningList.Count >= m_nMax)
            {
                return;
            }

            if (m_pWaitingList.Count > 0)
            {
                int nCount = m_nMax - m_pRuningList.Count;

                nCount = nCount > m_pWaitingList.Count ? m_pWaitingList.Count : nCount;

                for (int i = 0; i < nCount; i++)
                {
                    AsyncTask qvo = m_pWaitingList[i];
                    m_pRuningList.Add(m_pWaitingList[i]);
                    m_pWaitingList.RemoveAt(i);
                    qvo.LoadFunc(() =>
                    {
                        TaskLoop();
                        m_pRuningList.Remove(qvo);

                        if (m_pRuningList.Count == 0 && m_pWaitingList.Count == 0)
                        {
                            if (m_actAllDone != null)
                            {
                                m_actAllDone();
                            }
                        }
                    });
                }
            }
        }
    }

    public class AsyncTask
    {
        public Action<Action> LoadFunc;
        public float Priority;
        public string Label;

        public AsyncTask(Action<Action> loadFunc, float priority, string label)
        {
            LoadFunc = loadFunc;
            Priority = priority;
            Label = label;
        }
    }
}
