using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StandartNPC : MonoBehaviour 
{
    public GameObject food;
    public float eatingTimer;
    public Transform npcHand;
    public Transform[] foodStands;
    int standSelection;
    public float NPCHunger;
    
    public GameObject[] hungryNPC;
    int hungryNPCCount;

    Animator animator;
    NavMeshAgent navigationAgent;
    public bool playingGuitar;
    public bool inCafe;

    AudioSource npcReaction;
    public Transform player;
    public Transform npc;
    public float speedOfNPC= 3.5f;
    float distanceToPlayer = Mathf.Infinity;

    public Transform[] patrolledPoints;
    int currentPatrol = 0;
    int previousPatrol;
    public gameState currentState;

    public enum gameState { Patrolling, inDialogue, Drinking, Eating, Idle }

    private void Start() 
    {
        npcReaction = GetComponent<AudioSource>();
        NPCHunger = Random.Range(30f, 300f);
        if (playingGuitar || inCafe)
        {
            currentState = gameState.Idle;
        }
        else
        {
            currentState = gameState.Patrolling;
        }
        transform.gameObject.tag = "NPC";
        navigationAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        distanceToPlayer = Vector3.Distance(player.position, transform.position);
        
        if (currentState == gameState.Patrolling) 
        { 
            Patrol();
        }
        if (GetComponent<DialogueManagement>().npcTalking)
        {
            currentState = gameState.inDialogue;
            navigationAgent.isStopped = true;
        }
        else if (NPCHunger <= 10) 
        {
            if (currentState == gameState.Idle || currentState == gameState.Drinking){}
            else
            {
                currentState = gameState.Eating;
                transform.gameObject.tag = "Eating";
            }
        }
        else if (!GetComponent<DialogueManagement>().npcTalking)
        {
            currentState = gameState.Patrolling;
            navigationAgent.isStopped = false;
        }
        
        hungryNPC = GameObject.FindGameObjectsWithTag("Eating");

        if (hungryNPC.Length > 4)
        {
            transform.gameObject.tag = "NPC";
            NPCHunger = NPCHunger + Random.Range(800,1500);
            currentState = gameState.Patrolling;
            Patrol();
        }
        else
        {
            Eat();
        }
        

        if (!navigationAgent.isStopped) 
        {
            animator.SetBool("walkingAnim", true);
        }
        else
        {
            animator.SetBool("walkingAnim", false);
        }

        if (playingGuitar)
        {
            Guitar();
        }

        if (inCafe)
        {
            Cafe();
        }
    }

    void FixedUpdate() 
    {
        NPCHunger = NPCHunger - 0.03f;
        if (animator.GetBool("eatingAnim"))
        {
            eatingTimer += 1f;
        }
        else if (eatingTimer >= 500f && animator.GetBool("eatingAnim"))
        {
            animator.SetBool("eatingAnim", false);
            food.transform.SetParent(null);
            food.SetActive(false);
            eatingTimer = 0f;
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "player")
        {
            animator.SetBool("stepBack", true);
            npcReaction.Play();
        }
    }

    void Patrol()
    {
        Vector2 npcPos = new Vector2(npc.position.x,npc.position.z);
        Vector2 pointPos = new Vector2(patrolledPoints[currentPatrol].position.x, patrolledPoints[currentPatrol].position.z);
        if (Vector2.Distance(npcPos, pointPos) < 0.4f || currentPatrol == 0)
        {
            currentPatrol = Random.Range(0, patrolledPoints.Length);
            navigationAgent.SetDestination(patrolledPoints[currentPatrol].position);
            navigationAgent.speed = speedOfNPC;
        }
    }

    void Eat()
    {
        standSelection = Random.Range(0, foodStands.Length);
        navigationAgent.SetDestination(foodStands[standSelection].position);
        if (Vector3.Distance(transform.position, foodStands[standSelection].position) < 1f)
        {
            navigationAgent.isStopped = true;
            animator.SetBool("gettingFood", true);
            food.SetActive(true);
            food.transform.SetParent(npcHand);
            StartCoroutine(WaitAndGo(1f));
            animator.SetBool("gettingFood", false);
            animator.SetBool("eatingAnim", true);
            NPCHunger = NPCHunger + 1500;
            
        }
    }

    void Guitar()
    {
        animator.SetBool("guitarAnim", true);
    }

    void Cafe()
    {
        animator.SetBool("drinkingAnim", true);
        StartCoroutine(WaitAndDrink(6.5f));
    }

    IEnumerator WaitAndGo(float seconds) 
    {
        yield return new WaitForSeconds(seconds);
        navigationAgent.isStopped = false;
    }

    IEnumerator WaitAndDrink(float seconds)
    {
        animator.SetBool("drinkingAnim", false);
        yield return new WaitForSeconds(seconds);
        animator.SetBool("drinkingAnim", true);
    }
}