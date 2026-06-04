using UnityEngine;

public class KaabaPathMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetToMove;

    [Header("Path")]
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private bool loopPath = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.8f;
    [SerializeField] private float reachDistance = 0.08f;

    [Header("Rotation")]
    [SerializeField] private bool rotateTowardsPath = true;
    [SerializeField] private float rotationSpeed = 4f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmosAlways = true;
    [SerializeField] private Color pointColor = Color.yellow;
    [SerializeField] private Color lineColor = Color.green;
    [SerializeField] private float pointRadius = 0.25f;

    private int currentPointIndex;
    private int completedLaps;
    private bool canMove;
    private bool manualMoveInput;
    private bool autoMove;

    public int CompletedLaps => completedLaps;
    public int CurrentPointIndex => currentPointIndex;
    public bool IsMovingNow => canMove && (manualMoveInput || autoMove);

    private void Awake()
    {
        if (targetToMove == null)
            targetToMove = transform;
    }

    private void Update()
    {
        if (!IsMovingNow)
            return;

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        if (currentPointIndex >= pathPoints.Length)
            currentPointIndex = 0;

        Transform point = pathPoints[currentPointIndex];

        if (point == null)
        {
            AdvancePoint();
            return;
        }

        Vector3 targetPosition = point.position;
        Vector3 direction = targetPosition - targetToMove.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            if (rotateTowardsPath)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                targetToMove.rotation = Quaternion.Slerp(
                    targetToMove.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            targetToMove.position = Vector3.MoveTowards(
                targetToMove.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        if (Vector3.Distance(targetToMove.position, targetPosition) <= reachDistance)
            AdvancePoint();
    }

    private void AdvancePoint()
    {
        currentPointIndex++;

        if (currentPointIndex >= pathPoints.Length)
        {
            if (loopPath)
            {
                currentPointIndex = 0;
                completedLaps++;
            }
            else
            {
                currentPointIndex = pathPoints.Length - 1;
                canMove = false;
            }
        }
    }

    public void JumpToNextPoint()
    {
        if (!canMove || pathPoints == null || pathPoints.Length == 0)
            return;

        if (currentPointIndex >= pathPoints.Length)
            currentPointIndex = 0;

        Transform point = pathPoints[currentPointIndex];

        if (point != null && targetToMove != null)
        {
            targetToMove.position = point.position;

            if (rotateTowardsPath)
            {
                int nextIndex = currentPointIndex + 1;

                if (nextIndex >= pathPoints.Length)
                    nextIndex = loopPath ? 0 : pathPoints.Length - 1;

                if (pathPoints[nextIndex] != null)
                {
                    Vector3 direction = pathPoints[nextIndex].position - targetToMove.position;
                    direction.y = 0f;

                    if (direction.sqrMagnitude > 0.0001f)
                        targetToMove.rotation = Quaternion.LookRotation(direction.normalized);
                }
            }
        }

        AdvancePoint();
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
        manualMoveInput = false;
        autoMove = false;
    }

    public void SetManualMoveInput(bool value)
    {
        manualMoveInput = value;
    }

    public void SetAutoMove(bool value)
    {
        autoMove = value;
    }

    public void ResetPath()
    {
        currentPointIndex = 0;
        completedLaps = 0;
        manualMoveInput = false;
        autoMove = false;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmosAlways)
            return;

        DrawPathGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawPathGizmos();
    }

    private void DrawPathGizmos()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        Gizmos.color = pointColor;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
                continue;

            Gizmos.DrawSphere(pathPoints[i].position, pointRadius);
        }

        Gizmos.color = lineColor;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
                continue;

            int nextIndex = i + 1;

            if (nextIndex >= pathPoints.Length)
            {
                if (!loopPath)
                    continue;

                nextIndex = 0;
            }

            if (pathPoints[nextIndex] != null)
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[nextIndex].position);
        }
    }
}