using UnityEngine;

/// <summary>
/// Невидимая зона смерти под трассой. Следует за игроком по Z, чтобы
/// бесконечный раннер всегда имел триггер ниже текущей позиции.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DeathTrigger : MonoBehaviour
{
    [Tooltip("За кем следим (по Z-координате)")]
    public Transform player;

    [Tooltip("Глубина зоны под трассой. Игрок должен успеть свободно падать " +
             "достаточно долго, чтобы это ощущалось как падение, а не мгновенная смерть.")]
    public float depthBelow = 8f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (player == null) return;

        // Сдвигаем зону по Z вместе с игроком, держим Y фиксированным ниже трассы.
        Vector3 pos = transform.position;
        pos.z = player.position.z;
        pos.y = -depthBelow;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.GameOver();
    }
}