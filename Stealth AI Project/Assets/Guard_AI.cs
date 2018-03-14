using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard_AI : MonoBehaviour
{
    public enum Guard_State { Idle, Patrol, Suspicious, Chase, Attack };
    NavMeshAgent guardAgent;
    public Transform playerTarget;
    public Vector3 ChaseDest;
    public Transform GuardNPC;
    public GameObject PlayerCharacter;

    [Header("Patrol And Chase")]
    public List<PatrolPoint> patrol;
    int totalWaitTime;
    public bool isTravelling;
    int currentPatrolPoint;
    public bool waiting;
    float waitTimer;
    int maxWait = 9;
    int minWait = 3;
    public float patrolSpeed = 10;
    public float chaseSpeed = 35;
    


    [Space(5)]
    [Header("Line Of Sight & Hearing")]
    public bool canSee;
    public bool canHear;
    public bool canAttack;
    public bool canChase;
    [Range(0.0f, 360.0f)]
    public float FOVDist = 20;
    [Range(0.0f, 360.0f)]
    public float FOVAngle = 45;
    [Range(0f, 10f)]
    public float AttackDist = 5;
    [Range(0f, 75f)]
    public float MaxViewRange = 75;
    public float currentDist;

    public float detectCounter;
    public float maxDetectCount;


    [Space(5)]
    [Header("State Control")]
    [SerializeField]
    public Guard_State baseStates;
    public Animator anim;
    public Light Torch;



    public void Awake()
    {
        anim = GetComponent<Animator>();
        currentPatrolPoint = 0;
        guardAgent = this.GetComponent<NavMeshAgent>();
        baseStates = Guard_AI.Guard_State.Patrol;
        anim.SetBool("isPatrolling", true);
    }
    void Start()
    {
        SetDestination();
    }

    // Update is called once per frame
    void Update()
    {
        //currentDist = Vector3.Distance(playerTarget.position, transform.position);
        //Debug.Log("Distance to player: " + currentDist);
        PatrolRoute();
        StateChecker();
        DetectPlayer();
    }
    private void StateChecker()
    {
        if (baseStates == Guard_State.Idle)
        {
            anim.SetBool("isWaiting", true);
            anim.SetBool("isPatrolling", false);
            Torch.color = Color.white;
        }
        else if (baseStates == Guard_State.Patrol)
        {
            anim.SetBool("isPatrolling", true);
            anim.SetBool("isWaiting", false);
            anim.SetBool("isChasing", false);
            anim.SetBool("playerDetected", false);
            Torch.color = Color.cyan;
            guardAgent.speed = patrolSpeed;
        }
        else if (baseStates == Guard_State.Chase)
        {
            Torch.color = Color.red;
            guardAgent.speed = chaseSpeed;
            anim.SetBool("isChasing", true);
            anim.SetBool("playerDetected", true);
            anim.SetBool("isPatrolling", false);
            anim.SetBool("isWaiting", false);
        }
        else if (baseStates == Guard_State.Attack)
        {
            anim.SetBool("closeAttack", true);
            anim.SetBool("isChasing", false);
            anim.SetBool("isPatrolling", false);
            anim.SetBool("isWaiting", false);
            Torch.color = Color.red;
        }
    }
    void PatrolRoute()
    {
        if (canSee || canChase)
        {
            SetDestination();
        }      
        if (isTravelling && guardAgent.remainingDistance <= 0.5f)
        {
            isTravelling = false;
            waiting = true;
            waitTimer = 0;
            totalWaitTime = Random.Range(minWait, maxWait);
        }
        if (waiting)
        {
            baseStates = Guard_State.Idle;
            waitTimer += Time.deltaTime;
            if (waitTimer >= totalWaitTime)
            {
                baseStates = Guard_State.Patrol;
                waiting = false;
                currentPatrolPoint = (currentPatrolPoint + 1) % patrol.Count;
                SetDestination();
            }
        }
    }
    // setting the next wayppoint destination 
    private void SetDestination()
    {
        if (canSee || canChase)
        {
            guardAgent.destination = playerTarget.position;
            baseStates = Guard_State.Chase;
        }
        if(!canSee)
        {
            Vector3 targetPoint = patrol[currentPatrolPoint].transform.position;
            guardAgent.SetDestination(targetPoint);
            isTravelling = true;          
        }
    }

    //takes the players position and the npc position, check is the player is wintin 30 of the enemy or 60 angle
    void DetectPlayer()
    {
        Vector3 direction = playerTarget.position - this.transform.position;
        direction.y = 0;

        float FOVAngle = Vector3.Angle(direction, GuardNPC.forward);

        if (Vector3.Distance(playerTarget.position, this.transform.position) < FOVDist && FOVAngle < 75)
        {
            isTravelling = false;
            canSee = true;
            detectCounter += Time.deltaTime;
            canChase = true;

            if (detectCounter >= maxDetectCount)
            {
                canChase = true;
                isTravelling = false;
                baseStates = Guard_State.Chase;
                detectCounter = 0;
                guardAgent.destination = playerTarget.position;
            }
            else if(Vector3.Distance(playerTarget.position, this.transform.position) < 20 && FOVAngle < 20)
            {
                detectCounter = 0;
                canChase = true;
                isTravelling = false;
                baseStates = Guard_State.Chase;
            }
            if (!canSee)
            {
                detectCounter = 0;
            }
           
        }
        if(Vector3.Distance(playerTarget.position, this.transform.position) > MaxViewRange && !isTravelling && !waiting)
        {         
            canSee = false;
            canChase = false;
            isTravelling = true;
            baseStates = Guard_State.Patrol;
            PatrolRoute();
        }

        if (Vector3.Distance(playerTarget.position, this.transform.position) <= AttackDist)
        {
            Attack();          
        }
        else
        {
            anim.SetBool("closeAttack", false);
        }
    }

    void Attack()
    {
        canAttack = true;
        if (canAttack)
        {
            canChase = false;
            baseStates = Guard_State.Attack;
        }
    }
}





