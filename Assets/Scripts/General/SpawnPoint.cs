using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string targetScene;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}