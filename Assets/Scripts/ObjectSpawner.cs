using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Behaviour")]
    [SerializeField] private int spawnAmount;

    [Header("References")]
    [SerializeField] private GameObject prefab;

    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            GameObject obj = Instantiate(prefab);
            Transform child = transform.GetChild(Random.Range(0, transform.childCount - 1));
            obj.GetComponent<NavigationController>().SetCurrentWaypoint(child.GetComponent<Waypoint>());
            obj.transform.position = child.position;

            yield return new WaitForEndOfFrame();
        }
    }
}
