using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkedMaterialAssigner : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer renderer;

    public static List<Color> PlayerColors = new List<Color>()
    {
        new Color32(0xdf, 0x89, 0x20, 0xff), // Fulvous
        new Color32(0xe1, 0x93, 0x5f, 0xff), // Persian Orange
        new Color32(0xd0, 0x69, 0x1f, 0xff), // Cocoa Brown
        new Color32(0x73, 0x45, 0x2a, 0xff), // Kobicha
    };

    private void Awake()
    {
        if (renderer is null)
        {
            Debug.LogError($"{nameof(renderer)} is not assigned. This will throw an error when starting the game.");
        }
    }

    [ClientRpc]
    public void SetColorClientRpc(Color color)
    {
        renderer.material.color = color;
    }
}
