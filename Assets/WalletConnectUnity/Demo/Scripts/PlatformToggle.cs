using UnityEngine;

public class PlatformToggle : MonoBehaviour
{
    public bool activeOnDesktop;
    public bool activeOnAndroid;
    public bool activeOniOS;
    
    void Start()
    {
        #if UNITY_ANDROID
        gameObject.SetActive(activeOnMobile);
        #elif UNITY_IOS
        gameObject.SetActive(activeOniOS);
        #else
        gameObject.SetActive(activeOnDesktop);
        #endif
    }
}
