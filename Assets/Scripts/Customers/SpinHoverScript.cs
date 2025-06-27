using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinHoverScript : MonoBehaviour
{
    public AnimationClip spinHoverAnimation;
    private void Start()
    {
        var animScript = gameObject.AddComponent<Animation>();
        animScript.playAutomatically = true;
        animScript.AddClip(spinHoverAnimation, "SpinHover");
        animScript.enabled = true;
        animScript.Play("SpinHover");
    }
}
