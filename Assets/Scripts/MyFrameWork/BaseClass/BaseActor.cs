
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrameWork
{
    /// <summary>
    /// 所有场景对象基类：NPC,Role,Monster
    /// </summary>
	public class BaseActor : IDynamicProperty
	{
        #region Base Data
        // 对象属性字典
        protected Dictionary<int, PropertyItem> dicProperty = null;

        public event PropertyChangedHandle PropertyChanged;

        public EnumActorType ActorType { set; get; }

        public int ID { set; get; }

        private BaseScene currentScene;

        public BaseScene CurrentScene
        {
            set
            {
                //add Change Scene Logic...
                currentScene = value;
            }
            get
            {
                return currentScene;
            }
        }

        public BaseActor()
        {
        }
        #endregion

        #region Property Register & UnRegister
        public virtual void AddProperty(PropertyType propertyType, object content)
        {
            AddProperty((int)propertyType, content);
        }

        public virtual void AddProperty(int id, object content)
        {
            PropertyItem property = new PropertyItem(id, content);
            AddProperty(property);
        }

        public virtual void AddProperty(PropertyItem property)
        {
            if (null == dicProperty)
            {
                dicProperty = new Dictionary<int, PropertyItem>();
            }
            if (dicProperty.ContainsKey(property.ID))
            {
                //remove same property
            }
            dicProperty.Add(property.ID, property);
            property.Owner = this;
        }

        public void RemoveProperty(PropertyType propertyType)
        {
            RemoveProperty((int)propertyType);
        }

        public void RemoveProperty(int id)
        {
            if (null != dicProperty && dicProperty.ContainsKey(id))
                dicProperty.Remove(id);
        }

        public void ClearProperty()
        {
            if (null != dicProperty)
            {
                dicProperty.Clear();
                dicProperty = null;
            }
        }

        #endregion

        #region Property Update
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public virtual PropertyItem GetProperty(PropertyType propertyType)
        {
            return GetProperty((int)propertyType);
        }

        /// <summary>
        /// 属性变更处理
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
		protected virtual void OnPropertyChanged(int id, object oldValue, object newValue)
        {
            //add update ....
            //			if (id == (int)EnumPropertyType.HP)
            //			{
            //
            //			}
        }

        /// <summary>
        /// 属性变更回调
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
		public void DoChangeProperty(int id, object oldValue, object newValue)
        {
            OnPropertyChanged(id, oldValue, newValue);
            if (null != PropertyChanged)
                PropertyChanged(this, id, oldValue, newValue);
        }

        public PropertyItem GetProperty(int id)
        {
            if (null == dicProperty)
                return null;
            if (dicProperty.ContainsKey(id))
                return dicProperty[id];
            Debug.LogWarning("Actor dicProperty non Property ID: " + id);
            return null;
        }
        #endregion

	}
}

