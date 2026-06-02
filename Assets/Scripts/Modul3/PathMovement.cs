using UnityEngine;

public class PathMovement : MonoBehaviour
{
    [Header("Настройки пути")]
    public Transform[] pathPoints;
    public bool loopPath = true;

    [Header("Настройки движения")]
    public float speed = 5f;
    public float reachDistance = 0.5f;
    public bool autoStart = true;

    [Header("Настройки поворота")]
    public bool rotateTowardsPath = true;
    public float rotationSpeed = 10f;

    private int currentPointIndex = 0;
    private bool isMoving = true;
    private int completedLaps = 0;

    public int CurrentPointIndex => currentPointIndex;
    public int CompletedLaps => completedLaps;
    public bool IsMoving => isMoving;

    void Start()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("PathMovement: Добавьте точки пути в инспектор!");
            isMoving = false;
            return;
        }

        isMoving = autoStart;
    }

    void Update()
    {
        if (!isMoving || pathPoints == null || pathPoints.Length == 0)
            return;

        if (currentPointIndex >= pathPoints.Length)
        {
            if (loopPath)
            {
                currentPointIndex = 0;
                completedLaps++;
            }
            else
            {
                isMoving = false;
                return;
            }
        }

        Transform targetPoint = pathPoints[currentPointIndex];
        if (targetPoint == null)
        {
            currentPointIndex++;
            return;
        }

        Vector3 direction = targetPoint.position - transform.position;
        direction.y = 0f;

        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (distance > reachDistance)
        {
            transform.position += direction.normalized * speed * Time.deltaTime;

            if (rotateTowardsPath && direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentPointIndex++;
        }
    }

    public void PlayMove()
    {
        isMoving = true;
    }

    public void StopMove()
    {
        isMoving = false;
    }

    public void ResetPathState()
    {
        currentPointIndex = 0;
        completedLaps = 0;
    }

    public void SetAutoStart(bool value)
    {
        autoStart = value;
        isMoving = value;
    }

    void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] != null)
            {
                Gizmos.DrawSphere(pathPoints[i].position, 0.3f);

                int nextIndex = (i + 1) % pathPoints.Length;
                if (nextIndex < pathPoints.Length && pathPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(pathPoints[i].position, pathPoints[nextIndex].position);
                }
            }
        }
    }
}