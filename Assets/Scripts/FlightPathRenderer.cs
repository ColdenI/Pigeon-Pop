using UnityEngine;

[RequireComponent(typeof(FlightPath))]
public class FlightPathRenderer : MonoBehaviour
{
    [Header("Пунктирная линия")]
    public Color pathColor = Color.gray;
    [Min(0.01f)] public float dashLength = 0.3f;
    [Min(0.01f)] public float gapLength = 0.2f;
    [Range(0.01f, 0.3f)] public float lineWidth = 0.08f;
    [Tooltip("Слой отрисовки — должен быть ниже птицы и UI")]

    [Header("Стрелки направления")]
    public GameObject arrowPrefab; // Префаб стрелки
    [Min(0.1f)] public float arrowSpacing = 1.5f; // Расстояние между стрелками
    [Min(0.1f)] public float arrowScale = 1f; // Масштаб стрелок
    [Tooltip("Слой для стрелок (должен быть выше линии, но ниже/выше по желанию)")]
    public int arrowSortingOrder = 0;

    private FlightPath flightPath;
    private LineRenderer lineRenderer;
    private Transform[] arrowInstances;

    void Start()
    {
        flightPath = GetComponent<FlightPath>();

        if (flightPath.lengths.Count == 0)
        {
            flightPath.CachePathLength();
        }

        CreateDashedLine();
        CreateArrows();
    }

    void Update()
    {
        UpdateDashedLine();
        UpdateArrows();
    }

    void CreateDashedLine()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Настройка материала
        var material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = material;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;

        // 🔽 Важно: чтобы линия была ПОД птицей и стрелками
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 8;

        // Цвет с прозрачностью
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(pathColor, 0), new GradientColorKey(pathColor, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(0.5f, 1) }
        );
        lineRenderer.colorGradient = gradient;

        // Количество сегментов: dash + gap
        int dashCount = Mathf.CeilToInt(flightPath.totalLength / (dashLength + gapLength));
        lineRenderer.positionCount = dashCount * 2;

        // Инициализируем позиции
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, Vector3.zero);
        }

        // Сразу обновляем, чтобы не было прыжков
        UpdateDashedLine();
    }

    void UpdateDashedLine()
    {
        if (lineRenderer == null || flightPath.totalLength <= 0) return;

        float accumulatedLength = 0f;
        int segmentIndex = 0;
        int maxSegments = lineRenderer.positionCount / 2;

        while (segmentIndex < maxSegments)
        {
            float startProgress = accumulatedLength / flightPath.totalLength;
            float endProgress = (accumulatedLength + dashLength) / flightPath.totalLength;
            endProgress = Mathf.Clamp01(endProgress);

            Vector3 startPos = flightPath.GetPositionAt(startProgress);
            Vector3 endPos = flightPath.GetPositionAt(endProgress);

            lineRenderer.SetPosition(segmentIndex * 2, startPos);
            lineRenderer.SetPosition(segmentIndex * 2 + 1, endPos);

            accumulatedLength += dashLength + gapLength;
            segmentIndex++;

            if (endProgress >= 1f) break;
        }

        // Оставшиеся сегменты — в последнюю точку (не в 0,0,0)
        Vector3 lastValidPos = lineRenderer.GetPosition(Mathf.Max(0, segmentIndex * 2 - 1));
        for (int i = segmentIndex * 2; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, lastValidPos);
        }
    }

    void CreateArrows()
    {
        if (arrowPrefab == null) return;

        int arrowCount = Mathf.FloorToInt(flightPath.totalLength / arrowSpacing);
        arrowInstances = new Transform[arrowCount];

        for (int i = 0; i < arrowCount; i++)
        {
            float progress = (i + 1) * arrowSpacing / flightPath.totalLength;
            progress = Mathf.Clamp01(progress);

            Vector3 pos = flightPath.GetPositionAt(progress);
            Vector3 dir = flightPath.GetDirectionAt(progress);

            GameObject arrowObj = Instantiate(arrowPrefab, pos, Quaternion.identity, transform);
            arrowObj.name = $"Arrow_{i}";

            // 🔼 Установка слоя и масштаба
            if (arrowObj.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 9; // выше всех
            }

            arrowObj.transform.localScale = Vector3.one * arrowScale;
            arrowObj.transform.up = dir;

            arrowInstances[i] = arrowObj.transform;
        }
    }

    void UpdateArrows()
    {
        if (arrowInstances == null || arrowInstances.Length == 0) return;

        for (int i = 0; i < arrowInstances.Length; i++)
        {
            float progress = (i + 1) * arrowSpacing / flightPath.totalLength;
            progress = Mathf.Clamp01(progress);

            Vector3 pos = flightPath.GetPositionAt(progress);
            Vector3 dir = flightPath.GetDirectionAt(progress);

            arrowInstances[i].position = pos;
            arrowInstances[i].up = dir;
            arrowInstances[i].localScale = Vector3.one * arrowScale; // На случай, если scale меняется
        }
    }
}