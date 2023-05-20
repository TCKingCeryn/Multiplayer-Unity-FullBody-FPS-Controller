using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace PlanetMaenad.FPS
{
    public class Mirror_ScopeController : MonoBehaviour
    {
        public Mirror_MasterPlayerController PlayerController;
        public Mirror_FPSWeapon Weapon;
        [Space(10)]

        public Camera SniperCam;
        public Animator blackLensAnim;



        void Start()
        {

        }


        void Update()
        {
            if (PlayerController && PlayerController.isLocalPlayer)
            {
                if (Weapon && Weapon.aiming)
                {
                    SniperCam.gameObject.SetActive(true);
                    SniperCam.enabled = true;

                    blackLensAnim.SetBool("aiming", true);
                }
                else
                {
                    SniperCam.enabled = false;
                    SniperCam.gameObject.SetActive(false);

                    blackLensAnim.SetBool("aiming", false);
                }
            }            
        }

        void OnDisable()
        {
            if (PlayerController && PlayerController.isLocalPlayer)
            {
                SniperCam.enabled = false;
                SniperCam.gameObject.SetActive(false);
            }
        }
    }
}
