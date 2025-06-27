using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public class DoorTriggerScript : MonoBehaviour
    {
        private DoorScript _doorScript;
        [SerializeField] private bool direction;
            
        private void Start()
        {
            _doorScript = GetComponentInParent<DoorScript>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<PlayerActionHandler>() == null || _doorScript.doorCooldown > 0) return;
            _doorScript.doorCooldown = 0.8f;
            SoundsScript.Instance.SoundDoorOpen();
            _doorScript.TriggerDoorOpenServerRpc(direction);
        }
    }
}
