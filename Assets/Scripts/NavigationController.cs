using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NavigationController : MonoBehaviour
{
    private enum Direction
    {
        FORWARD,
        BACKWARDS
    }

    [Header("Navigation")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float speedVariation = .1f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float stopDistance = 1f;
    [SerializeField] private Waypoint startWaypoint;
    [SerializeField] private bool enableDirection = false;

    private Waypoint currentWaypoint;
    private Direction direction;

    private bool reachedDestination;
    private Vector3 destination;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        speed = Random.Range(speed - speedVariation, speed + speedVariation);
        if (enableDirection)
            direction = (Direction)Mathf.RoundToInt(Random.Range(0f, 1f));
        else
            direction = Direction.FORWARD;

        if (startWaypoint != null)
            currentWaypoint = startWaypoint;

        if (currentWaypoint != null)
            SetDestination(currentWaypoint.GetPosition());
    }

    private void Update()
    {
        reachedDestination = false;

        if (destination != null && transform.position != destination)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0;

            float distance = direction.magnitude;
            if (distance >= stopDistance)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot,
                    rotationSpeed * Time.deltaTime));
            }
            else reachedDestination = true;
        }

        if (reachedDestination)
        {
            if (currentWaypoint == null) return;

            bool shouldBranch = false;
            if (currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
            {
                if ((direction == Direction.FORWARD && currentWaypoint.next == null) ||
                    (direction == Direction.BACKWARDS && currentWaypoint.previous == null))
                    shouldBranch = true;
                else
                    shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchRatio ? true : false;
            }
            if (shouldBranch)
                currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
            else
            {
                if (direction == Direction.FORWARD)
                {
                    if (currentWaypoint.next != null)
                        currentWaypoint = currentWaypoint.next;
                    else
                    {
                        currentWaypoint = currentWaypoint.previous;
                        direction = Direction.BACKWARDS;
                    }
                }
                else
                {
                    if (currentWaypoint.previous != null)
                        currentWaypoint = currentWaypoint.previous;
                    else
                    {
                        currentWaypoint = currentWaypoint.next;
                        direction = Direction.FORWARD;
                    }
                }
            }

            SetDestination(currentWaypoint.GetPosition());
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * speed);
    }

    private void SetDestination(Vector3 destination)
    {
        this.destination = destination;
        reachedDestination = false;
    }

    public void SetCurrentWaypoint(Waypoint wp)
    {
        currentWaypoint = wp;
        SetDestination(currentWaypoint.GetPosition());
    }
}
