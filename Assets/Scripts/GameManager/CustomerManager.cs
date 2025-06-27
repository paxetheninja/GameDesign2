using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Controls the customer flow. Handles spawning and despawning of customers. Provides callback functions for different events regarding customers.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    // Customer behaviour of one level:
    // - We provide a method to start the level
    // - We want to spawn X customers in Y seconds/minutes.
    // - The shop provides 3(?) parallel spots for customers to order something.
    //   - The others wait outside in a queue.
    // - All customers have a patience bar, in which they have to get their ordered product - otherwise the party fails this level.
    // - If a customers receives his product, the customer leaves and another one takes his spot if anyone was in queue.
    // - The party wins the level if all X customers are satisfied (Max time available for this is Y seconds/minutes + patience of last customer)

    [SerializeField] private GameObject customerPrefab;

    private const float queueDistanceInterval = 1.3f;

    private int nextCustomerId = 0;

    public List<Vector3> transformsOfOrderSpots = new List<Vector3>();

    public List<OrderSpot> OrderSpots = new List<OrderSpot>();
    private Queue<Customer> queue = new Queue<Customer>();

    private object queueLock = new object();
    private object orderSpotsLock = new object();

    public int CurrentCustomerCount => queue.Count + OrderSpots.Count(o => o.CurrentCustomer is not null);

    public Vector3 queueFirstPosition;
    public Quaternion queueFirstFacingRotation;

    public Vector3 spawnPosition;
    public Vector3 exitPosition;

    public int Difficulty = 0;
    private void Start()
    {
        // We don't want this manager on all clients, only the server/host.
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            Destroy(gameObject);

        if (customerPrefab is null)
            Debug.LogError("Customer prefab is not set.");

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DEBUG
        lock (orderSpotsLock)
        {
            OrderSpots.Add(new OrderSpot() { Position = transformsOfOrderSpots[0], Rotation = Quaternion.Euler(0,90,0) });
            OrderSpots.Add(new OrderSpot() { Position = transformsOfOrderSpots[1], Rotation = Quaternion.Euler(0, 90, 0) });
            OrderSpots.Add(new OrderSpot() { Position = transformsOfOrderSpots[2], Rotation = Quaternion.Euler(0, 90, 0) });
        }

        //queueFirstFacingRotation = Quaternion.Euler(0, 180, 0);
        //queueFirstPosition = new Vector3(-6, 0, 1);

        //spawnPosition = new Vector3(-10, 0, -2);
        //dexitPosition = new Vector3(6, 0, -3);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    public void SpawnNextCustomer(int patienceSeconds=10)
    {
        var newCustomer = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);

        newCustomer.name = $"ID {nextCustomerId} " + newCustomer.name;
        nextCustomerId++;

        newCustomer.GetComponent<CustomerProductLogic>().OrderFulfilledCallback = OrderFulfilledCallback;
        newCustomer.GetComponent<CustomerProductLogic>().difficulty = Difficulty;
        newCustomer.GetComponentInChildren<CustomerPatienceBarScript>().totalPatience = patienceSeconds;

        newCustomer.GetComponent<NetworkObject>().Spawn(true);

        AddCustomerToQueue(newCustomer.GetComponent<Customer>());
    }

    private void TryDequeueCustomer()
    {
        // Check if there is a spot available.
        int freeSpotIndex = -1;

        lock (orderSpotsLock)
        {
            for (int i = 0; i < OrderSpots.Count; i++)
            {
                if (OrderSpots[i].CurrentCustomer is not null)
                    continue;

                freeSpotIndex = i;
                break;
            }

            Customer customerFromQueue = null;

            lock (queueLock)
            {
                if (freeSpotIndex == -1 || !queue.TryDequeue(out customerFromQueue))
                    return;
            }

            OrderSpot freeOrderSpot = OrderSpots[freeSpotIndex];
            freeOrderSpot.CurrentCustomer = customerFromQueue;

            customerFromQueue.targetPosition = freeOrderSpot.Position;
            customerFromQueue.targetRotation = freeOrderSpot.Rotation;

            customerFromQueue.SetCustomerOrderVisualsActiveClientRpc(true);
        }

        UpdateCustomerQueueTransform();
    }

    private void AddCustomerToQueue(Customer newCustomer)
    {
        if (newCustomer is null)
            Debug.LogError("An invalid new customer has been added to the queue.");

        lock (queueLock)
        {
            queue.Enqueue(newCustomer);
        }

        UpdateCustomerQueueTransform();

        // We instantly try to dequeue the new customer - There might be a free spot already.
        TryDequeueCustomer();
    }

    public void OrderFulfilledCallback(CustomerProductLogic sender)
    {
        Customer senderCustomer = sender.GetComponent<Customer>();

        lock (orderSpotsLock)
        {
            foreach (var orderSpot in OrderSpots)
            {
                if (orderSpot.CurrentCustomer != senderCustomer) 
                    continue;

                orderSpot.CurrentCustomer = null;
                TryDequeueCustomer();

                senderCustomer.targetPosition = exitPosition;
                senderCustomer.SetCustomerOrderVisualsActiveClientRpc(false);

                // We reverse this component list because we want to despawn the most nested children first.
                foreach (var childNetworkObjects in sender.GetComponentsInChildren<NetworkObject>().Reverse())
                    StartCoroutine(DespawnIn(8, childNetworkObjects));

                return;
            }
        }

        Debug.LogWarning("OrderFulfilledCallback has been called but sender was not registered in a spot. Skipping this callback.");
    }

    private IEnumerator DespawnIn(float time, NetworkObject netObject)
    {
        yield return new WaitForSeconds(time);
        netObject.Despawn();
    }

    private void UpdateCustomerQueueTransform()
    {
        lock (queueLock)
        {
            var queueArray = queue.ToArray();

            Vector3 dirToNextCustomer = queueFirstFacingRotation * Vector3.back;

            for (int i = 0; i < queueArray.Length; i++)
            {
                queueArray[i].targetRotation = queueFirstFacingRotation;
                queueArray[i].targetPosition = queueFirstPosition + dirToNextCustomer * i * queueDistanceInterval;
            }
        }
    }
}

public class OrderSpot
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Customer CurrentCustomer;
}
