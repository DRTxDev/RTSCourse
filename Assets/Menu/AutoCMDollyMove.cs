using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AutoCMDollyMove : MonoBehaviour
{
    CinemachineDollyCart CM;

    [SerializeField] float dollyCartSpeed = 1f;

    void Awake()
    {
        CM = GetComponent<CinemachineDollyCart>();
    }

    void Update()
    {
        CM.m_Position += dollyCartSpeed * Time.deltaTime;
    }
}
