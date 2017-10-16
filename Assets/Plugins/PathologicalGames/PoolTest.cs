using UnityEngine;
using System.Collections;
using PathologicalGames;
using System.Collections.Generic;

public class PoolTest : MonoBehaviour {

	SpawnPool spawnPool;
	PrefabPool r1;
    PrefabPool r2;
    List<PrefabPool> m_pPrePools;
    List<Transform> m_pTf;
	void Start()
	{
        m_pPrePools = new List<PrefabPool>();
        m_pTf = new List<Transform>();
        // Shapes这个缓冲池已经在面板中创建
		spawnPool = PoolManager.Pools["Shapes"];
        r1 = new PrefabPool(Resources.Load<Transform>("PoolTest/momo"));
        r2 = new PrefabPool(Resources.Load<Transform>("PoolTest/cube"));
        m_pPrePools.Add(r1);
        m_pPrePools.Add(r2);
	}

	void OnGUI()
	{
		if(GUILayout.Button("初始化内存池"))
		{
            for (int i = 0; i < m_pPrePools.Count; i++)
            {
                r1 = m_pPrePools[i];
                CreatePrefabPool(r1);
            }

            //spawnPool.CreatePrefabPool(spawnPool._perPrefabPoolOptions[spawnPool.Count]);
        }

		if(GUILayout.Button("从内存池里面取对象"))
		{
            int nIndex = Random.Range(0, m_pPrePools.Count);
			///从内存池里面取一个GameObjcet
			//Transform momo = 	spawnPool.Spawn("momo");
            Transform momo = spawnPool.Spawn(m_pPrePools[nIndex].prefab);
            momo.localPosition = new Vector3(0,nIndex,0);
            m_pTf.Add(momo);
        }


		if(GUILayout.Button("清空内存池"))
		{
			//清空池子
			spawnPool.DespawnAll();
            m_pTf.Clear();
		}

        if (GUILayout.Button("干掉一个"))
        {
            if (m_pTf.Count > 0)
            {
                // 通知缓存池，缓存对象实例
                spawnPool.Despawn(m_pTf[m_pTf.Count - 1]);
                m_pTf.RemoveAt(m_pTf.Count -1);
            }
        }
	}

    /// <summary>
    /// 为指定对象创建缓存池
    /// </summary>
    /// <param name="_pp"></param>
    private void CreatePrefabPool(PrefabPool _pp)
    {
        r1 = _pp;
        if (!spawnPool._perPrefabPoolOptions.Contains(r1))
        {
            //r1 = new PrefabPool(Resources.Load<Transform>("momo"));
            //默认初始化5个Prefab实例
            r1.preloadAmount = 5;
            //开启限制
            r1.limitInstances = true;
            //关闭无限取Prefab
            r1.limitFIFO = true;
            //限制池子里最大的Prefab实例数量,这个和preloadAmount是相互冲突的，如果都设置了，那么默认取limitAmount
            r1.limitAmount = 5;
            //开启自动清理池子
            r1.cullDespawned = true;
            //缓存池自动清理，但是始终保留几个对象不清理。
            r1.cullAbove = 10;
            //每过多久执行一遍自动清理，单位是秒
            r1.cullDelay = 5;
            //每次清理几个
            r1.cullMaxPerPass = 5;
            //初始化内存池
            spawnPool._perPrefabPoolOptions.Add(r1);
        }
    }
}
