using System.Collections;
using UnityEngine;

public class IntroCamera : MonoBehaviour
{
    public Transform playerCamPos;   // Camera position that follows the player
    public Transform crabCamPos;     // Camera position that looks at the crab
    
    public float moveSpeed = 2f;
    public float waitTime = 3f;      // How long we look at the crab first

    private bool followingPlayer = false;
    private Transform targetPos;
    public FirstPersonController player;
    public CustomerSpawner customerSpawner;
    public PauseController pauseController;
    
    
    public GameObject crab;
    
    void Start()
    {
        pauseController.canBePaused = false;
        Time.timeScale = 1;
        StartCoroutine(CameraSequence());
    }

    IEnumerator CameraSequence()
    {
        player.enabled = false;
        targetPos = crabCamPos;
        yield return MoveToTarget();

        // Step 2: Wait while looking at crab
        yield return new WaitForSeconds(waitTime);

        // Step 3: Move back to player and start following
        targetPos = playerCamPos;
        yield return MoveToTarget();
        player.enabled = true;
        crab.SetActive(false);
        customerSpawner.Run();
        pauseController.canBePaused = true;
    }

    IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(transform.position, targetPos.position) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos.position, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetPos.rotation, Time.deltaTime * moveSpeed);
            yield return null;
        }
    }
}