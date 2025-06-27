using System;
using UnityEngine;

namespace Misc
{
    public class TweeningFunctions : MonoBehaviour
    {
        public static float Linear(float start, float end, float state)
        { 
            Debug.Assert(state is >= 0.0f and <= 1.0f);
            return start + (end - start) * state;
        }

        // https://easings.net/#easeOutBounce
        // https://gist.github.com/cjddmut/d789b9eb78216998e95c
        public static float EaseOutBounce(float start, float end, float state)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
        
            end -= start;
            if (state < 1 / d1)
            {
                return end * (n1 * state * state) + start;
            }

            if (state < 2 / d1)
            {
                state -= 1.5f / d1;
                return end * (n1 * state * state + 0.75f) + start;
            }
        
            if (state < 2.5 / d1)
            {
                state -= 2.25f / d1;
                return end * (n1 * state * state + 0.9375f) + start;
            }
        
            state -= 2.625f / d1;
            return end * (n1 * state * state + 0.984375f) + start;
        }

        public static float EaseOutBack(float start, float end, float state)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return (end - start) * (1 + c3 * MathF.Pow(state - 1, 3) + c1 * MathF.Pow(state - 1, 2)) + start;
        }
        
        public static float EaseOutElastic(float start, float end, float state)
        {
            float c4 = (2 * MathF.PI) / 3;

            if (state == 0)
            {
                return start;
            }

            if (Math.Abs(state - 1) < 0.001f)
            {
                return end;
            }

            return (end - start) * (MathF.Pow(2, -10 * state) * MathF.Sin((state * 10 - 0.75f) * c4) + 1) + start;
        }
        
        public static float EaseOutExpo(float start, float end, float state)
        {
            var factor = Math.Abs(state - 1) < 0.001f ? 1 : 1 - MathF.Pow(2, -10 * state);
            
            return (end - start) * factor + start;
        }
        
        public static float EaseInOutBack(float start, float end, float state)
        {
            float c1 = 1.70158f;
            float c2 = c1 * 1.525f;

            var factor = state < 0.5f
                ? (MathF.Pow(2 * state, 2) * ((c2 + 1) * 2 * state - c2)) / 2
                : (MathF.Pow(2 * state - 2, 2) * ((c2 + 1) * (state * 2 - 2) + c2) + 2) / 2;
            
            return (end - start) * factor + start;
        }
         
        public static float EaseInSin(float start, float end, float state)
        {
            return (end - start) * (1 - MathF.Cos((state * MathF.PI) / 2)) + start;
        }
          
        public static float EaseInCirc(float start, float end, float state)
        {
            return (end - start) * (1 - MathF.Sqrt(1 - MathF.Pow(state, 2))) + start;
        }
        
        public static float EaseInOutQuad(float start, float end, float state)
        {
            return (end - start) * (state < 0.5f ? 2 * state * state : 1 - MathF.Pow(-2 * state + 2, 2) / 2) + start;
        }
    }
}