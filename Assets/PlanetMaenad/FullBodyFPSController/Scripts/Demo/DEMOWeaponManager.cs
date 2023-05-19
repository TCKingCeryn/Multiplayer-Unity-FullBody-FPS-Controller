using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanetMaenad.FPS
{
    public class DEMOWeaponManager : MonoBehaviour
    {
        public Animator animator;
        //public OffsetYRotation SpineOffsetRotation;
        public float weaponsSwapTime = .25f;
        [Space(10)]

        public int index = 0;
        public FPSWeapon[] weapons;



        void Start()
        {
            ChangeWeapon();
        }
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                index++;

                //Reset to First
                if (index >= weapons.Length)
                {
                    index = 0;
                }
                //Set to Next
                if (index < 0)
                {
                    index = weapons.Length - 1;
                }

                ChangeWeapon();
            }

        }


        public void ChangeWeapon()
        {
            var selectedWeapon = weapons[index];

            Invoke("swapWeapons", weaponsSwapTime);

            foreach (FPSWeapon weapon in weapons)
            {
                weapon.swapping = true;
            }

            animator.SetBool("Unequip", true);
        }

        void swapWeapons()
        {
            var selectedWeapon = weapons[index];

            //Set every other weapon except the one we want to swap to at index to false
            for (int i = 0; i < weapons.Length; i++)
            {
                if (i != index)
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


            Invoke("setSwappedWeaponPositions", .01f);

            animator.SetBool("Unequip", false);
        }
        void setSwappedWeaponPositions()
        {
            var selectedWeapon = weapons[index];

            //Initialize the correct original aim position if it is the first time swapping

            selectedWeapon.PlayerController.BodyAnimator.SetFloat("MoveSetID", selectedWeapon.MoveSetID);
            selectedWeapon.PlayerController.ArmsAnimator.SetFloat("MoveSetID", selectedWeapon.MoveSetID);


            foreach (FPSWeapon weapon in weapons)
            {
                weapon.swapping = false;
            }
        }


    }
}
