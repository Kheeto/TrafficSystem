using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad()]
public class WaypointEditor
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(Waypoint wp, GizmoType gizmo)
    {
        // Draw a sphere to show the waypoint pos
        if ((gizmo & GizmoType.Selected) != 0)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.yellow * 0.5f;
        Gizmos.DrawSphere(wp.transform.position, .1f);

        // Draw a line showing the waypoint width
        Gizmos.color = Color.white;
        Gizmos.DrawLine(wp.transform.position + (wp.transform.right * wp.width / 2f),
            wp.transform.position - (wp.transform.right * wp.width / 2f));

        // Draw lines towards the previous and next waypoints
        if (wp.previous != null)
        {
            Gizmos.color = Color.red;
            Vector3 offset = wp.transform.right * wp.width / 2f;
            Vector3 offsetTo = wp.previous.transform.right * wp.previous.width / 2f;
            Gizmos.DrawLine(wp.transform.position + offset, wp.previous.transform.position + offsetTo);
        }
        if (wp.next != null)
        {
            Gizmos.color = Color.green;
            Vector3 offset = wp.transform.right * -wp.width / 2f;
            Vector3 offsetTo = wp.next.transform.right * -wp.next.width / 2f;
            Gizmos.DrawLine(wp.transform.position + offset, wp.next.transform.position + offsetTo);
        }
        if (wp.branches != null)
        {
            foreach (Waypoint branch in wp.branches)
            {
                if (branch == null)
                {
                    wp.branches.Remove(branch);
                    continue;
                }
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(wp.transform.position, branch.transform.position);
            }
        }
    }
}
