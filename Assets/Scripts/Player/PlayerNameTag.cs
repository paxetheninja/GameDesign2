using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerNameTag : NetworkBehaviour
{
    private Camera _mainCamera;

    private Camera MainCamera
    {
        get
        {
            if (_mainCamera == null || _mainCamera.IsDestroyed())
                _mainCamera = Camera.main;

            return _mainCamera;
        }
    }

    [SerializeField] private TMP_Text text;

    private void Start()
    {
        gameObject.SetActive(!IsOwner);

        GetComponentInParent<NetworkedPlayerName>().Name.OnValueChanged += PlayerNameChanged;
        PlayerNameChanged("", GetComponentInParent<NetworkedPlayerName>().Name.Value);
    }

    private void PlayerNameChanged(NetworkString previousvalue, NetworkString newvalue)
    {
        text.text = newvalue;
    }

    private void Update()
    {
        transform.rotation = MainCamera.transform.rotation;
    }
}
