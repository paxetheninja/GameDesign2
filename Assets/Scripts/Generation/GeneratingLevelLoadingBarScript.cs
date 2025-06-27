using System;
using UnityEngine;

namespace Generation
{
    public class GeneratingLevelLoadingBarScript : MonoBehaviour
    {
        [SerializeField] private GameObject loadingBar;
        [SerializeField] private UIManager uiManager;
        
        private void Start()
        {
            uiManager.DisableAndroidTouch();
        }

        private void OnDisable()
        {
            if(uiManager.enableAndroidTouch)
            {
                uiManager.EnableAndroidTouch();
            }
        
        }

        public void SetScale(float scale)
        {
            loadingBar.transform.localScale = new Vector3(scale, 1, 1);
        }
    }
}
