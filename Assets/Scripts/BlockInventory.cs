using System;
using UnityEngine;

/// <summary>
/// Хранит количество блоков каждого типа у игрока.
/// Единственный источник правды о ресурсах — BlockShooter и UI обращаются сюда.
/// </summary>
public class BlockInventory : MonoBehaviour
{
    [Tooltip("Стартовое количество блоков по индексу типа. " +
             "Длина массива должна совпадать с количеством типов в BlockShooter.")]
    public int[] startingCounts = new int[] { 3, 3, 3 };

    /// <summary>Подписчики (UI) обновляются при любом изменении.</summary>
    public event Action OnInventoryChanged;

    private int[] counts;

    private void Awake()
    {
        // Делаем независимую копию массива, чтобы изменения в Inspector в Play-режиме
        // не сбивали внутреннее состояние и наоборот.
        counts = new int[startingCounts.Length];
        for (int i = 0; i < startingCounts.Length; i++)
        {
            counts[i] = startingCounts[i];
        }
    }

    public int GetCount(int typeIndex)
    {
        if (counts == null || typeIndex < 0 || typeIndex >= counts.Length) return 0;
        return counts[typeIndex];
    }

    public int GetTypeCount()
    {
        return counts != null ? counts.Length : 0;
    }

    public bool CanShoot(int typeIndex)
    {
        return GetCount(typeIndex) > 0;
    }

    /// <summary>
    /// Списывает один блок указанного типа. Возвращает false, если блоков нет —
    /// в этом случае состояние не меняется.
    /// </summary>
    public bool TrySpend(int typeIndex)
    {
        if (!CanShoot(typeIndex)) return false;
        counts[typeIndex]--;
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Возвращает блок в инвентарь (например, при автовозврате далёкого блока).
    /// </summary>
    public void Refund(int typeIndex)
    {
        if (counts == null || typeIndex < 0 || typeIndex >= counts.Length) return;
        counts[typeIndex]++;
        OnInventoryChanged?.Invoke();
    }
}