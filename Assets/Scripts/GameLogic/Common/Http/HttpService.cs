using UnityEngine;
using System.Collections;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using Jhqc.EditorCommon;
using SimpleJson;
using LitJson;
using ArtsWork;
using System.Text;
using SimpleJSON;

public static class HttpService
{
    //非网络400 500错误，而是后台定义的逻辑错误
    private static string lastErrorMsg = null;
    public static string LastErrorMsg { get { return lastErrorMsg; } }
    public static string ERRORSTR_FILEEXIT = "{\"error\":\"file_crc_exist\"}";
    public const string URL_PRE = "http";
    public static void Login(string userName, string password, Action<bool, HttpResp> callback)
    {
        WWWManager.Instance.Login(userName, password, resp =>
        {
            if (resp.Error != HttpResp.ErrorType.None)
            {
                callback(false, resp);
            }
            else
            {
                callback(true, resp);
            }
        });
    }

    /// <summary>修改建筑制作信息(照片绑定+建筑分级)</summary>
    public static void UploadBuildingProductionInfo(string block_id, string buildings_sinfo, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["block_id"] = block_id;
        jclass["building_production"] = buildings_sinfo;
        string strJson = jclass.ToString();

        WWWManager.Instance.Post("block_details/update_detail", strJson, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("ERROR: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }

    /// <summary>获取建筑制作信息(照片绑定+建筑分级)</summary>
    public static void GetBuildingProductionInfo(string block_id, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("block_details/get_detail", new NameValueCollection()
        {
            { "block_id", block_id },
            { "detail","building_production"}
        }, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("GetBuildingProductionInfo.ERROR: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }

    /// <summary>基础信息录入 - 建筑信息(按区块为单位上传)</summary>
    public static void UploadBlockInfo(string block_id, string buildings_sinfo, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {

        var jclass = new SimpleJSON.JSONClass();
        jclass["block_id"] = block_id;
        jclass["buildings_info"] = buildings_sinfo;
        string strJson = jclass.ToString();

        WWWManager.Instance.Post("block_details/update_detail", strJson, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("ERROR: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }
    /// <summary>基础信息录入 - 建筑信息(按区块为单位上传)</summary>
    public static void GetBlockInfo(string block_id, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("block_details/get_detail", new NameValueCollection()
        {
            { "block_id", block_id },
            { "detail", "buildings_info"}
        }, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("ERROR: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }

    public static void UploadCity(string city_id, string city_name, string prov_id, string isHot, string j_city_id, JSONNode birth_coordinate, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {

        var jclass = new SimpleJSON.JSONClass();
        jclass["name"] = city_name;
        jclass["city"] = city_id;
        jclass["prov_id"] = prov_id;
        jclass["j_city_id"] = j_city_id;
        jclass["is_hotcity"] = isHot;
        if (birth_coordinate != null)
            jclass["birth_coordinate"] = birth_coordinate.ToString();
        string strJson = jclass.ToString();

        WWWManager.Instance.Post("cities/update_city", strJson, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("ERROR: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }


    public static void GetStreet(string city_name, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("roads/fetch_road", new NameValueCollection()
        {
            { "name", city_name }
        }, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("获取街道失败: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }
    public static void UploadStreet(string city_name, JSONNode city_info, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["name"] = city_name;
        jclass["info"] = city_info.ToString();
        string strJson = jclass.ToString();

        WWWManager.Instance.Post("roads/new_road", strJson, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else
            {
                if (failCallback != null)
                    failCallback(resp.Error, resp.WwwText);
                else
                    Debug.LogError("修改街道失败: " + resp.Error + ", " + resp.WwwText);
            }
        });
    }


    /*
citiy 增加几个字段：prov_id 省ID，birth_coordinate：出生点，is_hotcity，是否是热门城市，true/false; j_city_id：城市代码

获取省市相关接口
provices/fetch_provice  get  参数 : provice_id=xxx 不传就返回所有省
provices/fetch_city  get 参数：provice_id=xxx&city_id=xxx，provice_id必须传，city_id不传就返回该省对应的所有城市

provices/fetch_country get 返回城市所有区县数据 参数：city_id=xx&country_id=xxx country_id也可以不传
roads/new_road city=xxx&info=xxx
        **/

    public static void GetProvice(Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("provices/fetch_provice", null, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else if (failCallback != null)
                failCallback(resp.Error, resp.WwwText);
        });
    }
    public static void GetCity(string provice_id, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("provices/fetch_city", new NameValueCollection()
        {
            { "provice_id", provice_id }
        }, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else if (failCallback != null)
                failCallback(resp.Error, resp.WwwText);
        });
    }
    public static void GetCountry(string city_id, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("provices/fetch_country", new NameValueCollection()
        {
            { "city_id", city_id }
        }, resp =>
        {
            if (!HasRespError(resp))
                callback(resp.WwwText);
            else if (failCallback != null)
                failCallback(resp.Error, resp.WwwText);
        });
    }


    public static void GetMyBlocks(Action<string> _cb)
    {
        WWWManager.Instance.Get("blocks/user", null, resp =>
        {
            if (!HasRespError(resp))
            {
                _cb(resp.WwwText);
            }
        });
    }




    public static void GetRemoteTexture(string url, Action<Texture2D> callback, bool _bFromQY = true,
        Action<string> _failCallBack = null)
    {
        WWWManager.Instance.GetFile(url, LocalCacheEntry.CacheType.Texture, (resp, entry) =>
        {
            if (!HasRespError(resp))
            {
                callback(entry.Texture);
            }
            else if (_failCallBack != null)
            {
                _failCallBack(resp.WwwText);
            }
        }, _bFromQY);
    }

    public static void GetRemoteFbxBytes(string url, Action<byte[]> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.GetFile(url, LocalCacheEntry.CacheType.Fbx, (resp, entry) =>
        {
            if (!HasRespError(resp))
                callback(File.ReadAllBytes(entry.FbxPath));
            else if (failCallback != null)
            {
                failCallback(resp.Error, resp.WwwText);
            }
        }, true);
    }

    public static void GetRemoteAB(string url, Action<AssetBundle> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.GetFile(url, LocalCacheEntry.CacheType.AssetBundle, (resp, entry) =>
        {
            if (!HasRespError(resp))
            {
                callback(entry.AB);
            }
            else if (failCallback != null)
                failCallback(resp.Error, resp.WwwText);
        }, true);
    }

    public static void GetRemoteRaw(string url, Action<byte[]> successCallback, Action<HttpResp> failCallback)
    {
        WWWManager.Instance.GetFile(url, LocalCacheEntry.CacheType.Raw, (resp, entry) =>
        {
            if (!HasRespError(resp))
            {
                if (successCallback != null)
                {
                    successCallback(entry.Bytes);
                }
            }
            else if (failCallback != null)
            {
                failCallback(resp);
            }
        }, true);
    }

    public static void GetDownloadURL(string name, string crc, Action<string> callback, Action<HttpResp.ErrorType, string> failCallback = null)
    {
        WWWManager.Instance.Get("entities/download_url", new NameValueCollection()
        {
            { "name", name },
            { "crc", crc }
        }, resp =>
        {
            if (!HasRespError(resp))
            {
                var downloadInfo = JsonUtility.FromJson<DownloadInfo>(resp.WwwText);
                callback(downloadInfo.url);
            }
            else if (failCallback != null)
            {
                failCallback(resp.Error, resp.WwwText+string.Format("  Name:{0},Crc:{1}",name,crc));
            }

        });
    }

    /// <summary>
    /// 根据材质名称，获取缩略图URL
    /// </summary>
    /// <param name="_strMatName"></param>
    /// <param name="_cb"></param>
    public static void GetMatURL(string _strMatName, Action<string> _cb)
    {
        WWWManager.Instance.Get("/meterials/get_url", new NameValueCollection()
        {
            { "name", _strMatName }
        }, resp =>
        {
            // 没有错误
            if (!HasRespError(resp))
            {
                var downloadInfo = JsonUtility.FromJson<DownloadInfo>(resp.WwwText);
                _cb(downloadInfo.thumbnail);
            }
            else
            {
                Debug.Log("材质图片获取URL失败：" + _strMatName);
            }
        });
    }


    /// <summary>查看传入文件列表是否在服务器上已存在, 返回一个 未上传/不存在 的文件列表(重要! 传入文件个数不应超过200)</summary>
    /// <param name="str">[{"name":"fileName1", "crc":"fileCrc1"}]格式的json</param>
    /// <param name="successCallback">服务器上没有的文件列表, 格式与传入json相同</param>
    public static void CheckFileExist(string str, Action<string> successCallback, Action<HttpResp.ErrorType, string> failCallback)
    {
        Debug.Log(str);
        //str = SongeUtil.Escape(str);
        //Debug.Log(str);
        WWWManager.Instance.Get("entities/does_file_exist", new NameValueCollection()
        {
            {"file_list", str}
        }, resp =>
        {
            if (!HasRespError(resp))
            {
                successCallback(resp.WwwText);
            }
            else if (failCallback != null)
            {
                failCallback(resp.Error, resp.WwwText);
            }
        });

    }

    /// <summary>
    /// Update City Information
    /// </summary>
    /// <param name="_strCityName"></param>
    /// <param name="_strJsData"></param>
    /// <param name="sCb"></param>
    /// <param name="fCb"></param>
    public static void UpdateCityInfo(string _strCityName, string _strJsData, Action sCb, Action<HttpResp.ErrorType, string> fCb)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["name"] = _strCityName;
        jclass["block_file"] = _strJsData;
        string strJson = jclass.ToString();
        WWWManager.Instance.Post("cities/update_city", strJson, resp =>
        {
            if (!HasRespError(resp))
            {
                sCb();
            }
            else
            {
                fCb(resp.Error, resp.WwwText);
            }
        });
    }

    public static void UploadFile(string name, string crc, byte[] fileData, Action successCallback, Action<HttpResp.ErrorType, string> failCallback,
        bool isFileExistSuccess = true)
    {
        WWWManager.Instance.Post("entities/upload", new NameValueCollection()
        {
            { "name", name },
            { "crc", crc }
        }, "tmpfile", fileData, resp =>
        {
            //TEST CHECK songyi 160818 保证file_crc_exist类错误 也会有回调执行
            if (!HasRespError(resp))
            {
                successCallback();
            }
            else if (failCallback != null)
            {
                if (isFileExistSuccess)
                {
                    // 服务器已存在
                    if (resp.Error == HttpResp.ErrorType.LogicError && ERRORSTR_FILEEXIT == resp.WwwText)
                    {
                        successCallback();
                    }
                    else
                    {
                        failCallback(resp.Error, resp.WwwText);
                    }
                }
                else
                {
                    failCallback(resp.Error, resp.WwwText);
                }
            }
        });
    }

    #region 单粒度更新

    /// <summary>
    /// 单粒度更新类型
    /// </summary>
    public enum SingleTargetType
    {
        Component,
        Building,
        Maps,
        Shop,
        BuildingInfo,
        StreetInfo
    }

    /// <summary>
    /// 组件单粒度更新
    /// </summary>
    /// <param name="_strBlockName"></param>
    /// <param name="_strCompJd"></param>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public static void UpdateCompnent(string _strBlockName, string _strCompJd, Action successCallback, Action<string> failCallback)
    {

        // 这种拼接方式，服务器协议不认！ 
        Debug.LogWarning("info:" + _strCompJd + "\nBlockName:" + _strBlockName);
        WWWManager.Instance.Post("block_components/upsert", new NameValueCollection()
        {
            { "block_name", _strBlockName },
            { "info", _strCompJd }
        }, resp =>
        {
            if (!HasRespError(resp))
            {
                successCallback();
            }
            else if (failCallback != null)
            {
                Debug.LogError(string.Format("更新组件：{0}，失败:{1}", _strBlockName, resp.Error));
                failCallback(resp.Error.ToString());
            }
        });
    }

    /// <summary>
    /// 单粒度更新
    /// </summary>
    /// <param name="_strCityName"></param>
    /// <param name="_strJsData"></param>
    /// <param name="sCb"></param>
    /// <param name="fCb"></param>
    public static void UpdateSingleTargetByJson(string _strCityName, string _strJsData, Action<string> sCb, Action<string> fCb,
        SingleTargetType _sType = SingleTargetType.Building)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["block_name"] = _strCityName;
        jclass["info"] = _strJsData;
        string strJson = jclass.ToString();

        string strPort = "Error";
        switch (_sType)
        {
            case SingleTargetType.Building:
                strPort = "buildings/upsert";
                break;
            case SingleTargetType.Component:
                strPort = "block_components/upsert";
                break;
            case SingleTargetType.Maps:
                strPort = "sub_blocks/upsert";
                break;
            case SingleTargetType.Shop:
                strPort = "shop_signs/upsert";
                break;

            case SingleTargetType.BuildingInfo:
                strPort = "building_infos/upsert";
                break;

            case SingleTargetType.StreetInfo:
                strPort = "streets/upsert";
                jclass = new SimpleJSON.JSONClass();
                jclass["city_name"] = _strCityName;
                jclass["info"] = _strJsData;
                strJson = jclass.ToString();
                break;
            default:
                return;
        }

        Debug.LogWarning("更新，发送的Js:\n" + _strJsData);

        WWWManager.Instance.Post(strPort, strJson, resp =>
        {
            if (!HasRespError(resp))
            {
                sCb(resp.ToString());
            }
            else
            {
                Debug.LogError(string.Format("更新：{0}，失败:{1}", _strCityName, resp.ToString()));

                fCb(resp.WwwText);
            }
        });
    }

    public static void UpdateBuildingInfo(string _strBlockName, string _strNodeGUID, string _strJsData, Action<string> sCb, Action<string> fCb,
       SingleTargetType _sType = SingleTargetType.Building)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["block_name"] = _strBlockName;
        jclass["build_guid"] = _strNodeGUID;
        jclass["info"] = _strJsData;
        string strJson = jclass.ToString();

        string strPort = "building_infos/upsert";

        Debug.LogWarning("更新，发送的Js:\n" + _strJsData);
        Debug.LogWarning(string.Format("更新：地块ID：{0}\n建筑gID：{1}", _strBlockName, _strNodeGUID));

        WWWManager.Instance.Post(strPort, strJson, resp =>
        {
            if (!HasRespError(resp))
            {
                sCb(resp.ToString());
            }
            else
            {
                Debug.LogError(string.Format("更新建筑基础信息：{0}，失败:{1}", _strNodeGUID, resp.WwwText));

                fCb(resp.WwwText);
            }
        });
    }

    /// <summary>
    /// 获取地块单粒度列表
    /// </summary>
    /// <param name="_strBlockName"></param>
    /// <param name="callback"></param>
    /// <param name="_fCb"></param>
    public static void GetBlockSingleTargetList(string _strBlockName, Action<string> callback, Action<string> _fCb,
        SingleTargetType _sType = SingleTargetType.Building)
    {
        string strPort = "Error";
        string strParamKey = "block_name";
        switch (_sType)
        {
            case SingleTargetType.Building:
                strPort = "buildings/in_block";
                break;
            case SingleTargetType.Component:
                strPort = "block_components/in_block";
                break;
            case SingleTargetType.Maps:
                strPort = "sub_blocks/in_block";
                break;
            case SingleTargetType.Shop:
                strPort = "shop_signs/in_block";
                break;

            case SingleTargetType.BuildingInfo:
                strPort = "building_infos/in_block";
                break;

            case SingleTargetType.StreetInfo:
                strPort = "streets/in_city";
                strParamKey = "city_name";
                break;
            default:
                return;
        }

        WWWManager.Instance.Get(strPort, new NameValueCollection()
            {
                { strParamKey, _strBlockName }
            }, resp =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    Debug.LogWarning("加载出来的BuildingConfig:" + resp.WwwText);
                    _fCb(resp.WwwText);

                }
                else
                {
                    callback(resp.WwwText);
                }
            });
    }


    /// <summary>
    /// 单粒度删除：_bType: 1-组件 2-建筑 3-九宫格
    /// </summary>
    /// <param name="_strCityName"></param>
    /// <param name="_strGUID"></param>
    /// <param name="sCb"></param>
    /// <param name="fCb"></param>
    /// <param name="_bType"></param>
    public static void DeleteSingleTargetByJson(string _strCityName, string _strGUID, Action sCb, Action<string> fCb,
        SingleTargetType _sType = SingleTargetType.Building, bool _bClearStreetID = false)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["block_name"] = _strCityName;
        jclass["guid"] = _strGUID;
        string strJson = jclass.ToString();

        string strPort = "Error";
        switch (_sType)
        {
            case SingleTargetType.Building:
                strPort = "buildings/delete";
                break;
            case SingleTargetType.Component:
                strPort = "block_components/delete";
                break;
            case SingleTargetType.Maps:
                strPort = "sub_blocks/delete";
                break;
            case SingleTargetType.Shop:
                strPort = "shop_signs/delete";
                break;
            case SingleTargetType.BuildingInfo:
                strPort = "building_infos/delete";
                break;

            case SingleTargetType.StreetInfo:

                strPort = "streets/delete";

                jclass = new SimpleJSON.JSONClass();
                jclass["city_name"] = _strCityName;
                jclass["guid"] = _strGUID;
                jclass["unbind_building"] = _bClearStreetID ? "true" : "false";
                strJson = jclass.ToString();
                break;
            default:
                return;
        }

        WWWManager.Instance.Post(strPort, strJson, resp =>
        {
            if (!HasRespError(resp))
            {
                sCb();
            }
            else
            {
                Debug.LogWarning(string.Format("删除：{0}，失败:{1}", strPort, resp.Error));
                fCb(resp.WwwText);
            }
        });
    }

    public static void DeleteSingleBuildingInfoByJson(string _strBlockName, string _strGUID, Action sCb, Action<string> fCb)
    {
        var jclass = new SimpleJSON.JSONClass();
        jclass["block_name"] = _strBlockName;
        jclass["build_guid"] = _strGUID;
        string strJson = jclass.ToString();

        string strPort = "building_infos/delete";

        WWWManager.Instance.Post(strPort, strJson, resp =>
        {
            if (!HasRespError(resp))
            {
                sCb();
            }
            else
            {
                Debug.LogWarning(string.Format("删除：{0}，失败:{1}", strPort, resp.Error));
                fCb(resp.WwwText);
            }
        });
    }

    #endregion

    public static void UploadFile(string name, string fileStr, Action successCallback, Action<HttpResp.ErrorType, string> failCallback,
        bool isFileExistSuccess = true)
    {
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(fileStr);
        UploadFile(name, Crc32.CountCrc(byteArray).ToString(), byteArray, successCallback, failCallback, isFileExistSuccess);
    }

    /// <summary>
    /// 更新地块配置
    /// </summary>
    /// <param name="_strConfig"></param>
    /// <param name="_doneCb"></param>
    /// <param name="_failCb"></param>
    public static void UpdateBlockEntity(string _strConfig, Action _doneCb, Action<HttpResp> _failCb)
    {
        WWWManager.Instance.Post("block_entities/new_block_entity", _strConfig, resp =>
        {
            if (!HasRespError(resp))
            {
                _doneCb();
            }
            else if (_failCb != null)
            {
                _failCb(resp);
            }
        });
    }



    #region 组件编辑器
    public static void UploadFile(string name, string crc, byte[] fileData, string _type, Action successCallback, Action<HttpResp.ErrorType, string> failCallback,
  bool isFileExistSuccess = true)
    {
        WWWManager.Instance.Post("entities/upload", new NameValueCollection()
        {
            { "name", name },
            { "crc", crc },
            {"type",_type}

        }, "tmpfile", fileData, resp =>
        {
            //TEST CHECK songyi 160818 保证file_crc_exist类错误 也会有回调执行
            if (!HasRespError(resp))
            {
                successCallback();
            }
            else if (failCallback != null)
            {
                if (isFileExistSuccess)
                {
                    // 服务器已存在
                    if (resp.Error == HttpResp.ErrorType.LogicError && ERRORSTR_FILEEXIT == resp.WwwText)
                    {
                        successCallback();
                    }
                    else
                    {
                        failCallback(resp.Error, resp.WwwText);
                    }
                }
                else
                {
                    failCallback(resp.Error, resp.WwwText);
                }
            }
        }, false);
    }

    /// <summary>打包请求-请求服务器打包文件</summary>
    public static void RequestPackage(string name, string crc, string tag, byte[] fileData, string _type, Action successCallback, Action<HttpResp.ErrorType, string> failCallback)
    {
        NameValueCollection nvc = new NameValueCollection()
            {
                { "name", name },
                { "crc", crc },
                { "type", _type },
                {"tag",tag}
            };
        //Debug.Log("RequestPackage.tag = " + tag);
        /*
        
        
        WWWManager.Instance.Post("entities/upload", nvc, "tmpfile", fileData, resp =>
        {
        
        
        */

        WWWManager.Instance.Post("entities/package", nvc, "tmpfile", fileData, resp =>
       {
            //TEST CHECK songyi 160818 保证file_crc_exist类错误 也会有回调执行
            if (!HasRespError(resp))
           {
               successCallback();
           }
           else if (failCallback != null)
           {
                // 服务器已存在
                if (resp.Error == HttpResp.ErrorType.LogicError && ERRORSTR_FILEEXIT == resp.WwwText)
               {
                   successCallback();
               }
               else
               {
                   failCallback(resp.Error, resp.WwwText);
               }
           }
       }, false);
    }

    public static void UploadFile(string name, string crc, string tag, byte[] fileData,
        string _type, Action successCallback,
        Action<HttpResp.ErrorType, string> failCallback,
        bool isFileExistSuccess = true)
    {
        NameValueCollection nvc;
        if (tag != null)
        {
            nvc = new NameValueCollection()
            {
                { "name", name },
                { "crc", crc },
                {"type",_type},
                {"tag",tag}

            };
        }
        else
        {
            nvc = new NameValueCollection()
            {
                { "name", name },
                { "crc", crc },
                {"type",_type}

            };
        }

        WWWManager.Instance.Post("entities/upload", nvc, "tmpfile", fileData, resp =>
        {
            if (!HasRespError(resp))
            {
                successCallback();
            }
            else if (failCallback != null)
            {
                if (isFileExistSuccess)
                {
                    // 服务器已存在
                    if (resp.Error == HttpResp.ErrorType.LogicError && ERRORSTR_FILEEXIT == resp.WwwText)
                    {
                        successCallback();
                    }
                    else
                    {
                        failCallback(resp.Error, resp.WwwText);
                    }
                }
                else
                {
                    failCallback(resp.Error, resp.WwwText);
                }
            }
        }, false);
    }

    public static void GetCmpConfig(string _strCmpName, Action<string> callback)
    {
        WWWManager.Instance.Get("components/fetch_info", new NameValueCollection()
            {
                { "name", _strCmpName }
            }, resp =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    InfoTips.LogInfo("Get Material Error: " + resp.ToString());
                    callback(resp.ToString());
                }
                else
                {
                    //Debug.LogWarning("加载出来的CmpConfig:" + resp.WwwText);
                    //MonoHelper.Instance.SaveInfo(resp.WwwText,"原始组件配置");
                    callback(resp.WwwText);
                }
            });
    }

    /// <summary>
    /// 获取城市信息
    /// </summary>
    /// <param name="_strCityName"></param>
    /// <param name="callback"></param>
    public static void GetCityInfo(string _strCityName, Action<string> callback)
    {
        if (_strCityName.Contains(" "))
        {
            Debug.LogWarning("城市名称[" + _strCityName + "]包含空格，请留意！");
            _strCityName = _strCityName.Trim();
        }

        WWWManager.Instance.Get("cities/fetch_city", new NameValueCollection()
            {
                { "city_name", _strCityName }
            }, resp =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    InfoTips.LogInfo("Get Material Error: " + resp.ToString());
                    callback(resp.ToString());
                }
                else
                {
                    //Debug.LogWarning("加载出来的CmpConfig:" + resp.WwwText);
                    //MonoHelper.Instance.SaveInfo(resp.WwwText);
                    callback(resp.WwwText);
                }
            });
    }

    /// <summary>
    /// 获取临时数据接口：name,type 不指定，获取的是所有临时数据
    /// </summary>
    /// <param name="_strName"></param>
    /// <param name="_strType"></param>
    /// <param name="callback"></param>
    public static void GetTempData(string _strName, string _strType, Action<string> callback, Action<string> _failCb = null)
    {
        WWWManager.Instance.Get("temp_buildings/fetch_info", new NameValueCollection()
            {
                { "name", _strName },
                {"type",_strType}
            }, resp =>
            {
                if (resp.Error != HttpResp.ErrorType.None)
                {
                    InfoTips.LogInfo("temp_buildings/fetch_info" + resp.ToString());
                    if (_failCb != null)
                    {
                        _failCb(resp.WwwText);
                    }
                    else
                    {
                        callback(resp.ToString());
                    }
                }
                else
                {
                    callback(resp.WwwText);

                }
            });
    }



    /// <summary>
    /// 保存组件配置
    /// </summary>
    /// <param name="_strName"></param>
    /// <param name="_strInfo"></param>
    /// <param name="_cb"></param>
    public static void SaveCompConfig(string _strName, string _strInfo, Action<bool, HttpResp> _cb)
    {
        // 转为json
        var jclass = new SimpleJSON.JSONClass();
        jclass["name"] = _strName;
        jclass["info"] = _strInfo;


        WWWManager.Instance.Post("components/set_info", jclass.ToString(), resp =>
        {
            if (!HasRespError(resp))
            {
                _cb(true, resp);
            }
            else
            {
                _cb(false, resp);
            }
        });

    }

    /// <summary>
    /// 批量删除组件
    /// </summary>
    /// <param name="_strNames">组件ID字符串</param>
    /// <param name="_cb"></param>
    public static void DeleteCompConfig(string _strNames, Action<bool, HttpResp> _cb)
    {
        // 转为json
        var jclass = new SimpleJSON.JSONClass();
        jclass["name_list"] = _strNames;

        WWWManager.Instance.Post("components/drop_by_list", jclass.ToString(), resp =>
        {
            Debug.Log("删除结果返回：" + resp.WwwText);
            if (!HasRespError(resp))
            {
                _cb(true, resp);
            }
            else
            {
                _cb(false, resp);
            }
        });

    }

    /// <summary>
    /// 保存临时数据接口
    /// </summary>
    /// <param name="_strName"></param>
    /// <param name="_strType"></param>
    /// <param name="_strInfo"></param>
    /// <param name="_cb"></param>
    public static void SaveTempData(string _strName, string _strType, string _strInfo, Action<bool, HttpResp> _cb)
    {
        // 转为json
        var jclass = new SimpleJSON.JSONClass();
        jclass["name"] = _strName;
        jclass["type"] = _strType;
        jclass["info"] = _strInfo;

        WWWManager.Instance.Post("temp_buildings/set_info", jclass.ToString(), resp =>
        {
            if (!HasRespError(resp))
            {
                _cb(true, resp);
            }
            else
            {
                _cb(false, resp);
            }
        });
    }


    #endregion


    /// <summary>
    /// 获取组件配置文件
    /// </summary>
    /// <param name="_cb"></param>
    /// <param name="_failCb"></param>
    public static void GetCompConfigUrl(Action<string> _cb, Action<string> _failCb)
    {
        WWWManager.Instance.Get("components/fetch_txt", new NameValueCollection(), res =>
        {
            if (!HasRespError(res))
            {
                _cb(res.WwwText);
            }
            else if (_failCb != null)
            {
                _failCb(res.WwwText);
            }
        });
    }

    public static void Audit(int entityId, OpinionString opinion, Action callback, Action<HttpResp> _fCb = null)
    {
        WWWManager.Instance.Post("block_entities/audit", new NameValueCollection()
        {
            { "block_entity_id", entityId.ToString() },
            { "status", opinion.ToString() }
        }, resp =>
        {
            if (!HasRespError(resp))
            {
                callback();
            }
            else if (_fCb != null)
            {
                _fCb(resp);
            }
        });
    }

    public static bool HasResponsError(HttpResp resp)
    {
        return resp.Error != HttpResp.ErrorType.None;
    }

    public static bool HasRespError(HttpResp resp, bool autoShowError = true)
    {
        var hasError = resp.Error != HttpResp.ErrorType.None;

        if (hasError)
        {
            lastErrorMsg = resp.ToString();

            if (resp.Error == HttpResp.ErrorType.AccessExpired)
            {
            }
            else if (autoShowError)
            {
                //Dialog.ShowLastHttpError();
                //new WindowAlertDialog(resp.Error.ToString()).Show();
            }
            InfoTips.LogInfo("网络错误" + resp.Error.ToString() + ": " + resp.WwwText);
        }

        return hasError;
    }

    //token过期后自动弹出重登UI，重登成功以后
    //好像啥也不需要干了
    private static void Relogin()
    {

    }

    /// <summary>
    /// 简单判断, 之后用不到
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsUrl(string str)
    {
        return str.StartsWith(URL_PRE);
    }

    #region 商铺数据
    /// <summary>
    /// 获取走向图数据
    /// </summary>
    /// <param name="callback"></param>
    public static void GetExtralRouteData(Action<LoginDataCollection> callback, Action<HttpResp> _failCb)
    {

        WWWManager.Instance.Get("extra_libs/fetch_info", null, resp =>
        {
            if (!HasRespError(resp))
            {
                LoginDataCollection data = JsonUtility.FromJson<LoginDataCollection>(resp.WwwText);
                //JsonData jd = JsonMapper.ToObject(resp.WwwText);
                //MonoHelper.Instance.SaveInfo(resp.WwwText, "独立接口获取Extral数据");
                callback(data);
            }
            else
            {
                _failCb(resp);
            }
        });
    }
    /// <summary>
    /// 获取走向图数据
    /// </summary>
    /// <param name="callback"></param>
    public static void GetExtralRouteData(string routeName, Action<string> callback, Action<HttpResp> _failCb)
    {

        WWWManager.Instance.Get("extra_libs/fetch_info", new NameValueCollection()
            {
                { "name", routeName }
            }, resp =>
            {
                if (!HasRespError(resp))
                {
                    callback(resp.WwwText);
                }
                else
                {
                    _failCb(resp);
                }
            });
    }
    #endregion
}

public enum OpinionString
{
    unstarted,
    present,
    @object,
    approved,
    review,
    pre_approved // Super账号提交给全局审核账号
}
