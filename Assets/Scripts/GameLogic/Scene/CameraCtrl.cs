using UnityEngine;
using System.Collections;
using System;
using ZFrameWork;

public enum EnumCameraState
{
    normal, overshoulder
}

public class CameraCtrl : MonoBehaviour
{
    public Func<bool> IsOnUICallback;

    bool isOnUI
    {
        get { return IsOnUICallback != null && IsOnUICallback(); }
    }

    public Vector3 OriVec = new Vector3();
    public float OriRotY = 45f;
    public float OriDis = 50f;
    public float OriSpeed = 100f;

    public bool CreateAvatar = true;

    Camera _tarCamera;
    public Camera tarCamera
    {
        get
        {
            if (_tarCamera == null)
                _tarCamera = GetComponent<Camera>();
            return _tarCamera;
        }
    }

    private EnumCameraState _cameraState;

    public EnumCameraState CameraState
    {
        get { return _cameraState; }
        set
        {
            //EnumCameraState oldState;
            //oldState = _cameraState;
            _cameraState = value;
            switch (_cameraState)
            {
                case EnumCameraState.normal:
                    TheFocusCtrl.RotV = 45f;
                    TheFocusCtrl.FocusDistance = 50f;
                    break;
                case EnumCameraState.overshoulder:
                    TheFocusCtrl.RotV = -30;
                    TheFocusCtrl.FocusDistance = 3f;
                    break;
            }
        }
    }

    /// <summary>是否需要自行接受键盘事件并移动相机</summary>
    public bool needKeyboardCtrl = true;

    public FocusCtrl TheFocusCtrl = new FocusCtrl();

    private float moveSpeed = 100f;

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = Mathf.Clamp(value, 1f, 10000f); }
    }

    void Awake()
    {
        //TheFocusCtrl
        TheFocusCtrl.createModel = CreateAvatar;


        TheFocusCtrl.FocusPos = OriVec;
        TheFocusCtrl.RotH = OriRotY;
        TheFocusCtrl.FocusDistance = OriDis;
        MoveSpeed = OriSpeed;
        CameraState = EnumCameraState.overshoulder;
        TheFocusCtrl.refreshBiasPos();
    }

    void FixedUpdate()
    {
        if (Utils.IsOnInputField())
        {
            //Debug.Log("Input Field");
            return;
        }

        //if (Utils.IsOnUI)
        //{
        //    //Debug.LogWarning("Is Ui................");
        //    return;
        //}

        if (CameraState == EnumCameraState.normal)
            FocusDistanceCtrl();
        CameraRotate();

        if (needKeyboardCtrl)
            CameraMove(GetMoveInput(false));
    }

    public static Vector3 GetMoveInput(bool allowArrow = true)
    {

        float kz = 0;
        if (allowArrow)
        {
            kz += Input.GetKey(KeyCode.UpArrow) ? 1f : 0f * 1f;
            kz += Input.GetKey(KeyCode.DownArrow) ? -1f : 0f * 1f;
        }
        kz += Input.GetKey(KeyCode.W) ? 1f : 0f * 1f;
        kz += Input.GetKey(KeyCode.S) ? -1f : 0f * 1f;
        float kx = 0;
        if (allowArrow)
        {
            kx -= Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f * 1f;
            kx -= Input.GetKey(KeyCode.RightArrow) ? 1f : 0f * 1f;
        }
        kx -= Input.GetKey(KeyCode.A) ? -1f : 0f * 1f;
        kx -= Input.GetKey(KeyCode.D) ? 1f : 0f * 1f;
        return new Vector3(kx, 0, kz);
    }

    void LateUpdate()
    {
        TheFocusCtrl.refreshBiasPos();
        SetPosAndLookpoint();
    }

    void Update()
    {
        if (needKeyboardCtrl)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CameraState =
                    CameraState == EnumCameraState.normal ?
                    EnumCameraState.overshoulder : EnumCameraState.normal;
            }
        }
    }

    void SetPosAndLookpoint()
    {
        if (CameraState == EnumCameraState.overshoulder)
        {
            TheFocusCtrl.height = Mathf.Sin(45f / 180 * Mathf.PI) * TheFocusCtrl.FocusDistance;
            TheFocusCtrl.radius = Mathf.Cos(45f / 180 * Mathf.PI) * TheFocusCtrl.FocusDistance;
            float deltaX = TheFocusCtrl.radius * Mathf.Cos((180 - TheFocusCtrl.RotH) / 180 * Mathf.PI);
            float deltaY = TheFocusCtrl.radius * Mathf.Sin((180 - TheFocusCtrl.RotH) / 180 * Mathf.PI);
            TheFocusCtrl.DeltaVec.Set(deltaX, TheFocusCtrl.height, deltaY);
            tarCamera.transform.position = TheFocusCtrl.FocusPos + TheFocusCtrl.DeltaVec;
            float lookAtHeight = Mathf.Sin(TheFocusCtrl.RotV / 180 * Mathf.PI) * TheFocusCtrl.FocusDistance;
            Vector3 lookAt = TheFocusCtrl.FocusPos - new Vector3(0, lookAtHeight, 0);
            tarCamera.transform.LookAt(lookAt);
        }
        else if (CameraState == EnumCameraState.normal)
        {
            tarCamera.transform.position = TheFocusCtrl.FocusPos + TheFocusCtrl.DeltaVec;
            tarCamera.transform.LookAt(TheFocusCtrl.FocusPos);
        }
    }

    public void FocusDistanceCtrl()
    {
        if (isOnUI)
            return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float scrollDelta = 1 + TheFocusCtrl.FocusDistance * 0.1f;
        float scrollMulti = 0;
        if (scroll > 0)
            scrollMulti = -1;
        else if (scroll < 0)
            scrollMulti = 1;
        if (scrollMulti != 0)
            TheFocusCtrl.FocusDistance += scrollDelta * scrollMulti;
    }

    public void CameraMove(Vector3 moveInput)
    {
        float multi = 1;//
        switch (CameraState)
        {
            case EnumCameraState.normal:
                multi = TheFocusCtrl.FocusDistance / 10f * MoveSpeed / 300f;
                break;
            case EnumCameraState.overshoulder:
                multi = TheFocusCtrl.FocusDistance / 10f * MoveSpeed / 300f * 5;
                break;
            default:
                multi = TheFocusCtrl.FocusDistance / 10f * MoveSpeed / 300f;
                break;
        }
        Vector3 deltaMove = moveInput.GetScaledVector(multi);
        TheFocusCtrl.FocusPos += MoveTowards((TheFocusCtrl.RotH) / 180 * Mathf.PI, deltaMove);
    }


    /// <summary>获取朝目标方向前进后退左右平移后, 坐标的改变</summary>
    /// <param name="rotH">水平面旋转方向</param>
    /// <param name="dirX">左右移动距离</param>
    public static Vector3 MoveTowards(float rotH, float dirX, float dirZ)
    {
        float dx = dirX * Mathf.Sin(rotH) + dirZ * Mathf.Cos(rotH);
        float dz = dirX * Mathf.Cos(rotH) - dirZ * Mathf.Sin(rotH);
        return new Vector3(dx, 0, dz);
    }
    public static Vector3 MoveTowards(float rotH, Vector3 dir)
    {
        float dx = dir.x * Mathf.Sin(rotH) + dir.z * Mathf.Cos(rotH);
        float dz = dir.x * Mathf.Cos(rotH) - dir.z * Mathf.Sin(rotH);
        return new Vector3(dx, 0, dz);
    }


    public void CameraRotate()
    {
        if (Input.GetMouseButton(1))
        {
            TheFocusCtrl.RotH += Input.GetAxis("Mouse X") * 2;
            TheFocusCtrl.RotV -= Input.GetAxis("Mouse Y") * 2;
        }
    }
}

public class FocusCtrl
{
    bool needRefreshBiasPos = false;
    public Vector3 DeltaVec;
    public float radius = 0;
    public float height = 0;
    public bool createModel = false;
    Animator _rollAnimator;
    Animator rollAnimator
    {
        get
        {
            if (_rollAnimator == null && focusBall != null)
                _rollAnimator = focusBall.GetComponent<Animator>();
            return _rollAnimator;
        }
    }

    public GameObject focusBall;
    bool isWalk;
    public bool IsWalk
    {
        get { return isWalk; }
        set
        {
            isWalk = value;
            if (rollAnimator != null)
            {
                //rollAnimator.SetBool("IsWalk", isWalk);
                rollAnimator.SetBool("Dash", isWalk);
            }
        }
    }

    private Vector3 focusPos;
    /// <summary>镜头焦点</summary>
    public Vector3 FocusPos
    {
        get { return focusPos; }
        set
        {
            Vector3 oldFocus = focusPos;
            IsWalk = oldFocus != value;

            focusPos = value;
            if (focusBall == null && createModel)
            {
                focusBall = (GameObject)GameObject.Instantiate(Resources.Load(Defines.PlayerModel));
                focusBall.name = "Role";
            }
            if (focusBall != null)
            {
                focusBall.transform.position = focusPos;
                focusBall.transform.eulerAngles = new Vector3(0, rotH + 90f, 0);
            }
        }
    }

    float rotH = 0;
    /// <summary>水平旋转角度 0~360</summary>
    public float RotH
    {
        get { return rotH; }
        set
        {
            rotH = value;
            rotH = rotH % 360f;
            needRefreshBiasPos = true;
        }
    }

    float rotV = 2;
    /// <summary>垂直旋转角度 2~88, 88度时俯视地面</summary>
    public float RotV
    {
        get { return rotV; }
        set
        {
            rotV = value;
            rotV = Mathf.Clamp(rotV, -88f, 88f);
            needRefreshBiasPos = true;
        }
    }

    /// <summary>镜头离焦点距离</summary>

    float minFocusDis = 1f;
    float maxFocusDis = float.MaxValue;

    private float focusDistance = 1f;
    public float FocusDistance
    {
        get { return focusDistance; }
        set
        {
            focusDistance = Mathf.Clamp(value, minFocusDis, maxFocusDis);
            needRefreshBiasPos = true;
        }
    }

    /// <summary>刷新镜头偏移坐标</summary>
    public void refreshBiasPos(bool immediatly = false)
    {
        if (needRefreshBiasPos || immediatly)
        {
            height = Mathf.Sin(RotV / 180 * Mathf.PI) * FocusDistance;

            radius = Mathf.Cos(RotV / 180 * Mathf.PI) * FocusDistance;
            float deltaX = radius * Mathf.Cos((180 - RotH) / 180 * Mathf.PI);
            float deltaY = radius * Mathf.Sin((180 - RotH) / 180 * Mathf.PI);
            DeltaVec.Set(deltaX, height, deltaY);
            needRefreshBiasPos = false;
        }
    }
}
