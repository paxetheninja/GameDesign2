using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ProgressBarScript : MonoBehaviour
{
    [SerializeField] private float progress;

    [SerializeField] private GameObject bar;

    private NetworkVariable<float> _interactionProgressRef;
    private NetworkVariable<int> _interactionPlayersCountRef;
    private bool _init;

    public GameObject cameraObject;

    public GameObject Bar => bar;

    private void Init(NetworkVariable<float> interactionProgress, NetworkVariable<int> interactionPlayerCount)
    {
        _interactionProgressRef = interactionProgress;
        _interactionPlayersCountRef = interactionPlayerCount;
        _init = true;
    }
    
    private void Update()
    {
        if (!_init) return;

        if (_interactionPlayersCountRef.Value == 0)
        {
            gameObject.GetComponentInParent<BaseWorkstation>().activeProgressBar = null;
            Destroy(gameObject);
            return;
        }
        
        progress = _interactionProgressRef.Value;
        bar.transform.localScale = new Vector3(-progress, 1, 1);
        transform.LookAt(cameraObject.transform.position);
    }

    public static GameObject SpawnProgressBar(GameObject progressBarPrefab, Transform parent, 
        NetworkVariable<float> interactionProgress, NetworkVariable<int> interactionPlayerCount)
    {
        GameObject progressBarObj = Instantiate(progressBarPrefab, parent);
        
        ProgressBarScript progressBarScript = progressBarObj.GetComponent<ProgressBarScript>();
        
        progressBarScript.Init(interactionProgress, interactionPlayerCount);
        progressBarObj.transform.position += new Vector3(0, 2, 0);
        // progressBarObj.transform.rotation = Quaternion.identity;

        Camera cam = Camera.main;
        progressBarScript.cameraObject = cam.gameObject;

        progressBarObj.transform.LookAt(progressBarScript.cameraObject.transform.position); 
        
        progressBarObj.SetActive(true);

        return progressBarObj;
    }
}
