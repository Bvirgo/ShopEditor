
using System;
namespace ZFrameWork
{
	public class MsgType 
	{
        #region Test
        public static string Net_MessageTestOne = "Net_MessageTestOne";
        public static string Net_MessageTestTwo = "Net_MessageTestTwo";
        #endregion

        #region Common

        // ÊôÐÔ×÷±×Òì³£
        public const string Com_PropertyException = "PropertyItemDataException";
        public const string Com_AnyKeyHeld = "Com_AnyKeyHeld";
        public const string Com_AnyKeyDown = "Com_AnyKeyDown";
        public const string Com_MouseEvent = "Com_MouseEvent";

        public const string Com_CloseUI = "Com_CloseUI";
        public const string Com_OpenUI = "Com_OpenUI";

        #endregion

        #region WaitingView
        public const string WV_ShowWaiting = "Com_ShowWaiting";
        public const string WV_NewWaiting = "Com_NewWating";
        public const string WV_HideWaiting = "Com_HideWaiting";
        public const string WV_UpdateWaiting = "Com_UpdateWaiting";
        public const string WV_PushWaiting = "Com_PushWaiting";
        public const string WV_PopWaiting = "Com_PopWaiting";
        #endregion

        #region AlertWindow
        public const string Win_Show = "Win_ShowWindow";
        public const string Win_ItemClick = "Win_ItemClick";
        public const string Win_Affirm = "Win_Affirm";
        public const string Win_Refresh = "Win_RefreshWindow";
        public const string Win_Finish = "Win_Finish";
        #endregion

        #region Login
        public const string LoginView_Login = "LoginView_Login";

        #endregion

        #region ShopView
        public const string ShopView_Show = "ShopView_Show";
        public const string ShopView_LoadRoute = "ShopView_LoadRoute";
        public const string ShopView_SaveRoute = "ShopView_SaveRouteToServer";
        public const string ShopView_LocalSave = "ShopView_SaveRouteToLocal";

        public const string ShopView_RefreshSampleBoard = "ShopView_RefreshSampleBoard";
        public const string ShopView_RefreshShopList = "ShopView_RefreshShopList";
        public const string ShopView_RefreshBoardList = "ShopView_RefreshBoardList";

        public const string ShopView_DeleteShop = "ShopView_DeleteShop";
        public const string ShopView_DeleteBoard = "ShopView_DeleteBoard";

        public const string ShopView_SampleBoardClicked = "ShopView_SampleBoardClicked";
        public const string ShopView_BoardClicked = "ShopView_BoardClicked";
        public const string ShopView_ShopItemClicked = "ShopView_ShopClicked";

        public const string ShopView_NewPoint = "ShopView_NewPoint";
        public const string ShopView_CancelPoint = "ShopView_CancelPoint";

        public const string ShopView_OnlyShop = "ShopView_OnlyShop";
        public const string ShopView_OnlyBoard = "ShopView_OnlyBoard";
        public const string ShopView_ShopAndBoard = "ShopView_ShopBoard";
        #endregion

        #region Component Editor
        public const string MainView_Show = "MainView_Show";
        public const string MainView_ReplaceAll = "MainView_ReplaceAll";
        public const string MainView_RefreshTag = "MainView_RefreshTagList";
        public const string MainView_RefreshCom = "MainView_RefreshComList";
        public const string MainView_TagItemClick = "MainView_TagItemClick";
        public const string MainView_ComItemClick = "MainView_ComItemClick";
        public const string MainView_LoadRes = "MainView_LoadRes";
        public const string MainView_Save = "MainView_Save";
        public const string MainView_NewComp = "MainView_NewComp";
        public const string MainView_Affirm = "MainView_Affirm";
        
        #endregion
    }
}

