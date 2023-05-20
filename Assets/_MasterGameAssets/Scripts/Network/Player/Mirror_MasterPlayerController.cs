using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace PlanetMaenad.FPS
{
    public class Mirror_MasterPlayerController : NetworkBehaviour
    {
        public bool LockCursor = true;
        [Space(10)]

        public Mirror_FPSArmsController PlayerArmsController;
        public Mirror_FPSBodyController PlayerBodyController;
        public Mirror_HealthController healthControl;
        public HUDController HUDController;
        //public DEMOKillCounter KillCounter;
        [Space(5)]
        public Animator BodyAnimator;
        public Animator ArmsAnimator;
        [Space(10)]


        public KeyCode BlockButton = KeyCode.Mouse2;
        public KeyCode KickButton = KeyCode.F;
        [Space(10)]


        public float kickTime = .8f;
        public Collider KickTrigger;
        public GameObject DamageCrosshair;
        public Transform DamageCrosshairHolder;
        [Space(10)]


        public bool UseRandomDialogue;
        public float DialoguePercentageChance = 0.5f;
        public AudioSource DialogueAudioSource;
        public AudioClip[] DialogueAudioClips;
        public GameObject[] DialogueHUDTexts;



        [Header("-------------------------------------------------")]
        public GameObject ChatWindow;
        [SyncVar]
        public string playerName = "Player";
        public List<Text> playerNameText;
        [Space(10)]


        [Header("-------------------------------------------------")]
        public GameObject MainCam;
        public Camera _cam;
        [Space(10)]


        public KeyCode ToggleCursorButton = KeyCode.X;
        public Texture2D CursorSprite;
        [Space(10)]


        public float weaponsSwapTime = .25f;
        [SyncVar]
        public int equippedWeaponIndex = 0;
        public Mirror_FPSWeapon[] weapons;
        [Space(10)]


        [SyncVar]
        public bool LockFullbodyArms;
        [Space(10)]


        [SyncVar]
        public Vector3 CurrentShootPosition;
        [SyncVar]
        public Vector3 CurrentShootForwardVector;
        [SyncVar]
        public Vector3 CurrentShootUpVector;
        [SyncVar]
        public Vector3 CurrentShootRightVector;



        internal float DialogueTimer = 0f;
        internal bool IsBlocking;
        internal static readonly HashSet<string> playerNames = new HashSet<string>();


        internal WaitForSeconds TinyDelay = new WaitForSeconds(0.1f);
        internal WaitForSeconds SmallDelay = new WaitForSeconds(0.2f);
        internal WaitForSeconds MedDelay = new WaitForSeconds(0.5f);
        internal WaitForEndOfFrame WaitForEndOfFrameDelay;




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
            if (MainCam)
            {
                MainCam.SetActive(true);
                _cam.enabled = true;
            }


            if (!HUDController) HUDController = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDController>();
            healthControl.HUDController = HUDController;
            DamageCrosshairHolder = HUDController.DamageCrosshairHolder;
            //if (!KillCounter) KillCounter = GameObject.FindGameObjectWithTag("KillCounter").GetComponent<DEMOKillCounter>();

            ChatWindow = FindObjectOfType<MAIN_ChatUI>().transform.GetChild(0).gameObject;
            ChatWindow.SetActive(true);
            MAIN_ChatUI.localPlayerName = playerName;
        }

        public override void OnStopLocalPlayer()
        {
            ChatWindow.SetActive(false);
        }


        void Start()
        {
            //Is Not Local Player
            if (!isLocalPlayer)
            {
               
                return;
            }

            //Is Local Player
            if (isLocalPlayer)
            {
                if (CursorSprite) Cursor.SetCursor(CursorSprite, Vector2.zero, CursorMode.Auto);

                CmdChangeWeapon();             

                if (LockCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
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
                UpdateBodyController();
                UpdateInputValues();
                UpdateAnimator();

                HandlePassiveControls();
                HandleEquippedWeapons();
            }
        }
        void LateUpdate()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (isLocalPlayer)
            {
                UpdateArmsController();
            }
        }


        void UpdateBodyController()
        {
            PlayerBodyController.CheckGrounded();
            PlayerBodyController.CheckInputValues();
            PlayerBodyController.CheckAnimator();
            PlayerBodyController.StandardMove();
            PlayerBodyController.StandardJumpAndGravity();
        }
        void UpdateArmsController()
        {
            PlayerArmsController.CameraRotate();
            PlayerArmsController.HandleLockBones();
        }
        void UpdateInputValues()
        {
            //Kick
            if (Input.GetKeyDown(KickButton))
            {
                Kick();
            }

            //Block
            if (Input.GetKey(BlockButton))
            {
                BlockInput(true);

            }
            if (Input.GetKeyUp(BlockButton))
            {
                BlockInput(false);
            }

            //Cursor
            if (Input.GetKeyDown(ToggleCursorButton))
            {
                ToggleCursor();
            }

        }
        void UpdateAnimator()
        {
            ArmsAnimator.SetBool("Block", IsBlocking);
        }

        void HandlePassiveControls()
        {
            if (UseRandomDialogue) HandleRandomDialogue();
        }
        void HandleEquippedWeapons()
        {
            //Swap Weapons
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                equippedWeaponIndex++;
                CmdChangeEquippedWeaponIndex(equippedWeaponIndex);

                //Reset to First
                if (equippedWeaponIndex >= weapons.Length)
                {
                    CmdChangeEquippedWeaponIndex(0);
                }
                //Set to Next
                if (equippedWeaponIndex < 0)
                {
                    CmdChangeEquippedWeaponIndex(weapons.Length - 1);
                }

                CmdChangeWeapon();
            }
        }

        #region Combat

        public void BlockInput(bool newBlockState)
        {
            IsBlocking = newBlockState;
            healthControl.IsBlocking = newBlockState;
        }
        public void Kick()
        {
            CmdCrossFadeBodyAnimation("Kick", 0.1f);
            KickTrigger.enabled = true;

            Invoke("kickCancel", kickTime);
        }
        public void kickCancel()
        {
            KickTrigger.enabled = false;
        }



        [Command(requiresAuthority = false)]
        public void CmdPassDamage(GameObject other, Vector3 damagePoint, float hitForce, int Damage)
        {
            if (other.GetComponentInParent<Mirror_HealthController>())
            {
                var parentHealth = other.GetComponentInParent<Mirror_HealthController>();
                parentHealth.Damage(transform.forward * 360, hitForce, Damage);

                if (DamageCrosshair)
                {
                    Instantiate(DamageCrosshair, DamageCrosshairHolder.position, DamageCrosshair.transform.rotation, DamageCrosshairHolder);
                }

                //if (parentHealth.CharacterAnimator && hitReaction != null && !parentHealth.CharacterAnimator.GetCurrentAnimatorStateInfo(1).IsName(hitReaction) && !parentHealth.CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName(hitReaction)) parentHealth.CharacterAnimator.Play(hitReaction);              
            }

        }



        [Command(requiresAuthority = false)]
        public void CmdChangeEquippedWeaponIndex(int IndexNew)
        {
            equippedWeaponIndex = IndexNew;
        }


        [Command(requiresAuthority = false)]
        public void CmdChangeWeapon()
        {
            if (!NetworkClient.active)
            {
                ChangeWeapon();
            }

            RpcChangeWeapon();
        }
        [ClientRpc]
        void RpcChangeWeapon()
        {
            ChangeWeapon();
        }
        void ChangeWeapon()
        {
            var selectedWeapon = weapons[equippedWeaponIndex];

            Invoke("SwapWeapons", weaponsSwapTime);

            foreach (Mirror_FPSWeapon weapon in weapons)
            {
                weapon.swapping = true;
            }

            BodyAnimator.SetBool("Unequip", true);
        }
        void SwapWeapons()
        {
            var selectedWeapon = weapons[equippedWeaponIndex];

            //Set every other weapon except the one we want to swap to at index to false
            for (int i = 0; i < weapons.Length; i++)
            {
                if (i != equippedWeaponIndex)
                {
                    weapons[i].gameObject.SetActive(false);

                    if (weapons[i].SecondaryObjects.Length > 0)
                    {
                        for (int s = 0; s < weapons[i].SecondaryObjects.Length; s++)
                        {
                            weapons[i].SecondaryObjects[s].SetActive(false);
                        }
                    }

                }
            }

            //Set desired weapon to active
            selectedWeapon.gameObject.SetActive(true);
            if (selectedWeapon.SecondaryObjects.Length > 0)
            {
                for (int s = 0; s < selectedWeapon.SecondaryObjects.Length; s++)
                {
                    selectedWeapon.SecondaryObjects[s].SetActive(true);
                }
            }


            Invoke("SetSwappedWeaponPositions", .01f);

            BodyAnimator.SetBool("Unequip", false);
        }
        void SetSwappedWeaponPositions()
        {
            var selectedWeapon = weapons[equippedWeaponIndex];

            //Initialize the correct original aim position if it is the first time swapping

            BodyAnimator.SetFloat("MoveSetID", selectedWeapon.MoveSetID);
            ArmsAnimator.SetFloat("MoveSetID", selectedWeapon.MoveSetID);


            foreach (Mirror_FPSWeapon weapon in weapons)
            {
                weapon.swapping = false;
            }
        }



        //Set Shooter Point & Vector
        [Command(requiresAuthority = false)]
        public void CmdSetShooterVector(Vector3 _point, Vector3 _forwardVector, Vector3 _upVector, Vector3 _rightVector)
        {
            if (!NetworkClient.active)
            {
                CurrentShootPosition = _point;

                CurrentShootForwardVector = _forwardVector;
                CurrentShootUpVector = _upVector;
                CurrentShootRightVector = _rightVector;
            }

            RpcSetShooterVector(_point, _forwardVector, _upVector, _rightVector);
        }
        [ClientRpc]
        void RpcSetShooterVector(Vector3 _point, Vector3 _forwardVector, Vector3 _upVector, Vector3 _rightVector)
        {
            CurrentShootPosition = _point;

            CurrentShootForwardVector = _forwardVector;
            CurrentShootUpVector = _upVector;
            CurrentShootRightVector = _rightVector;
        }


        #endregion

        #region Special Controls

        public void HandleRandomDialogue()
        {
            if (DialogueAudioClips.Length > 0)
            {
                DialogueTimer += Time.deltaTime;

                if (DialogueTimer >= 10f)
                {
                    float random = Random.Range(0f, 1f);

                    if (random <= DialoguePercentageChance)
                    {
                        int randomIndex = Random.Range(0, DialogueAudioClips.Length);
                        DialogueAudioSource.clip = DialogueAudioClips[randomIndex];

                        if (!DialogueAudioSource.isPlaying) DialogueAudioSource.Play();
                        if (DialogueHUDTexts.Length > 0) DialogueHUDTexts[randomIndex].SetActive(true);
                    }

                    DialogueTimer = 0f;
                }
            }
        }



        public void ToggleCursor()
        {
            if (Cursor.visible == false)
            {
                StartCoroutine(ToggleCursorDelay(true));
            }
            if (Cursor.visible == true)
            {
                StartCoroutine(ToggleCursorDelay(false));
            }
        }
        IEnumerator ToggleCursorDelay(bool Bool)
        {
            yield return WaitForEndOfFrameDelay;

            if (Bool == true)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (Bool == false)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }


        #endregion

        [Command(requiresAuthority = false)]
        public void CmdSetLockFullbodyArms(bool Bool)
        {
            LockFullbodyArms = Bool;
        }


        #region Arms Controls

        [Command(requiresAuthority = false)]
        public void CmdPlayArmsAnimation(string clipName)
        {
            if (!NetworkClient.active)
            {
                ArmsAnimator.Play(clipName);
            }

            RpcPlayArmsAnimation(clipName);
        }
        [ClientRpc]
        void RpcPlayArmsAnimation(string clipName)
        {
            ArmsAnimator.Play(clipName);
        }


        [Command(requiresAuthority = false)]
        public void CmdCrossFadeArmsAnimation(string clipName, float startTime)
        {
            if (!NetworkClient.active)
            {
                ArmsAnimator.CrossFade(clipName, startTime);
            }

            RpcCrossFadeArmsAnimation(clipName, startTime);
        }
        [ClientRpc]
        void RpcCrossFadeArmsAnimation(string clipName, float startTime)
        {
            ArmsAnimator.CrossFade(clipName, startTime);
        }

        #endregion

        #region Body Controls

        [Command(requiresAuthority = false)]
        public void CmdPlayBodyAnimation(string clipName)
        {
            if (!NetworkClient.active)
            {
                BodyAnimator.Play(clipName);
            }

            RpcPlayBodyAnimation(clipName);
        }
        [ClientRpc]
        void RpcPlayBodyAnimation(string clipName)
        {
            BodyAnimator.Play(clipName);
        }


        [Command(requiresAuthority = false)]
        public void CmdCrossFadeBodyAnimation(string clipName, float startTime)
        {
            if (!NetworkClient.active)
            {
                BodyAnimator.CrossFade(clipName, startTime);
            }

            RpcCrossFadeBodyAnimation(clipName, startTime);
        }
        [ClientRpc]
        void RpcCrossFadeBodyAnimation(string clipName, float startTime)
        {
            BodyAnimator.CrossFade(clipName, startTime);
        }

        #endregion


    }
}


