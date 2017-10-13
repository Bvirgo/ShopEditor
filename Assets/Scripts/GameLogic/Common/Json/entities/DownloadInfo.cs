using UnityEngine;
using System.Collections;
using System;
using Jhqc.EditorCommon;

/*
{
    "id": 1,
    "name": "fewffewf",
    "crc": "fewf",
    "url": "http://xx/public/商家入驻信息.jpg",
    "created_at": "2016-06-20T06:27:47.000Z",
    "updated_at": "2016-06-20T06:27:47.000Z"
}
 */
[Serializable]
public class DownloadInfo : JsonBase
{
    public int id;
    public string name;
    public string crc;
    public string url;
    public string created_at;
    public string updated_at;
    public string thumbnail;
}
