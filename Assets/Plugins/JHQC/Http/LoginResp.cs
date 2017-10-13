using UnityEngine;
using System;

namespace Jhqc.EditorCommon
{
    [Serializable]
    public class LoginResp : JsonBase
    {
        public string access_token;
        public int user_id;
    }

    [Serializable]
    public class JsonBase
    {
        public string error;
    }
}