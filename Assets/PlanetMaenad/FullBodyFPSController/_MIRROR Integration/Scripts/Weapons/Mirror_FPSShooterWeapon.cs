using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlanetMaenad.FPS
{

    public class Mirror_FPSShooterWeapon : Mirror_FPSWeapon
    {
        public ShootTypes shootType;
        public bool firing;
        public bool reloading;
        [Space(10)]


        //public GameObject DamageCrosshair;
        //[Space(10)]



        public bool shootFromCamera;
        [Space(5)]
        public GameObject shootPoint;
        public string hitReaction = "Hit";
        public LayerMask HitLayers = -1;
        public string[] DamageTags;
        [Space(5)]
        public bool UseRigidbodyBullet;
        public GameObject Bullet;
        public float bulletVelocity;
        public float bulletDespawnTime;
        [Space(10)]

        public float hitVolume = .35f;
        public float hitForce = 5f;
        public AudioClip[] hitSounds;
        [Space(10)]


        public int bulletsPerMag;
        public int bulletsInMag;
        public TextMeshPro CurrentAmmoTextMesh;
        public TextMeshPro MaxAmmoTextMesh;
        public int totalBullets;
        [Space(5)]
        public float reloadTime = 0.2f;
        public float grenadeTime;
        public float fireRate = 0.1f;
        [Space(10)]


        public bool UseRecoil;
        public float recoilAmount = .2f;
        public float recoilDuration = .5f;
        public float returnSpeed = .5f;
        [Space(10)]


        public float ShootVolume = .35f;
        public AudioClip fireSound;
        public float ReloadVolume = 0.35f;
        public AudioClip reloadSound;
        [Space(10)]


        public ParticleSystem[] muzzleFlashes;
        public GameObject ejectionPoint;
        public GameObject magDropPoint;
        public GameObject Shell;
        public GameObject Mag;
        [Space(10)]


        public float shellVelocity;
        public float magVelocity;
        public float shellDespawnTime;
        public float magDespawnTime;
        public float cycleTimeBoltAction;
        public float cycleTimeSemiAuto;
        [Space(10)]



        public bool AutoDestroyImpacts;
        public GameObject[] impactParticles;
        public GameObject[] impactBloodParticles;
        public float impactDespawnTime = 3f;
        [Space(10)]




        internal Vector3 originalPosition;
        internal Quaternion originalRotation;
        internal bool isRecoiling;
        internal float recoilTimer;
        internal bool recoilAuto = false;
        internal bool recoilSemi = false;
        internal bool throwing = false;
        internal bool cycling = false;



        Coroutine lastRoutine = null;

        public enum ShootTypes { SemiAuto, FullAuto, BoltAction };


        void OnEnable()
        {
            //Reset adjuster to sync up every time gun is loaded
            if (WeaponAdjust) WeaponAdjust.enabled = false;
            if (WeaponAdjust) WeaponAdjust.enabled = true;

            if (UseFPSArms)
            {
                if (PlayerController) PlayerController.CmdSetLockFullbodyArms(true);
            }
            if (!UseFPSArms)
            {
                if (PlayerController) PlayerController.CmdSetLockFullbodyArms(false);
            }

            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
        void Start()
        {
            if (PlayerController.isLocalPlayer)
            {
                originalCamPos = new Vector3(0, .35f, 0.15f);

                //Set the ammo count
                bulletsInMag = bulletsPerMag;
            }           
        }
        void Update()
        {
            if (PlayerController.isLocalPlayer)
            {
                //Shoot
                if (Input.GetKeyDown(AttackButton) && !PlayerController.IsBlocking && !firing && !reloading && bulletsInMag > 0 && !cycling && !swapping && !Cursor.visible)
                {
                 
                    firing = true;

                    foreach (ParticleSystem ps in muzzleFlashes)
                    {
                        ps.Play();
                    }

                    //Shoot Bullet
                    ShootBullet();
                    gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound, ShootVolume);

                    bulletsInMag--;

                    if (shootType == ShootTypes.FullAuto)
                    {
                        SpawnShell();

                        recoilAuto = true;
                        recoilSemi = false;
                        lastRoutine = StartCoroutine(ShootBulletDelay());
                    }
                    else if (shootType == ShootTypes.SemiAuto)
                    {
                        SpawnShell();

                        recoilAuto = false;
                        recoilSemi = true;

                        if (Weapon == WeaponTypes.Rifle)
                        {
                            Invoke("fireCancel", .25f);
                        }

                        Invoke("cycleFire", cycleTimeSemiAuto);

                        cycling = true;
                    }
                    else if (shootType == ShootTypes.BoltAction)
                    {
                        recoilAuto = false;
                        recoilSemi = true;

                        if (Weapon == WeaponTypes.Rifle)
                        {
                            Invoke("fireCancel", .25f);
                            Invoke("cycleFire", cycleTimeBoltAction);
                            Invoke("ejectShellBoltAction", cycleTimeBoltAction / 2);
                            cycling = true;

                            //gameObject.GetComponent<Animator>().SetBool("cycle", true);
                        }
                    }
                }
                else if (firing && (Input.GetKeyUp(AttackButton) || bulletsInMag == 0))
                {
                

                    firing = false;
                    recoilSemi = false;
                    recoilAuto = false;

                    if (shootType == ShootTypes.FullAuto)
                    {
                        StopCoroutine(lastRoutine);
                    }
                }

                //Reload
                if (Input.GetKeyDown(KeyCode.R) && !PlayerController.IsBlocking && !firing && !reloading && bulletsInMag < bulletsPerMag && totalBullets > 0 && !Cursor.visible)
                {
                    PlayerController.CmdCrossFadeArmsAnimation("Reload", 0.1f);
                    PlayerController.BodyAnimator.SetBool("Reload", true);

                    reloading = true;
                    gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound, ReloadVolume);
                    Invoke("reloadFinished", reloadTime);

                    SpawnMag();
                }

                //UI
                if (CurrentAmmoTextMesh) CurrentAmmoTextMesh.text = bulletsInMag.ToString();
                if (MaxAmmoTextMesh) MaxAmmoTextMesh.text = totalBullets.ToString();

                //Animators
                PlayerController.ArmsAnimator.SetBool("Shoot", firing);
            }          
        }


        IEnumerator ShootBulletDelay()
        {
            while (true)
            {
                yield return new WaitForSeconds(fireRate);
                if (bulletsInMag > 0)
                {
                    gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound, ShootVolume);
                    foreach (ParticleSystem ps in muzzleFlashes)
                    {
                        ps.Play();
                    }

                    ShootBullet();
                    SpawnShell();
                    bulletsInMag--;
                }
            }
        }
        void ShootBullet()
        {
            if(PlayerController.isLocalPlayer)
            {
                PlayerController.CmdSetShooterVector(PlayerController.MainCam.transform.position, PlayerController.MainCam.transform.forward, PlayerController.MainCam.transform.up, PlayerController.MainCam.transform.right);

                RaycastHit hit;
                var rayDirection = shootFromCamera ? PlayerController.MainCam.transform.forward : shootPoint.transform.forward;

                //Hits an Object
                if (Physics.Raycast(shootFromCamera ? PlayerController.MainCam.transform.position : shootPoint.transform.position, rayDirection, out hit, 1000 + 1, HitLayers))
                {
                    var hitTransform = hit.collider.transform;
                    //Debug.Log("Hit Object: " + hitTransform.gameObject.name);

                    for (int i = 0; i < DamageTags.Length; i++)
                    {
                        //Can See Target
                        if (hitTransform.CompareTag(DamageTags[i]))
                        {
                            if (hitTransform.gameObject.GetComponentInParent<Mirror_HealthController>())
                            {
                                var parentHealth = hitTransform.gameObject.GetComponentInParent<Mirror_HealthController>();
                                if (parentHealth.gameObject != PlayerController.gameObject)
                                {
                                    if (parentHealth.gameObject != null) PlayerController.CmdPassDamage(parentHealth.gameObject, hit.point, hitForce, Damage);

                                    if (hitSounds.Length > 0)
                                    {
                                        int randomIndex = Random.Range(0, hitSounds.Length);
                                        AudioSource.PlayClipAtPoint(hitSounds[randomIndex], hit.point, hitVolume);

                                        float random = Random.Range(0f, 1f);
                                        if (random <= 0.3f)
                                        {

                                        }
                                    }

                                    for (int b = 0; b < impactBloodParticles.Length; b++)
                                    {
                                        GameObject tempImpact;
                                        tempImpact = Instantiate(impactBloodParticles[b], hit.point, impactBloodParticles[b].transform.rotation) as GameObject;
                                        tempImpact.transform.Rotate(Vector3.left * 90);

                                        Destroy(tempImpact, impactDespawnTime);
                                    }

                                }
                            }                            
                        }
                    }                  

                    for (int i = 0; i < impactParticles.Length; i++)
                    {
                        GameObject tempImpact;
                        tempImpact = Instantiate(impactParticles[i], hit.point, impactParticles[i].transform.rotation) as GameObject;
                        tempImpact.transform.Rotate(Vector3.left * 90);

                        Destroy(tempImpact, impactDespawnTime);

                        if (hitTransform.GetComponent<Rigidbody>())
                        {
                            hitTransform.GetComponent<Rigidbody>().AddForce(PlayerController.MainCam.transform.forward * 1 * hitForce);
                        }
                    }

                }

            }

                  
        }
        void SpawnShell()
        {
            //Spawn bullet
            GameObject tempShell;
            tempShell = Instantiate(Shell, ejectionPoint.transform.position, ejectionPoint.transform.rotation) as GameObject;

            //Orient it
            tempShell.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where ejection point is pointing (blue axis)
            Rigidbody tempRigidBody;
            tempRigidBody = tempShell.GetComponent<Rigidbody>();
            tempRigidBody.AddForce(ejectionPoint.transform.forward * shellVelocity);

            //Destroy after time
            Destroy(tempShell, shellDespawnTime);
        }
        void SpawnMag()
        {
            //Spawn bullet
            GameObject tempMag;
            tempMag = Instantiate(Mag, magDropPoint.transform.position, magDropPoint.transform.rotation) as GameObject;

            //Orient it
            tempMag.transform.Rotate(Vector3.left * 90);

            //Add forward force based on where ejection point is pointing (blue axis)
            Rigidbody tempRigidBody;
            tempRigidBody = tempMag.GetComponent<Rigidbody>();
            tempRigidBody.AddForce(magDropPoint.transform.forward * magVelocity);

            //Destroy after time
            Destroy(tempMag, magDespawnTime);
        }


        //public void PassDamage(GameObject other, Vector3 damagePoint)
        //{
        //    if (other != null && other.GetComponentInParent<HealthController>())
        //    {
        //        var parentHealth = other.GetComponentInParent<HealthController>();
        //        parentHealth.Damage(transform.forward * 360, hitForce, Damage);                         
        //    }
        //}


        void cycleFire()
        {
            cycling = false;

            if (shootType == ShootTypes.BoltAction)
            {
                //gameObject.GetComponent<Animator>().SetBool("cycle", false);
            }
        }
        void ejectShellBoltAction()
        {
            SpawnShell();
        }
        void fireCancel()
        {
            firing = false;
        }
        void reloadFinished()
        {
            reloading = false;

            PlayerController.BodyAnimator.SetBool("Reload", false);

            int bulletsToRemove = (bulletsPerMag - bulletsInMag);
            if (totalBullets >= bulletsPerMag)
            {
                bulletsInMag = bulletsPerMag;
                totalBullets -= bulletsToRemove;
            }
            else if (bulletsToRemove <= totalBullets)
            {
                bulletsInMag += bulletsToRemove;
                totalBullets -= bulletsToRemove;
            }
            else
            {
                bulletsInMag += totalBullets;
                totalBullets -= totalBullets;
            }
        }

      
    }
}
