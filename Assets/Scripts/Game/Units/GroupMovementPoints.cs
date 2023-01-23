using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GroupMovementPoints : NetworkBehaviour
{
    [SerializeField] List<Transform> points;

    public List<Transform> GetMovementPoints(int numberOfUnits)
    {
        List<Transform> modifiedList = new List<Transform>();

        for(int i = 0; i < numberOfUnits; i++)
        {
            modifiedList.Add(points[i]);
        }

        return modifiedList;
    }
}
