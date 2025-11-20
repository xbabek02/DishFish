using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public CustomerAI customerPrefab;
    public Transform customerSpot;
    public Transform exitSpot;

    private float minSpawn = 15;
    private float maxSpawn = 40;
    private bool running = true;

    void Start()
    {
        minSpawn = DifficultyManager.Instance.customerMinSpawnTime;
        maxSpawn = DifficultyManager.Instance.customerMaxSpawnTime;
    }
    
    IEnumerator RunCourutine()
    {
        while (running)
        {
            SpawnCustomer();
            if (Random.value < 0.1)
            {
                yield return new WaitForSeconds(3);
                SpawnCustomer();
            }
            
            yield return new WaitForSeconds(Random.Range(minSpawn, maxSpawn));
        }
    }

    public void SpawnCustomer()
    {
        CustomerAI customer = Instantiate(customerPrefab, transform.position, transform.rotation);
        customer.customerSpot = customerSpot;
        customer.exitPoint = exitSpot;
        customer.Run(transform);
    }

    public void Run()
    {
        running = true;
        StartCoroutine(RunCourutine());
    }

    public void Stop()
    {
        running = false;
    }
}