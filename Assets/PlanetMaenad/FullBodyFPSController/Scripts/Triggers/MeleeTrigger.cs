﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetMaenad.FPS
{
    public class MeleeTrigger : MonoBehaviour
    {
        public bool IsPlayer;
        public Mirror_MasterPlayerController PlayerController;
        [Space(5)]
        public bool IsAi;
        public Mirror_AIController AIController;
        [Space(10)]


        public GameObject RootCharacter;
        public GameObject ForceForwardTransform;
        public Collider Trigger;
        [Space(10)]

        public string hitReaction = "Hit";
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(5)]
        public int Damage;
        public float hitforce;
        public float hitVolume = .35f;
        public AudioClip[] hitSounds;
        public AudioClip[] damageSounds;
        [Space(10)]

        public bool IsBigHit;
        public GameObject[] impactParticles;
        public GameObject[] impactBloodParticles;
        public float impactDespawnTime = 3f;



        internal WaitForSeconds DamageFrequency;
        internal bool CanApplyDamage = true;
        internal GameObject CurrentObject;


        void Start()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            if (IsPlayer && PlayerController && PlayerController.isLocalPlayer && !PlayerController.IsBlocking)
            {
                if ((HitLayers.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
                {
                    CurrentObject = other.gameObject;

                    //Check Tags
                    if (DamageTags.Length > 0)
                    {
                        for (int i = 0; i < DamageTags.Length; i++)
                        {
                            if (other.transform.CompareTag(DamageTags[i]))
                            {
                                if (other.GetComponentInParent<Mirror_HealthController>())
                                {
                                    var parentHealth = other.GetComponentInParent<Mirror_HealthController>();
                                    if (parentHealth.gameObject != PlayerController.gameObject)
                                    {
                                        PlayerController.CmdPassDamage(parentHealth.gameObject, transform.position, hitforce, Damage);

                                        if (damageSounds.Length > 0)
                                        {
                                            float random = Random.Range(0f, 1f);

                                            if (random <= 0.3f)
                                            {
                                                int randomIndex = Random.Range(0, damageSounds.Length);
                                                AudioSource.PlayClipAtPoint(damageSounds[randomIndex], transform.position, hitVolume);
                                            }
                                        }

                                        for (int b = 0; b < impactBloodParticles.Length; b++)
                                        {
                                            GameObject tempImpact;
                                            tempImpact = Instantiate(impactBloodParticles[b], transform.position, impactBloodParticles[b].transform.rotation) as GameObject;
                                            tempImpact.transform.Rotate(Vector3.left * 90);

                                            Destroy(tempImpact, impactDespawnTime);
                                        }
                                    }
                                }                              
                            }
                        }
                    }

                    //if (hitSounds.Length > 0)
                    //{
                    //    float random = Random.Range(0f, 1f);

                    //    if (random <= 0.3f)
                    //    {
                    //        int randomIndex = Random.Range(0, hitSounds.Length);
                    //        AudioSource.PlayClipAtPoint(hitSounds[randomIndex], transform.position, hitVolume);
                    //    }
                    //}


                    for (int i = 0; i < impactParticles.Length; i++)
                    {
                        GameObject tempImpact;
                        tempImpact = Instantiate(impactParticles[i], this.transform.position, this.transform.rotation) as GameObject;
                        tempImpact.transform.Rotate(Vector3.left * 90);

                        Destroy(tempImpact, impactDespawnTime);

                        if (other.GetComponent<Rigidbody>() && IsPlayer && ForceForwardTransform)
                        {
                            other.GetComponent<Rigidbody>().AddForce(ForceForwardTransform.transform.forward * 360 * hitforce);
                        }
                    }
                }
            } 
            
            if (IsAi && AIController && AIController.isServer)
            {
                CurrentObject = other.gameObject;

                //Check Tags
                if (DamageTags.Length > 0)
                {
                    for (int i = 0; i < DamageTags.Length; i++)
                    {
                        if (other.transform.CompareTag(DamageTags[i]))
                        {
                            if (other.GetComponentInParent<Mirror_HealthController>())
                            {
                                var parentHealth = other.GetComponentInParent<Mirror_HealthController>();
                                if (parentHealth.gameObject != RootCharacter.gameObject)
                                {
                                    parentHealth.Damage(transform.forward * 360, hitforce, Damage);

                                    if (damageSounds.Length > 0)
                                    {
                                        float random = Random.Range(0f, 1f);

                                        if (random <= 0.3f)
                                        {
                                            int randomIndex = Random.Range(0, damageSounds.Length);
                                            AudioSource.PlayClipAtPoint(damageSounds[randomIndex], transform.position, hitVolume);
                                        }
                                    }

                                    for (int b = 0; b < impactBloodParticles.Length; b++)
                                    {
                                        GameObject tempImpact;
                                        tempImpact = Instantiate(impactBloodParticles[b], transform.position, impactBloodParticles[b].transform.rotation) as GameObject;
                                        tempImpact.transform.Rotate(Vector3.left * 90);

                                        Destroy(tempImpact, impactDespawnTime);
                                    }
                                }                              
                            }                        
                        }
                    }
                }

                //if (hitSounds.Length > 0)
                //{
                //    float random = Random.Range(0f, 1f);

                //    if (random <= 0.3f)
                //    {
                //        int randomIndex = Random.Range(0, hitSounds.Length);
                //        AudioSource.PlayClipAtPoint(hitSounds[randomIndex], transform.position, hitVolume);
                //    }
                //}


                for (int i = 0; i < impactParticles.Length; i++)
                {
                    GameObject tempImpact;
                    tempImpact = Instantiate(impactParticles[i], this.transform.position, this.transform.rotation) as GameObject;
                    tempImpact.transform.Rotate(Vector3.left * 90);

                    Destroy(tempImpact, impactDespawnTime);

                    if (other.GetComponent<Rigidbody>() && IsPlayer && ForceForwardTransform)
                    {
                        other.GetComponent<Rigidbody>().AddForce(ForceForwardTransform.transform.forward * 360 * hitforce);
                    }
                }
            }
        }




        //public void PassEnemyDamage(GameObject other, Vector3 damagePoint)
        //{
        //    if (other != null)
        //    {
        //        if (other.GetComponentInParent<HealthController>())
        //        {
        //            var parentHealth = other.GetComponentInParent<HealthController>();

        //            if (parentHealth.gameObject != RootCharacter.gameObject)
        //            {
        //                parentHealth.Damage(transform.forward * 360, hitforce, Damage);


        //                //if (damageSounds.Length > 0)
        //                //{
        //                //    float random = Random.Range(0f, 1f);

        //                //    if (random <= 0.3f)
        //                //    {
        //                //        int randomIndex = Random.Range(0, damageSounds.Length);
        //                //        AudioSource.PlayClipAtPoint(damageSounds[randomIndex], transform.position, hitVolume);
        //                //    }
        //                //}
        //            }
        //        }
        //    }
        //}
    }
}