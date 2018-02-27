using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guard_AI : MonoBehaviour
{
    public enum Guard_State { Idle, Patrol, Suspicious, Chase, Attack };
    NavMeshAgent guardAgent;
    public Transform playerTarget;
    public Transform npcHead;

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

    [Space(5)]
    [Header("Line Of Sight & Hearing")]
    public bool canSee;
    public bool canHear;
    public float sightDist = 10;
    float viewAngle;
    public float timeToSpot = 1f;
    float playerVisTimer;
    


    [Space(5)]
    [Header("State Control")]
    [SerializeField]public Guard_State baseStates;
    public Animator anim;
    public Light Torch;
    bool isAlive = true;


    void Start()
    {
        guardAgent = this.GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentPatrolPoint = 0;
        SetDestination();
        baseStates = Guard_AI.Guard_State.Patrol;
    }

    // Update is called once per frame
    void Update()
    {
        PatrolRoute();
        StateChecker();

        if (isTravelling)
        {
            baseStates = Guard_State.Patrol;
        }
        else if (waiting)
        {
            baseStates = Guard_State.Idle; 
        }
        else if (canSee || canHear)
        {
            baseStates = Guard_State.Chase;
        }     
    }
    private void StateChecker()
    {
        if(baseStates == Guard_State.Idle)
        {
            anim.SetBool("Walking", false);
            anim.SetBool("Idle", true);
            Torch.color = Color.white;         
        }
        else if (baseStates == Guard_State.Patrol)
        {
            anim.SetBool("Walking", true);
            anim.SetBool("Idle", false);
            Torch.color = Color.cyan;
            guardAgent.speed = patrolSpeed;
        }
        else if (baseStates == Guard_State.Chase)
        {
            anim.SetBool("Running", true);
            anim.SetBool("Walking", false);
            anim.SetBool("Idle", false);
            Torch.color = Color.red;
            guardAgent.speed = chaseSpeed; 
        }
        else if (baseStates == Guard_State.Attack)
        {
            anim.SetBool("Attack", true);
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
            waitTimer += Time.deltaTime;
            if (waitTimer >= totalWaitTime)
            {                
                waiting = false;
                currentPatrolPoint = (currentPatrolPoint + 1) % patrol.Count;
                SetDestination();
            }
        }
    }
    private void SetDestination()
    {
        Vector3 targetPoint = patrol[currentPatrolPoint].transform.position;
        guardAgent.SetDestination(targetPoint);
        isTravelling = true;
    }
    void Chase()
    {



    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
}
