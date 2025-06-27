using System;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    [SerializeField] private GameObject customerPatienceBar;
    
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    private CustomerProductLogic _productLogic;

    private NetworkVariable<bool> IsMoving = new NetworkVariable<bool>();

    public float movementSpeed = 2.5f;
    public float turnSpeed = 90;

    public CustomerProductLogic ProductLogic => _productLogic;

    private Animator animator;

    private void Start()
    {
        _productLogic = GetComponent<CustomerProductLogic>();

        if (_productLogic is null)
            Debug.LogError($"Customer initialized without {nameof(CustomerProductLogic)} script.");

        animator = GetComponent<Animator>();
        IsMoving.OnValueChanged += UpdateIsMoving;
    }

    private void UpdateIsMoving(bool previousvalue, bool newvalue)
    {
        animator.SetBool("IsMoving", newvalue);
    }

    private void Update()
    {
        if (!IsServer)
            return;

        Vector3 distance = targetPosition - transform.position;
        distance.y = 0;
        if (distance.magnitude > 0.01f)
        {
            var lookDir = targetPosition - transform.position;
            lookDir.y = 0;
            Quaternion targetDir = Quaternion.LookRotation(lookDir);
            if (Quaternion.Angle(transform.rotation, targetDir) > 5.0f)
            {
                Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir),
                    turnSpeed * Time.deltaTime);

                Quaternion oldRotation = transform.rotation;

                transform.eulerAngles =
                    new Vector3(oldRotation.eulerAngles.x, rot.eulerAngles.y, oldRotation.eulerAngles.z);
            }
            else
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            }

            IsMoving.Value = true;
        }
        else if (Quaternion.Angle(transform.rotation, targetRotation) > 1.0f)
        {
            Quaternion rot = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            Quaternion oldRotation = transform.rotation;

            transform.eulerAngles =
                new Vector3(oldRotation.eulerAngles.x, rot.eulerAngles.y, oldRotation.eulerAngles.z);

            IsMoving.Value = true;
        }
        else
        {
            IsMoving.Value = false;
        }
    }

    [ClientRpc]
    public void SetCustomerOrderVisualsActiveClientRpc(bool value)
    {
        GetComponent<CustomerOrderVisuals>().enabled = value;
        if(customerPatienceBar != null)
            customerPatienceBar.SetActive(value);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(targetPosition + 0.5f * Vector3.up, Vector3.one);
        Gizmos.DrawLine(transform.position + 0.5f * Vector3.up, targetPosition + 0.5f * Vector3.up);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition + 1.1f * Vector3.up + 0.4f * (targetRotation * Vector3.forward),  targetRotation * new Vector3(0.2f, 0.2f, 0.8f));
    }
}
