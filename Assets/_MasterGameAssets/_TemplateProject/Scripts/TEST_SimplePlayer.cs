using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class TEST_SimplePlayer : NetworkBehaviour
{
    public CharacterController controller;
    [Space(10)]

    public Transform DirectionPivot;
    public Transform FPSCamPivot;
    public MouseLook Look;
    [Space(10)]




    [Header("-------------------------------------------------")]
    public GameObject ChatWindow;
    [SyncVar]
    public string playerName;
    public List<Text> playerNameText;
    [Space(10)]


    public GameObject _mainCamera;
    public Camera _cam;


    [System.Serializable]
    public class MouseLook
    {
        public bool Enabled;
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public float SensivityMultiplier = 1f;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public float SmoothTime = 15f;
        public bool ClampVerticalRotation = true;

        public string AxisXName = "Mouse X";
        public string AxisYName = "Mouse Y";

        private Quaternion characterTargetRot;
        private Quaternion cameraTargetRot;

        private Transform character;
        private Transform camera;


        public void Init(Transform character, Transform camera)
        {
            characterTargetRot = character.localRotation;
            cameraTargetRot = camera.localRotation;

            this.character = character;
            this.camera = camera;
        }

        public void ApplyDelta(Vector2 delta)
        {
            LookRotation(delta.x, delta.y, Time.deltaTime);
        }
        public void LookRotation(float deltaTime)
        {
            if (!Enabled)
                return;

            LookRotation(Input.GetAxis(AxisXName) * XSensitivity * SensivityMultiplier, Input.GetAxis(AxisYName) * YSensitivity * SensivityMultiplier, deltaTime);
        }
        public void LookRotation(float yRot, float xRot, float deltaTime)
        {
            characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (ClampVerticalRotation)
                cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            character.localRotation = Quaternion.Slerp(character.localRotation, characterTargetRot, SmoothTime * deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRot, SmoothTime * deltaTime);
        }
        public void RotateCameraSmoothlyTo(float xRot, float deltaTime)
        {
            cameraTargetRot = Quaternion.Euler(xRot, 0f, 0f);

            if (ClampVerticalRotation)
                cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

            camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRot, SmoothTime * deltaTime);
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }


    #region Internal Variables

    internal static readonly HashSet<string> playerNames = new HashSet<string>();

    internal Vector3 FPSCamPivotPosition;
    internal float _verticalVelocity;
    internal Vector2 move;

    internal bool jump;
    internal bool sprint;

    internal float _initialCapsuleHeight;
    internal float _initialCapsuleRadius;

    internal float _speed;
    internal float _animationBlend;
    internal float _targetRotation = 0.0f;
    internal float _rotationVelocity;
    internal float _terminalVelocity = 53.0f;

    internal float _jumpTimeoutDelta;
    internal float _fallTimeoutDelta;
    internal float m_OrigGroundCheckDistance;

    const float k_Half = 0.5f;

    internal float m_TurnAmount;
    internal float m_ForwardAmount;

    internal float m_CapsuleHeight;
    internal Vector3 m_CapsuleCenter;


    bool strafe;
    bool forwards;
    bool backwards;
    bool right;
    bool left;

    float horizontalInput;
    float verticalInput;

    internal Vector3 m_CamForward;

    float maxCamOriginal;
    float minCamOriginal;

    bool toggle;
    bool m_Jump;

    #endregion


    public override void OnStartServer()
    {
        playerName = (string)connectionToClient.authenticationData;

        if (isLocalPlayer)
        {
            gameObject.name = playerName;
        }
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
    }
    public override void OnStartLocalPlayer()
    {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _cam = _mainCamera.GetComponent<Camera>();


        ChatWindow = FindObjectOfType<TEST_ChatUI>().transform.GetChild(0).gameObject;
        ChatWindow.SetActive(true);
        TEST_ChatUI.localPlayerName = playerName;

        //CmdChangeNameText(playerName);
    }


    public override void OnStopLocalPlayer()
    {
        ChatWindow.SetActive(false);
    }
    public void OnApplicationQuit()
    {

    }


    void Start()
    {
      
        //Is Not Local Player
        if (!isLocalPlayer)
        {
            if (_mainCamera)
            {
                _cam.enabled = false;
                _mainCamera.gameObject.SetActive(false);
            }
            
            controller.enabled = false;
            return;
        }

        //Is Local Player
        if (isLocalPlayer)
        {
            Look.Init(transform, FPSCamPivot);
            FPSCamPivotPosition = FPSCamPivot.localPosition;
        }

    }
    void Update()
    {
        //Is Not Local Player
        if (!isLocalPlayer)
        {
            return;
        }


        if (isLocalPlayer)
        {
            HandleFPSLook();
        }
    }


    void HandleFPSLook()
    {
        Look.LookRotation(Time.deltaTime);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        FPSCamPivot.localPosition = FPSCamPivotPosition;

        Vector3 forward = DirectionPivot.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 moveVector = forward * v + DirectionPivot.right * h;

        controller.Move(moveVector * 5f * Time.deltaTime);
    }

}
