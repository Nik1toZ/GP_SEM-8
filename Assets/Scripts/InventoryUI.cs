using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Обновляет визуальный счётчик блоков и подсвечивает активный тип.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Tooltip("Источник данных")]
    public BlockInventory inventory;

    [Tooltip("Используется только для подсветки активного слота")]
    public BlockShooter shooter;

    [Tooltip("Текстовые счётчики по индексу типа (0=Flat, 1=Bouncy, 2=Ramp)")]
    public Text[] countTexts;

    [Tooltip("Фоновые Image слотов по тому же индексу — для подсветки активного")]
    public Image[] slotBackgrounds;

    [Tooltip("Цвет фона активного слота (полная яркость)")]
    public float activeAlpha = 1f;

    [Tooltip("Цвет фона неактивных слотов (приглушённые)")]
    public float inactiveAlpha = 0.45f;

    private void Start()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateCounts;
        }
        UpdateCounts();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateCounts;
        }
    }

    private void Update()
    {
        // Подсветка активного слота — каждый кадр, потому что переключение
        // типа происходит без события.
        UpdateActiveHighlight();
    }

    private void UpdateCounts()
    {
        if (inventory == null || countTexts == null) return;

        for (int i = 0; i < countTexts.Length; i++)
        {
            if (countTexts[i] == null) continue;
            countTexts[i].text = inventory.GetCount(i).ToString();
        }
    }

    private void UpdateActiveHighlight()
    {
        if (shooter == null || slotBackgrounds == null) return;

        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            Image bg = slotBackgrounds[i];
            if (bg == null) continue;

            float a = (i == shooter.currentTypeIndex) ? activeAlpha : inactiveAlpha;
            Color c = bg.color;
            c.a = a;
            bg.color = c;
        }
    }
}