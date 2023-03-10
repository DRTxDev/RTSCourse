using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EditorExplosionForceRB : MonoBehaviour
{   
    [SerializeField] float power = 1f;

    [SerializeField] MeshRenderer solidMesh;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Numlock))
        {

            foreach(Rigidbody rigidBody in GetComponentsInChildren<Rigidbody>())
            {
                rigidBody.isKinematic = false;
                rigidBody.AddExplosionForce(power, GameObject.FindGameObjectWithTag("EditorOnly").transform.position, 1, 3.0f);
            }
        }

        if(Input.GetKeyDown(KeyCode.ScrollLock))
        {
            solidMesh.enabled = false;
            GetComponent<Animator>().enabled = true;
        }
    }
}
