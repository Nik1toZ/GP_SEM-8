using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [Tooltip("Постоянная скорость автобега вперёд")]
    public float forwardSpeed = 8f;

    [Tooltip("Скорость движения влево-вправо")]
    public float lateralSpeed = 6f;

    [Tooltip("Сила прыжка (начальная вертикальная скорость)")]
    public float jumpForce = 10f;

    [Tooltip("Дополнительная гравитация поверх стандартной. " +
             "Делает прыжок более резким и коротким — нужно для аркадного платформинга.")]
    public float extraGravity = 12f;

    [Header("Проверка земли")]
    [Tooltip("Дистанция луча вниз для проверки, стоит ли игрок на земле")]
    public float groundCheckDistance = 1.1f;

    [Tooltip("Слои, считающиеся землёй (по умолчанию — все)")]
    public LayerMask groundMask = ~0;

    [Header("Защита от застревания")]
    [Tooltip("Если игрок не продвинулся вперёд (по Z) на эту дистанцию " +
             "за время stuckTimeLimit — засчитываем как смерть.")]
    public float stuckMinProgress = 0.5f;

    [Tooltip("Через сколько секунд застревания считать игрока мёртвым")]
    public float stuckTimeLimit = 1.5f;

    private Rigidbody rb;
    private bool isGrounded;

    private float stuckTimer;
    private float lastProgressZ;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        lastProgressZ = transform.position.z;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundMask
        );

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;
        }

        // Stuck-детектор: критерий — продвижение по Z. Не важно, на земле игрок,
        // в полёте, или висит упёршись в стенку. Если Z не растёт достаточно
        // долго — это тупик.
        float currentZ = transform.position.z;
        if (currentZ - lastProgressZ >= stuckMinProgress)
        {
            lastProgressZ = currentZ;
            stuckTimer = 0f;
        }
        else
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeLimit && GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 velocity = rb.linearVelocity;
        velocity.x = horizontal * lateralSpeed;
        velocity.z = forwardSpeed;
        rb.linearVelocity = velocity;

        // Дополнительная гравитация. Приложенная как сила со SmartScale-режимом
        // VelocityChange — она напрямую модифицирует скорость, не зависит от массы,
        // и даёт предсказуемый результат.
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }
}