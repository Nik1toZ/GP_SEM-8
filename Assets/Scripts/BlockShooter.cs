using System.Collections.Generic;
using UnityEngine;

public class BlockShooter : MonoBehaviour
{
    [System.Serializable]
    public class BlockType
    {
        [Tooltip("Имя для отладки (Flat, Bouncy, Ramp)")]
        public string name = "Flat";

        [Tooltip("Префаб, который вылетает при выстреле этим типом")]
        public GameObject prefab;

        [Tooltip("Цвет дуги превью для этого типа")]
        public Color previewColor = Color.yellow;

        [Tooltip("Материал маркера, надевается при выборе этого типа.")]
        public Material markerMaterial;
    }

    [Header("Типы блоков")]
    public BlockType[] blockTypes;
    public int currentTypeIndex = 0;

    [Header("Инвентарь")]
    [Tooltip("Источник блоков — без него выстрелы будут бесплатными.")]
    public BlockInventory inventory;

    [Header("Выстрел")]
    public Transform muzzle;
    public float shootSpeed = 11f;
    [Range(0f, 60f)]
    public float launchAngle = 20f;
    public float shootCooldown = 0.15f;

    [Header("Превью траектории")]
    public LineRenderer trajectoryLine;
    public GameObject landingMarker;
    public int trajectorySamples = 25;
    public float trajectoryMaxTime = 1f;

    [Tooltip("Множитель прозрачности превью, когда у текущего типа закончились блоки")]
    [Range(0f, 1f)]
    public float emptyPreviewAlpha = 0.25f;

    [Header("Управление превью")]
    public KeyCode togglePreviewKey = KeyCode.T;
    public bool previewVisible = true;

    private float lastShootTime = -999f;
    private Renderer markerRenderer;
    private int lastAppliedTypeIndex = -1;

    private void Start()
    {
        if (landingMarker != null)
        {
            markerRenderer = landingMarker.GetComponent<Renderer>();
        }

        // Если ссылка на инвентарь не задана в Inspector — пытаемся найти
        // компонент на том же объекте (Player).
        if (inventory == null)
        {
            inventory = GetComponent<BlockInventory>();
        }
    }

    private void Update()
    {
        HandleTypeSwitch();

        if (Input.GetKeyDown(togglePreviewKey))
        {
            previewVisible = !previewVisible;
        }

        UpdateTrajectory();

        bool firePressed = Input.GetMouseButtonDown(0);
        bool ready = Time.time - lastShootTime >= shootCooldown;

        if (firePressed && ready)
        {
            if (TryShoot())
            {
                lastShootTime = Time.time;
            }
        }
    }

    private void HandleTypeSwitch()
    {
        for (int i = 0; i < blockTypes.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentTypeIndex = i;
            }
        }
    }

    private BlockType GetCurrent()
    {
        if (blockTypes == null || blockTypes.Length == 0) return null;
        currentTypeIndex = Mathf.Clamp(currentTypeIndex, 0, blockTypes.Length - 1);
        return blockTypes[currentTypeIndex];
    }

    private Vector3 ComputeLaunchVelocity()
    {
        Quaternion lift = Quaternion.AngleAxis(-launchAngle, transform.right);
        Vector3 direction = lift * transform.forward;
        return direction * shootSpeed;
    }

    /// <returns>true если выстрел действительно произошёл (был патрон).</returns>
    private bool TryShoot()
    {
        BlockType current = GetCurrent();
        if (current == null || current.prefab == null || muzzle == null) return false;

        // Проверяем и списываем блок из инвентаря. Если 0 — выстрела не будет.
        if (inventory != null && !inventory.TrySpend(currentTypeIndex)) return false;

        GameObject block = Instantiate(current.prefab, muzzle.position, transform.rotation);

        BlockProjectile bp = block.GetComponent<BlockProjectile>();
        if (bp != null)
        {
            // Привязываем блок к инвентарю, чтобы он смог вернуть патрон при автовозврате.
            bp.Initialize(currentTypeIndex, inventory);
        }

        Rigidbody rbBlock = block.GetComponent<Rigidbody>();
        if (rbBlock != null)
        {
            rbBlock.linearVelocity = ComputeLaunchVelocity();
        }

        return true;
    }

    private void UpdateTrajectory()
    {
        if (!previewVisible)
        {
            if (trajectoryLine != null) trajectoryLine.enabled = false;
            if (landingMarker != null) landingMarker.SetActive(false);
            return;
        }

        if (trajectoryLine == null || muzzle == null) return;
        trajectoryLine.enabled = true;

        BlockType current = GetCurrent();
        if (current != null)
        {
            // Цвет и прозрачность превью зависят от наличия блоков в инвентаре.
            bool hasAmmo = inventory == null || inventory.CanShoot(currentTypeIndex);
            Color c = current.previewColor;
            if (!hasAmmo) c.a *= emptyPreviewAlpha;

            trajectoryLine.startColor = c;
            trajectoryLine.endColor = c;

            if (markerRenderer != null
                && current.markerMaterial != null
                && currentTypeIndex != lastAppliedTypeIndex)
            {
                markerRenderer.material = current.markerMaterial;
                lastAppliedTypeIndex = currentTypeIndex;
            }
        }

        Vector3 startPos = muzzle.position;
        Vector3 startVel = ComputeLaunchVelocity();
        Vector3 g = Physics.gravity;

        List<Vector3> points = new List<Vector3>(trajectorySamples + 1);
        Vector3 prev = startPos;
        points.Add(prev);

        bool hitFound = false;
        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.up;

        for (int i = 1; i <= trajectorySamples; i++)
        {
            float t = trajectoryMaxTime * (i / (float)trajectorySamples);
            Vector3 next = startPos + startVel * t + 0.5f * g * t * t;

            Vector3 segment = next - prev;
            float segLen = segment.magnitude;

            if (segLen > 0.001f &&
                Physics.Raycast(prev, segment.normalized, out RaycastHit hit, segLen,
                    ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    points.Add(hit.point);
                    hitFound = true;
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    break;
                }
            }

            points.Add(next);
            prev = next;
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());

        if (landingMarker != null)
        {
            landingMarker.SetActive(hitFound);
            if (hitFound)
            {
                landingMarker.transform.position = hitPoint + hitNormal * 0.02f;
                landingMarker.transform.up = hitNormal;
            }
        }
    }
}