
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZFrameWork
{
	public class MonoHelper : DDOLSingleton<MonoHelper>
	{
        List<Action> m_pUpateActor;
        List<Action> m_pFixedUpdateActor;
        List<Action> m_pLateUpdateActor;

        void Awake()
        {
            m_pFixedUpdateActor = new List<Action>();
            m_pLateUpdateActor = new List<Action>();
            m_pUpateActor = new List<Action>();
        }

        public void UpdateRegister(Action _cbUpdate)
        {
            if (!m_pUpateActor.Contains(_cbUpdate))
            {
                m_pUpateActor.Add(_cbUpdate);
            }
        }

        public void FixedUpdateRegister(Action _cbUpdate)
        {
            if (!m_pFixedUpdateActor.Contains(_cbUpdate))
            {
                m_pFixedUpdateActor.Add(_cbUpdate);
            }
        }

        public void LateUpdateRegister(Action _cbUpdate)
        {
            if (!m_pLateUpdateActor.Contains(_cbUpdate))
            {
                m_pLateUpdateActor.Add(_cbUpdate);
            }
        }

        void Update()
        {
            for (int i = 0; i < m_pUpateActor.Count; i++)
            {
                m_pUpateActor[i]();
            }
        }

        void FixedUpdate()
        {
            for (int i = 0; i < m_pFixedUpdateActor.Count; i++)
            {
                m_pFixedUpdateActor[i]();
            }
        }

        void LateUpdate()
        {
            for (int i = 0; i < m_pLateUpdateActor.Count; i++)
            {
                m_pLateUpdateActor[i]();
            }
        }
	}
}

