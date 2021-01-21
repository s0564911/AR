using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.LookAt(Vector3.zero);
    }
    
    // public void UpdatePosition(Transform imageTarget)
    // {
    //     transform.position = imageTarget.position;
    //     transform.rotation = imageTarget.rotation;
    //
    //     gameObject.SetActive(true);
    // }
}
