
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ZFrameWork;
using System;

public class TestTwo : BaseUI
{

	private Button btn;
    [AutoUGUI,HideInInspector]
    public Button btn_hit;
    [AutoUGUI, HideInInspector]
    public Toggle tg_btn;
    [AutoUGUI, HideInInspector]
    public InputField ipt_info;
    [AutoUGUI, HideInInspector]
    public Text tx_info;
    [AutoUGUI, HideInInspector]
    public Slider sld_value;
    #region implemented abstract members of BaseUI
    public override UIType GetUIType ()
	{
		return UIType.TestTwo;
	}
    protected override void OnAwake()
    {
        base.OnAwake();
    }
    #endregion

    // Use this for initialization
    void Start ()
	{
		btn = transform.Find ("Panel/Button").GetComponent<Button> ();
		btn.onClick.AddListener (OnClickBtn);
        
	}

    private void OnClickBtn ()
	{
		UIManager.Instance.OpenUICloseOthers(UIType.TestOne,true);
	}
}