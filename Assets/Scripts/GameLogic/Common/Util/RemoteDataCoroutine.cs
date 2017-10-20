using Jhqc.EditorCommon;
using System;
using System.Collections;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using ZFrameWork;

namespace ArtsWork
{
    public interface IRemoteCoroutine
    {
        void AsyncSendRequest(bool isForceRequest = false);
        Coroutine WaitForResult();
        void OnRequestFinished(bool isSuccess, HttpResp resp);
    }

    //RemoteDataCoroutine<TResult> HttpResp
    public class RemoteDataCoroutine : IRemoteCoroutine
    {
        static protected MonoBehaviour _coroutineProxy = ZFrameWork.MonoHelper.Instance;

        protected bool _isDataInterchanging;
        protected bool _isForceRequest;

        protected bool _isFinishDataInterchange;
        public bool IsFinishDataInterchange { get { return _isFinishDataInterchange & !_isForceRequest; } }

        int _currentRequestTimes;
        const int DefaultReconnectTimes = 3;
        protected int _maxReconnectTimes = DefaultReconnectTimes;
        public int MaxReconnectTimes
        {
            get { return _maxReconnectTimes; }
            set
            {
                if (_maxReconnectTimes != value)
                {
                    _maxReconnectTimes = value;
                    float newTime = WWWManager.Instance.TimeOut * _maxReconnectTimes;
                    if (newTime > _maxWaitTime)
                    {
                        _maxWaitTime = newTime;
                    }
                }
            }
        }

        protected bool _isRequestSuccess;
        public bool IsRequestSuccess { get { return _isRequestSuccess; } }

        protected object _data;

        //public delegate void FuncPramsObj(params object[] pms);
        public delegate void FuncPramsObj(List<object> listPms);
        public FuncPramsObj ActionRequest { get; set; }
        List<object> _actionRequestParams;
        public List<object> ActionRequestParams
        {
            get
            {
                if (null == _actionRequestParams)
                {
                    _actionRequestParams = new List<object>();
                }
                return _actionRequestParams;
            }
        }

        public Func<bool> ActionCheckIsValidRequest { get; set; }
        #region Generic to do?       
        //public FuncPramsObj CallBack_RequestSuccess { get; set; }
        //List<object> _allBack_RequestResultParams;
        //public List<object> CallBack_RequestSuccessParams
        //{
        //    get
        //    {
        //        if (null == _allBack_RequestResultParams)
        //        {
        //            _allBack_RequestResultParams = new List<object>();
        //        }
        //        return _allBack_RequestResultParams;
        //    }
        //}
        //public FuncPramsObj CallBack_RequestFailed { get; set; }
        //public List<object> CallBack_RequestFailedParams
        //{
        //    get
        //    {
        //        if (null == _allBack_RequestResultParams)
        //        {
        //            _allBack_RequestResultParams = new List<object>();
        //        }
        //        return _allBack_RequestResultParams;
        //    }
        //}
        #endregion

        public HttpResp HttpResp_Error { get; set; }
        public Action<HttpResp> CallBack_AfterRequestSuccess { get; set; }
        public Action<HttpResp> CallBack_AfterRequestFailed { get; set; }

        public bool IsShowErrorInfo { get; set; }
        public bool IsShowErrorDlg { get; set; }

        /// <summary>
        /// Set Before OnRequestFinished
        /// </summary>
        public string ErrorMsg { get; set; }

        protected StringBuilder _sb;

        float _maxWaitTime;
        public float MaxWaitTime { get; set; }
        public RemoteDataCoroutine()
        {
            Init();
        }

        protected virtual void Init()
        {
            ResetStateFlags();
            IsShowErrorInfo = true;
            IsShowErrorDlg = false;
            _sb = new StringBuilder();
            _maxWaitTime = WWWManager.Instance.TimeOut * _maxReconnectTimes;
        }

        bool _isValidRequest;
        public virtual bool IsValidRequest()
        {
            if (ActionCheckIsValidRequest != null)
            {
                _isValidRequest = ActionCheckIsValidRequest();
            }
            else
            {
                _isValidRequest = true;
            }
            return _isValidRequest;
        }

        public virtual void AsyncSendRequest(bool isForceRequest = false)
        {
            if (!IsValidRequest())
            {
                if (ActionCheckIsValidRequest != null)
                {
                    Debug.LogError(ActionCheckIsValidRequest.ToString());
                }
                Debug.LogError("Invalide Request");
                OnRequestFinished(false, null);
                return;
            }

            if (isForceRequest)
            {
                if (_isForceRequest)
                {
                    return;
                }
                else
                {
                    _isForceRequest = isForceRequest;
                }
            }
            else
            {
                if (_isDataInterchanging)
                {
                    return;
                }
            }
            _coroutineProxy.StartCoroutine(Co_SendRequest(isForceRequest));
        }

        /// <summary>
        /// yield return WaitForResult();
        /// </summary>
        /// <returns></returns>
        public Coroutine WaitForResult()
        {
            return _coroutineProxy.StartCoroutine(Co_WaitForResult());
        }

        float _timeWaitStart;
        IEnumerator Co_WaitForResult()
        {
            _timeWaitStart = Time.time;
            while (!_isFinishDataInterchange)
            {
                if (Time.time - _timeWaitStart > _maxWaitTime)
                {
                    HttpResp resp = new HttpResp();
                    resp.Error = HttpResp.ErrorType.Timeout;
                    resp.WwwText = "Client Wait TimeOut";
                    After_RequestFinished(false, resp);
                }
                yield return null;
            }
        }

        public byte[] GetDataBytes()
        {
            return GetData<byte[]>();
        }

        public object GetData()
        {
            return _data;
        }

        public void SetData(object data, bool isSetFlag = false)
        {
            _data = data;
            if (isSetFlag)
            {
                SetStateFlags_RequestFinished();
                SetStateFlags_RequestSuccess();
            }
        }

        public T GetData<T>()
        where T : class
        {
            return GetData() as T;
        }

        IEnumerator Co_SendRequest(bool isForceRequest = false)
        {
            if (_isDataInterchanging)
            {
                yield return null;
            }

            if (isForceRequest || !_isRequestSuccess)
            {
                ResetStateFlags();
                _isDataInterchanging = true;
                _coroutineProxy.StartCoroutine(Co_RecurSendRequest());
            }
        }

        void ResetStateFlags()
        {
            _currentRequestTimes = 0;
            _isFinishDataInterchange = _isDataInterchanging = _isRequestSuccess = false;
            _isForceRequest = false;
            ErrorMsg = string.Empty;
        }

        protected virtual IEnumerator Co_RecurSendRequest()
        {
            if (ActionRequest != null)
            {
                #region Generic to do?
                //if (_actionRequestParams != null)
                //{
                //    var funcSuccess = _actionRequestParams.TryGetReturnValue(_actionRequestParams.Count - 2) as FuncPramsObj;
                //    var funcFail= _actionRequestParams.TryGetReturnValue(_actionRequestParams.Count - 1) as FuncPramsObj;

                //    if (funcSuccess != null)
                //    {
                //        // default callback
                //        funcSuccess += CallBack_RequestSuccess;
                //    }
                //    else
                //    {
                //        funcSuccess = CallBack_RequestSuccess;
                //    }
                //    if (funcFail != null)
                //    {
                //        funcFail += CallBack_RequestFailed;
                //    }
                //    else
                //    {
                //        funcFail = CallBack_RequestFailed;
                //    }
                //}
                #endregion
                //yield return null;
                ActionRequest.Invoke(ActionRequestParams);
                yield return null;
            }
            //throw new NotImplementedException();
        }

        public virtual void OnRequestFinished(bool isSuccess, HttpResp resp)
        {
            ++_currentRequestTimes;
            if (isSuccess)
            {
                OnRequestSuccessed(resp);
            }
            else
            {
                OnRequestFailed(resp);
            }
        }

        public void OnRequestFinished(bool isSuccess, HttpResp.ErrorType errType, string wwwText)
        {
            var resp = new HttpResp();
            resp.Error = errType;
            resp.WwwText = wwwText;
            OnRequestFinished(isSuccess, resp);
        }

        void OnRequestSuccessed(HttpResp resp)
        {
            After_RequestFinished(true, resp);
        }

        void OnRequestFailed(HttpResp resp)
        {
            if (_currentRequestTimes >= _maxReconnectTimes || !_isValidRequest)
            {
                After_RequestFinished(false, resp);
            }
            else
            {
                _coroutineProxy.StartCoroutine(Co_RecurSendRequest());
            }
        }

        void SetStateFlags_RequestFinished()
        {
            _isDataInterchanging = false;
            _isFinishDataInterchange = true;
        }

        protected virtual void After_RequestFinished(bool isSuccess, HttpResp resp)
        {
            HttpResp_Error = resp;
            SetStateFlags_RequestFinished();
            if (isSuccess)
            {
                After_RequestSuccess(resp);
            }
            else
            {
                After_RequestFailed(resp);
            }
        }

        void SetStateFlags_RequestSuccess()
        {
            _isRequestSuccess = true;
        }

        protected void After_RequestSuccess(HttpResp resp)
        {
            SetStateFlags_RequestSuccess();
            if (CallBack_AfterRequestSuccess != null)
            {
                CallBack_AfterRequestSuccess(resp);
            }
        }

        protected void After_RequestFailed(HttpResp resp)
        {
            string erMsg = ErrorMsg;
            if (string.IsNullOrEmpty(erMsg))
            {
                if (resp != null)
                {
                    _sb.Length = 0;
                    _sb.Append(resp.Error);
                    _sb.AppendLine();
                    _sb.Append(resp.WwwText);
                    erMsg = _sb.ToString();
                }
                else
                {
                    if (ActionRequest != null)
                    {
                        erMsg = ActionRequest.ToString();
                    }
                    else
                    {
                        erMsg = "Reqeust Failed" + this.ToString();
                    }
                }
            }

            if (IsShowErrorInfo)
            {
                Debug.LogError(erMsg);
            }
            if (IsShowErrorDlg)
            {

            }

            if (CallBack_AfterRequestFailed != null)
            {
                CallBack_AfterRequestFailed(resp);
            }
        }

    }
}