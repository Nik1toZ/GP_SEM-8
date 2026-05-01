using UnityEngine;

/// <summary>
/// Наклонный блок: при приземлении автоматически становится "горкой" —
/// поднимается под фиксированным углом по направлению движения игрока.
/// Удобно, чтобы перепрыгивать препятствия с разгона.
/// </summary>
public class RampBlock : BlockProjectile
{
    [Tooltip("Угол наклона горки от горизонтали (градусы)")]
    [Range(5f, 60f)]
    public float rampAngle = 40f;

    [Tooltip("Куда смотрит подъём горки (обычно совпадает с forward игрока)")]
    public Vector3 forwardAxis = Vector3.forward;

    protected override void Land(Collision collision)
    {
        // Сохраняем XZ-направление, заданное при выстреле (transform.rotation
        // мы передавали из BlockShooter — это поворот игрока в момент выстрела).
        // Из него берём только yaw (поворот вокруг Y), чтобы горка смотрела вперёд.
        float yaw = transform.eulerAngles.y;

        // Поворачиваем блок: по yaw сохраняем направление, добавляем pitch вверх.
        // Pitch отрицательный, потому что в Unity положительный X-поворот наклоняет нос вниз.
        transform.rotation = Quaternion.Euler(-rampAngle, yaw, 0f);

        // Дальше — стандартное приземление.
        base.Land(collision);
    }

    protected override void Awake()
    {
        base.Awake();

        // Наклонный блок изначально допускает большой "наклон" — это его суть.
        // Иначе CheckStability сразу отбракует его как неустойчивый.
        maxTiltAngle = Mathf.Max(maxTiltAngle, rampAngle + 10f);
    }
}