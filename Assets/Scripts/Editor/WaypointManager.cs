using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointManager : EditorWindow
{
    [MenuItem("Tools/Waypoint Manager")]
    public static void Open()
    {
        GetWindow<WaypointManager>();
    }

    [Tooltip("The parent object of all the waypoints (they will be created as children of this object)")]
    public Transform root;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty(nameof(root)));
        if (root == null)
            EditorGUILayout.HelpBox("Root transform must be assigned.", MessageType.Error);
        else
        {
            EditorGUILayout.BeginVertical("box");
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        obj.ApplyModifiedProperties();
    }

    private void DrawButtons()
    {
        if (GUILayout.Button("New Waypoint"))
        {
            CreateWaypoint();
        }
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Waypoint>())
        {
            if (GUILayout.Button("Creater Waypoint Before this"))
            {
                CreateWaypointBefore();
            }
            if (GUILayout.Button("Create Waypoint After this"))
            {
                CreateWaypointAfter();
            }
            if (GUILayout.Button("Create Branch"))
            {
                CreateBranch();
            }
            if (GUILayout.Button("Delete Waypoint"))
            {
                DeleteWaypoint();
            }
        }
    }

    private void CreateWaypoint()
    {
        GameObject obj = new GameObject("Waypoint " + root.childCount, typeof(Waypoint));
        obj.transform.SetParent(root, false);

        Waypoint wp = obj.GetComponent<Waypoint>();
        if (root.childCount > 1)
        {
            wp.previous = root.GetChild(root.childCount - 2).GetComponent<Waypoint>();
            wp.previous.next = wp;
            // Place the waypoint at the last waypoint's position
            wp.transform.position = wp.previous.transform.position;
            wp.transform.forward = wp.previous.transform.forward;
        }

        Selection.activeGameObject = obj;
    }

    private void CreateWaypointBefore()
    {
        GameObject obj = new GameObject("Waypoint " + root.childCount, typeof(Waypoint));
        obj.transform.SetParent(root, false);

        Waypoint wp = obj.GetComponent<Waypoint>();
        Waypoint selected = Selection.activeGameObject.GetComponent<Waypoint>();

        obj.transform.position = selected.transform.position;
        obj.transform.forward = selected.transform.forward;

        if (selected.previous != null)
        {
            wp.previous = selected.previous;
            selected.previous.next = wp;
        }
        wp.next = selected;
        selected.previous = wp;

        wp.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());

        Selection.activeGameObject = obj;
    }

    private void CreateWaypointAfter()
    {
        GameObject obj = new GameObject("Waypoint " + root.childCount, typeof(Waypoint));
        obj.transform.SetParent(root, false);

        Waypoint wp = obj.GetComponent<Waypoint>();
        Waypoint selected = Selection.activeGameObject.GetComponent<Waypoint>();

        obj.transform.position = selected.transform.position;
        obj.transform.forward = selected.transform.forward;

        wp.previous = selected;

        if (selected.next != null)
        {
            selected.next.previous = wp;
            wp.next = selected.next;
        }

        selected.next = wp;

        wp.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());

        Selection.activeGameObject = obj;
    }

    private void CreateBranch()
    {
        GameObject obj = new GameObject("Waypoint " + root.childCount, typeof(Waypoint));
        obj.transform.SetParent(root, false);

        Waypoint wp = obj.GetComponent<Waypoint>();
        Waypoint selected = Selection.activeGameObject.GetComponent<Waypoint>();

        if (selected.branches == null)
            selected.branches = new List<Waypoint>();

        selected.branches.Add(wp);

        wp.transform.position = selected.transform.position;
        wp.transform.forward = selected.transform.forward;

        Selection.activeGameObject = obj;
    }

    private void DeleteWaypoint()
    {
        Waypoint selected = Selection.activeGameObject.GetComponent<Waypoint>();
        if (selected.next != null)
            selected.next.previous = selected.previous;
        if (selected.previous != null)
        {
            selected.previous.next = selected.next;
            Selection.activeGameObject = selected.previous.gameObject;
        }

        DestroyImmediate(selected.gameObject);
    }
}
