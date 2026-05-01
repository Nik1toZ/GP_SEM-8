using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [Header("Префабы сегментов")]
    [Tooltip("Безопасный прямой сегмент для старта и в качестве запасного варианта")]
    public GameObject straightPrefab;

    [Tooltip("Случайно выбираемые сегменты — после стартового разгона")]
    public GameObject[] randomPrefabs;

    [Header("Параметры")]
    [Tooltip("Длина одного сегмента по оси Z. Все наши сегменты = 20.")]
    public float segmentLength = 20f;

    [Tooltip("Сколько простых сегментов гарантированно идут в начале")]
    public int safeStartSegments = 5;

    [Tooltip("Сколько сегментов держим перед игроком")]
    public int segmentsAhead = 6;

    [Tooltip("Сколько сегментов оставляем позади, прежде чем удалить")]
    public int segmentsBehind = 2;

    [Header("Цель")]
    [Tooltip("Игрок, за которым следим")]
    public Transform player;

    // Очередь активных сегментов: первый = самый дальний позади.
    private readonly Queue<GameObject> active = new Queue<GameObject>();

    // Z-координата правого края последнего заспавненного сегмента.
    private float nextSpawnZ;

    // Общее число уже заспавненных сегментов (для safeStart).
    private int spawnedCount;

    private void Start()
    {
        // Начинаем спавн чуть позади игрока, чтобы было на чём стоять.
        nextSpawnZ = player.position.z - segmentLength;

        for (int i = 0; i < segmentsAhead; i++)
        {
            SpawnNext();
        }
    }

    private void Update()
    {
        // Передний край последнего сегмента.
        float frontEdge = nextSpawnZ;

        // Если игрок подошёл слишком близко к переднему краю — досыпаем.
        // Условие: расстояние от игрока до края меньше "запаса" в N сегментов.
        if (frontEdge - player.position.z < segmentLength * segmentsAhead)
        {
            SpawnNext();
            CleanupBehind();
        }
    }

    private void SpawnNext()
    {
        GameObject prefab = ChoosePrefab();

        // Центр сегмента смещён на полдлины вперёд от nextSpawnZ.
        Vector3 pos = new Vector3(0f, 0f, nextSpawnZ + segmentLength * 0.5f);
        GameObject segment = Instantiate(prefab, pos, Quaternion.identity, transform);

        active.Enqueue(segment);
        nextSpawnZ += segmentLength;
        spawnedCount++;
    }

    private GameObject ChoosePrefab()
    {
        // Первые N сегментов — всегда прямые, чтобы игрок успел освоиться.
        if (spawnedCount < safeStartSegments) return straightPrefab;

        // Если случайных нет — используем прямые.
        if (randomPrefabs == null || randomPrefabs.Length == 0) return straightPrefab;

        return randomPrefabs[Random.Range(0, randomPrefabs.Length)];
    }

    private void CleanupBehind()
    {
        // Пока в очереди больше сегментов, чем мы хотим суммарно держать —
        // удаляем самый старый (он гарантированно позади).
        int maxAlive = segmentsAhead + segmentsBehind;
        while (active.Count > maxAlive)
        {
            GameObject old = active.Dequeue();
            if (old != null) Destroy(old);
        }
    }
}