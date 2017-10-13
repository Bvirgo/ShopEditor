using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using System.Collections;
using System.IO;
using System.Text;


namespace Jhqc.EditorCommon
{
    /// <summary>
    /// 处理编辑器相关的网络请求
    /// </summary>
    public class WWWManager : MonoBehaviour
    {
        #region  Base Member
        public delegate void OnHttpResponse(HttpResp resp);
        public delegate void OnHttpGetFileResponse(HttpResp resp, LocalCacheEntry entry);
        public delegate void OnHttpStatusChange(HttpStatus status);
        public delegate void OnHttpError(HttpResp.ErrorType error);

        /// <summary>
        /// Http状态变化的event，如果要做收发请求的modal，可以+=这个事件
        /// 现在没实现，科科
        /// </summary>
        public event OnHttpStatusChange ChangeEvent;

        private readonly string EXPIRED_ERROR = "access_token_expired";

        // 服务器地址
        private string address;
        private string loginAddress;
        private string LoginAddress
        {
            get
            {
                if (string.IsNullOrEmpty(loginAddress))
                {
                    return address + "/users/user_login";
                }
                else
                {
                    return loginAddress;
                }
            }
        }

        private string accessToken;
        private int userId = -1;
        private bool isInit = false;
        public bool IsInit
        {
            get { return isInit; }
        }

        private float timeout = 5f;
        /// <summary>
        /// timeout in seconds
        /// 默认5s
        /// </summary>
        public float TimeOut
        {
            set
            {
                if (value > 0)
                {
                    timeout = value;
                }
            }
            get
            {
                return timeout;
            }
        }

        private int concurrentLimit = 3;
        /// <summary>
        /// 并发(请求返回前，最大允许发起的)请求数
        /// 默认3个
        /// </summary>
        private int ConcurrentLimit
        {
            set
            {
                if (value >= 1)
                {
                    concurrentLimit = value;
                }
            }
            get
            {
                return concurrentLimit;
            }
        }

        //不是真的multi-thread就是爽
        private static int requestIdGen = 0;

        // www 请求队列
        private Queue<WWWRequest> waitingRequests = new Queue<WWWRequest>();

        // www对象添加唯一标识
        private Dictionary<int, WWWRequest> sendingRequests = new Dictionary<int, WWWRequest>();

        private static WWWManager instance;
        public static WWWManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("WWWManager");
                    instance = go.AddComponent<WWWManager>();
                    DontDestroyOnLoad(go);

                    if (!Directory.Exists(CacheFolder))
                    {
                        Directory.CreateDirectory(CacheFolder);
                    }
                }

                return instance;
            }
        }

        // 本地缓存文件夹
        public static string CacheFolder
        {
            get
            {
                return Application.persistentDataPath + "/wwwcache/";
            }
        }
        #endregion

        #region  Main Loop
        /// <summary>
        /// 网络请求发动机
        /// </summary>
        /// <returns></returns>
        private IEnumerator MainLoop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                var now = DateTime.Now;

                //handle response
                if (sendingRequests.Count > 0)
                {
                    // 已经发起过的RawWWW
                    var disposableKeys = new List<int>();

                    foreach (var key in sendingRequests.Keys)
                    {
                        var sendingOne = sendingRequests[key];
                        var www = sendingOne.RawWWW;

                        var isTimeout = false;

                        if (!www.isDone)
                        {
                            var cost = (now - sendingOne.StartTime).TotalMilliseconds;
                            //timeout 
                            if (cost > TimeOut * 1000)
                            {
                                sendingOne.RespCallback(null);
                                isTimeout = true;
                                InfoTips.LogInfo("Timeout cost: " + cost.ToString());
                            }
                            else
                            {
                                // www没有下载完，也没超时，那就继续
                                continue;
                            }
                        }

                        // www，没有完成，但是超时
                        if (!isTimeout)
                        {
                            if (www.error != null)
                            {
                                InfoTips.LogInfo(www.error);
                                sendingOne.RespCallback(www);
                            }
                            else //not timeout & isdone
                            {
                                InfoTips.LogInfo("[HttpResp] " + www.text);
                                sendingOne.RespCallback(www);
                            }
                        }

                        // 既然能走到这里，说明www已经完成
                        www.Dispose();
                        disposableKeys.Add(key);
                    }

                    foreach (var key in disposableKeys)
                    {
                        sendingRequests.Remove(key);
                    }
                }

                //do send
                var availableCount = concurrentLimit - sendingRequests.Count;

                // 当前发起请求数，没有超过阈值，从等待队列中选
                if (availableCount > 0 && waitingRequests.Count > 0)
                {
                    //one packet per frame
                    var req = waitingRequests.Dequeue();

                    BeginRequest(req);
                }
            }
        }

        private void SetStatus(HttpStatus status)
        {
            if (ChangeEvent != null)
            {
                ChangeEvent(status);
            }
        }

        //unity www的设计是：new出来的瞬间，请求就立即发起了
        //由于排队的需求，只能推迟new的时机
        private void BeginRequest(WWWRequest req)
        {
            WWW www = null;
            switch (req.Type)
            {
                case RequestType.Login:
                    www = new WWW(req.Url, req.Data);
                    break;
                case RequestType.Get:
                    www = new WWW(req.Url, null, req.Headers);
                    break;
                case RequestType.Post:
                    www = new WWW(req.Url, req.Data, req.Headers);
                    break;
                default:
                    InfoTips.LogWarning("undefined type: " + req.Type.ToString());
                    break;
            }

            req.RawWWW = www;
            req.StartTime = DateTime.Now;

            sendingRequests.Add(requestIdGen++, req);
        }
        /// <summary>
        /// 网络模块初始化
        /// </summary>
        /// <param name="address">服务器地址</param>
        /// <param name="logType">日志</param>
        /// <param name="loginAddress">登录接口地址</param>
        /// <param name="userId">已经登录的用户ID</param>
        /// <param name="accessToken">用户票据</param>
        public void Init(string address, LogType logType, string loginAddress = null, int userId = -1, string accessToken = null)
        {
            if (isInit)
            {
                return;
            }

            this.address = address;
            this.loginAddress = loginAddress;

            var logger = gameObject.AddComponent<InfoTips>();
            logger.logType = logType;

            StartCoroutine(MainLoop());

            if (userId != -1)
            {
                this.userId = userId;
                this.accessToken = accessToken;
            }

            isInit = true;
        }
        #endregion

        #region Login
        /// <summary>
        /// 登录，其他所有操作都要在登录成功后进行
        /// 跟其他http谓词都不一样，没有排队，直接发送了
        /// </summary>
        public void Login(string name, string password, OnHttpResponse cb)
        {
            var queryString = string.Format("email={0}&password={1}", name, password);
            var fullURL = LoginAddress + "?" + queryString;
            var seemsUselessBytes = Encoding.UTF8.GetBytes(queryString);

            var req = new WWWRequest()
            {
                Data = seemsUselessBytes,
                Headers = null,
                RespCallback = www =>
                {
                    if (www == null)
                    {
                        cb(GenTimeoutResp());
                        return;
                    }

                    var resp = new HttpResp();

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        resp.Error = HttpResp.ErrorType.NetworkError;
                        resp.ErrorText = www.error;
                    }
                    else
                    {
                        var loginResp = JsonUtility.FromJson<LoginResp>(www.text);

                        if (!string.IsNullOrEmpty(loginResp.error))
                        {
                            resp.Error = HttpResp.ErrorType.LogicError;
                            resp.ErrorText = loginResp.error;
                        }
                        else
                        {
                            resp.Error = HttpResp.ErrorType.None;

                            //set global identity
                            userId = loginResp.user_id;
                            accessToken = loginResp.access_token;
                        }
                    }

                    resp.WwwText = www.text;
                    cb(resp);
                },
                Type = RequestType.Login,
                Url = fullURL
            };
            waitingRequests.Enqueue(req);
        }

        #endregion

        #region Get
        /// <summary>
        /// 普通get
        /// </summary>
        public void Get(string resource, NameValueCollection query, OnHttpResponse callback)
        {
            var fullURL = GetHttpPrefix() + resource + QueryToString(query);

            InfoTips.LogInfo("[HttpGet] " + fullURL);

            var req = new WWWRequest()
            {
                Data = null,
                Headers = GetAuthHeader(),
                RespCallback = RespWrapper(callback),
                Type = RequestType.Get,
                Url = fullURL
            };
            waitingRequests.Enqueue(req);
        }

        private void GetRaw(string resource, NameValueCollection query, Action<WWW> callback)
        {
            var fullURL = resource + QueryToString(query);

            InfoTips.LogInfo("[HttpPrivateGet] " + fullURL);

            var req = new WWWRequest()
            {
                Data = null,
                Headers = GetAuthHeader(),
                RespCallback = callback,
                Type = RequestType.Get,
                Url = fullURL
            };
            waitingRequests.Enqueue(req);
        }

        /// <summary>
        /// 带本地缓存的http文件获取
        /// 本地版本实际上是同步，网络版本是异步</summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        /// <param name="fromQY">是否是从青云服务器上下载，区别于：百度API获取街景</param>
        public void GetFile(string url, LocalCacheEntry.CacheType type, OnHttpGetFileResponse callback, bool fromQY = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("Attempt to getfile from an empty URL");
            }

            // 文件缓存之后的文件名
            var local = URLToLocalPath(url);
            InfoTips.LogInfo("[GetFile] related local path:" + local);

            // 缓存中已经存在
            if (File.Exists(local))
            {
                InfoTips.LogInfo("[GetFile] load from local");

                var fakeResp = new HttpResp();
                fakeResp.Error = HttpResp.ErrorType.None;
                fakeResp.WwwText = "";

                switch (type)
                {
                    case LocalCacheEntry.CacheType.Texture:
                        var tex = new Texture2D(2, 2);
                        var imgBytes = File.ReadAllBytes(local);
                        tex.LoadImage(imgBytes);

                        callback(fakeResp, new LocalCacheEntry()
                        {
                            Type = LocalCacheEntry.CacheType.Texture,
                            Texture = tex
                        });
                        break;

                    case LocalCacheEntry.CacheType.Fbx:
                        callback(fakeResp, new LocalCacheEntry()
                        {
                            Type = LocalCacheEntry.CacheType.Fbx,
                            FbxPath = local
                        });
                        break;

                    case LocalCacheEntry.CacheType.AssetBundle:
                        var ab = AssetBundle.LoadFromFile(local);

                        callback(fakeResp, new LocalCacheEntry()
                        {
                            Type = LocalCacheEntry.CacheType.AssetBundle,
                            AB = ab
                        });
                        break;

                    case LocalCacheEntry.CacheType.Raw:
                        var bytes = File.ReadAllBytes(local);

                        callback(fakeResp, new LocalCacheEntry()
                        {
                            Type = LocalCacheEntry.CacheType.Raw,
                            Bytes = bytes
                        });

                        break;
                    default:
                        break;
                }
            }
            else
            {
                InfoTips.LogInfo("[GetFile] load from remote");

                string realUrl;
                NameValueCollection query;

                if (fromQY)
                {
                    realUrl = GetHttpPrefix() + "entities/download";
                    query = new NameValueCollection()
                    {
                        { "path", url }
                    };
                }
                else
                {
                    realUrl = url;
                    query = null;
                }

                GetRaw(realUrl, query, www =>
                {
                    var resp = GenerateGetfileResp(www);

                    if (resp.Error != HttpResp.ErrorType.None)
                    {
                        callback(resp, null);
                    }
                    else
                    {
                        //write cache
                        //TODO: more scientific way: async
                        var localPath = URLToLocalPath(url);

                        if (localPath.Length > 250)
                        {
                            throw new IOException("localpath length overflowwww!!!");
                        }

                        File.WriteAllBytes(localPath, www.bytes);

                        switch (type)
                        {
                            case LocalCacheEntry.CacheType.Texture:
                                callback(resp, new LocalCacheEntry()
                                {
                                    Type = LocalCacheEntry.CacheType.Texture,
                                    Texture = www.texture
                                });
                                break;
                            case LocalCacheEntry.CacheType.Fbx:
                                callback(resp, new LocalCacheEntry()
                                {
                                    Type = LocalCacheEntry.CacheType.Fbx,
                                    FbxPath = localPath
                                });
                                break;
                            case LocalCacheEntry.CacheType.AssetBundle:
                                callback(resp, new LocalCacheEntry()
                                {
                                    Type = LocalCacheEntry.CacheType.AssetBundle,
                                    AB = www.assetBundle
                                });
                                break;
                            case LocalCacheEntry.CacheType.Raw:
                                callback(resp, new LocalCacheEntry()
                                {
                                    Type = LocalCacheEntry.CacheType.Raw,
                                    Bytes = www.bytes
                                });
                                break;
                            default:
                                break;
                        }
                    }
                });
            }
        }
        #endregion

        #region Post
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="standard">标准的上传文件接口，都会默认在query里带上name和crc</param>
        public void Post(string resource, NameValueCollection query, string fileName, byte[] fileData, OnHttpResponse callback, bool standard = true)
        {
            string fullURL = GetHttpPrefix() + resource;

            var postParameters = new Dictionary<string, object>();

            // 标准模式：只需要name,crc
            if (standard)
            {
                postParameters.Add("name", query["name"]);
                postParameters.Add("crc", query["crc"]);
            }
            // Post过去所有的键值对
            else
            {
                foreach (var key in query.AllKeys)
                {
                    postParameters.Add(key, query[key]);
                }
            }
            postParameters.Add("file", new FormUpload.FileParameter(fileData, fileName, "application/octet-stream"));

            var headers = GetAuthHeader();
            string contentType;
            var bytes = FormUpload.GetHTTPBody(postParameters, out contentType);

            headers.Add("Content-Type", contentType);

            var req = new WWWRequest()
            {
                Data = bytes,
                Headers = headers,
                RespCallback = RespWrapper(callback),
                Type = RequestType.Post,
                Url = fullURL
            };
            waitingRequests.Enqueue(req);
        }

        /// <summary>
        /// 普通post
        /// </summary>
        public void Post(string resource, NameValueCollection query, OnHttpResponse callback)
        {
            Post(resource, ToJson(query), callback);
        }

        public void Post(string resource, string jsonString, OnHttpResponse callback)
        {
            var fullURL = GetHttpPrefix() + resource;
            var headers = GetAuthHeader();
            headers.Add("Content-Type", "application/json");

            var postJson = jsonString;//.ToString();

            InfoTips.LogInfo("[HttpPost] " + fullURL);
            InfoTips.LogInfo("postJson:" + postJson + "--Headers:" + headers);

            var formData = Encoding.UTF8.GetBytes(jsonString);

            var req = new WWWRequest()
            {
                Data = formData,
                Headers = headers,
                RespCallback = RespWrapper(callback),
                Type = RequestType.Post,
                Url = fullURL
            };
            waitingRequests.Enqueue(req);
        }

        public void RemoveCache()
        {
            var allCacheFiles = Directory.GetFiles(CacheFolder);
            var count = 0;
            foreach (var file in allCacheFiles)
            {
                File.Delete(file);
                count++;
            }

            InfoTips.LogInfo("[Http] Remove total:" + count.ToString() + " cache files");
        }

        #endregion

        #region Response
        //不需要解json
        private HttpResp GenerateGetfileResp(WWW www)
        {
            if (www == null)
            {
                return GenTimeoutResp();
            }

            var resp = new HttpResp();

            if (!string.IsNullOrEmpty(www.error))
            {
                resp.Error = HttpResp.ErrorType.NetworkError;
                resp.ErrorText = www.error;
            }
            else
            {
                //allright
                resp.Error = HttpResp.ErrorType.None;
                resp.WwwText = www.text;
            }

            return resp;
        }

        //需要解json
        private HttpResp GenerateResp(WWW www)
        {
            if (www == null)
            {
                return GenTimeoutResp();
            }

            var resp = new HttpResp();

            if (!string.IsNullOrEmpty(www.error))
            {
                resp.Error = HttpResp.ErrorType.NetworkError;
                resp.ErrorText = www.error;
            }
            else
            {
                try
                {
                    var respJson = JsonUtility.FromJson<JsonBase>(www.text);

                    if (respJson.error == EXPIRED_ERROR)
                    {
                        resp.Error = HttpResp.ErrorType.AccessExpired;
                        resp.ErrorText = resp.Error.ToString();
                    }
                    else if (!string.IsNullOrEmpty(respJson.error))
                    {
                        resp.Error = HttpResp.ErrorType.LogicError;
                        resp.ErrorText = respJson.error;
                    }
                    else
                    {
                        resp.Error = HttpResp.ErrorType.None;
                    }
                }
                catch (Exception e)
                {
                    InfoTips.LogWarning("parse json error: " + e.ToString());

                    resp.Error = HttpResp.ErrorType.JsonError;
                    resp.ErrorText = e.ToString();
                }
            }

            resp.WwwText = www.text;
            return resp;
        }

        private Action<WWW> RespWrapper(OnHttpResponse rawCb)
        {
            return (WWW www) =>
            {
                rawCb(GenerateResp(www));
            };
        }

        private static HttpResp GenTimeoutResp()
        {
            return new HttpResp()
            {
                Error = HttpResp.ErrorType.Timeout,
                ErrorText = "",
                WwwText = "",
            };
        }

        private string URLToLocalPath(string url)
        {
            var fileName = WWW.EscapeURL(url).GetHashCode().ToString(); //collision了我也没办法，报错呗
            return CacheFolder + fileName;
        }

        private string QueryToString(NameValueCollection query)
        {
            var result = "";

            if (query == null || query.AllKeys.Length == 0)
            {
                return result;
            }

            foreach (var key in query.AllKeys)
            {
                var escapedValue = WWW.EscapeURL(query[key]);
                result += string.Format("{0}={1}&", key, escapedValue);
            }

            return "?" + result.Substring(0, result.Length - 1);
        }

        //根据URL参数转出来的Json十分简单
        //自己写就可以了
        private static string ToJson(NameValueCollection nvc)
        {
            var result = "{";

            foreach (var key in nvc.AllKeys)
            {
                result += string.Format("\"{0}\": \"{1}\",", key, nvc[key]);
            }

            result = result.TrimEnd(',');
            result += "}";

            return result;
        }

        private Dictionary<string, string> GetAuthHeader()
        {
            if (string.IsNullOrEmpty(accessToken) || userId == -1)
            {
                throw new NullReferenceException("user auth not initialized, make sure call http method after login");
            }
            else
            {
                return new Dictionary<string, string>()
                {
                    { "accesstoken", accessToken },
                    { "playerid", userId.ToString() }
                };
            }
        }

        private string GetHttpPrefix()
        {
            return string.Format("http://{0}/", address);
        }
        #endregion

        #region Private Class
        private class WWWRequest
        {
            public string Url { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public byte[] Data { get; set; }
            public RequestType Type { get; set; }
            public Action<WWW> RespCallback { get; set; }
            public DateTime StartTime { get; set; }
            public WWW RawWWW { get; set; }
        }

        private enum RequestType
        {
            Login,
            Get,
            Post
        }

        /// <summary>
        /// 
        /// </summary>
        public enum HttpStatus
        {
            Transmitting,
            Finished
        }
        #endregion
    }
}