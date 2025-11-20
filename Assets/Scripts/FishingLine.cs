using System;
using System.Collections;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(LineRenderer))]
public class FishingRodController : MonoBehaviour
{
    [Header("References")]
    public Transform lineStartPoint;   // Rod tip
    public LineRenderer lineRenderer;
    public Transform rodHandle;        // Handle to spin when scrolling
    public Bar throwBar;
    public GameObject hookPrefab;
    public Rigidbody hookRb;   
    public Player player;

    [Header("States")]
    private bool isCasting;
    private bool isFishing;

    [Header("Params")]
    public float minLineLength;
    public float maxLineLength; 
    public float ropeSpring;
    public float ropeDamper;
    public float ropeTolerance;
    public float scrollStep;

    private float maxThrowForce = 140f;
    private float minThrowForce = 3f;
    private float throwForce;
    private bool throwAsc = true;
    private float throwOscillationSpeed = 420f;

    [Header("Handle Settings")]
    public float handleSmoothSpeed = 8f;

    private SpringJoint ropeJoint;
    private float currentLineLength;
    private float currentHandleAngle;
    private float handleAngleVelocity = 200f;
    private float accumulatedAngle = 0f;

    private Hook hookInstance;
    
    private float biteTimeMin = 10f;
    private float biteTimeMax = 30f;

    private Coroutine biteRoutine;

    private bool isPulling;
    public Item fishPrefab;
    public Item crabPrefab;
    [CanBeNull] public Item fish;
    
    
    public AudioSource audioSource;
    public AudioClip sfxLineRunning;
    public AudioClip sfxLinePulling;
    public AudioClip sfxLineSwish;
    public bool pullStarted = false;
    
    private float fishPullSpinSpeed = 700f;
    private float fishPullTorqueMultiplier = 2f; 
    
    private Coroutine failPullRoutine;
    private float pullFailTime = 5f;
    
    
    void Awake()
    {
        player = GetComponentInParent<Player>();
        throwBar = player.throwBar;
        
        //spawning a hook
        Vector3 hookSpawnOffset = new Vector3(0, 0, 0.5f);
        if (hookRb == null && hookPrefab != null)
        {
            Vector3 spawnPos = lineStartPoint.position + lineStartPoint.forward * hookSpawnOffset.z 
                                                       + lineStartPoint.up * hookSpawnOffset.y
                                                       + lineStartPoint.right * hookSpawnOffset.x;

            hookInstance = Instantiate(hookPrefab, spawnPos, Quaternion.identity).GetComponent<Hook>();

            // Make sure it's in the scene root (no parent)
            hookInstance.transform.SetParent(null);

            // Get Rigidbody reference
            hookRb = hookInstance.GetComponent<Rigidbody>();
            hookInstance.GetComponent<Hook>().fishingRod = this;

            if (hookRb == null)
                Debug.LogError("Hook prefab is missing a Rigidbody!");
        }
        
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    void Start()
    {
        biteTimeMin = DifficultyManager.Instance.minFishBite;
        biteTimeMax = DifficultyManager.Instance.maxFishBite;
        pullFailTime = DifficultyManager.Instance.pullLimit; 
        
        CreateRopeJoint();
        ApplyRopeParams(true);
        InitThrowBar();
        
    }

    private void InitThrowBar()
    {
        throwBar.SetMinMax(minThrowForce, maxThrowForce);
    }

    void Update()
    {
        if (player.activeItem && player.activeItem.gameObject == gameObject)
        {
            HandleScrollInput();
            HandleCastingInput();
        }
        handleEarlyFishingStop();
        HandleFishing();
        HandleFishPullSpin();
        
        ApplyRopeParams(false);
        UpdateLine();
    }
    
    private void HandleFishPullSpin()
    {
        // Only rotate when fishing, fish actually caught, and player is NOT pulling
        if (!isFishing || fish == null || pullStarted) return;

        if (!player.activeItem || player.activeItem.gameObject != gameObject)
        {
            float targetAngle = accumulatedAngle - fishPullSpinSpeed * fishPullTorqueMultiplier * Time.deltaTime;
            accumulatedAngle = targetAngle;

            // Smooth rotation
            currentHandleAngle = Mathf.Lerp(
                currentHandleAngle,
                accumulatedAngle,
                Time.deltaTime * handleSmoothSpeed
            );
        }
        // Spin backwards (opposite of reeling direction)
        

        // Apply rotation to handle
        rodHandle.localRotation = Quaternion.Euler(currentHandleAngle, -90f, 0f);

        // Play running sound if not playing
        PlayLoop(sfxLineRunning);
    }


    private void HandleFishing()
    {
        if (!isFishing) return;
    }
    
    private IEnumerator FishBiteTimer()
    {
        float waitTime = Random.Range(biteTimeMin, biteTimeMax);
        yield return new WaitForSeconds(waitTime);

        OnFishBite();
    }

    private void OnFishBite()
    {
        if (!isFishing) return;
        
        float distance = currentLineLength;

        float minDist = 5f;
        float maxDist = 25f;
        
        float t = Mathf.InverseLerp(minDist, maxDist, distance);
        
        float crabProbability = Mathf.Lerp(0.9f, 0.1f, t);
        float fishProbability = 1f - crabProbability;

        Item prefab;
        Transform slot;
        
        if (Random.value < fishProbability)
        {
            prefab = fishPrefab;
            slot = hookInstance.fishSlot;
        }
        else
        {
            prefab = crabPrefab;
            slot = hookInstance.crabSlot;
        }
        
        fish = Instantiate(prefab, slot.position, Quaternion.identity, slot.transform);
        fish.transform.position = slot.position;
        fish.transform.rotation = slot.rotation;
        fish.transform.localScale = Vector3.one;
        
        if (player.activeItem == null || player.activeItem.gameObject != gameObject)
        {
            PlayLoop(sfxLineRunning);
        }
        
        if (failPullRoutine != null)
            StopCoroutine(failPullRoutine);

        failPullRoutine = StartCoroutine(FailPullCountdown());
    }
    
    
    private IEnumerator FailPullCountdown()
    {
        float elapsed = 0f;

        while (elapsed < pullFailTime)
        {
            // If the player starts pulling, cancel the punishment
            if (pullStarted)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ShootRodAndDestroy();
    }
    
    private void ShootRodAndDestroy()
    {
        if (!hookRb) return;

        // Compute shot direction (towards hook, but a bit upward)
        Vector3 dir = (hookRb.position - lineStartPoint.position).normalized;
        dir += Vector3.up * 0.4f;           // tweak upward bias
        dir.Normalize();

        // Add a rigidbody to the rod if missing
        Rigidbody rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();

        // Shoot it fast
        float explosionForce = 40f;
        rb.AddForce(dir * explosionForce, ForceMode.Impulse);

        // Detach hook, stop fishing
        isFishing = false;
        isCasting = false;
        StopLoop();

        // Destroy rod after flight, let physics show the launch
        Destroy(gameObject, 2f);
    }
    
    
    void HandleScrollInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput > 0f)
        {
            return;
        }
        
        scrollInput = Mathf.Abs(scrollInput);
        float delta = scrollInput * scrollStep;
            
        currentLineLength = Mathf.Clamp(
            currentLineLength - delta,
            minLineLength,
            maxLineLength
        );
        
        accumulatedAngle += scrollInput * handleAngleVelocity;
        
        currentHandleAngle = Mathf.Lerp(
            currentHandleAngle,
            accumulatedAngle,
            Time.deltaTime * handleSmoothSpeed
        );

        rodHandle.localRotation = Quaternion.Euler(currentHandleAngle, -90f, 0f);

        if (!pullStarted && fish)
        {
            pullStarted = true;
            PlayLoop(sfxLinePulling);
        }

        if (currentLineLength <= minLineLength + 0.1f)
        {
            isFishing = false;
            if (fish != null)
            {
                player.Equip(fish);
            } 
            StopLoop();
        }
    }
    
    void CreateRopeJoint()
    {
        if (!hookRb || !lineStartPoint) return;

        ropeJoint = hookRb.gameObject.AddComponent<SpringJoint>();
        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedAnchor = lineStartPoint.position;
    }
    
    void ApplyRopeParams(bool applySpring)
    {
        if (!ropeJoint) return;

        ropeJoint.maxDistance = currentLineLength;
        ropeJoint.minDistance = currentLineLength * 0.95f;
        ropeJoint.damper = ropeDamper;
        ropeJoint.tolerance = ropeTolerance;
        ropeJoint.massScale = 1f;
        if (applySpring)
            ropeJoint.spring = ropeSpring;
    }

    void UpdateLine()
    {
        if (!lineRenderer || !hookRb || !lineStartPoint) return;

        lineRenderer.SetPosition(0, lineStartPoint.position);
        lineRenderer.SetPosition(1, hookRb.position);

        if (ropeJoint)
            ropeJoint.connectedAnchor = lineStartPoint.position;
    }
    
    
    void HandleCastingInput()
    {
        if (Input.GetMouseButtonDown(0) && !isCasting && !isFishing)
        {
            throwForce = minThrowForce;
            throwAsc = true;
        }
        
        if (Input.GetMouseButton(0) && !isCasting && !isFishing)
        {
            // Move throwForce
            if (throwAsc)
                throwForce += throwOscillationSpeed * Time.deltaTime;
            else
                throwForce -= throwOscillationSpeed * Time.deltaTime;

            // Flip direction when hitting limits
            if (throwForce >= maxThrowForce)
            {
                throwForce = maxThrowForce;
                throwAsc = false;
            }
            else if (throwForce <= minThrowForce)
            {
                throwForce = minThrowForce;
                throwAsc = true;
            }
            throwBar.SetValue(throwForce);
        }
        
        if (Input.GetMouseButtonUp(0) && !isCasting && !isFishing)
        {
            PlaySound(sfxLineSwish);
            // Consistent shoot direction — adjust if your rod faces another axis
            Vector3 shootDir = lineStartPoint.forward;
            float shootForce = throwForce;

            // Fully reset hook state
            hookRb.position = lineStartPoint.position;
            hookRb.rotation = Quaternion.identity;
            hookRb.linearVelocity = Vector3.zero;
            hookRb.angularVelocity = Vector3.zero;
            // Apply the impulse
            hookRb.AddForce(shootDir * shootForce, ForceMode.Impulse);
            isCasting = true;
            ropeJoint.spring = 0;
            
            
        }
    }

    public void startFishing()
    {
        if (isCasting && !isFishing)
        {
            isCasting = false;
            isFishing = true;
            
            currentLineLength = Vector3.Distance(lineStartPoint.position, hookRb.position);
            ApplyRopeParams(true);
            
            // Start bite timer
            if (biteRoutine != null)
                StopCoroutine(biteRoutine);

            biteRoutine = StartCoroutine(FishBiteTimer());
        }
    }

    public void handleEarlyFishingStop()
    {
        if (!isFishing)
        {
            return;
        }
        
        if (currentLineLength <= 1f)
        {
            isFishing = false;
            StopLoop();
        }
        
    }

    public void OnDestroy()
    {
        if (hookInstance != null)
        {
            Destroy(hookInstance.gameObject);
        }
        Destroy(gameObject);
    }
    
    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        audioSource.PlayOneShot(clip, volume);
    }
    
    private void PlayLoop(AudioClip clip)
    {
        if (!audioSource || !clip) return;
        if (audioSource.isPlaying && audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopLoop()
    {
        if (!audioSource) return;
        audioSource.loop = false;
        audioSource.Stop();
    }
    
}
