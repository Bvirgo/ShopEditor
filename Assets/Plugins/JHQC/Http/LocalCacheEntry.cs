using UnityEngine;
using System.Collections;

namespace Jhqc.EditorCommon
{
    public class LocalCacheEntry
    {
        public CacheType Type { get; set; }

        public Texture2D Texture { get; set; }
        public string FbxPath { get; set; }
        public AssetBundle AB { get; set; }
        public byte[] Bytes { get; set; }

        public enum CacheType
        {
            Texture,
            Fbx,
            AssetBundle,
            Raw
        }
    }
}