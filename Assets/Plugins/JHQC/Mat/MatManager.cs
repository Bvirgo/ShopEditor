using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace Jhqc.EditorCommon
{
    /// <summary>
    /// 目前只能在编辑器中使用，如果要在客户端中使用，还要大改
    /// </summary>
    public class MatManager : MonoBehaviour
    {
        // private static readonly string PREVIEWER_PREFAB_PATH = "EditorCommon/MaterialPreview";
        // private MaterialPreview previewer;
        private Dictionary<string, MatCacheItem> items = new Dictionary<string, MatCacheItem>();
        private Dictionary<string, List<MatCacheItem>> itemsByTag = new Dictionary<string, List<MatCacheItem>>();
        private List<MaterialInfo> toLoadMats = null;
        private int totalMatCount = -1;
        private bool autoRebuild = true;

        public bool IsLoading { get { return toLoadMats == null || toLoadMats.Count != 0; } }
        public int TotalCount { get { return totalMatCount; } }
        public int RemainingCount { get { return toLoadMats == null? 0: toLoadMats.Count; } }
        /// <summary>
        /// 默认三个
        /// </summary>
        public int MaxCount { set; get; }
        /// <summary>
        /// 网络error的回调：会传回一个出错的url；如果是拉列表出错，则返回string.empty
        /// </summary>
        public Action<string> OnLoadError { set; get; }
        /// <summary>
        /// 所有材质的信息，CAUTION：只有信息，可能对应的材质并没有load好，还需要手动调用load方法
        /// </summary>
        public List<MaterialInfo> AllMatInfo { get; private set; }

        private static MatManager instance;
        public static MatManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("MatManager");
                    instance = go.AddComponent<MatManager>();
                    instance.MaxCount = 3;

                    // var prePrefab = Resources.Load<GameObject>(PREVIEWER_PREFAB_PATH);

                    // if(prePrefab == null)
                    // {
                    // 	throw new NullReferenceException("材质预览的prefab没有拖进项目里？对不起，我没法确定GUID，不过他们说后面就没这个功能了");
                    // }

                    // var preGo = Instantiate(prePrefab) as GameObject;
                    // preGo.transform.SetParent(go.transform);
                    // instance.previewer = preGo.GetComponent<MaterialPreview>();

                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="preload">是否在init的时候就load所有材质（可能耗时较长）</param>
        /// <param name="dev">开发时不会load所有材质，而是MaxCount个</param>
        /// <param name="tags">tag == "" 时加载所有材质</param>
        public void Init(bool preload = true, bool dev = true, string tags = "", bool autoRebuild = true)
        {
            if (!WWWManager.Instance.IsInit)
            {
                throw new Exception("MatManager依赖于WWWManager，请先初始化WWWManager");
            }

            this.autoRebuild = autoRebuild;

            HttpGetAllMats(mats =>
            {
                AllMatInfo = mats;

                if (preload)
                {
                    if (dev)
                    {
                        toLoadMats = CutOffInDev(mats);
                    }
                    else
                    {
                        toLoadMats = mats;
                    }

                    totalMatCount = toLoadMats.Count;

                    LoadNext();
                }
                else    
                {
                    //no thing to load
                    toLoadMats = new List<MaterialInfo>();
                }

            }, tags);
        }

        private void LoadNext()
        {
            var mat = toLoadMats[0];
            var matCopy = mat;
            InfoTips.LogInfo("材质AB包URL：" + mat.url);
            HttpGetAb(mat.url, ab =>
            {
                StartCoroutine(LoadProceduralMaterial(ab, matCopy, null));
            });
        }

        private IEnumerator LoadProceduralMaterial(AssetBundle ab, MaterialInfo matInfo, Action<MatCacheItem> loadDone)
        {
            if (ab == null)
            {
                yield break;
            }

            var pm = ab.LoadAllAssets<ProceduralMaterial>()[0];
            if (!pm.isProcessing && autoRebuild)
            {
                pm.RebuildTextures(); 
            }

            while (pm.isProcessing)
            {
                yield return new WaitForEndOfFrame();
            }

            InfoTips.LogInfo(pm.name + " material build done");

            // previewer.gameObject.SetActive(true);

            var item = new MatCacheItem();
            item.MatName = matInfo.name;
            item.Info = matInfo;
            item.Material = pm;
            item.Loaded = true;

            items.Add(item.MatName, item);

            //cache mat items by tag
            foreach (var tag in matInfo.Tags)
            {
                if (itemsByTag.ContainsKey(tag))
                {
                    itemsByTag[tag].Add(item);
                }
                else
                {
                    itemsByTag.Add(tag, new List<MatCacheItem>() { item });
                }
            }

            // previewer.gameObject.SetActive(false);

            //可能单个加载
            if (toLoadMats.Contains(matInfo))
            {
                toLoadMats.Remove(matInfo);
            }

            if (loadDone != null)
            {
                loadDone(item);
            }

            if(toLoadMats.Count > 0)
            {
                LoadNext();
            }
            else
            {
                // Debug.Log("load done ----------------------------------------------------------");
            }
        }

        /// <summary>
        /// 可以在Init的时候不load，用到的时候再load 
        /// </summary>
        /// <param name="matNames"></param>
        public void LoadMat(string[] matNames)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, List<Action<MatCacheItem>>> loadCallbacksCache = 
                                            new Dictionary<string, List<Action<MatCacheItem>>>();

        /// <summary>
        /// load单个材质，保证每个回调都必然被调用
        /// </summary>
        public void LoadMat(string matName, Action<MatCacheItem> callback)
        {
            if (items.ContainsKey(matName))
            {
                callback(items[matName]);
                return;
            }

            //幸好不是真正的多线程
            if (loadCallbacksCache.ContainsKey(matName))
            {
                loadCallbacksCache[matName].Add(callback);
                return;
            }   

            var found = false;
            foreach (var item in AllMatInfo)
            {
                if (item.name == matName)
                {
                    found = true;

                    HttpGetAb(item.url, ab => 
                    {
                        StartCoroutine(LoadProceduralMaterial(ab, item, cacheItem => 
                        {
                            var callbackList = loadCallbacksCache[matName];
                            foreach (var cb in callbackList)
                            {
                                cb(cacheItem);
                            }
                        }));
                    });

                    loadCallbacksCache.Add(matName, new List<Action<MatCacheItem>>() { callback });

                    break;
                }
            }

            if (!found)
            {
                InfoTips.LogWarning(matName + "not found");
                callback(null);
            }
        }

        /// <summary>
        /// 已缓存过的所有tag
        /// </summary>
        public List<string> GetAllCachedTags()
        {
            return itemsByTag.Keys.ToList();
        }

        /// <summary>
        /// server返回列表里的所有tag
        /// </summary>
        public List<string> GetAllTags()
        {
            return AllMatInfo.Select(m => m.Tags).SelectMany(t => t).Distinct().ToList();
        }

        /// <summary>
        /// cache item
        /// </summary>
        /// <param name="matName"></param>
        /// <returns></returns>
        public MatCacheItem GetByName(string matName)
        {
            if (items.ContainsKey(matName))
            {
                return items[matName];
            }
            else
            {
                InfoTips.LogWarning("找不到该材质：" + matName);
                return null;
            }
        }

        /// <summary>
        /// info
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public List<MatCacheItem> GetByTag(string tagName)
        {
            if (itemsByTag.ContainsKey(tagName))
            {
                return itemsByTag[tagName];
            }
            else
            {
                InfoTips.LogWarning("找不到该材质：" + tagName);
                return null;
            }
        }

        /// <summary>
        /// cache item
        /// </summary>
        /// <param name="matName"></param>
        /// <returns></returns>
        public MaterialInfo GetInfoByName(string matName)
        {
            return AllMatInfo.SingleOrDefault(m => m.name == matName);
        }

        /// <summary>
        /// info
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public List<MaterialInfo> GetInfoByTag(string tagName)
        {
            return AllMatInfo.Where(m => m.Tags.Contains(tagName)).ToList();
        }

        private void HttpGetAllMats(Action<List<MaterialInfo>> callback, string tags = "")
        {
            WWWManager.Instance.Get("meterials/search", new NameValueCollection()
            {
                { "tags", tags }
            }, resp =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    if (OnLoadError != null)
                    {
                        OnLoadError("");
                    }

                    InfoTips.LogWarning("Get Material Error: " + resp.ToString());
                }
                else
                {
                    var mats = JsonUtility.FromJson<MaterialInfoCollection>(resp.WwwText);
                    callback(mats.data);
                }
            });
        }

        private void HttpGetAb(string url, Action<AssetBundle> callback)
        {
            WWWManager.Instance.GetFile(url, LocalCacheEntry.CacheType.AssetBundle, (resp, entry) =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    if (OnLoadError != null)
                    {
                        OnLoadError(url);
                    }

                    InfoTips.LogWarning("Get Material AssetBundle Error: " + resp.ToString());
                }
                else
                {
                    callback(entry.AB);
                }
            });
        }

        //editor读ab包不能使用procedural material的缓存
        //只读两三个意思一下就行了
        private List<MaterialInfo> CutOffInDev(List<MaterialInfo> mats)
        {
            var result = new List<MaterialInfo>();
            var distinctTags = new List<string>();
            // var maxCount = 3;

            foreach (var m in mats)
            {
                if (!distinctTags.Contains(m.Tags[0]))
                {
                    distinctTags.Add(m.Tags[0]);
                    result.Add(m);
                }

                if (result.Count >= MaxCount)
                {
                    break;
                }
            }

            return result;
        }

        public class MatCacheItem
        {
            public string MatName { get; set; }
            public ProceduralMaterial Material { get; set; }
            public Texture2D Preview { get; set; }
            public bool Loaded { get; internal set; }
            public MaterialInfo Info { get; set; }
        }
    }
}