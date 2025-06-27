using Unity.Netcode;
using UnityEngine;

public class CustomerPatienceBarScript : NetworkBehaviour
{
    [SerializeField] private GameObject bar;
    [SerializeField] private Material barOrange;
    [SerializeField] private Material barRed;
    
    public float totalPatience;

    public Camera cameraObject;

    private float _currentPatience;
    private NetworkVariable<float> _netTotalPatience = new();

    private void Start()
    {
        cameraObject = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _currentPatience = totalPatience;
            _netTotalPatience.Value = totalPatience;
        }
        else
        {
            totalPatience = _netTotalPatience.Value;
            _currentPatience = totalPatience;
        }
    }

    private void Update()
    {
        _currentPatience -= Time.deltaTime;
        
        if (_currentPatience <= 0)
        {
            if (IsServer)
            {
                if(GameplayManager.Instance != null) 
                    GameplayManager.Instance.RanOutOfPatience();
            }
            Destroy(gameObject);
            return;
        }

        float currentFraction = _currentPatience / totalPatience;
        
        bar.transform.localScale = new Vector3(-currentFraction, 1, 1);
        transform.LookAt(cameraObject.gameObject.transform.position);

        var meshRenderer = bar.GetComponentInChildren<MeshRenderer>();
        if (currentFraction is > 0.2f and <= 0.5f)
        {
            meshRenderer.material = barOrange;
        }
        else if (currentFraction <= 0.2f)
        {
            meshRenderer.material = barRed;
        }
    }

    public void DisablePatience()
    {
        bar.SetActive(false);
        enabled = false;
    }
}
