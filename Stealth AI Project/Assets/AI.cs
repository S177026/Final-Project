using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public float detectionTimer;
    public bool beingDetected;
    public int totalDetectionTime;
    public int maxWait = 9;
    public int minWait = 3;
    public float patrolSpeed = 10f;
    public float chaseSpeed = 30f;
    public Light detectionLight;

    [Space(5)]
    [Header("Line Of Sight & Hearing")]
    [Range(0f, 360f)]
    public int FOV = 45;
    [Range(0f, 360f)]
    public int viewDistance = 75;
    [Range(0f, 360f)]
    public int shortFOV = 15;
    [Range(0f, 360f)]
    public int shortViewDistance = 15;
    [Range(0f, 360f)]
    public int longFOV = 20;
    [Range(0f, 360f)]
    public int longViewDistance = 60;
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

    public Player playerRef;

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

    void OnGUI()
    {
        GUIStyle stateCheck = new GUIStyle();
        stateCheck.fontSize = 25;
        GUI.Label(new Rect(1000, 0, Screen.width, Screen.height), "Guard State: " + CStates, stateCheck);
    }

    void DetectView()
    {
        // shoot out ray from the guards postion, checks if the ray has hit the player tag, if yes chase else not detected.
        RaycastHit hitPlayer;
        RayDir = playerTransform.position - transform.position;
        if ((Vector3.Angle(RayDir, transform.forward)) < FOV)
        {
            if (Physics.Raycast(transform.position, RayDir, out hitPlayer, viewDistance))
            {
                if (hitPlayer.transform.CompareTag("Player"))
                {
                    ClearLineOfSight = true;
                    //print("Player Found");
                    if (ClearLineOfSight)
                    {
                        LastPos = playerTransform.position;
                    }
                }
            }
        }
        //does the same as the above ray but shoots out backwards so player cant go behind and touch the guard without being detected.
        if ((Vector3.Angle(RayDir, -transform.forward)) < shortFOV)
        {
            if (Physics.Raycast(transform.position, RayDir, out hitPlayer, shortViewDistance))
            {
                if (hitPlayer.transform.CompareTag("Player"))
                {
                    ClearLineOfSight = true;
                    //print("Player Found");
                    if (ClearLineOfSight)
                    {
                        LastPos = playerTransform.position;
                    }
                }

            }
        }
        //ray that shoots out a far distance, check how long the player has been in the ray then compares that to the detection timer.
        if ((Vector3.Angle(RayDir, transform.forward)) < longFOV)
        {
            if (Physics.Raycast(transform.position, RayDir, out hitPlayer, longViewDistance))
            {
                if (hitPlayer.transform.CompareTag("Player"))
                {
                    beingDetected = true;
                }
                if (beingDetected)
                {
                    detectionTimer += Time.deltaTime;
                    GuardNav.isStopped = true;
                }
                if (detectionTimer >= totalDetectionTime)
                {
                    //if timer is greater than or equal to the total timer change to chase.
                    ClearLineOfSight = true;
                }
                if (ClearLineOfSight)
                {
                    LastPos = playerTransform.position;
                }
                if (!beingDetected)
                {
                    // resets the detection timer to 00 if not being detected
                    detectionTimer = 0;
                }
            }
        }
        else
        {
            beingDetected = false;
            ClearLineOfSight = false;
            detectionTimer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        //visualy drawing the viewcones in the inspector.
        Vector3 midRay = transform.position + (transform.forward * viewDistance);

        Vector3 leftRay = midRay;
        leftRay.x += FOV * 0.5f;

        Vector3 rightRay = midRay;
        rightRay.x -= FOV * 0.5f;

        Debug.DrawLine(transform.position, midRay, Color.red);
        Debug.DrawLine(transform.position, leftRay, Color.red);
        Debug.DrawLine(transform.position, rightRay, Color.red);

        Vector3 behindMidRay = transform.position + (-transform.forward * shortViewDistance);

        Vector3 behindLeftRay = behindMidRay;
        behindLeftRay.x += shortFOV * 0.5f;

        Vector3 behindRightRay = behindMidRay;
        behindRightRay.x -= shortFOV * 0.5f;

        Debug.DrawLine(transform.position, behindMidRay, Color.green);
        Debug.DrawLine(transform.position, behindLeftRay, Color.green);
        Debug.DrawLine(transform.position, behindRightRay, Color.green);

        Vector3 longdMidRay = transform.position + (transform.forward * longViewDistance);

        Vector3 longLeftRay = longdMidRay;
        longLeftRay.x += longFOV * 0.5f;

        Vector3 longRightRay = longdMidRay;
        longRightRay.x -= longFOV * 0.5f;

        Debug.DrawLine(transform.position, longdMidRay, Color.blue);
        Debug.DrawLine(transform.position, longLeftRay, Color.blue);
        Debug.DrawLine(transform.position, longRightRay, Color.blue);
    }

    //simply ontrigger if the player is making too much noise the collider radius will increase causing ontrigger enter.
    public void OnTriggerEnter(Collider Sound)
    {
        canHear = true;
        CurrentState = Guard_State.Chase;
    }

    public void OnTriggerExit(Collider Sound)
    {
        canHear = false;
    }


    public IEnumerator GuardPatrol()
    {
        while (CStates == Guard_State.Patrol)
        {
            detectionLight.color = Color.yellow;
            GuardNav.speed = patrolSpeed;
            isTravelling = true;
            waiting = false;
            isSuspicious = false;
            GuardNav.isStopped = false;
            anim.SetBool("isWaiting", false);
            anim.SetBool("isChasing", false);
            anim.SetBool("isPatrolling", true);

            //Sets target to the next waypoint in the list
            Vector3 WaypointTarget = patrol[currentPatrolPoint].transform.position;
            GuardNav.SetDestination(WaypointTarget);

            //path is null is it wasn't completed yet, fixes the waypoint skip problem
            while (GuardNav.pathPending)
                yield return null;
            //is close enought to the target waypoint start waiting
            if (isTravelling && GuardNav.remainingDistance <= 0.5f)
            {
                isTravelling = false;
                waiting = true;
            }
            //waits at waypoint for random ammount of seconds
            if (waiting)
            {
                anim.SetBool("isPatrolling", false);
                anim.SetBool("isWaiting", true);
                waitTimer += Time.deltaTime;
                GuardNav.isStopped = true;
                if (waitTimer >= totalWaitTime)
                {
                    //when the timer reaches the max timer take current target then add 1, sets next waypoint as destination
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
                //if guard see's player chnage to chase state
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

            detectionLight.color = Color.red;
            GuardNav.speed = chaseSpeed;
            GuardNav.isStopped = false;
            beingDetected = false;
            waiting = false;
            canAttack = false;
            isTravelling = false;
            isSuspicious = false;
            anim.SetBool("isChasing", true);
            anim.SetBool("isWaiting", false);
            anim.SetBool("isPatrolling", false);
            anim.SetBool("closeAttack", false);


            //sets to dest to lastknownpos
            GuardNav.SetDestination(LastPos);

            while (GuardNav.pathPending)
                yield return null;

            if (!ClearLineOfSight)
            {
                CurrentState = Guard_State.Suspicious;
            }

            //if the guard is close enought to attack stop the nav agent moving then switch to attack, if lost sight go to suspicious
            if (GuardNav.remainingDistance <= 10f)
            {
                GuardNav.isStopped = true;
                {
                    CurrentState = Guard_State.Attack;
                    yield break;
                }
            }
            yield return null;
        }
    }

    public IEnumerator GuardSus()
    {
        while(CStates == Guard_State.Suspicious)
        {
            //walks to lastkknown pos, then stops and waits for 5 seconds, if player detected during the 5 seconds back to chase else return to patrol.
            detectionLight.color = Color.blue;
            canAttack = false;
            isTravelling = true;
            isSuspicious = true;
            anim.SetBool("closeAttack", false);
            GuardNav.SetDestination(LastPos);

            while (GuardNav.pathPending)
                yield return null;

            if (isTravelling && GuardNav.remainingDistance <= 0.5f)
            {              
                waiting = true;
                isTravelling = false;
            }
            if (waiting)
            {
                anim.SetBool("isWaiting", true);
                anim.SetBool("isChasing", false);
                waitTimer += Time.deltaTime;
                GuardNav.isStopped = true;
                if (waitTimer >= totalWaitTime)
                {
                    CurrentState = Guard_State.Patrol;
                }
            }
            else if (!waiting)
            {
                waitTimer = 0;
                totalWaitTime = 5; 
            }
            if (ClearLineOfSight)
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
            //whilst close enough attack the player, if still has lineofsight but too far to attack back to chase, If lineofsight lost go to suspicious.
            detectionLight.color = Color.black;
            GuardNav.SetDestination(LastPos);

            while (GuardNav.pathPending)
                yield return null;

            if(GuardNav.remainingDistance < 10f)
            {
                GuardNav.isStopped = true;
                canAttack = true;
                anim.SetBool("closeAttack", true);
            }
            if(GuardNav.remainingDistance > 10f)
            {
                CurrentState = Guard_State.Chase;
            }
            else if(!ClearLineOfSight)
            {
                CurrentState = Guard_State.Suspicious;
                yield break;
            }
            yield return null;
        }
    }
}
