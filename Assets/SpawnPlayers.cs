using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
  
    public List<GameObject> SpawnPoints;
    public List<GameObject> players;
    void Start()
    {
    
        int index = 0;
        if (SpawnPoints.Count < 4)
            throw new Exception("Not enough spawnpoints");
        players = GameObject.FindGameObjectsWithTag("Character").ToList();
        foreach (GameObject player in players)
        {
            player.transform.position = SpawnPoints[index].transform.position;
            player.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
            index++;
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
