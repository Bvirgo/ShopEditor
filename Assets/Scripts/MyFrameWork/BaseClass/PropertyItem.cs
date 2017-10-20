
using System;
namespace ZFrameWork
{
    /// <summary>
    /// 单一角色属性：比如：血量，魔法值等等
    /// 属性防作弊，也可是做成触发器
    /// </summary>
	public class PropertyItem
	{
        // 属性ID
		public int ID { get; set; }

        // 属性内存值
		private object content;

        // 属性原始值:内存中暴露给作弊者随便改
		private object rawContent;

        // 属性值是否是可随机
		private bool canRandom = false;
		private int curRandomInt;
		private float curRandomFloat;

		private Type propertyType;

		// owner
		public IDynamicProperty Owner = null;

        /// <summary>
        /// 防内存修改作弊
        /// </summary>
		public object Content
		{
			get 
			{
				return GetContent();
			}
			set 
			{
				if (value != GetContent())
				{
					object oldContent = GetContent();

                    // 存储新值
					SetContent(value);

                    // 通知属性所属对象值变更
					if (Owner != null)
						Owner.DoChangeProperty(ID, oldContent, value);
				}
			}
		}

        /// <summary>
        /// 属性改变，不通知Owner
        /// </summary>
        /// <param name="content"></param>
		public void SetValueWithoutEvent(object content)
		{
			if (content != GetContent())
			{
				//object oldContent = GetContent();
				SetContent(content);
			}
		}

		public object RawContent
		{
			get { return rawContent; }
		}

		public PropertyItem (int id , object content)
		{
			propertyType = content.GetType();
			if (propertyType == typeof(System.Int32) || propertyType == typeof(System.Single))
			{
				canRandom = true;
			}

			ID = id;
			SetContent(content);
			rawContent = content;
		}

		private void SetContent(object content)
		{
            rawContent = content;

            if (canRandom)
			{
                // 生成随机数，隐藏真实值
				if (propertyType == typeof(System.Int32))
				{
					curRandomInt = UnityEngine.Random.Range(1, 1000);
					this.content = (int)content + curRandomInt;
				}
				else if (propertyType == typeof(System.Single))
				{
					curRandomFloat = UnityEngine.Random.Range(1.0f, 1000.0f);
					this.content = (float)content + curRandomFloat;
				}
			}
			else
			{
				this.content = content;
			}
		}

        /// <summary>
        /// 获取属性真实值
        /// </summary>
        /// <returns></returns>
		private object GetContent()
        {
            if (canRandom)
            {
                if (propertyType == typeof(System.Int32))
                {
                    int ret = (int)this.content - curRandomInt;

                    // 存在作弊异常，通知
                    if (ret != (int)rawContent)
                    {
                        Message message = new Message(MsgType.Com_PropertyException, this, ID);
                        message.Send();
                    }
                    return ret;
                }
                else if (propertyType == typeof(System.Single))
                {
                    float ret = (float)this.content - curRandomFloat;
                    if (ret != (float)rawContent)
                    {
                        Message message = new Message(MsgType.Com_PropertyException, this, ID);
                        message.Send();
                    }
                    return ret;
                }
            }
            return this.content;
        }
    }
}

