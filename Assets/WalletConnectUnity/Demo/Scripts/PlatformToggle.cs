using UnityEngine;

public class PlatformToggle : MonoBehaviour
{
    public bool activeOnDesktop;
    public bool activeOnMobile;
    
    void Start()
    {
        #if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(activeOnMobile);
        #else
        gameObject.SetActive(activeOnDesktop);
        #endif
    }
}
