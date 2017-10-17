
using UnityEngine;
using System.Collections;

namespace MyFrameWork
{

    #region Global delegate 委托
    // UI状态改变
    public delegate void StateChangedEvent(object sender, EnumObjectState newState, EnumObjectState oldState);

    public delegate void MessageEvent(Message message);

    public delegate void OnTouchEventHandle(GameObject _listener, object _args, params object[] _params);

    public delegate void PropertyChangedHandle(BaseActor actor, int id, object oldValue, object newValue);
    #endregion

    #region Global enum 枚举
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResType
    {
        /// <summary>
        /// The Texture
        /// </summary>
        Texture,
        /// <summary>
        /// The Fbx
        /// </summary>
        Fbx,
        /// <summary>
        /// The AseetBundle
        /// </summary>
        AssetBundle,
        /// <summary>
        /// The Byts
        /// </summary>
        Raw
    }

    /// <summary>
    /// 对象当前状态 
    /// </summary>
    public enum EnumObjectState
    {
        /// <summary>
        /// The none.
        /// </summary>
        None,
        /// <summary>
        /// The initial.
        /// </summary>
        Initial,
        /// <summary>
        /// The loading.
        /// </summary>
        Loading,
        /// <summary>
        /// The ready.
        /// </summary>
        Ready,
        /// <summary>
        /// The disabled.
        /// </summary>
        Disabled,
        /// <summary>
        /// The closing.
        /// </summary>
        Closing
    }

    /// <summary>
    /// Enum user interface type.
    /// UI面板类型
    /// </summary>
    public enum UIType : int
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = -1,
        /// <summary>
        /// The test one.
        /// </summary>
        TestOne,
        /// <summary>
        /// The test two.
        /// </summary>
        TestTwo,
        /// <summary>
        /// Alert Window
        /// </summary>
        AlertWindow,
        /// <summary>
        /// Waiting View
        /// </summary>
        Waiting,
        /// <summary>
        /// Login View
        /// </summary>
        Login,
        /// <summary>
        /// Main View
        /// </summary>
        CompEditor,
        /// <summary>
        /// Shop View
        /// </summary>
        ShopEditor
    }

    public enum EnumTouchEventType
    {
        OnClick,
        OnDoubleClick,
        OnDown,
        OnUp,
        OnEnter,
        OnExit,
        OnSelect,
        OnUpdateSelect,
        OnDeSelect,
        OnDrag,
        OnDragEnd,
        OnDrop,
        OnScroll,
        OnMove,
    }

    public enum PropertyType : int
    {
        RoleName = 1, // 角色名
        Sex,     // 性别
        RoleID,  // Role ID
        Gold,    // 宝石(元宝)
        Coin,    // 金币(铜板)
        Level,   // 等级
        Exp,     // 当前经验

        AttackSpeed,//攻击速度
        HP,     //当前HP
        HPMax,  //生命最大值
        Attack, //普通攻击（点数）
        Water,  //水系攻击（点数）
        Fire,   //火系攻击（点数）
    }

    /// <summary>
    /// 角色类型
    /// </summary>
    public enum EnumActorType
    {
        None = 0,
        Role,
        Monster,
        NPC,
    }

    /// <summary>
    /// 场景类型
    /// </summary>
    public enum ScnType
    {
        None = 0,
        StartGame,
        LoadingScene,
        LoginScene,
        MainScene,
        CopyScene,
        PVPScene,
        PVEScene,
        /////////////
        Login,
        ShopEditor,
        CompEditor,
    }

    #endregion

    #region Defines static class & cosnt

    /// <summary>
    /// 路径定义。
    /// </summary>
    public static class UIPathDefines
    {
        /// <summary>
        /// UI预设。
        /// </summary>
        public const string UI_PREFAB = "UIPrefabs/";
        /// <summary>
        /// UI小控件预设。
        /// </summary>
        public const string UI_CONTROLS_PREFAB = "UIPrefab/Control/";
        /// <summary>
        /// ui子页面预设。
        /// </summary>
        public const string UI_SUBUI_PREFAB = "UIPrefab/SubUI/";
        /// <summary>
        /// icon路径
        /// </summary>
        public const string UI_IOCN_PATH = "UI/Icon/";

        /// <summary>
        /// Gets the type of the prefab path by.
        /// </summary>
        /// <returns>The prefab path by type.</returns>
        /// <param name="_uiType">_ui type.</param>
        public static string GetPrefabPathByType(UIType _uiType)
        {
            string _uiPrefab = string.Empty;
            switch (_uiType)
            {
                case UIType.TestOne:
                    _uiPrefab = "TestUIOne";
                    break;
                case UIType.TestTwo:
                    _uiPrefab = "TestUITwo";
                    break;
                case UIType.Login:
                    _uiPrefab = "LoginView";
                    break;
                case UIType.Waiting:
                    _uiPrefab = "WaitingView";
                    break;

                case UIType.AlertWindow:
                    _uiPrefab = "WinView";
                    break;

                case UIType.ShopEditor:
                    _uiPrefab = "ShopView";
                    break;

                case UIType.CompEditor:
                    _uiPrefab = "MainView";
                    break;

                default:
                    Debug.Log("Not Find EnumUIType! type: " + _uiType.ToString());
                    break;
            }
            return UI_PREFAB + _uiPrefab;
        }

        /// <summary>
        /// Gets the type of the user interface script by.
        /// </summary>
        /// <returns>The user interface script by type.</returns>
        /// <param name="_uiType">_ui type.</param>
        public static System.Type GetUIScriptByType(UIType _uiType)
        {
            System.Type _scriptType = null;
            switch (_uiType)
            {
                case UIType.TestOne:
                    _scriptType = typeof(TestOne);
                    break;
                case UIType.TestTwo:
                    _scriptType = typeof(TestTwo);
                    break;
                case UIType.Login:
                    _scriptType = typeof(LoginView);
                    break;

                case UIType.Waiting:
                    _scriptType = typeof(WaitingView);
                    break;
                case UIType.AlertWindow:
                    _scriptType = typeof(AlertWindowView);
                    break;

                case UIType.ShopEditor:
                    _scriptType = typeof(ShopView);
                    break;

                case UIType.CompEditor:
                    _scriptType = typeof(CompView);
                    break;
                default:
                    Debug.Log("Not Find EnumUIType! type: " + _uiType.ToString());
                    break;
            }
            return _scriptType;
        }

    }

    #endregion

    #region Global Const String
    public static class Defines
    {
        /**WaitingView**/
        public const string WaitingType_Clock = "clock";
        public const string WaitingType_Percent = "percent";

        /**Server**/
        public const string ServerAddress = "139.198.2.58:8000";

        /**AlertWindow**/
        public const string AlertType_Single = "Alert_Single";
        public const string AlertType_List = "Alert_List";

        /**Test Scene**/
        public const string PlayerModel = "SantaMale/Prefabs/SantaMale";
        public const string MainGroundPath = "Maps/Ground";
        public const string WhiteHousePath = "Building/WhiteHouse";
        public const string MapsLayerName = "Ground";
        public const string TestShopRoute = "03941001001";
    }
    #endregion

}
