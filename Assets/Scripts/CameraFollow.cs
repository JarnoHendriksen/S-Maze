using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping;
    [SerializeField] private float cameraSize;
    [SerializeField] private Camera mainCamera;

    public Transform target;

    private Vector3 vel = Vector3.zero;

    private void FixedUpdate()
    {
        Vector3 targetPostion = target.position + offset;
        targetPostion.z = transform.position.z;
        mainCamera.orthographicSize = cameraSize;

        transform.position = Vector3.SmoothDamp(transform.position, targetPostion, ref vel, damping);
    }
}
