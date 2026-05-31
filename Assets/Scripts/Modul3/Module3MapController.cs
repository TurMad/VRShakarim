using UnityEngine;

public class Module3MapController : MonoBehaviour
{
    [SerializeField] private Module3MapPoint[] pointsInOrder;

    private int currentIndex = -1;

    private void Awake()
    {
        for (int i = 0; i < pointsInOrder.Length; i++)
        {
            if (pointsInOrder[i] != null)
                pointsInOrder[i].SetAvailable(false);
        }
    }

    public void BeginMapSequence()
    {
        currentIndex = 0;

        if (pointsInOrder.Length > 0 && pointsInOrder[0] != null)
            pointsInOrder[0].SetAvailable(true);
    }

    public void NotifyPointFinished(Module3MapPoint finishedPoint)
    {
        int finishedIndex = System.Array.IndexOf(pointsInOrder, finishedPoint);
        if (finishedIndex < 0)
            return;

        if (pointsInOrder[finishedIndex] != null)
            pointsInOrder[finishedIndex].SetAvailable(false);

        int nextIndex = finishedIndex + 1;
        if (nextIndex < pointsInOrder.Length && pointsInOrder[nextIndex] != null)
        {
            currentIndex = nextIndex;
            pointsInOrder[nextIndex].SetAvailable(true);
        }
        else
        {
            currentIndex = -1;
        }
    }
}