using UnityEngine;

public class LocalLineMover : MonoBehaviour
{
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float speed = 0.6f;
    [SerializeField] private Vector3 localDirection = new Vector3(0f, 0f, 1f);

    private bool isMoving;

    private void OnEnable()
    {
        if (autoStart)
            isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        transform.Translate(localDirection.normalized * speed * Time.deltaTime, Space.Self);
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