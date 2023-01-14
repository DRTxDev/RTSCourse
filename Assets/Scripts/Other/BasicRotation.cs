using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRotation : MonoBehaviour
{
    [SerializeField] Vector3 rotation;
    [SerializeField] float rotationSpeed;


    void FixedUpdate() 
    {
        transform.Rotate(rotation * rotationSpeed * Time.deltaTime, Space.Self);
    }

}
