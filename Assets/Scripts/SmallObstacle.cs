using UnityEngine;

public class SmallObstacle : MonoBehaviour
{
    public int hitsRequired = 2;

    private int currentHits = 0;

    public void HitObstacle()
    {
        currentHits++;

        Debug.Log(name + " cut!");

        if (currentHits >= hitsRequired)
        {
            Destroy(gameObject);
        }
    }
}