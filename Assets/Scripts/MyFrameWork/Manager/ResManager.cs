
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Jhqc.EditorCommon;
using System.IO;
using Jhqc.UnityFbxLoader;

namespace ZFrameWork
{
    public class ResManager : Singleton<ResManager>
	{
        #region Member
        private Dictionary<string, AssetInfo> dicAssetInfo = null;

        private Queue<IResourceNode> m_qUpdate;
        private Dictionary<String, IResourceNode> key_rNode;
        private List<IResourceNode> m_pErrorNode;

        private Dictionary<string, Texture2D> imgCrc_img;

        public override void Init()
        {
            dicAssetInfo = new Dictionary<string, AssetInfo>();
            m_qUpdate = new Queue<IResourceNode>();
            key_rNode = new Dictionary<string, IResourceNode>();
            m_pErrorNode = new List<IResourceNode>();
            imgCrc_img = new Dictionary<string, Texture2D>();
            FbxLoader.InitLoader();
        }

        /// <summary>
        /// 获取资源类型
        /// </summary>
        /// <param name="_strFileName"></param>
        /// <returns></returns>
        public static ResType GetResType(string _strFileName)
        {
            ResType type = ResType.Raw;
            if (Utils.IsAB(_strFileName))
            {
                return ResType.AssetBundle;
            }
            else if (Utils.IsPic(_strFileName))
            {
                return ResType.Texture;

            }
            else if (Utils.IsFbx(_strFileName))
            {
                return ResType.Fbx;
            }
            return type;
        }
        #endregion

        #region Resources Folder
        #region Load Resources & Instantiate Object

        /// <summary>
        /// Loads the instance.
        /// </summary>
        /// <returns>The instance.</returns>
        /// <param name="_path">_path.</param>
        public UnityEngine.Object LoadInstance(string _path)
        {
            UnityEngine.Object _obj = Load(_path);
            return Instantiate(_obj);
        }

        /// <summary>
        /// Loads the coroutine instance.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        public void LoadCoroutineInstance(string _path, Action<UnityEngine.Object> _loaded)
        {
            LoadCoroutine(_path, (_obj) => { Instantiate(_obj, _loaded); });
        }

        /// <summary>
        /// Loads the async instance.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        public void LoadAsyncInstance(string _path, Action<UnityEngine.Object> _loaded)
        {
            LoadAsync(_path, (_obj) => { Instantiate(_obj, _loaded); });
        }

        /// <summary>
        /// Loads the async instance.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        /// <param name="_progress">_progress.</param>
        public void LoadAsyncInstance(string _path, Action<UnityEngine.Object> _loaded, Action<float> _progress)
        {
            LoadAsync(_path, (_obj) => { Instantiate(_obj, _loaded); }, _progress);
        }
        #endregion

        #region Load Resources
        /// <summary>
        /// Load the specified _path.
        /// </summary>
        /// <param name="_path">_path.</param>
        public UnityEngine.Object Load(string _path)
        {
            AssetInfo _assetInfo = GetAssetInfo(_path);
            if (null != _assetInfo)
                return _assetInfo.AssetObject;
            return null;
        }
        #endregion

        #region Load Coroutine Resources

        /// <summary>
        /// Loads the coroutine.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        public void LoadCoroutine(string _path, Action<UnityEngine.Object> _loaded)
        {
            AssetInfo _assetInfo = GetAssetInfo(_path, _loaded);
            if (null != _assetInfo)
                MonoHelper.Instance.StartCoroutine(_assetInfo.GetCoroutineObject(_loaded));
        }
        #endregion

        #region Load Async Resources

        /// <summary>
        /// Loads the async.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        public void LoadAsync(string _path, Action<UnityEngine.Object> _loaded)
        {
            LoadAsync(_path, _loaded, null);
        }

        /// <summary>
        /// Loads the async.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        /// <param name="_progress">_progress.</param>
        public void LoadAsync(string _path, Action<UnityEngine.Object> _loaded, Action<float> _progress)
        {
            AssetInfo _assetInfo = GetAssetInfo(_path, _loaded);
            if (null != _assetInfo)
                MonoHelper.Instance.StartCoroutine(_assetInfo.GetAsyncObject(_loaded, _progress));
        }
        #endregion

        #region Get AssetInfo & Instantiate Object

        /// <summary>
        /// Gets the asset info.
        /// </summary>
        /// <returns>The asset info.</returns>
        /// <param name="_path">_path.</param>
        private AssetInfo GetAssetInfo(string _path)
        {
            return GetAssetInfo(_path, null);
        }

        /// <summary>
        /// Gets the asset info.
        /// </summary>
        /// <returns>The asset info.</returns>
        /// <param name="_path">_path.</param>
        /// <param name="_loaded">_loaded.</param>
        private AssetInfo GetAssetInfo(string _path, Action<UnityEngine.Object> _loaded)
        {
            if (string.IsNullOrEmpty(_path))
            {
                Debug.LogError("Error: null _path name.");
                if (null != _loaded)
                    _loaded(null);
            }
            // Load Res....
            AssetInfo _assetInfo = null;
            if (!dicAssetInfo.TryGetValue(_path, out _assetInfo))
            {
                _assetInfo = new AssetInfo();
                _assetInfo.Path = _path;
                dicAssetInfo.Add(_path, _assetInfo);
            }
            _assetInfo.RefCount++;
            return _assetInfo;
        }

        /// <summary>
        /// Instantiate the specified _obj.
        /// </summary>
        /// <param name="_obj">_obj.</param>
        private UnityEngine.Object Instantiate(UnityEngine.Object _obj)
        {
            return Instantiate(_obj, null);
        }

        /// <summary>
        /// Instantiate the specified _obj and _loaded.
        /// </summary>
        /// <param name="_obj">_obj.</param>
        /// <param name="_loaded">_loaded.</param>
        private UnityEngine.Object Instantiate(UnityEngine.Object _obj, Action<UnityEngine.Object> _loaded)
        {
            UnityEngine.Object _retObj = null;
            if (null != _obj)
            {
                _retObj = MonoBehaviour.Instantiate(_obj);
                if (null != _retObj)
                {
                    if (null != _loaded)
                    {
                        _loaded(_retObj);
                        return null;
                    }
                    return _retObj;
                }
                else
                {
                    Debug.LogError("Error: null Instantiate _retObj.");
                }
            }
            else
            {
                Debug.LogError("Error: null Resources Load return _obj.");
            }
            return null;
        }

        #endregion
        #endregion

        #region Load Resource To Memery
        /// <summary>
        /// 加载文件到内存,返回文件信息
        /// </summary>
        /// <param name="_strFilePath">文件路径</param>
        /// <param name="_cb">回调</param>
        public void OnLoadLocalRes(string _strFilePath,Action<IResourceNode> _cb)
        {
            IResourceNode rNode = null;
            rNode = new LocalResourceNode(_strFilePath);
            rNode.LoadResource(resp =>
            {
                _cb(resp);
            }, (er, resp) =>
            {
                Debug.LogError(resp);
            });
        }

        /// <summary>
        /// 加载服务器资源
        /// </summary>
        /// <param name="_strName"></param>
        /// <param name="_strCrc"></param>
        /// <param name="_cb"></param>
        public void OnLoadServerRes(string _strName,string _strCrc,Action<IResourceNode> _cb,Action<string> _fCb = null)
        {
            IResourceNode rNode = new HttpResourceNode(_strName, _strCrc);
            rNode.LoadResource(resp => {
                _cb(resp);
            }, (er, resp) =>
            {
                if (_fCb != null)
                {
                    _fCb(resp);
                }
                Debug.LogError(resp);
            });
        }

        /// <summary>
        /// 排队上传
        /// </summary>
        private void UpdateToServer(Action _cb, Action<string> _eCb)
        {
            if (m_qUpdate.Count > 0)
            {
                LocalResourceNode fileNode = m_qUpdate.Dequeue() as LocalResourceNode;
                string fileName = fileNode.GetName();
                string fileCrc = fileNode.GetCrc();

                string strName = fileName;
                int nIndex = strName.LastIndexOf('.');
                if (nIndex > 0 && nIndex < strName.Length - 1)
                {
                    strName = strName.Substring(strName.LastIndexOf('.') + 1);
                }
                else
                {
                    strName = "ErrorType";
                }

                HttpService.UploadFile(fileName, fileCrc, null, fileNode.GetResource() as byte[], strName, () =>
                {
                    PopWaiting();
                    UpdateToServer(_cb, _eCb);

                }, (errorType, errorInfo) =>
                {
                    PopWaiting();

                    string strEr = string.Format("资源：{0},上传错误：{1}",fileName,errorInfo);

                    string strKey = fileNode.GetCrc() + fileNode.GetName();
                    if (key_rNode.ContainsKey(strKey))
                    {
                        IResourceNode rNode = key_rNode[strKey];
                        if (!m_pErrorNode.Contains(rNode))
                        {
                            m_pErrorNode.Add(rNode);
                        }
                    }
                      
                    _eCb(strEr);

                    UpdateToServer(_cb, _eCb);

                }); // End Http

                Debug.Log(string.Format("余下：{0}",m_qUpdate.Count));
            }
            else
            {
                // 弹框提示上传失败，点击重新上传
                if (m_pErrorNode.Count > 0)
                {
                    string strTips = string.Format("资源上传失败个数：{0} /n点击【确定】按钮，失败重传！", m_pErrorNode.Count);
                    LogicUtils.Instance.OnAlert(strTips, () => {
                        m_qUpdate = new Queue<IResourceNode>(m_pErrorNode);
                        UpdateToServer(_cb,_eCb);
                    });
                }
                else
                {
                    LogicUtils.Instance.OnAlert("资源上传完成！");
                    _cb();
                }

            }
        }

        private void PopWaiting()
        {
            LogicUtils.Instance.OnPopWaiting(1);
        }

        /// <summary>
        /// 资源上传入口
        /// </summary>
        /// <param name="resourcesDic"></param>
        /// <param name="_cb"></param>
        /// <param name="errorCallBack"></param>
        public void OnUpdateToServer(List<IResourceNode> _pRes, Action _cb, Action<string> _eCb)
        {
            m_qUpdate = new Queue<IResourceNode>(_pRes);
            key_rNode.Clear();
            m_pErrorNode.Clear();
            for (int i = 0; i < _pRes.Count; i++)
            {
                string strKey = _pRes[i].GetCrc() + _pRes[i].GetName();
                key_rNode.AddOrReplace(strKey,_pRes[i]);
            }

            if (m_qUpdate.Count > 0)
            {
                LogicUtils.Instance.OnShowWaiting(1, "资源上传中...",false,m_qUpdate.Count);
                UpdateToServer(_cb,_eCb);
            }
            else
            {
                _cb();
            }
        }
        #endregion

        #region Load Fbx
        public void OnCreateFbx(byte[] byteArr, Action<GameObject> _cb)
        {
            GameObject objFbx = null;
            ErrorCode er = FbxLoader.LoadFbx(byteArr, out objFbx);
            if (er == ErrorCode.LoadOk)
            {
                 _cb(objFbx);
            }
        }

        /// <summary>
        /// 加载本地FBX文件
        /// </summary>
        /// <param name="_strPath"></param>
        /// <param name="_cb"></param>
        public void OnLoadLocalFBXByPath(string _strPath, Action<GameObject> _cb)
        {
            OnLoadLocalRes(_strPath, (rNode) => {
                OnCreateFbx(rNode.GetResource() as byte[], (obj) => _cb(obj));
            });
        }

        /// <summary>
        /// 加载服务器FBX模型 
        /// </summary>
        /// <param name="_strFileName"></param>
        /// <param name="_strFileCrc"></param>
        /// <param name="_cb"></param>
        public void OnLoadServerFbx(string _strFileName, string _strFileCrc, Action<GameObject> _cb)
        {
            OnLoadServerRes(_strFileName, _strFileCrc, (rNode) =>
            {
                OnCreateFbx(rNode.GetResource() as byte[], (obj) => _cb(obj));
            });
        }

        #endregion

        #region Load AssetBundle

        /// <summary>
        /// 加载本地AB包
        /// </summary>
        /// <param name="_strPath"></param>
        /// <param name="_cb"></param>
        public void OnLoadLocalAbByPath(string _strPath, Action<AssetBundle> _cb)
        {
            OnLoadLocalRes(_strPath,(rNode)=> {
                OnLoadLocalAb(rNode.GetResource() as byte[],(ab)=> _cb(ab));
            });
        }

        public void OnLoadLocalAb(byte[] _bytes,Action<AssetBundle> _cb)
        {
            AssetBundle newAB = AssetBundle.LoadFromMemory(_bytes);
            _cb(newAB);
        }

        /// <summary>
        /// 加载服务器AB文件
        /// </summary>
        /// <param name="_strFileName"></param>
        /// <param name="_strFileCrc"></param>
        /// <param name="_cb"></param>
        public void OnLoadServerAb(string _strFileName, string _strFileCrc, Action<AssetBundle> _cb)
        {
            OnLoadServerRes(_strFileName, _strFileCrc, (rNode) =>
            {
                if (rNode.GetResource() as AssetBundle != null)
                {
                    _cb(rNode.GetResource() as AssetBundle);
                }
                else
                {
                    OnLoadLocalAb(rNode.GetResource() as byte[], (ab) => _cb(ab));
                }
            });
        }
        #endregion

        #region Load Texture
        public void OnLoadLocalTexture(string _strPath , Action<Texture2D> _cb)
        {
            if (imgCrc_img.ContainsKey(_strPath))
            {
                _cb(imgCrc_img[_strPath]);
            }
            else
            {
                OnLoadLocalRes(_strPath, (rNode) => {
                    Texture2D tx = new Texture2D(500, 500);
                    tx.LoadImage(rNode.GetResource() as byte[]);
                    imgCrc_img.AddOrReplace(_strPath,tx);

                    _cb(tx);
                });
            }
        }

        /// <summary>
        /// 下载图片资源
        /// </summary>
        /// <param name="_strImg"></param>
        /// <param name="_strImgCrc"></param>
        /// <param name="_cb"></param>
        /// <param name="_fCb"></param>
        public void OnLoadServerTexure(string _strImg, string _strImgCrc, Action<Texture2D> _cb, Action<string> _fCb = null)
        {
            string strKey = _strImg + _strImgCrc;

            if (imgCrc_img.ContainsKey(strKey))
            {
                _cb(imgCrc_img[strKey]);
                return;
            }

            HttpService.GetDownloadURL(_strImg, _strImgCrc, url =>
            {
                HttpService.GetRemoteTexture(url, tex =>
                {
                    imgCrc_img.AddOrReplace(strKey, tex);
                    _cb(tex);

                }, true, (er) => {
                    _fCb(er);
                });
            });
        }
        #endregion
    }

    #region Local Resource Info
    public class AssetInfo
    {
        private UnityEngine.Object _Object;
        public Type AssetType { get; set; }
        public string Path { get; set; }
        public int RefCount { get; set; }
        public bool IsLoaded
        {
            get
            {
                return null != _Object;
            }
        }

        public UnityEngine.Object AssetObject
        {
            get
            {
                if (null == _Object)
                {
                    _ResourcesLoad();
                }
                return _Object;
            }
        }

        public IEnumerator GetCoroutineObject(Action<UnityEngine.Object> _loaded)
        {
            while (true)
            {
                yield return null;
                if (null == _Object)
                {
                    //yield return null;
                    _ResourcesLoad();
                    yield return null;
                }
                if (null != _loaded)
                    _loaded(_Object);
                yield break;
            }

        }

        private void _ResourcesLoad()
        {
            try
            {
                _Object = Resources.Load(Path);
                if (null == _Object)
                    Debug.Log("Resources Load Failure! Path:" + Path);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public IEnumerator GetAsyncObject(Action<UnityEngine.Object> _loaded)
        {
            return GetAsyncObject(_loaded, null);
        }

        public IEnumerator GetAsyncObject(Action<UnityEngine.Object> _loaded, Action<float> _progress)
        {
            // have Object
            if (null != _Object)
            {
                _loaded(_Object);
                yield break;
            }

            // Object null. Not Load Resources
            ResourceRequest _resRequest = Resources.LoadAsync(Path);

            // 
            while (_resRequest.progress < 0.9)
            {
                if (null != _progress)
                    _progress(_resRequest.progress);
                yield return null;
            }

            // 
            while (!_resRequest.isDone)
            {
                if (null != _progress)
                    _progress(_resRequest.progress);
                yield return null;
            }

            // ???
            _Object = _resRequest.asset;
            if (null != _loaded)
                _loaded(_Object);

            yield return _resRequest;
        }
    }
    #endregion

    #region Local Resource Node
    /// <summary>
    /// Resource Memory Node
    /// </summary>
    public class LocalResourceNode : IResourceNode
    {
        private string path;
        private string crc;
        private string fileName;
        private byte[] bins;
        public bool loaded;
        private ResType resType;

        public LocalResourceNode(string path)
        {
            this.path = path;
            loaded = false;
        }

        public string GetCrc()
        {
            return crc;
        }
        public object GetResource()
        {
            return bins;
        }

        public ResType GetResType()
        {
            return resType;
        }

        public string GetName()
        {
            return fileName;
        }

        /// <summary>
        /// Load File To Memory
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="failCallback"></param>
        public void LoadResource(Action<IResourceNode> callback, Action<HttpResp.ErrorType, string> failCallback = null)
        {
            if (File.Exists(path))
            {
                GetFileNameFromPath(path, out this.fileName);

                resType = ResManager.GetResType(this.fileName);

                // 开新线程，处理文件加载
                Loom.RunAsync(() => {
                    using (var file = File.OpenRead(path))
                    {
                        long filesize = file.Length;
                        byte[] fbin = new byte[filesize];
                        file.BeginRead(fbin, 0, (int)filesize, ar =>
                        {
                            int bytesRead = file.EndRead(ar);
                            if (bytesRead == (int)filesize)
                            {
                                bins = fbin;
                                crc = Crc32.CountCrc(bins).ToString();

                                //string crc1 = Utils.CaclCRC(bins).ToString();

                                //Debug.LogWarning(string.Format("Crc32:{0},Utils-Crc:{1}",crc,crc1));

                                // 加载完成，切换到主线程回调
                                Loom.QueueOnMainThread(() => {
                                    callback(this);
                                });

                                loaded = true;
                            }
                            else
                            {
                                if (failCallback != null)
                                    failCallback(HttpResp.ErrorType.LogicError, "file read error");
                            }
                        }, null);
                    }
                });

            }
            else
            {
                if (failCallback != null)
                    failCallback(HttpResp.ErrorType.LogicError, "file not found");

            }
        }

        /// <summary>
        /// 获取文件标准路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        bool GetFileNameFromPath(string path, out string filename)
        {
            int index = path.LastIndexOf("\\");
            if (index > 0)
            {
                filename = path.Substring(index + 1);
                return true;
            }

            index = path.LastIndexOf("/");
            if (index > 0)
            {
                filename = path.Substring(index + 1);
                return true;
            }
            filename = "";
            return false;
        }

    }
    #endregion

    #region Http Resource Node
    public class HttpResourceNode : IResourceNode
    {
        private string path;
        private string crc;
        private string fileName;
        private byte[] bins;
        private object res;
        public bool loaded;
        private ResType resType;
        public HttpResourceNode(string fName, string fcrc)
        {
            fileName = fName;
            crc = fcrc;
            resType = ResManager.GetResType(fileName);
            loaded = false;
        }

        public string GetCrc()
        {
            return crc;
        }
        public object GetResource()
        {
            return res;
        }

        public string GetName()
        {
            return fileName;
        }

        public ResType GetResType()
        {
            return resType;
        }

        public void LoadResource(Action<IResourceNode> callback, Action<HttpResp.ErrorType, string> failCallback = null)
        {

            HttpService.GetDownloadURL(fileName.ToLower(), crc, downloadUrl =>
            {
                if (resType.Equals(ResType.AssetBundle))
                {
                    HttpService.GetRemoteAB(downloadUrl, (ab) =>
                    {
                        res = ab;
                        loaded = true;
                        callback(this);

                    }, (eType, eInfo) => {
                        if (failCallback != null)
                            failCallback(HttpResp.ErrorType.LogicError, "file read error");
                    });
                }
                else
                {
                    HttpService.GetRemoteRaw(downloadUrl, filebins =>
                    {
                        res = filebins;
                        this.loaded = true;
                        callback(this);
                    }, (resp) =>
                    {
                        if (failCallback != null)
                            failCallback(HttpResp.ErrorType.LogicError, "file read error");
                    });
                }


            }, (errorType, errorInfo) =>
            {
                if (failCallback != null)
                {
                    failCallback(HttpResp.ErrorType.LogicError, errorInfo);
                }
            });
        }
    }
    public interface IResourceNode
    {
        string GetCrc();
        object GetResource();

        string GetName();

        ResType GetResType();

        void LoadResource(Action<IResourceNode> callback, Action<HttpResp.ErrorType, string> failCallback = null);
    }
    #endregion
}

