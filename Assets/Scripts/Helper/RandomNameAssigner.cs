using TMPro;
using UnityEngine;

public class RandomNameAssigner : MonoBehaviour
{
    string[] shortNames = {
        // Female names
        "Emma",
        "Ava",
        "Mia",
        "Lily",
        "Zoe",
        "Nora",
        "Ella",
        "Grace",
        "Maya",
        "Ivy",
    
        // Male names
        "Liam",
        "Noah",
        "Ethan",
        "Logan",
        "Caleb",
        "Lucas",
        "Henry",
        "Leo",
        "Owen",
        "Max"
    };


    void Start()
    {
        TMP_InputField inputField = GetComponent<TMP_InputField>();

        inputField.text = shortNames[Random.Range(0, shortNames.Length)];
    }
}
