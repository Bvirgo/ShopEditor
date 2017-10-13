﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MyFrameWork
{
    /// <summary>
    /// 场景管理类
    /// </summary>
	public class LevelManager : Singleton<LevelManager>
	{
		#region SceneInfoData class

		public class SceneInfoData
		{
            // 关卡管理类
			public Type SceneType { get; private set; }

			public string SceneName { get; private set; }

			public object[] Params { get; private set; }

			public SceneInfoData(string _sceneName, Type _sceneType, params object[] _params)
			{
				this.SceneType = _sceneType;
				this.SceneName = _sceneName;
				this.Params = _params;
			}
		}
        #endregion

        #region Base Data
        private Dictionary<EnumSceneType, SceneInfoData> dicSceneInfos = null;

        private BaseScene currentScene = new BaseScene();

        public EnumSceneType LastSceneType { get; set; }

        public EnumSceneType ChangeSceneType { get; private set; }

        private UIType sceneOpenUIType = UIType.None;
        private object[] sceneOpenUIParams = null;

        public BaseScene CurrentScene
        {
            get { return currentScene; }
            set
            {
                currentScene = value;
                //				if (null != currentScene)
                //				{
                //					currentScene.Load();
                //				}
            }
        }

        public override void Init()
        {
            dicSceneInfos = new Dictionary<EnumSceneType, SceneInfoData>();
        }

        #endregion

        #region Scene Register & UnRegister
        /// <summary>
        /// 场景注册
        /// </summary>
        public void RegisterAllScene()
        {
            RegisterScene(EnumSceneType.StartGame, "StartGameScene", null, null);
            RegisterScene(EnumSceneType.LoginScene, "LoginScene", typeof(BaseScene), null);
            RegisterScene(EnumSceneType.MainScene, "MainScene", null, null);
            RegisterScene(EnumSceneType.CopyScene, "CopyScene", null, null);
        }

        /// <summary>
        /// 关卡管理类注册
        /// </summary>
        /// <param name="_sceneID">关卡ID</param>
        /// <param name="_sceneName">关卡名</param>
        /// <param name="_sceneType">关卡管理类</param>
        /// <param name="_params">参数</param>
        public void RegisterScene(EnumSceneType _sceneID, string _sceneName, Type _sceneType, params object[] _params)
        {
            if (_sceneType == null || _sceneType.BaseType != typeof(BaseScene))
            {
                throw new Exception("Register scene type must not null and extends BaseScene");
            }
            if (!dicSceneInfos.ContainsKey(_sceneID))
            {
                SceneInfoData sceneInfo = new SceneInfoData(_sceneName, _sceneType, _params);
                dicSceneInfos.Add(_sceneID, sceneInfo);
            }
        }

        public void UnRegisterScene(EnumSceneType _sceneID)
        {
            if (dicSceneInfos.ContainsKey(_sceneID))
            {
                dicSceneInfos.Remove(_sceneID);
            }
        }

        public bool IsRegisterScene(EnumSceneType _sceneID)
        {
            return dicSceneInfos.ContainsKey(_sceneID);
        }

        internal BaseScene GetBaseScene(EnumSceneType _sceneType)
        {
            Debug.Log(" GetBaseScene  sceneId = " + _sceneType.ToString());
            SceneInfoData sceneInfo = GetSceneInfo(_sceneType);
            if (sceneInfo == null || sceneInfo.SceneType == null)
            {
                return null;
            }
            BaseScene scene = System.Activator.CreateInstance(sceneInfo.SceneType) as BaseScene;
            return scene;
        }

        public SceneInfoData GetSceneInfo(EnumSceneType _sceneID)
        {
            if (dicSceneInfos.ContainsKey(_sceneID))
            {
                return dicSceneInfos[_sceneID];
            }
            Debug.LogError("This Scene is not register! ID: " + _sceneID.ToString());
            return null;
        }

        public string GetSceneName(EnumSceneType _sceneID)
        {
            if (dicSceneInfos.ContainsKey(_sceneID))
            {
                return dicSceneInfos[_sceneID].SceneName;
            }
            Debug.LogError("This Scene is not register! ID: " + _sceneID.ToString());
            return null;
        }

        public void ClearScene()
        {
            dicSceneInfos.Clear();
        }
        #endregion

        #region Change Scene Direction (场景无缝切换)

        /// <summary>
        /// 直接切换
        /// </summary>
        /// <param name="_sceneType"></param>
        public void ChangeSceneDirect(EnumSceneType _sceneType)
		{
			UIManager.Instance.CloseUIAll();

			if (CurrentScene != null)
			{
				CurrentScene.Release();
				CurrentScene = null;
			}

			LastSceneType = ChangeSceneType;
			ChangeSceneType = _sceneType;
			string sceneName = GetSceneName(_sceneType);
			//change scene
			CoroutineController.Instance.StartCoroutine(AsyncLoadScene(sceneName));
		}

        /// <summary>
        /// 场景切换
        /// </summary>
        /// <param name="_sceneType"></param>
        /// <param name="_uiType"></param>
        /// <param name="_params"></param>
		public void ChangeSceneDirect(EnumSceneType _sceneType, UIType _uiType, params object[] _params)
		{
			sceneOpenUIType = _uiType;
			sceneOpenUIParams = _params;

            // 场景已经切换
			if (LastSceneType == _sceneType)
			{
                // 场景对应UI已经打开
				if (sceneOpenUIType == UIType.None)
				{
					return;
				}
                // 场景已经切换了，但是对应UI没有打开
				UIManager.Instance.OpenUI( sceneOpenUIType, false,sceneOpenUIParams);
				sceneOpenUIType = UIType.None;
			}else
			{
				ChangeSceneDirect(_sceneType);
			}
		}

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
		private IEnumerator<AsyncOperation> AsyncLoadScene(string sceneName)
		{
            AsyncOperation oper = SceneManager.LoadSceneAsync(sceneName);

            yield return oper;
			// message send

			if (sceneOpenUIType != UIType.None)
			{
				UIManager.Instance.OpenUI(sceneOpenUIType,false ,sceneOpenUIParams);
				sceneOpenUIType = UIType.None;
			}
		}
        #endregion

        #region Change Scene By Loading (场景过渡切换)
        public void ChangeScene(EnumSceneType _sceneType)
        {
            UIManager.Instance.CloseUIAll();

            if (CurrentScene != null)
            {
                CurrentScene.Release();
                CurrentScene = null;
            }

            LastSceneType = ChangeSceneType;
            ChangeSceneType = _sceneType;
            //change loading scene
            CoroutineController.Instance.StartCoroutine(AsyncLoadOtherScene());
        }

        /// <summary>
        /// 加载有过渡loading的界面切换
        /// </summary>
        /// <param name="_sceneType"></param>
        /// <param name="_uiType"></param>
        /// <param name="_params"></param>
        public void ChangeScene(EnumSceneType _sceneType, UIType _uiType, params object[] _params)
        {
            sceneOpenUIType = _uiType;
            sceneOpenUIParams = _params;
            if (LastSceneType == _sceneType)
            {
                if (sceneOpenUIType == UIType.None)
                {
                    return;
                }
                UIManager.Instance.OpenUI(sceneOpenUIType, false, sceneOpenUIParams);
                sceneOpenUIType = UIType.None;
            }
            else
            {
                ChangeScene(_sceneType);
            }
        }

        private IEnumerator AsyncLoadOtherScene()
        {
            string sceneName = GetSceneName(EnumSceneType.LoadingScene);
            AsyncOperation oper = SceneManager.LoadSceneAsync(sceneName);
            yield return oper;
            // message send
            if (oper.isDone)
            {

                // Loading UI

                //				GameObject go = GameObject.Find("LoadingScenePanel");
                //				LoadingSceneUI loadingSceneUI = go.GetComponent<LoadingSceneUI>();
                //				BaseScene scene = CurrentScene;
                //				if (null != scene)
                //				{
                //					scene.CurrentSceneId = ChangeSceneId;
                //				}
                //				//检测是否注册该场景
                //				if (!SceneManager.Instance.isRegisterScene(ChangeSceneId))
                //				{
                //					Debug.LogError("没有注册此场景！" + ChangeSceneId.ToString());
                //				}
                //				LoadingSceneUI.Load(ChangeSceneId);
                //				LoadingSceneUI.LoadCompleted += SceneLoadCompleted;
            }
        }

        /// <summary>
        /// loading 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void SceneLoadCompleted(object sender, EventArgs e)
        {
            Debug.Log("切换场景完成 + " + sender as String);
            //场景切换完成
            //MessageCenter.Instance.SendMessage(MessageType.GAMESCENE_CHANGECOMPLETE, this, null, false);

            //有要打开的UI
            if (sceneOpenUIType != UIType.None)
            {
                UIManager.Instance.OpenUI(sceneOpenUIType, false, sceneOpenUIParams);
                sceneOpenUIType = UIType.None;
            }
        }
        #endregion
    }
}

