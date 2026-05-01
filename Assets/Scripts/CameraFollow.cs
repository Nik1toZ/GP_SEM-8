using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("За кем следим (перетащить Player сюда)")]
    public Transform target;

    [Tooltip("Смещение камеры относительно цели")]
    public Vector3 offset = new Vector3(0f, 4f, -7f);

    [Tooltip("Чем больше — тем плавнее (и ленивее) следует камера")]
    public float smoothTime = 0.15f;

    [Tooltip("На какую высоту над целью смотреть")]
    public float lookAtHeightOffset = 1.5f;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // SmoothDamp плавно подтягивает камеру к нужной точке без рывков.
        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref currentVelocity,
            smoothTime
        );

        // Камера всегда смотрит чуть выше центра игрока.
        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);
    }
}