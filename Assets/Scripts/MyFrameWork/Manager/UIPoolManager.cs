using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using PathologicalGames;

public class UIPoolManager : DDOLSingleton<UIPoolManager>
{
    SpawnPool m_uiPool;
    Dictionary<string, PrefabPool> uiName_uiPrefab;

    public void OnInit()
    {
        m_uiPool = gameObject.AddComponent<SpawnPool>();
        m_uiPool.poolName = "UIPools";
        m_uiPool.matchPoolLayer = true;
        m_uiPool.dontDestroyOnLoad = true;
        uiName_uiPrefab = new Dictionary<string, PrefabPool>();
    }

    /// <summary>
    /// Push UI Prefabs To Pool
    /// </summary>
    /// <param name="_strName"></param>
    /// <param name="_tf"></param>
    public void PushPrefab(string _strName)
    {
        if (!uiName_uiPrefab.ContainsKey(_strName))
        {
            GameObject obj = ResManager.Instance.Load(UIPathDefines.UI_PREFAB + _strName) as GameObject;
            if (obj != null)
            {
                PrefabPool pp = new PrefabPool(obj.transform);
                uiName_uiPrefab.Add(_strName, pp);
                CreatePrefabPool(pp);
            }
        }
    }

    /// <summary>
    /// Get Transform By UI Prefabs Name
    /// </summary>
    /// <param name="_strPrefabName"></param>
    /// <returns></returns>
    public Transform OnGetItem(string _strPrefabName)
    {
        if (uiName_uiPrefab.ContainsKey(_strPrefabName))
        {
            return m_uiPool.Spawn(uiName_uiPrefab[_strPrefabName].prefab);
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Destory Spawn
    /// </summary>
    /// <param name="_tf"></param>
    public void DeSpawn(Transform _tf)
    {
        if (m_uiPool.IsSpawned(_tf))
        {
            if (_tf != null)
            {
                m_uiPool.Despawn(_tf);
                _tf.SetParent(transform);
            }
        }
    }

    /// <summary>
    /// Destory All Spawn
    /// </summary>
    /// <param name="_tf"></param>
    public void DeSpawnAll(Transform _tf)
    {
        if (_tf != null )
        {
            for (int i = _tf.childCount -1 ; i > -1; i--)
            {
                DeSpawn(_tf.GetChild(i));
            }
        }
    }

    /// <summary>
    /// Create Pool For Prefab
    /// </summary>
    /// <param name="_pp"></param>
    private void CreatePrefabPool(PrefabPool _pp)
    {
        PrefabPool pp = _pp;
        if (!m_uiPool._perPrefabPoolOptions.Contains(pp))
        {
            //默认初始化5个Prefab实例
            pp.preloadAmount = 10;
            //开启限制
            pp.limitInstances = true;
            //关闭无限取Prefab
            pp.limitFIFO = true;
            //限制池子里最大的Prefab实例数量,这个和preloadAmount是相互冲突的，如果都设置了，那么默认取limitAmount
            pp.limitAmount = 50;
            //开启自动清理池子
            pp.cullDespawned = true;
            //缓存池自动清理，但是始终保留几个对象不清理。
            pp.cullAbove = 10;
            //每过多久执行一遍自动清理，单位是秒
            pp.cullDelay = 5;
            //每次清理几个
            pp.cullMaxPerPass = 10;
            //初始化内存池
            m_uiPool._perPrefabPoolOptions.Add(pp);
        }
    }
}
