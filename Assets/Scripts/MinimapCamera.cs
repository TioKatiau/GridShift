using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );
    }
}