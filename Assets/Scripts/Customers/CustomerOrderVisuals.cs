using Unity.Netcode;
using UnityEngine;

public class CustomerOrderVisuals : MonoBehaviour
{
    private CustomerProductLogic productLogicRef;

    [SerializeField] private GameObject visualObjectParentRef;
    [SerializeField] private GameObject questionMark;
    [SerializeField] private GameObject objectToProduceRoot;
    [SerializeField] private AnimationClip spinHoverAnimationClip;
    
    private void Start()
    {
        productLogicRef = GetComponent<CustomerProductLogic>();

        if (productLogicRef is null)
            Debug.LogError("Prefab is broken. This should not be null.");

        productLogicRef.Order.OnValueChanged += UpdateOrderServerRpc;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UpdateOrderServerRpc(ComponentType previousvalue, ComponentType newvalue)
    {
        UpdateOrderClientRpc(newvalue);
    }

    [ClientRpc]
    private void UpdateOrderClientRpc(ComponentType newvalue)
    {
        questionMark.SetActive(false);

        var prefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(newvalue);
        var objectToProduce = Instantiate(prefab, objectToProduceRoot.transform);
        objectToProduce.GetComponent<NetworkObject>().enabled = false;
        objectToProduce.GetComponent<ServerLocalNetworkTransform>().enabled = false;
        
        objectToProduce.AddComponent<SpinHoverScript>();
        objectToProduce.GetComponent<SpinHoverScript>().spinHoverAnimation = spinHoverAnimationClip;
        objectToProduce.GetComponent<SpinHoverScript>().enabled = true;


       objectToProduce.transform.localScale *= 1.5f;
        objectToProduce.transform.localPosition = Vector3.zero;
        objectToProduce.transform.SetParent(objectToProduceRoot.transform);
    }
    
    private void SetVisualsActive(bool value)
    {
        visualObjectParentRef.SetActive(value);
    }

    private void OnEnable()
    {
        SetVisualsActive(true);
    }

    private void OnDisable()
    {
        SetVisualsActive(false);
    }
}
