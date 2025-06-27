using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GeneratedLevel", LoadSceneMode.Single);
        }
    }
}
