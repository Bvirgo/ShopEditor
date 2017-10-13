
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MyFrameWork;
using UniRx;
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

    // You can monitor/modifie in inspector by SpecializedReactiveProperty
    public IntReactiveProperty IntRxProp = new IntReactiveProperty();

    Enemy enemy;

    #region implemented abstract members of BaseUI
    public override UIType GetUIType ()
	{
		return UIType.TestTwo;
	}
    protected override void OnAwake()
    {
        base.OnAwake();
        enemy = new Enemy(1000);
    }
    #endregion

    // Use this for initialization
    void Start ()
	{
		btn = transform.Find ("Panel/Button").GetComponent<Button> ();
		btn.onClick.AddListener (OnClickBtn);
        
        InitUIByUniRx();
	}

    /// <summary>
    /// 利用UniRx扩展UGUI
    /// </summary>
    void InitUIByUniRx()
    {
        // UnityEvent as Observable
        // (shortcut, MyButton.OnClickAsObservable())
        btn_hit.onClick.AsObservable().Subscribe(_ => {
            enemy.CurrentHp.Value -= 99;
            enemy.m_nMoney += 1;
        } );

        // Toggle, Input etc as Observable(OnValueChangedAsObservable is helper for provide isOn value on subscribe)
        // SubscribeToInteractable is UniRx.UI Extension Method, same as .interactable = x)
        tg_btn.OnValueChangedAsObservable().SubscribeToInteractable(btn_hit); // Toggle 勾选，控制按钮是否可用

        // input shows delay after 1 second
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
        ipt_info.OnValueChangedAsObservable()
#else
            MyInput.OnValueChangeAsObservable()
#endif
            .Where(x => x != null)
            .Delay(TimeSpan.FromSeconds(1))
            .SubscribeToText(tx_info); // SubscribeToText is UniRx.UI Extension Method

        // converting for human visibility
        sld_value.OnValueChangedAsObservable()
            .SubscribeToText(tx_info, x => Math.Round(x, 2).ToString());

        // from RxProp, CurrentHp changing(Button Click) is observable
        enemy.CurrentHp.SubscribeToText(tx_info);
        enemy.IsDead.Where(isDead => isDead == true)
            .Subscribe((Action<bool>)(_ =>
            {
                tg_btn.interactable = this.btn_hit.interactable = false;
            }));

        // initial text:)
        IntRxProp.SubscribeToText(tx_info);

        // 给enemy的Money挂一个触发器，实时监控
        enemy
            .ObserveEveryValueChanged(c => c.m_nMoney)
            .Where(x => x > 0) // 只是监视Money > 0时候的值变化
            .Subscribe(_ => Debug.Log("Monitor Money:"+_))
            .AddTo(gameObject); // 生命周期关联该对象
    }


    private void OnClickBtn ()
	{
		UIManager.Instance.OpenUICloseOthers(UIType.TestOne,true);
	}
}

// Reactive Notification Model
public class Enemy
{
    public IReactiveProperty<long> CurrentHp { get; private set; }

    public IReactiveProperty<bool> IsDead { get; private set; }

    public int m_nMoney;

    public Enemy(int initialHp)
    {
        // Declarative Property
        CurrentHp = new ReactiveProperty<long>(initialHp);
        IsDead = CurrentHp.Select(x => x <= 0).ToReactiveProperty();

        m_nMoney = 0;
    }
}