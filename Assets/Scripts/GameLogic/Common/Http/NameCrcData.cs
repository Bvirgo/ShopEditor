using Jhqc.EditorCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using ZFrameWork;

namespace ArtsWork
{
    public class NameCrcData
    {
        public RemoteDataCoroutine Request { get { return _request; } }
        RemoteDataCoroutine _request;
        //public bool IsFinishDataInterchange { get { return _request.IsFinishDataInterchange; } }
        public bool IsRequestSuccess { get { return _request.IsRequestSuccess; } }
        public bool IsFinishDataInterchange { get { return _request.IsFinishDataInterchange; } }
        public HttpResp HttpResp_Error { get { return _request.HttpResp_Error; } }

        NameCrc _nameCrc;
        public NameCrc InsNameCrc { get { return _nameCrc; } }
        public string Name { get { return _nameCrc.Name; } }
        public string Crc { get { return _nameCrc.Crc; } }

        LocalCacheEntry.CacheType _cacheType = LocalCacheEntry.CacheType.Raw;
        public LocalCacheEntry.CacheType CacheType
        {
            get { return _cacheType; }
            set { _cacheType = value; }
        }

        //Texture2D _dataTexture2D;
        //AssetBundle _dataAB;

        public NameCrcData()
        {
            Init();
            _cacheType = LocalCacheEntry.CacheType.Raw;
            _nameCrc = new NameCrc();
        }

        public NameCrcData(string name, string crc, LocalCacheEntry.CacheType cacheType = LocalCacheEntry.CacheType.Raw)
        {
            Init();
            _nameCrc = new NameCrc(name, crc);
            _cacheType = cacheType;
        }
        public NameCrcData(string urlOrNcKey, LocalCacheEntry.CacheType cacheType = LocalCacheEntry.CacheType.Raw)
        {
            Init();

            string name, crc;
            name = crc = string.Empty;
            if (IsUrlOrNcKey(urlOrNcKey, ref name, ref crc))
            {
                // url
                if (string.IsNullOrEmpty(name))
                {
                    _nameCrc = new NameCrc();
                    _nameCrc.SetData(urlOrNcKey, true);
                }
                else // ncKey
                {
                    _nameCrc = new NameCrc(name, crc);
                }
            }
            else
            {
                Debug.LogError("urlOrNcKey is not url or namecrcKey");
            }
            _cacheType = cacheType;
        }

        public static bool IsUrlOrNcKey(string str)
        {
            string name, crc;
            name = crc = null;
            return IsUrlOrNcKey(str, ref name, ref crc);
        }

        public static bool IsUrlOrNcKey(string str, ref string name, ref string crc)
        {
            string __name, __crc;
            //__name = __crc = string.Empty;
            if (HttpService.IsUrl(str))
            {
                return true;
            }
            else if (NameCrc.GetNameAndCrcFromKey(str, out __name, out __crc, false))
            {
                if (name != null)
                {
                    name = __name;
                }
                if (crc != null)
                {
                    crc = __crc;
                }
                return true;
            }
            return false;
        }

        public NameCrcData(string name, byte[] data)
        {
            Init();
            _request.SetData(data);

            _cacheType = LocalCacheEntry.CacheType.Raw;
            var crc = Crc32.CountCrc(data);
            _nameCrc = new NameCrc(name, crc.ToString());
        }

        void Init()
        {
            _request = new RemoteDataCoroutine();
            _request.IsShowErrorDlg = true;
        }

        public void AsyncRemoteGetData()
        {
            _request.ActionRequest = ActionReqest_RemoteGetData;
            _request.AsyncSendRequest();
        }

        void ActionReqest_RemoteGetData(List<object> listPms)
        {
            ZFrameWork.MonoHelper.Instance.StartCoroutine(Co_Reqest_RemoteGetData());
        }

        IEnumerator Co_Reqest_RemoteGetData()
        {
            if (!_nameCrc.IsRequestSuccess)
            {
                _nameCrc.CallBack_AfterRequestFailed = _request.CallBack_AfterRequestFailed;
                _nameCrc.AsyncSendRequest();

                yield return _nameCrc.WaitForResult();
            }

            if (!_nameCrc.IsRequestSuccess)
            {
                _request.HttpResp_Error = _nameCrc.HttpResp_Error;
                yield break;
            }

            switch (_cacheType)
            {
                case LocalCacheEntry.CacheType.Texture:
                    break;
                case LocalCacheEntry.CacheType.Fbx:
                    break;
                case LocalCacheEntry.CacheType.AssetBundle:
                    break;
                case LocalCacheEntry.CacheType.Raw:
                    {
                        Request_GetRemoteRaw();
                    }
                    break;
                default:
                    yield break;
            }
        }
        void Request_GetRemoteRaw()
        {
            HttpService.GetRemoteRaw(_nameCrc.URL, (data) =>
            {
                _request.SetData(data);
                _request.OnRequestFinished(true, null);
            }, (resp) =>
            {
                _request.OnRequestFinished(false, resp);
            });
        }

        
        bool ActionCheckIsValidRequest_UploadData()
        {
            return !string.IsNullOrEmpty(_nameCrc.Name) && !string.IsNullOrEmpty(_nameCrc.Crc) && _request.GetDataBytes() != null;
        }

        public IEnumerator WaitForResult()
        {
            yield return _request.WaitForResult();
        }

        public byte[] GetDataBytes()
        {
            return _request.GetDataBytes();
        }

        public T GetData<T>()
        where T : class
        {
            return _request.GetData<T>();
        }
    }
}
