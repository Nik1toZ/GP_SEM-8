using UnityEngine;

/// <summary>
/// Пружинный блок: при наступании на него игрок получает мощный импульс вверх.
/// Используется, чтобы перепрыгивать вертикальные стенки или просто получать высоту.
/// </summary>
public class BouncyBlock : BlockProjectile
{
    [Header("Пружина")]
    [Tooltip("Сила, с которой блок подкидывает игрока вверх")]
    public float bounceForce = 14f;

    [Tooltip("Минимальный интервал между подбрасываниями (защита от двойного срабатывания)")]
    public float bounceCooldown = 0.2f;

    [Tooltip("Через сколько секунд после приземления пружина становится активной. " +
             "Защита от мгновенного отскока в момент стрельбы рядом с игроком.")]
    public float armDelayAfterLanding = 0.15f;

    [Tooltip("Игнорировать столкновения с игроком в первые N секунд жизни блока. " +
             "Защита от 'катапульты' прямо в полёте, если блок задевает капсулу.")]
    public float ignorePlayerOnLaunchTime = 0.3f;

    private float lastBounceTime = -999f;
    private float spawnTime;
    private float landTime = -1f;

    protected override void Awake()
    {
        base.Awake();
        spawnTime = Time.time;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        // В первые мгновения жизни — игнорируем игрока полностью.
        // Блок не должен взаимодействовать с капсулой, пока он рядом с muzzle.
        if (collision.gameObject.CompareTag("Player")
            && Time.time - spawnTime < ignorePlayerOnLaunchTime)
        {
            return;
        }

        // Передаём дальше для базовой логики приземления (если врезался не в игрока).
        base.OnCollisionEnter(collision);

        // Если врезался в игрока ПОСЛЕ окна защиты — это уже легитимный отскок
        // (например, игрок прыгнул на блок сверху до приземления блока). Подбрасываем.
        if (collision.gameObject.CompareTag("Player") && IsArmed())
        {
            TryBouncePlayer(collision.rigidbody);
        }
    }

    protected override void Land(Collision collision)
    {
        base.Land(collision);
        landTime = Time.time;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Игрок касается блока И блок уже приземлился И прошло достаточно времени
        // после приземления — теперь можно подбрасывать.
        if (!IsArmed()) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            TryBouncePlayer(collision.rigidbody);
        }
    }

    /// <summary>
    /// "Готов ли" блок к подбрасыванию — приземлился ли и истекла ли задержка активации.
    /// </summary>
    private bool IsArmed()
    {
        return hasLanded && landTime > 0f && (Time.time - landTime) >= armDelayAfterLanding;
    }

    private void TryBouncePlayer(Rigidbody playerRb)
    {
        if (playerRb == null) return;
        if (Time.time - lastBounceTime < bounceCooldown) return;

        Vector3 v = playerRb.linearVelocity;
        v.y = bounceForce;
        playerRb.linearVelocity = v;

        lastBounceTime = Time.time;
    }
}