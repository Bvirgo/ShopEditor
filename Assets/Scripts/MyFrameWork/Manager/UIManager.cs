
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ZFrameWork
{
	/// <summary>
	/// User interface manager.
	/// </summary>
	public class UIManager : Singleton<UIManager>
	{
		#region UIInfoData class
		/// <summary>
		/// User interface UIInfoData.
		/// </summary>
		class UIInfoData
		{
			/// <summary>
			/// Gets the type of the user interface.
			/// </summary>
			/// <value>The type of the user interface.</value>
			public UIType UIType { get; private set; }

			public Type ScriptType { get; private set; }
			/// <summary>
			/// Gets the path.
			/// </summary>
			/// <value>The path.</value>
			public string Path { get; private set; }
			/// <summary>
			/// Gets the user interface parameters.
			/// </summary>
			/// <value>The user interface parameters.</value>
			public object[] UIParams { get; private set; }
			public UIInfoData(UIType _uiType, string _path, params object[] _uiParams)
			{
				this.UIType = _uiType;
				this.Path = _path;
				this.UIParams = _uiParams;
				this.ScriptType = UIPathDefines.GetUIScriptByType(this.UIType);
			}
		}
        #endregion

        #region Base Member

        /// <summary>
        /// The dic open U is.
        /// </summary>
        private Dictionary<UIType, GameObject> dicOpenUIs = null;
        
        /// <summary>
        /// The stack open U is.
        /// </summary>
        private Stack<UIInfoData> stackOpenUIs = null;

        /// <summary>
        /// Init this Singleton.
        /// </summary>
        public override void Init()
        {
            dicOpenUIs = new Dictionary<UIType, GameObject>();
            stackOpenUIs = new Stack<UIInfoData>();
        }
        #endregion

        #region Get UI & UIObject By EnunUIType 
        /// <summary>
        /// Gets the U.
        /// </summary>
        /// <returns>The U.</returns>
        /// <param name="_uiType">_ui type.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetUI<T>(UIType _uiType) where T : BaseUI
		{
			GameObject _retObj = GetUIObject(_uiType);
			if (_retObj != null)
			{
				return _retObj.GetComponent<T>();
			}
			return null;
		}

		/// <summary>
		/// Gets the user interface object.
		/// </summary>
		/// <returns>The user interface object.</returns>
		/// <param name="_uiType">_ui type.</param>
		public GameObject GetUIObject(UIType _uiType)
		{
			GameObject _retObj = null;
			if (!dicOpenUIs.TryGetValue(_uiType, out _retObj))
				throw new Exception("dicOpenUIs TryGetValue Failure! _uiType :" + _uiType.ToString());
			return _retObj;
		}
		#endregion
	
		#region Preload UI Prefab By EnumUIType
		/// <summary>
		/// Preloads the U.
		/// </summary>
		/// <param name="_uiTypes">_ui types.</param>
		public void PreloadUI(UIType[] _uiTypes)
		{
			for (int i=0; i<_uiTypes.Length; i++)
			{
				PreloadUI(_uiTypes[i]);
			}
		}
		
		/// <summary>
		/// Preloads the U.
		/// </summary>
		/// <param name="_uiType">_ui type.</param>
		public void PreloadUI(UIType _uiType)
		{
			string path = UIPathDefines.GetPrefabPathByType(_uiType);
			Resources.Load(path);
			//ResManager.Instance.ResourcesLoad(path);
		}
		
		#endregion

		#region Open UI By EnumUIType
		/// <summary>
		/// 打开界面。
		/// </summary>
		/// <param name="uiTypes">User interface types.</param>
		public void OpenUI(UIType[] uiTypes,bool _bAsync = true)
		{
			OpenUI(false, uiTypes,_bAsync,null);
		}

		/// <summary>
		/// Opens the U.
		/// </summary>
		/// <param name="uiType">User interface type.</param>
		/// <param name="uiObjParams">User interface object parameters.</param>
		public void OpenUI(UIType uiType, bool _bAsync, params object[] uiObjParams)
		{
			UIType[] uiTypes = new UIType[1];
			uiTypes[0] = uiType;
			OpenUI(false, uiTypes, _bAsync,uiObjParams);
		}

		/// <summary>
		/// Opens the user interface close others.
		/// </summary>
		/// <param name="uiTypes">User interface types.</param>
		public void OpenUICloseOthers(UIType[] uiTypes, bool _bAsync = true)
        {
			OpenUI(true, uiTypes,_bAsync, null);
		}

		/// <summary>
		/// Opens the user interface close others.
		/// </summary>
		/// <param name="uiType">User interface type.</param>
		/// <param name="uiObjParams">User interface object parameters.</param>
		public void OpenUICloseOthers(UIType uiType, bool _bAsync ,params object[] uiObjParams)
		{
			UIType[] uiTypes = new UIType[1];
			uiTypes[0] = uiType;
			OpenUI(true, uiTypes, _bAsync,uiObjParams);
		}

		/// <summary>
		/// Opens the U.
		/// </summary>
		/// <param name="_isCloseOthers">If set to <c>true</c> _is close others.</param>
		/// <param name="_uiTypes">_ui types.</param>
		/// <param name="_uiParams">_ui parameters.</param>
		private void OpenUI(bool _isCloseOthers, UIType[] _uiTypes, bool _bAsync,params object[] _uiParams)
		{
			// Close Others UI.
			if (_isCloseOthers)
			{
				CloseUIAll();
			}

			// push _uiTypes in Stack.
			for (int i=0; i<_uiTypes.Length; i++)
			{
				UIType _uiType = _uiTypes[i];
				if (!dicOpenUIs.ContainsKey(_uiType))
				{
                    // UI不存在，获取UI预制体路径
					string _path = UIPathDefines.GetPrefabPathByType(_uiType);

                    // UI信息堆栈:UI类型，UI预制体路径，参数，（UI指定View脚本）
					stackOpenUIs.Push(new UIInfoData(_uiType, _path, _uiParams));
				}
			}

			// Open UI.
			if (stackOpenUIs.Count > 0)
			{
                if (_bAsync)
                {
                    MonoHelper.Instance.StartCoroutine(AsyncLoadData());
                }
                else
                {
                    LoadUIData();
                }
            }
		}

        /// <summary>
        /// 同步加载UI
        /// </summary>
        private void LoadUIData()
        {
            UIInfoData _uiInfoData = null;
            UnityEngine.Object _prefabObj = null;
            GameObject _uiObject = null;

            if (stackOpenUIs != null && stackOpenUIs.Count > 0)
            {
                do
                {
                    _uiInfoData = stackOpenUIs.Pop();
                    _prefabObj = Resources.Load(_uiInfoData.Path);
                    if (_prefabObj != null)
                    {
                        //_uiObject = NGUITools.AddChild(Game.Instance.mainUICamera.gameObject, _prefabObj as GameObject);
                        _uiObject = MonoBehaviour.Instantiate(_prefabObj) as GameObject;
                        BaseUI _baseUI = _uiObject.GetComponent<BaseUI>();
                        if (null == _baseUI)
                        {
                            _baseUI = _uiObject.AddComponent(_uiInfoData.ScriptType) as BaseUI;
                        }

                        // 自动注册指定UGUI 
                        AutoInjectUGUI(_baseUI);

                        // 根据打开参数，View加载UI相关数据:比如音乐、动画等
                        if (null != _baseUI)
                        {
                            _baseUI.SetUIWhenOpening(_uiInfoData.UIParams);
                        }
                        dicOpenUIs.Add(_uiInfoData.UIType, _uiObject);
                    }

                } while (stackOpenUIs.Count > 0);
            }
        }

        /// <summary>
        /// 协程 加载UI
        /// </summary>
        /// <returns></returns>
		private IEnumerator<int> AsyncLoadData()
		{
			UIInfoData _uiInfoData = null;
			UnityEngine.Object _prefabObj = null;
			GameObject _uiObject = null;

			if (stackOpenUIs != null && stackOpenUIs.Count > 0)
			{
				do 
				{
					_uiInfoData = stackOpenUIs.Pop();
					_prefabObj = Resources.Load(_uiInfoData.Path);
					if (_prefabObj != null)
					{
						//_uiObject = NGUITools.AddChild(Game.Instance.mainUICamera.gameObject, _prefabObj as GameObject);
						_uiObject = MonoBehaviour.Instantiate(_prefabObj) as GameObject;
						BaseUI _baseUI = _uiObject.GetComponent<BaseUI>();
						if (null == _baseUI)
						{
							_baseUI = _uiObject.AddComponent(_uiInfoData.ScriptType) as BaseUI;
						}

                        // 自动注册指定UGUI 
                        AutoInjectUGUI(_baseUI);

                        // 根据打开参数，View加载UI相关数据:比如音乐、动画等
						if (null != _baseUI)
						{
							_baseUI.SetUIWhenOpening(_uiInfoData.UIParams);
						}
						dicOpenUIs.Add(_uiInfoData.UIType, _uiObject);
					}

				} while(stackOpenUIs.Count > 0);
			}
			yield return 0;
		}

		#endregion

		#region Close UI By EnumUIType
		/// <summary>
		/// 关闭界面。
		/// </summary>
		/// <param name="uiType">User interface type.</param>
		public void CloseUI(UIType _uiType)
		{
			GameObject _uiObj = null;
			if (!dicOpenUIs.TryGetValue(_uiType, out _uiObj))
			{
				Debug.Log("dicOpenUIs TryGetValue Failure! _uiType :" + _uiType.ToString());
				return;
			}
			CloseUI(_uiType, _uiObj);
		}

		/// <summary>
		/// Closes the U.
		/// </summary>
		/// <param name="_uiTypes">_ui types.</param>
		public void CloseUI(UIType[] _uiTypes)
		{
			for (int i=0; i<_uiTypes.Length; i++)
			{
				CloseUI(_uiTypes[i]);
			}
		}
		
		/// <summary>
		/// 关闭所有UI界面
		/// </summary>
		public void CloseUIAll()
		{
			List<UIType> _keyList = new List<UIType>(dicOpenUIs.Keys);
			foreach (UIType _uiType in _keyList)
			{
				GameObject _uiObj = dicOpenUIs[_uiType];
				CloseUI(_uiType, _uiObj);
			}
			dicOpenUIs.Clear();
		}

		private void CloseUI(UIType _uiType, GameObject _uiObj)
		{
			if (_uiObj == null)
			{
				dicOpenUIs.Remove(_uiType);
			}
			else
			{
				BaseUI _baseUI = _uiObj.GetComponent<BaseUI>();
				if (_baseUI != null)
				{
					_baseUI.StateChanged += CloseUIHandler;
					_baseUI.Release();
				}
				else
				{
                    GameObject.Destroy(_uiObj);

                    dicOpenUIs.Remove(_uiType);
				}
			}
		}

		/// <summary>
		/// Closes the user interface handler.
		/// </summary>
		/// <param name="_sender">_sender.</param>
		/// <param name="_newState">_new state.</param>
		/// <param name="_oldState">_old state.</param>
		private void CloseUIHandler(object _sender, EnumObjectState _newState, EnumObjectState _oldState)
		{
			if (_newState == EnumObjectState.Closing)
			{
				BaseUI _baseUI = _sender as BaseUI;
				dicOpenUIs.Remove(_baseUI.GetUIType());
				_baseUI.StateChanged -= CloseUIHandler;
			}
		}
        #endregion

        #region Auto Inject UGUI
        void AutoInjectUGUI(BaseUI _go)
        {
            Type type = _go.GetType();

            //fields :Get All Pulic Fields
            FieldInfo[] fis = type.GetFields();
            for (int i = 0; i < fis.Length; i++)
            {
                FieldInfo fi = fis[i];

                // Get AutoUGUI Attribute
                object[] attributes = fi.GetCustomAttributes(typeof(AutoUGUI), true);
                if (attributes.Length > 0)
                {
                    AutoUGUI att = attributes[0] as AutoUGUI;
                    SetValueByAttribute(_go, att,fi);
                }
            }
        }

        private void SetValueByAttribute(BaseUI _target, AutoUGUI attribute,FieldInfo _fi)
        {
            string fieldName = _fi.Name;
            Type type = _fi.FieldType;
            GameObject obj = _target.gameObject;
            Transform tf = null;

            if (!string.IsNullOrEmpty(attribute.Path))
            {
                tf = obj.transform.Find(attribute.Path);
            }
            else
            {
                Component[] pComp = obj.transform.GetComponentsInChildren(type);
                for (int i = 0; i < pComp.Length; i++)
                {
                    if (pComp[i].name.Equals(fieldName))
                    {
                        tf = pComp[i].transform;
                        break;
                    }
                }
            }
            if (tf != null && !type.IsValueType && tf.GetComponent(_fi.FieldType) != null)
            {
                try
                {
                    // _fi in _target,Set Value To _fi
                    _fi.SetValue(_target, tf.GetComponent(type));

                    //Debug.Log("注册UGUI：" + fieldName);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("注册UGUI失败：" + e);
                }

            }
        }
        #endregion
    }
}

