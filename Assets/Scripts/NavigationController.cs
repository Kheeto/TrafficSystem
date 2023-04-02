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
    [SerializeField] private float motorForce = 100f;
    [SerializeField] private float motorForceVariation = 5f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float rotationSpeed = 600f;
    [SerializeField] private float stopDistance = 1f;
    [SerializeField] private Waypoint startWaypoint;
    [SerializeField] private bool enableDirection = false;
    private float currentMotorforce;

    [Header("Crash Handling")]
    [Tooltip("The car will try to keep a minimum distance from other cars in front of it.")]
    [SerializeField] private float minimumDistance = 4f;

    [Tooltip("The speed the car will move at when it is too close to another car." +
        "(If the car in front is slower than this, this value will be adjusted accordingly)")]
    [SerializeField] private float slowMotorForce = 60f;

    // Spherecast radius when checking for other cars
    [SerializeField] private float castRadius = 1f;

    [Tooltip("The LayerMask of all the objects the car will try to avoid crashing into.")]
    [SerializeField] private LayerMask crashMask;

    private Waypoint currentWaypoint;
    private Direction direction;

    private bool reachedDestination;
    private Vector3 destination;

    private Rigidbody rb;

    public bool shouldAddForce, crashIncoming;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        motorForce = Random.Range(motorForce - motorForceVariation, motorForce + motorForceVariation);
        currentMotorforce = motorForce;

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
        shouldAddForce = true;
        crashIncoming = false;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, castRadius, transform.forward, out hit, minimumDistance, crashMask))
        {
            crashIncoming = true;
            Rigidbody r = hit.collider.gameObject.GetComponent<Rigidbody>();
            if (r != null && rb.velocity.magnitude > r.velocity.magnitude)
            {
                // There is a slower rigidbody in front of this car and it's too close
                Vector3 diff = rb.velocity - r.velocity;
                rb.AddForce(-diff * 1.1f); // Slows down with a force greater than the velocity difference
                shouldAddForce = false;
            }
            else
            {
                // The car isn't slower but still too close, so we slow down to a safe speed
                currentMotorforce = Mathf.Lerp(currentMotorforce, slowMotorForce, acceleration);
            }
        }
        else
            currentMotorforce = Mathf.Lerp(currentMotorforce, motorForce, acceleration);

        if (shouldAddForce) rb.AddForce(transform.forward * motorForce);
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
