using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorGizmo : MonoBehaviour
{   
    [SerializeField] float gizmoSize = 0.025f;
    void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}
