using System;
using System.Collections;
using Misc;
using Unity.Netcode;
using UnityEngine;

namespace Interactions
{
    public class DoorScript : NetworkBehaviour
    {
        private GameObject _doorAnchor;

        public IEnumerator CurrentAnimation;
        
        private const int MaxAngle = 90;
        private const int MinAngle = 270;

        public float doorCooldown = 0.3f;
        
        private void Start()
        {
            _doorAnchor = gameObject;
        }

        private void Update()
        {
            doorCooldown -= Time.deltaTime;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TriggerDoorOpenServerRpc(bool direction)
        {
            TriggerDoorOpenClientRpc(direction);
        }

        [ClientRpc]
        private void TriggerDoorOpenClientRpc(bool direction)
        {
            if (CurrentAnimation != null) StopCoroutine(CurrentAnimation);

                CurrentAnimation = direction
                ? StartForward(TweeningFunctions.EaseOutExpo, 0.6f, 10)
                : StartBackward(TweeningFunctions.EaseOutExpo, 0.6f, 10);
            StartCoroutine(CurrentAnimation);
        }
        
        IEnumerator StartForward(Func<float, float, float, float> tweeningFunction, float timeOpen, float timeClose)
        {
            float state = 0.0f;

            Quaternion startRotation = _doorAnchor.transform.localRotation;
            float startAngle = startRotation.eulerAngles.y > MaxAngle + 90
                ? startRotation.eulerAngles.y - 360
                : startRotation.eulerAngles.y;
            
            while (state <= 1.0f)
            {
                float delta = Time.deltaTime / timeOpen;
                
                float angle = tweeningFunction(startAngle, MaxAngle, state);
                _doorAnchor.transform.rotation = new Quaternion();
                _doorAnchor.transform.RotateAround(_doorAnchor.transform.position, Vector3.up, angle);
                if (state < 1.0f && state + delta > 1.0f)
                {
                    state = 1.0f;
                }
                else
                {
                    state += delta;
                }
                yield return true;
            }

            yield return new WaitForSeconds(0.5f);
            
            state = 0.0f;

            startRotation = _doorAnchor.transform.localRotation;
            
            while (state <= 1.0f)
            {
                float delta = Time.deltaTime / timeClose;
                float angle = TweeningFunctions.EaseOutElastic(startRotation.eulerAngles.y, 0, state);
                _doorAnchor.transform.rotation = new Quaternion();
                _doorAnchor.transform.RotateAround(_doorAnchor.transform.position, Vector3.up, angle);
                if (state < 1.0f && state + delta > 1.0f)
                {
                    state = 1.0f;
                }
                else
                {
                    state += delta;
                }
                yield return true;
            }

            CurrentAnimation = null;
        }

        IEnumerator StartBackward(Func<float, float, float, float> tweeningFunction, float timeOpen, float timeClose)
        {
            float state = 0.0f;

            Quaternion startRotation = _doorAnchor.transform.localRotation;
            float startAngle = startRotation.eulerAngles.y < MinAngle - 90
                ? startRotation.eulerAngles.y + 360
                : startRotation.eulerAngles.y;
            
            while (state <= 1.0f)
            {
                float delta = Time.deltaTime / timeOpen;
                
                float angle = tweeningFunction(startAngle, MinAngle, state);
                _doorAnchor.transform.rotation = new Quaternion();
                _doorAnchor.transform.RotateAround(_doorAnchor.transform.position, Vector3.up, angle);
                if (state < 1.0f && state + delta > 1.0f)
                {
                    state = 1.0f;
                }
                else
                {
                    state += delta;
                }
                yield return true;
            }

            yield return new WaitForSeconds(0.5f);
            
            state = 0.0f;

            startRotation = _doorAnchor.transform.localRotation;
            
            while (state <= 1.0f)
            {
                float delta = Time.deltaTime / timeClose;
                float angle = TweeningFunctions.EaseOutElastic(startRotation.eulerAngles.y, 360, state);
                _doorAnchor.transform.rotation = new Quaternion();
                _doorAnchor.transform.RotateAround(_doorAnchor.transform.position, Vector3.up, angle);
                if (state < 1.0f && state + delta > 1.0f)
                {
                    state = 1.0f;
                }
                else
                {
                    state += delta;
                }
                yield return true;
            }
            
            CurrentAnimation = null;
        }
    }
}