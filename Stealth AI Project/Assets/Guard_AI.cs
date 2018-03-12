using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard_AI : MonoBehaviour
{
    public enum Guard_State { Idle, Patrol, Chase, Attack };
    NavMeshAgent guardAgent;
    public Transform playerTarget;
    public Transform NPC;
    Vector3 CurrentNPC;
    public GameObject PlayerCharacter;
    public SphereCollider collider;

    [Header("Patrol And Chase")]
    public List<PatrolPoint> patrol;
    int totalWaitTime;
    bool isTravelling;
    int currentPatrolPoint;
    bool waiting;
    float waitTimer;
    int maxWait = 9;
    int minWait = 3;
    public float patrolSpeed = 8;
    public float chaseSpeed = 15;
    private Vector3 lastPos;
    private Ray sRay;
    private RaycastHit hitInfo;
    private Vector3 rayDirec;
    float maxDistCheck = 10.0f;
    Vector3 currentDist;
    Vector3 direcCheck;

    [Space(5)]
    [Header("Line Of Sight & Hearing")]
    public bool canSee;
    public bool canHear;
    public float FOVDist = 20;
    public float FOVAngle = 45;

    [Space(5)]
    [Header("State Control")]
    [SerializeField]
    public Guard_State baseStates;
    public Animator anim;
    public Light Torch;
    bool isAlive = true;


    public void Awake()
    {
        anim = GetComponent<Animator>();
        currentPatrolPoint = 0;
        guardAgent = this.GetComponent<NavMeshAgent>();
        baseStates = Guard_AI.Guard_State.Patrol;


    }
    void Start()
    {
        SetDestination();
    }

    private void FixedUpdate()
    {


    }

    // Update is called once per frame
    void Update()
    {
        PatrolRoute();
        StateChecker();
        DetectPlayer();
    }
    private void StateChecker()
    {
        if (baseStates == Guard_State.Idle)
        {
            Torch.color = Color.white;
        }
        else if (baseStates == Guard_State.Patrol)
        {
            Torch.color = Color.cyan;
            guardAgent.speed = patrolSpeed;
        }
        else if (baseStates == Guard_State.Chase)
        {
            Torch.color = Color.red;
            guardAgent.speed = chaseSpeed;
        }
        else if (baseStates == Guard_State.Attack)
        {
            Torch.color = Color.red;
        }
    }
    void PatrolRoute()
    {
        if (isTravelling && guardAgent.remainingDistance <= 0.5f)
        {
            isTravelling = false;
            waiting = true;
            waitTimer = 0;
            totalWaitTime = Random.Range(minWait, maxWait);
        }
        if (waiting)
        {
            anim.SetBool("isWaiting", true);
            anim.SetBool("isPatrolling", false);
            waitTimer += Time.deltaTime;
            if (waitTimer >= totalWaitTime)
            {
                anim.SetBool("isPatrolling", true);
                anim.SetBool("isWaiting", false);
                waiting = false;
                currentPatrolPoint = (currentPatrolPoint + 1) % patrol.Count;
                SetDestination();
            }
        }
    }

    // setting the next wayppoint destination 
    private void SetDestination()
    {
        Vector3 targetPoint = patrol[currentPatrolPoint].transform.position;
        guardAgent.SetDestination(targetPoint);
        isTravelling = true;
    }

    //takes the players position and the npc position, check is the player is wintin 30 of the enemy or 60 angle
    void DetectPlayer()
    {
        Vector3 direction = playerTarget.position - this.transform.position;
        direction.y = 0;

        float angle = Vector3.Angle(direction, NPC.forward);

        if (Vector3.Distance(playerTarget.position, this.transform.position) < 40 && angle < 75)
        {
            canSee = true;
        }
        else
        {
            canSee = false;
        }
    }
        void ChasePlayer()
    {
        if (canSee)
        {


        }


    }

}





