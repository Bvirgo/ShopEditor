using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Jhqc.EditorCommon
{
    [Serializable]
    public class MaterialInfo : JsonBase
    {
        public string name;
        public int id;
        public string crc;
        public string url;
        public string tags;
        public List<Attr> attrlist;
        // public string created_at;
        // public string updated_at;

        public string[] Tags
        {
            get
            {
                return tags.Split(',');
            }
        }
    }

    [Serializable]
    public class Attr : JsonBase
    {
        public string attrs;
        public string picture;
    }

    [Serializable]
    public class MaterialInfoCollection : JsonBase
    {
        public List<MaterialInfo> data;
    }
}