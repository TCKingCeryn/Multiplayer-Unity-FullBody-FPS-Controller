using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Mirror;

namespace PlanetMaenad.FPS
{
    public class Mirror_AIController : NetworkBehaviour
    {
        public bool IsServerInstance;
        [Space(10)]


        public Rigidbody _rigidbody;
        public NavMeshAgent agent;
        public Animator m_Animator;
        public Mirror_HealthController HealthControl;
        [Space(10)]



        public LayerMask groundLayers = -1;
        [Space(10)]



        public bool MoveWhileAttacking = true;
        public bool UseWander = true;
        public float wanderSpeed = 1.5f;
        public float wanderRadius = 5;
        public float wanderTimer = 10;
        [Space(5)]
        public float CurrentVelocity;
        [Space(10)]

        


        public bool targetInSightRange;
        public bool targetInAttackRange;
        [Space(10)]



        public Transform CurrentTarget;
        public Collider[] PotentialTargets;
        [Space(5)]
        public bool UseDetect = true;
        public Transform DetectEyes;
        public LayerMask DetectionLayers;
        public string[] DetectionTags;
        [Space(5)]
        public float DetectionFrequencyTimer = 1.5f;
        public float DetectionRadius = 15f;
        [Space(10)]




        public bool UseAttacks;
        public float AttackFrequencyTimer = 2f;
        public float AttackDistance = 2f;
        public RandomAttack[] _randomAttacks;
        [Space(5)]
        public bool IsAttacking;
        [Space(5)]
        public UnityEvent OnAttack;
        [Space(10)]


        public Vector3 AimOffset = new Vector3(0, .25f, 0);





        internal float m_ForwardAmount;
        internal float m_TurnAmount;
        internal float timer;

        internal bool moving;
        internal bool CanAttack = true;

        internal Vector3 lastPos;    

        internal WaitForSeconds DetectionFrequency;
        internal WaitForSeconds AttackFrequency;


        [System.Serializable]
        public class RandomAttack
        {
            public string AttackName;
            public float AttackDamage;
            public GameObject AttackHitBox;
            public float HitboxResetDelay = .5f;
        }



        void OnEnable()
        {
            //if (!UseWander && m_Animator) m_Animator.applyRootMotion = false;

            if (HealthControl) HealthControl.OnReceiveDamage.AddListener(AttackCancel);

            if (agent) agent.speed = wanderSpeed;

            wanderTimer = Random.Range(wanderTimer - 3, wanderTimer + 4);
            timer = wanderTimer;

            DetectionFrequencyTimer = Random.Range(DetectionFrequencyTimer - 0.25f, _randomAttacks.Length + 0.25f);
            DetectionFrequency = new WaitForSeconds(DetectionFrequencyTimer);
            AttackFrequency = new WaitForSeconds(AttackFrequencyTimer);

            IsServerInstance = isServer;

            StartCoroutine(CheckForTargets());
        }
        void Update()
        {
            if (agent)
            {
                if (agent.enabled)
                {
                    if (agent.isOnNavMesh)
                    {
                        if (CurrentTarget && targetInSightRange && !targetInAttackRange && MoveWhileAttacking)
                        {
                            ChaseTarget();
                        }
                        else
                        {
                            timer += Time.deltaTime;

                            if (timer >= wanderTimer)
                            {
                                if (!IsAttacking && !CurrentTarget)
                                {
                                    //Find New Destination
                                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, groundLayers);

                                    if (UseWander) agent.SetDestination(newPos);
                                }

                                timer = 0;
                            }
                        }

                        if (agent && agent.enabled && agent.isOnNavMesh) CurrentVelocity = agent.velocity.magnitude;

                        m_TurnAmount = transform.rotation.y;

                        if (transform.position != lastPos)
                        {
                            m_ForwardAmount = 1 * CurrentVelocity;
                            moving = true;
                        }
                        else
                        {
                            m_ForwardAmount = 0;
                            moving = false;
                        }

                        lastPos = transform.position;

                        UpdateAnimator();
                    }
                }
            }           
        }


        IEnumerator CheckForTargets()
        {
            while (enabled)
            {
                yield return DetectionFrequency;

                if (UseDetect)
                {
                    PotentialTargets = Physics.OverlapSphere(transform.position, DetectionRadius, DetectionLayers);

                    //Found Colliders
                    if (PotentialTargets.Length > 0)
                    {
                        //Check Each Tags
                        for (int i = 0; i < DetectionTags.Length; i++)
                        {
                            //Tag Match
                            if (PotentialTargets[0].transform.CompareTag(DetectionTags[i]))
                            {
                                CurrentTarget = PotentialTargets[0].transform;

                                Vector3 rayOrigin = DetectEyes.position;
                                RaycastHit hit;
                                var rayDirection = (CurrentTarget.position + AimOffset) - DetectEyes.position;

                                //Hits an Object
                                if (Physics.Raycast(rayOrigin, rayDirection, out hit, DetectionRadius + 1, DetectionLayers))
                                {
                                    var hitTransform = hit.collider.transform;

                                    //Can See Target
                                    if (hitTransform == CurrentTarget)
                                    {
                                        if (MoveWhileAttacking && agent.isOnNavMesh && CurrentTarget) agent.SetDestination(CurrentTarget.position);

                                        targetInSightRange = true;

                                        if (UseAttacks)
                                        {
                                            StartCoroutine(CheckAttackDistance());
                                        }
                                    }
                                }
                                else
                                {
                                    //No Target Found
                                    CurrentTarget = null;

                                    targetInSightRange = false;
                                    targetInAttackRange = false;

                                    if (UseAttacks)
                                    {
                                        StopCoroutine(CheckAttackDistance());
                                        AttackCancel();
                                    }
                                }
                            }
                        }
                    }

                    //No Target Found
                    if (PotentialTargets.Length == 0)
                    {
                        CurrentTarget = null;

                        targetInSightRange = false;
                        targetInAttackRange = false;

                        if (UseAttacks)
                        {
                            StopCoroutine(CheckAttackDistance());
                            AttackCancel();
                        }
                    }

                    if (CurrentTarget != null)
                    {
                        if (MoveWhileAttacking && agent.isOnNavMesh && CurrentTarget) agent.SetDestination(CurrentTarget.position);

                        targetInSightRange = true;

                        if (UseAttacks)
                        {
                            StartCoroutine(CheckAttackDistance());
                        }
                    }
                }                
            }
        }
        IEnumerator CheckAttackDistance()
        {
            while (enabled)
            {
                yield return AttackFrequency;

                if (UseDetect)
                {
                    //Has Target
                    if (CurrentTarget)
                    {
                        float dist = Vector3.Distance(CurrentTarget.position, transform.position);

                        if (dist < AttackDistance)
                        {
                            transform.LookAt(CurrentTarget, Vector3.up);

                            AttackTarget();

                            targetInAttackRange = true;
                        }
                        if (dist > AttackDistance)
                        {
                            ChaseTarget();

                            targetInAttackRange = false;
                        }
                    }
                }               
            }
        }

        void AttackTarget()
        {
            if (!MoveWhileAttacking) agent.SetDestination(transform.position);

            OnAttack.Invoke();

            if (CurrentTarget)
            {
                Vector3 targetPostitionXZ = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z);
                transform.LookAt(targetPostitionXZ);

                IsAttacking = true;

                //if (m_Animator && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("RandomAttack"));
            }
        }
        void AttackCancel()
        {
            IsAttacking = false;
        }
    
        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;

            randDirection += origin;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
        }

        public void ChaseTarget()
        {
            if (MoveWhileAttacking && agent.isOnNavMesh) agent.SetDestination(CurrentTarget.position);
        }
        public void UpdateAnimator()
        {
            //Update the blend trees
            if (m_Animator) m_Animator.SetBool("Attack", targetInAttackRange);
            
            if (m_Animator && (UseWander || MoveWhileAttacking)) m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            if (m_Animator && (UseWander || MoveWhileAttacking)) m_Animator.SetFloat("Turn", m_TurnAmount * 0.3f, 0.1f, Time.deltaTime);
        }

    }

}
