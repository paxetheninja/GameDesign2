using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class PlayerHelp : MonoBehaviour
{
    GameObject UI;
    [SerializeField]
    private VisualElement root;
    // Start is called before the first frame update
    void Start()
    {
        UI = GameObject.FindGameObjectWithTag("HelpWindow");
    }

    // Update is called once per frame
    void Update()
    { 
        if (UI is null || UI.IsDestroyed())
        {
            UI = GameObject.FindGameObjectWithTag("HelpWindow");
        }
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (UI is not null && gameObject.GetComponent<NetworkObject>().IsOwner)
            {
                UIManager manager = UI.GetComponent<UIManager>();
                if (!manager.IsHelpEnabled)
                    manager.EnableHelp();
                else
                {
                    manager.DisableHelp();
                }
            }

        }
    }
}
