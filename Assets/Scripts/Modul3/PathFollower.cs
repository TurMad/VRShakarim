using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [SerializeField] private bool autoStart = true;
    [SerializeField] private Transform targetToMove;
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private float moveSpeed = 0.35f;
    [SerializeField] private float rotateSpeed = 2f;
    [SerializeField] private float reachDistance = 0.05f;

    private int currentIndex;
    private bool isMoving;

    private void OnEnable()
    {
        if (targetToMove == null)
            targetToMove = transform;

        if (autoStart)
            isMoving = true;
    }

    private void Update()
    {
        if (!isMoving || pathPoints == null || pathPoints.Length == 0 || currentIndex >= pathPoints.Length)
            return;

        Transform targetPoint = pathPoints[currentIndex];
        if (targetPoint == null)
        {
            currentIndex++;
            return;
        }

        Vector3 targetPos = targetPoint.position;
        Vector3 direction = targetPos - targetToMove.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
            targetToMove.rotation = Quaternion.Slerp(targetToMove.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        targetToMove.position = Vector3.MoveTowards(targetToMove.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(targetToMove.position, targetPos) <= reachDistance)
            currentIndex++;
    }

    public void PlayMove()
    {
        isMoving = true;
    }

    public void StopMove()
    {
        isMoving = false;
    }

    public void SetAutoStart(bool value)
    {
        autoStart = value;
        isMoving = value;
    }
}