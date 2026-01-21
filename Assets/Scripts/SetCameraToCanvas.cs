using UnityEngine;

public class SetCameraToCanvas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var camera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = camera;
    }
}
