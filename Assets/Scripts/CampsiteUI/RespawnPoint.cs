using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static Vector3 LastCheckpoint;

    private void Start()
    {
        LastCheckpoint = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LastCheckpoint = transform.position;
            Debug.Log("Checkpoint Saved");
        }
    }
}