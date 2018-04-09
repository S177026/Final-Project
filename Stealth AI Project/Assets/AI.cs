using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour {

    public enum Guard_State { Patrol, Suspicious, Chase, Attack };

    [Header("Patrol And Chase")]
    public List<PatrolPoint> patrol;
    public int totalWaitTime;
    public bool isTravelling;
    public int currentPatrolPoint;
    public bool waiting;
    public float waitTimer;
    public int maxWait = 9;
    public int minWait = 3;
    public float patrolSpeed = 10f;
    public float chaseSpeed = 25f;

    [Space(5)]
    [Header("Line Of Sight & Hearing")]
    [Range(0f, 360f)]
    public int FOV = 45;
    [Range(0f, 360f)]
    public int viewDistance = 75;
    [Range(0f, 360f)]
    public int attackDist = 5;
    public bool ClearLineOfSight;
    public bool canHear;
    public bool canAttack;
    public bool isSuspicious;
    public Transform GuardTransform;
    private Transform playerTransform;
    private Vector3 RayDir;
    public Vector3 LastPos;

    public NavMeshAgent GuardNav;
    public Animator anim;

    [SerializeField]
    private Guard_State CStates = Guard_State.Patrol;

    public Guard_State CurrentState
    {
        get { return CStates; }
        set
        {
            CStates = value;

            StopAllCoroutines();

            switch(CStates)
             {
                case Guard_State.Patrol:
                    StartCoroutine(GuardPatrol());
                    break;

                case Guard_State.Chase:
                    StartCoroutine(GuardChase());
                    break;

                case Guard_State.Attack:
                    StartCoroutine(GuardAttack());
                    break;

                case Guard_State.Suspicious:
                    StartCoroutine(GuardSus());
                    break;
            }
        }
    }

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        GuardTransform = GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        CurrentState = Guard_State.Patrol;
        currentPatrolPoint = 0;
    }

    // Update is called once per frame
    void Update ()
    {
        DetectView();
	}

    void DetectView()
    {
        RaycastHit hitPlayer;
        RayDir = playerTransform.position - transform.position;
        if ((Vector3.Angle(RayDir, transform.forward)) < FOV)
        {
            if (Physics.Raycast(transform.position, RayDir, out hitPlayer, viewDistance))
            {
                if (hitPlayer.transform.CompareTag("Player"))
                {
                    ClearLineOfSight = true;
                    print("Player Found");
                    if (ClearLineOfSight)
                    {
                        LastPos = playerTransform.position;
                    }
                }
            }
        }
        else
            ClearLineOfSight = false;
        }
    private void OnDrawGizmos()
    {
        Vector3 midRay = transform.position + (transform.forward * viewDistance);

        Vector3 leftRay = midRay;
        leftRay.x += FOV * 0.5f;

        Vector3 rightRay = midRay;
        rightRay.x -= FOV * 0.5f;

        Debug.DrawLine(transform.position, midRay, Color.red);
        Debug.DrawLine(transform.position, leftRay, Color.red);
        Debug.DrawLine(transform.position, rightRay, Color.red);
    }

    public IEnumerator GuardPatrol()
    {
        while (CStates == Guard_State.Patrol)
        {
            GuardNav.speed = patrolSpeed;
            isTravelling = true;
            waiting = false;
            isSuspicious = false;
            GuardNav.isStopped = false;
            anim.SetBool("isWaiting", false);
            anim.SetBool("isChasing", false);
            anim.SetBool("isPatrolling", true);

            Vector3 WaypointTarget = patrol[currentPatrolPoint].transform.position;
            GuardNav.SetDestination(WaypointTarget);

            while (GuardNav.pathPending)
                yield return null;
         
            if (isTravelling && GuardNav.remainingDistance <= 0.5f)
            {
                isTravelling = false;
                waiting = true;
            }

            if (waiting)
            {
                anim.SetBool("isPatrolling", false);
                anim.SetBool("isWaiting", true);
                waitTimer += Time.deltaTime;
                GuardNav.isStopped = true;
                if (waitTimer >= totalWaitTime)
                {
                    GuardNav.isStopped = false;
                    waiting = false;
                    currentPatrolPoint = (currentPatrolPoint + 1) % patrol.Count;
                    GuardNav.SetDestination(WaypointTarget);
                }            
            }
            else if (!waiting)
            {
                waitTimer = 0;
                totalWaitTime = Random.Range(minWait, maxWait);
            }

            if (ClearLineOfSight)
            {
                CurrentState = Guard_State.Chase;
                yield break;
            }
            yield return null;
        }
    }

    public IEnumerator GuardChase()
    {
        while (CStates == Guard_State.Chase)
        {
            GuardNav.speed = chaseSpeed;
            canAttack = false;
            isTravelling = false;
            anim.SetBool("isWaiting", false);
            anim.SetBool("isPatrolling", false);
            anim.SetBool("isChasing", true);

            GuardNav.SetDestination(playerTransform.position);

            if (GuardNav.remainingDistance <= GuardNav.stoppingDistance)
            {
                GuardNav.isStopped = true;

                if (!ClearLineOfSight)               
                    CurrentState = Guard_State.Suspicious;
                else
                    CurrentState = Guard_State.Attack;
                yield break;
            }          
            yield return null;
        }
    }

    public IEnumerator GuardSus()
    {

        while(CStates == Guard_State.Suspicious)
        {
            isSuspicious = true;
            GuardNav.SetDestination(LastPos);
            anim.SetBool("isChasing", false);
            anim.SetBool("isWaiting", true);
            yield return new WaitForSeconds(5);

            if (!ClearLineOfSight)
            {
                CurrentState = Guard_State.Patrol;
            }
            else
            {
                CurrentState = Guard_State.Chase;
                yield break;
            }
            yield return null;
        }
    }

    public IEnumerator GuardAttack()
    {
        while(CStates == Guard_State.Attack)
        {
            GuardNav.isStopped = false;
            GuardNav.SetDestination(playerTransform.position);

            if(GuardNav.remainingDistance > GuardNav.stoppingDistance)
            {
                CurrentState = Guard_State.Chase;
                yield break;
            }
            else
            {
                canAttack = true;
                //ATTACK!
            }
            yield break;
        }
        yield return null;
    }
}
