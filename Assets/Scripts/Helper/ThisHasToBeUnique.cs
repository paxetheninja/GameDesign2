using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThisHasToBeUnique : MonoBehaviour
{
    // Pseudo Instance, this shouldn't be used.
    public static ThisHasToBeUnique Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
