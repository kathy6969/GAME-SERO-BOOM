using System.Collections.Generic;
using UnityEngine;

public class SnakeState
{
    public List<Vector3> segmentPositions;
    public List<Vector3> bananaPositions;
    public List<Vector3> medicinePositions;

    public SnakeState(List<Transform> segments, List<GameObject> bananas, List<GameObject> medicines)
    {
        segmentPositions = new List<Vector3>();
        foreach (var segment in segments)
            segmentPositions.Add(segment.position);

        bananaPositions = new List<Vector3>();
        foreach (var b in bananas)
            bananaPositions.Add(b.transform.position);

        medicinePositions = new List<Vector3>();
        foreach (var m in medicines)
            medicinePositions.Add(m.transform.position);
    }
}