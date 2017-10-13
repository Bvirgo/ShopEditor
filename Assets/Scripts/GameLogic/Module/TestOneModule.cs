
using System;
using MyFrameWork;

public class TestOneModule : BaseModule
{
	public int Gold { get; private set; }

	public TestOneModule ()
	{
        // M 是否加入 ModulManager管理
		this.AutoRegister = true;
	}

	protected override void OnLoad ()
	{
		MessageCenter.Instance.AddListener(MsgType.Net_MessageTestOne, UpdateGold);
		base.OnLoad ();
	}

	protected override void OnRelease ()
	{
		MessageCenter.Instance.RemoveListener(MsgType.Net_MessageTestOne, UpdateGold);
		base.OnRelease ();
	}

	private void UpdateGold(Message message)
	{
		int gold = (int) message["gold"];
		if (gold >= 0)
		{
			Gold = gold;
			Message temp = new Message("AutoUpdateGold", this);
			temp["gold"] = gold;
			temp.Send();
		}
	}
}

