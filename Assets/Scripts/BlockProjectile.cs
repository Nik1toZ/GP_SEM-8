using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlockProjectile : MonoBehaviour
{
    [Header("Проверка устойчивости")]
    [Tooltip("Через сколько секунд после приземления проверять устойчивость")]
    public float settleCheckDelay = 0.2f;

    [Tooltip("Максимально допустимый угол наклона блока от горизонтали (градусы)")]
    public float maxTiltAngle = 35f;

    [Tooltip("На какой дистанции вниз искать опору под блоком")]
    public float supportCheckDistance = 0.4f;

    [Tooltip("Через сколько секунд исчезнуть, если блок неустойчив")]
    public float unstableLifetime = 2f;

    [Header("Подсветка неустойчивого блока")]
    [Tooltip("Цвет, в который окрасится блок, если приземлился криво")]
    public Color unstableColor = new Color(1f, 0.3f, 0.3f);

    [Header("Автовозврат")]
    [Tooltip("На сколько метров позади игрока блок должен оказаться, " +
             "чтобы запустился таймер возврата")]
    public float behindDistanceThreshold = 30f;

    [Tooltip("Сколько секунд блок должен находиться позади порога перед возвратом")]
    public float refundDelay = 5f;

    [Tooltip("Если блок упал ниже этой Y-координаты, он считается потерянным " +
             "(улетел в пропасть) и удаляется БЕЗ возврата в инвентарь")]
    public float lostBelowY = -50f;

    protected Rigidbody rb;
    protected bool hasLanded;

    // Источник блока — заполняется через Initialize() сразу после Instantiate.
    private int sourceTypeIndex = -1;
    private BlockInventory sourceInventory;

    // Кэш коллайдеров для управления Physics.IgnoreCollision.
    private Collider[] myColliders;
    private Collider playerCollider;
    private Transform playerTransform;

    // Таймер ожидания возврата — копится только когда блок реально позади.
    private float behindTimer = 0f;
    private bool refundScheduled = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        myColliders = GetComponentsInChildren<Collider>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerCollider = player.GetComponent<Collider>();
            SetIgnorePlayerCollision(true);
        }
    }

    /// <summary>
    /// Вызывается из BlockShooter сразу после Instantiate. Связывает блок
    /// с инвентарём, чтобы автовозврат мог вернуть "патрон" в правильный слот.
    /// </summary>
    public void Initialize(int typeIndex, BlockInventory inventory)
    {
        sourceTypeIndex = typeIndex;
        sourceInventory = inventory;
    }

    private void Update()
    {
        // 1) Блок улетел в пропасть — потерян, инвентарь не пополняем.
        if (transform.position.y < lostBelowY)
        {
            Destroy(gameObject);
            return;
        }

        // 2) Логика автовозврата работает только после приземления.
        if (!hasLanded || playerTransform == null || refundScheduled) return;

        // Z-разница: положительная = блок позади игрока (игрок ушёл вперёд).
        float zBehind = playerTransform.position.z - transform.position.z;

        if (zBehind > behindDistanceThreshold)
        {
            behindTimer += Time.deltaTime;
            if (behindTimer >= refundDelay)
            {
                refundScheduled = true;
                if (sourceInventory != null && sourceTypeIndex >= 0)
                {
                    sourceInventory.Refund(sourceTypeIndex);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            // Игрок вернулся / блок ещё не достаточно позади — обнуляем таймер.
            behindTimer = 0f;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        if (collision.gameObject.CompareTag("Player")) return;

        Land(collision);
    }

    /// <summary>
    /// Вызывается в момент первого приземления блока. Базовая реализация
    /// "замораживает" блок на месте. Наследники могут переопределить.
    /// </summary>
    protected virtual void Land(Collision collision)
    {
        hasLanded = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        SetIgnorePlayerCollision(false);

        Invoke(nameof(CheckStability), settleCheckDelay);
    }

    private void CheckStability()
    {
        float tilt = Vector3.Angle(transform.up, Vector3.up);
        bool tooTilted = tilt > maxTiltAngle;

        Vector3 origin = transform.position;
        bool hasSupport = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit _,
            supportCheckDistance + 0.25f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (tooTilted || !hasSupport)
        {
            HighlightUnstable();
            // Неустойчивый блок умирает молча, патрон не возвращается —
            // игрок наказан за плохой бросок.
            Destroy(gameObject, unstableLifetime);
            refundScheduled = true;
        }
    }

    private void HighlightUnstable()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = unstableColor;
        }
    }

    private void SetIgnorePlayerCollision(bool ignore)
    {
        if (playerCollider == null || myColliders == null) return;

        foreach (var col in myColliders)
        {
            if (col != null)
            {
                Physics.IgnoreCollision(col, playerCollider, ignore);
            }
        }
    }
}