
using System;
namespace MyFrameWork
{
    /// <summary>
    /// ��һ��ɫ���ԣ����磺Ѫ����ħ��ֵ�ȵ�
    /// ���Է����ף�Ҳ�������ɴ�����
    /// </summary>
	public class PropertyItem
	{
        // ����ID
		public int ID { get; set; }

        // �����ڴ�ֵ
		private object content;

        // ����ԭʼֵ:�ڴ��б�¶������������
		private object rawContent;

        // ����ֵ�Ƿ��ǿ����
		private bool canRandom = false;
		private int curRandomInt;
		private float curRandomFloat;

		private Type propertyType;

		// owner
		public IDynamicProperty Owner = null;

        /// <summary>
        /// ���ڴ��޸�����
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

                    // �洢��ֵ
					SetContent(value);

                    // ֪ͨ������������ֵ���
					if (Owner != null)
						Owner.DoChangeProperty(ID, oldContent, value);
				}
			}
		}

        /// <summary>
        /// ���Ըı䣬��֪ͨOwner
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
                // �����������������ʵֵ
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
        /// ��ȡ������ʵֵ
        /// </summary>
        /// <returns></returns>
		private object GetContent()
        {
            if (canRandom)
            {
                if (propertyType == typeof(System.Int32))
                {
                    int ret = (int)this.content - curRandomInt;

                    // ���������쳣��֪ͨ
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

