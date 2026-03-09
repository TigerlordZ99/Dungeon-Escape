using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public float zoomSize = 8f; // Higher = more zoomed out

    void Start()
    {
        Camera.main.orthographicSize = zoomSize;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            // Automatically find player if not assigned
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            return;
        }

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}