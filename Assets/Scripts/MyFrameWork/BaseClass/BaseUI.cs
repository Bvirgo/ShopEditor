
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ZFrameWork
{
    /// <summary>
    /// View Base Class
    /// </summary>
    public abstract class BaseUI : MonoBehaviour
    {
        #region Cache gameObject & transfrom

        public object[] uiParams;

        private Transform _CachedTransform;
        /// <summary>
        /// Gets the cached transform.
        /// </summary>
        /// <value>The cached transform.</value>
        public Transform cachedTransform
        {
            get
            {
                if (!_CachedTransform)
                {
                    _CachedTransform = this.transform;
                }
                return _CachedTransform;
            }
        }

        private GameObject _CachedGameObject;
        /// <summary>
        /// Gets the cached game object.
        /// </summary>
        /// <value>The cached game object.</value>
        public GameObject cachedGameObject
        {
            get
            {
                if (!_CachedGameObject)
                {
                    _CachedGameObject = this.gameObject;
                }
                return _CachedGameObject;
            }
        }

        #endregion

        #region UIType & EnumObjectState
        /// <summary>
        /// The state.
        /// </summary>
        protected EnumObjectState state = EnumObjectState.None;

        /// <summary>
        /// Occurs when state changed.
        /// </summary>
        public event StateChangedEvent StateChanged;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public EnumObjectState State
        {
            protected set
            {
                if (value != state)
                {
                    EnumObjectState oldState = state;
                    state = value;
                    if (null != StateChanged)
                    {
                        StateChanged(this, state, oldState);
                    }
                }
            }
            get { return this.state; }
        }

        /// <summary>
        /// Gets the type of the user interface.
        /// </summary>
        /// <returns>The user interface type.</returns>
        public abstract UIType GetUIType();

        #endregion

        #region Event
        public Dictionary<string, MessageEvent> event_action;
        /// <summary>
        /// Clean Events
        /// </summary>
        protected virtual void OnRemoveEvent()
        {
            foreach (var item in event_action)
            {
                MessageCenter.Instance.RemoveListener(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Cache Events
        /// </summary>
        /// <param name="_strEvent"></param>
        /// <param name="_msg"></param>
        protected virtual void PushEvent(string _strEvent, MessageEvent _msg)
        {
            if (!event_action.ContainsKey(_strEvent))
            {
                event_action.Add(_strEvent, _msg);
            }
        }
        #endregion

        #region When Open
        void Awake()
        {
            this.State = EnumObjectState.Initial;
            event_action = new Dictionary<string, MessageEvent>();
            OnAwake();
        }

        // Use this for initialization
        void Start()
        {
            OnStart();
        }

        protected virtual void OnAwake()
        {
            this.State = EnumObjectState.Loading;
            // Play When Open
            this.OnPlayOpenUIAudio();
        }

        protected virtual void OnStart()
        {

        }


        /// <summary>
        /// Play Music When Open
        /// </summary>
        protected virtual void OnPlayOpenUIAudio()
        {

        }

        protected virtual void SetUI(params object[] uiParams)
        {
            this.State = EnumObjectState.Loading;
            this.uiParams = uiParams;
        }

        /// <summary>
        /// Load Data When Open
        /// </summary>
        protected virtual void OnLoadData()
        {

        }

        /// <summary>
        /// Set Params & Async Load Data When Open
        /// </summary>
        /// <param name="uiParams"></param>
        public void SetUIWhenOpening(params object[] uiParams)
        {
            SetUI(uiParams);

            MonoHelper.Instance.StartCoroutine(AsyncOnLoadData());
        }

        private IEnumerator AsyncOnLoadData()
        {
            yield return new WaitForSeconds(0);
            if (this.State == EnumObjectState.Loading)
            {
                this.OnLoadData();
                this.State = EnumObjectState.Ready;
            }
        }

        #endregion

        #region When Close

        /// <summary>
        /// Release this instance.
        /// </summary>
        public void Release()
        {
            this.State = EnumObjectState.Closing;
            GameObject.Destroy(cachedGameObject);
            OnRelease();
        }


        protected virtual void OnRelease()
        {
            this.OnPlayCloseUIAudio();
            OnRemoveEvent();
        }

        /// <summary>
        /// Play Music When Close
        /// </summary>
        protected virtual void OnPlayCloseUIAudio()
        {

        }
        #endregion

        #region Update
        /// <summary>
        /// UI Top
        /// </summary>
        protected virtual void SetDepthToTop()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (EnumObjectState.Ready == this.state)
            {
                OnUpdate(Time.deltaTime);
            }
        }

        protected virtual void OnUpdate(float deltaTime)
        {

        }
        #endregion
    }
}

