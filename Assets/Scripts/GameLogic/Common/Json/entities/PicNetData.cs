using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


namespace Jhqc.EditorCommon
{
        [Serializable]
    public class PicNetData : JsonBase
    {
        public string id;
        public string url;
        public string sign;
    }

        [Serializable]
    public class PicNetDataCollection : JsonBase
    {
        List<PicNetData> m_pPicNetData;
    }
}

