
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Jhqc.EditorCommon;

namespace ArtsWork
{
    public class NameCrc : RemoteDataCoroutine
    {
        public const string StrSplit = "#Name____Crc#";
        string _name;
        public string Name { get { return _name; } }

        string _crc;
        public string Crc { get { return _crc; } }

        string _keyCombined;
        public string KeyCombined
        {
            get
            {
                if (string.IsNullOrEmpty(_keyCombined))
                {
                    return GetCombinedKey(_name, _crc);
                }
                else
                {
                    return _keyCombined;
                }
            }
        }

        public string URL
        {
            get
            {
                var ret = GetData<string>();
                return ret != null ? ret : string.Empty;
            }
        }

        const int Default_MaxRemoteRequesetCount = 3;
        const bool Default_IsShowErrorDlg = true;

        public NameCrc(string name, string crc)
        {
            _name = name;
            _crc = crc;
        }

        public NameCrc()
        {

        }

        public NameCrc(string combinedKey)
        {
            SetNameCrcCombinedKey(combinedKey);
        }

        static public bool GetNameAndCrcFromKey(string keyCombined, out string name, out string crc, bool isErrorLog = true)
        {
            name = crc = string.Empty;
            string[] strSplited = System.Text.RegularExpressions.Regex.Split(keyCombined, StrSplit);
            if (strSplited.Length == 2)
            {
                name = strSplited[0];
                crc = strSplited[1];
                return true;
            }
            else
            {
                if (isErrorLog)
                {
                    Debug.LogError("文件名特殊");
                }                
                return false;
            }
        }

        static public string GetCombinedKey(string name, string crc)
        {
            return name + StrSplit + crc;
        }

        static public bool IsEqual(string keyCombined, string name, string crc)
        {
            return GetCombinedKey(name, crc).Equals(keyCombined);
        }

        public void SetNameCrcCombinedKey(string combinedKey)
        {
            _keyCombined = combinedKey;
            GetNameAndCrcFromKey(combinedKey, out _crc, out _name, true);
        }

        public override bool IsValidRequest()
        {
            return (!(string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(_crc)));
        }

        protected override IEnumerator Co_RecurSendRequest()
        {
            HttpService.GetDownloadURL(_name, _crc, (urlGot) =>
            {
                _data = urlGot;
                OnRequestFinished(true, null);
            },
            (errType, errStr) =>
            {
                HttpResp resp = new HttpResp();
                resp.Error = errType;
                resp.WwwText = errStr;

                OnRequestFinished(false, resp);
            }
            );
            yield return null;
        }
    }
}
