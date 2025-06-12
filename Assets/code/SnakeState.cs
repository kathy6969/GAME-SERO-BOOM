using System.Collections.Generic;
using UnityEngine;

public class SnakeState
{
    public List<Vector3> segmentPositions;

    public SnakeState(List<Transform> segments)
    {
        segmentPositions = new List<Vector3>();
        foreach (Transform t in segments)
        {
            segmentPositions.Add(t.position);
        }
    }
}
