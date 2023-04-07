using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JunctionState
{
    Stop,
    Go
}

public enum JunctionType
{
    Crossroad,
    TJunction,
    Pedestrian
}

public class Junction : MonoBehaviour
{
    [Header("Junction Settings")]
    [SerializeField] private JunctionType type;
    [SerializeField] private float updateInterval = 4f;
    [SerializeField] private float waitInterval = 1f;
    [SerializeField] private List<Waypoint> waypoints;

    private void Awake()
    {
        foreach (Waypoint wp in waypoints)
            wp.SetJunction(this);
    }

    private void Start()
    {
        StartCoroutine(UpdateJunction());
    }

    private int i = 0;
    public IEnumerator UpdateJunction()
    {
        if (type == JunctionType.Crossroad || type == JunctionType.TJunction)
        {
            for (int j = 0; j < waypoints.Count; j++)
            {
                // Only make one waypoint active at the same time
                if (j == i) waypoints[j].state = JunctionState.Go;
                else waypoints[j].state = JunctionState.Stop;
            }
        }
        else if (type == JunctionType.Pedestrian)
        {

        }

        // Cycle between the waypoints
        if (i == waypoints.Count - 1) i = 0;
        else i++;

        yield return new WaitForSeconds(updateInterval);

        foreach (Waypoint wp in waypoints)
        {
            wp.state = JunctionState.Stop;
        }
        yield return new WaitForSeconds(waitInterval);

        StartCoroutine(UpdateJunction());
    }

    #region External methods

    public void AddWaypoint(Waypoint wp)
    {
        waypoints.Add(wp);
    }

    public bool HasWaypoint(Waypoint wp)
    {
        return waypoints.Contains(wp);
    }

    public List<Waypoint> GetWaypoints()
    {
        return waypoints;
    }

    #endregion
}
