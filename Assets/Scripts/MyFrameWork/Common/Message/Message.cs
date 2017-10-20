
using System;
using System.Collections;
using System.Collections.Generic;


namespace ZFrameWork
{
	public class Message : IEnumerable<KeyValuePair<string, object>>
	{
        // 参数字典(key - value)
		private Dictionary<string, object> dicDatas = null;

        // 消息名称（key）
		public string Name { get; private set; }
        // 发送方
		public object Sender { get; private set; }
        // 附件内容
		public object Content { get; set; }

		#region message[key] = value or data = message[key]

		/// <summary>
		/// Gets or sets the <see cref="ZFrameWork.Message"/> with the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public object this[string key]
		{
			get
			{
				if (null == dicDatas || !dicDatas.ContainsKey(key))
					return null;
				return dicDatas[key];
			}
			set
			{
				if (null == dicDatas)
					dicDatas = new Dictionary<string, object>();
				if (dicDatas.ContainsKey(key))
					dicDatas[key] = value;
				else
					dicDatas.Add(key, value);
			}
		}

		#endregion

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
		{
			if (null == dicDatas)
				yield break;
			foreach (KeyValuePair<string, object> kvp in dicDatas)
			{
				yield return kvp;
			}
		}

        public bool IsKey(string _strKey)
        {
            return dicDatas.ContainsKey(_strKey);
        }

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return dicDatas.GetEnumerator();
		}

		#endregion

		#region Message Construction Function

		/// <summary>
		/// Initializes a new instance of the <see cref="ZFrameWork.Message"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="sender">Sender.</param>
		public Message (string name, object sender)
		{
			Name = name;
			Sender = sender;
			Content = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZFrameWork.Message"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="content">Content.</param>
		public Message (string name, object sender, object content)
		{
			Name = name;
			Sender = sender;
			Content = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZFrameWork.Message"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="content">Content.</param>
		/// <param name="_dicParams">_dic parameters.</param>
		public Message (string name, object sender, object content, params object[] _dicParams)
		{
			Name = name;
			Sender = sender;
			Content = content;
			if (_dicParams.GetType() == typeof(Dictionary<string, object>))
			{
				foreach (object _dicParam in _dicParams)
				{
					foreach (KeyValuePair<string, object> kvp in _dicParam as Dictionary<string, object>)
					{
						//dicDatas[kvp.Key] = kvp.Value;  //error
						this[kvp.Key] = kvp.Value;
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZFrameWork.Message"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public Message (Message message)
		{
			Name = message.Name;
			Sender = message.Sender;
			Content = message.Content;
			foreach (KeyValuePair<string, object> kvp in message.dicDatas)
			{
				this[kvp.Key] = kvp.Value;
			}
		}

        public Message(string _strName, Message _msg)
        {
            Name = _strName;
            Sender = _msg.Sender;
            Content = _msg.Content;
            foreach (KeyValuePair<string, object> kvp in _msg.dicDatas)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

		#endregion

		#region Add & Remove

		/// <summary>
		/// Add the specified key and value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void Add(string key, object value)
		{
			this[key] = value;
		}

		/// <summary>
		/// Remove the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		public void Remove(string key)
		{
			if (null != dicDatas && dicDatas.ContainsKey(key))
			{
				dicDatas.Remove(key);
			}
		}
		#endregion

		#region Send()

		/// <summary>
		/// Send this instance.
		/// </summary>
		public void Send()
		{
			//MessageCenter Send Message
			MessageCenter.Instance.SendMessage(this);
		}
		#endregion

	}
}

