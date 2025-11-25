using System;
using System.Collections;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CustomerAI : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform customerSpot;
    public Transform exitPoint;

    [Header("Visuals")]
    public Material happyMaterial;
    public Material unhappyMaterial;

    private Renderer rend;
    private ItemType desiredItem;
    private bool isHappy = true;

    [Header("Settings")]
    public float moveSpeed = 8f;
    public float waitTime = 80f;
    
    public bool wasServed;
    public AudioClip sfxCashier;
    public AudioSource audioSource;
    
    private NavMeshAgent agent;
    Quaternion lookRotation;
    public GameObject crabBubblePrefab;
    public GameObject fishBubblePrefab;
    public Transform bubblePosition;
    public GameObject bubbleInstance;

    void Start()
    {
        waitTime = DifficultyManager.Instance.customerWaitTime;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.avoidancePriority = 20;
    }
    
    void Awake()
    {
        rend = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Run(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        StartCoroutine(CustomerRoutine());
    }

    IEnumerator CustomerRoutine()
    {
        // Step 1: Move until we get close enough to the spot
        Vector3 spot = GetCustomerSpot();
        agent.isStopped = false;
        agent.SetDestination(spot);

        // Wait until close ENOUGH (not perfectly arrived)
        while (Vector3.Distance(transform.position, spot) > 1.2f && !wasServed)
        {
            yield return null;
        }

        // Stop moving (or will stop later due to collision logic)
        agent.isStopped = true;

        // Step 2: Decide what the customer wants
        desiredItem = (Random.value > 0.5f) ? ItemType.FishDish : ItemType.CrabDish;

        // Step 3: Display desire immediately when near the spot
        DisplayDesire(desiredItem);

        // Step 4: Start serving timer without needing to touch the spot exactly
        float timer = 0f;
        while (timer < waitTime && !wasServed)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(bubbleInstance);

        // Step 5: Mood decision
        SetMood(wasServed);

        // Step 6: Walk to exit
        yield return MoveTo(exitPoint.position);

        // Step 7: Despawn
        Destroy(gameObject);
    }

    
    Vector3 GetCustomerSpot()
    {
        // Small random offset so customers don't overlap
        Vector2 offset = Random.insideUnitCircle * 0.5f; // 0.5 units radius
        return customerSpot.position + new Vector3(offset.x, 0, offset.y);
    }


    private IEnumerator MoveTo(Vector3 target)
    {
        agent.isStopped = false;
        agent.SetDestination(target);

        while (Vector3.Distance(transform.position, target) > 0.2f)
            yield return null;

        agent.isStopped = true;
    }

    // ---------- Mood ---------------------
    private void SetMood(bool happy)
    {
        isHappy = happy;

        if (isHappy)
            rend.material = happyMaterial;
        else
            rend.material = unhappyMaterial;
    }

    // ---------- Placeholder --------------
    private void DisplayDesire(ItemType item)
    {
        GameObject bubblePrefab = item == ItemType.CrabDish ? crabBubblePrefab : fishBubblePrefab ;
        bubbleInstance = Instantiate(bubblePrefab);
        bubbleInstance.transform.SetParent(bubblePosition, false);
        // Reset local transform to match parent
        bubbleInstance.transform.localPosition = Vector3.zero;
        bubbleInstance.transform.localRotation = Quaternion.identity;
        bubbleInstance.transform.localScale = Vector3.one;
        
    }

    public void Update()
    {
        Vector3 vel = agent.velocity;
        if (vel.sqrMagnitude > 0.001f)
        {
            lookRotation = Quaternion.LookRotation(agent.velocity.normalized);
        }
        transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
    }
    
    void LateUpdate()
    {
        if (bubbleInstance != null)
            bubbleInstance.transform.LookAt(Camera.main.transform);
    }

    public void Interact(Player player)
    {
        if (!player.activeItem || wasServed)
        {
            return;
        }
        if (player.activeItem.itemType == desiredItem)
        {
            wasServed = true;
            Destroy(player.activeItem.gameObject);
            player.activeItem = null;

            player.Money += 50;
            SetMood(true);
            if (audioSource && sfxCashier)
                audioSource.PlayOneShot(sfxCashier);

        }
        else if (player.activeItem.itemType == ItemType.BurnedFood)
        {
            player.Money -= 23;
            wasServed = true;
            player.activeItem = null;
            SetMood(false);
            Destroy(player.activeItem);
        }
        agent.isStopped = false;
    }
    
    private int collisions = 0;
    private bool canMoveForAwhile = false;
    private Coroutine resumeRoutine = null;


    private IEnumerator ResumeRoutine()
    {
        canMoveForAwhile = true;
        agent.isStopped = false;
        yield return new WaitForSeconds(4f);
        canMoveForAwhile = false;
        if (!wasServed)
        {
            agent.isStopped = true;
        }
        resumeRoutine = null;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CustomerAI>())
        {
            collisions++;
            
            if (!wasServed && !canMoveForAwhile)
            {
                agent.isStopped = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CustomerAI>())
        {
            collisions--;
            if (collisions <= 0)
            {
                collisions = 0;
                agent.isStopped = false;
            }
            else
            {
                if (resumeRoutine != null)
                {
                    StopCoroutine(resumeRoutine);
                    canMoveForAwhile = false;
                    agent.isStopped = false;
                }
                resumeRoutine = StartCoroutine(ResumeRoutine());
            }
        }
    }
}
