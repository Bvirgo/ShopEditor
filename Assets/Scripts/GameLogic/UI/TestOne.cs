using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ZFrameWork;
using UnityEngine.EventSystems;

public class TestOne : BaseUI 
{
	private TestOneModule oneModule;

	private Button btn;
	private Text text;
	#region implemented abstract members of BaseUI
	public override UIType GetUIType ()
	{
		return UIType.TestOne;
	}
	#endregion

	// Use this for initialization
	void Start ()
	{
//		btn = transform.Find("Panel/Button").GetComponent<Button>();
//		btn.onClick.AddListener(OnClickBtn);

		text = transform.Find("Panel/Text").GetComponent<Text>();

		//EventTriggerListener.Get(transform.Find("Panel/Button").gameObject).SetEventHandle(EnumTouchEventType.OnClick, Close);
	
        // 绑定事件
		EventTriggerListener listener = EventTriggerListener.Get(transform.Find("Panel/Button").gameObject);
		listener.SetEventHandle(EnumTouchEventType.OnClick, Close, 1, "1234");

        // 拖拽事件
        listener.SetEventHandle(EnumTouchEventType.OnDrag, OnMyDrag, 1, "BeginDrag....");

        // 获取M 数据
		oneModule = ModuleManager.Instance.Get<TestOneModule>();
		text.text = "Gold: " + oneModule.Gold;
	}

    void OnMyDrag(GameObject _listener, object _args, params object[] _params)
    {
        PointerEventData ped = _args as PointerEventData;
        Debug.Log("Listener:"+ _listener + "--Args:"+_args + "---Params:"+_params.ToString());
        _listener.transform.position   = _listener.transform.position + new Vector3(ped.delta.x,ped.delta.y,0);
    }

	protected override void OnAwake ()
	{
        // 消息监听
		MessageCenter.Instance.AddListener("AutoUpdateGold", UpdateGold);
		base.OnAwake ();
	}

	protected override void OnRelease ()
	{
		MessageCenter.Instance.RemoveListener("AutoUpdateGold", UpdateGold);
		base.OnRelease ();
	}

	private void UpdateGold(Message message)
	{
		int gold = (int) message["gold"];
		Debug.Log("TestOne UpdateGold : " + gold);
		text.text = "Gold: " + gold;
	}

	private void OnClickBtn()
	{
		UIManager.Instance.OpenUICloseOthers(UIType.TestTwo,true);
//		GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/TestUITwo"));
//		TestTwo to = go.GetComponent<TestTwo>();
//		if (null == to)
//			to = go.AddComponent<TestTwo>();
//		Close();
	}

    //	private void Close()
    //	{
    //		Destroy(gameObject);
    //	}

    /// </summary>
    /// <param name="_listener">按钮本身</param>
    /// <param name="_args">Unity 响应点击，传入参数PointerEventData</param>
    /// <param name="_params">用户自定义参数</param>
    private void Close(GameObject _listener, object _args, params object[] _params)
	{
		int i = (int) _params[0];
		string s = (string) _params[1];
		Debug.Log(i);
		Debug.Log(s);
		UIManager.Instance.OpenUICloseOthers(UIType.TestTwo,true);
	}
}

