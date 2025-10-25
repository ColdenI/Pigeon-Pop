using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class FlightPath : MonoBehaviour
{
    [Header("Точки пути")]
    public Transform[] waypoints; // Контрольные точки

    [Header("Настройки")]
    public bool isClosed = false;
    public Color pathColor = new Color(0f, 1f, 0.5f, 0.6f);
    public int resolution = 50; // Точек для расчёта длины

    public List<float> lengths = new List<float>(); // Накопленная длина
    public float totalLength = 0f;

    private void OnValidate()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            CachePathLength();
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = pathColor;

        Vector3 prevPoint = GetCatmullRomPoint(0);
        for (int i = 1; i <= 100; i++)
        {
            float t = i / 100f;
            Vector3 point = GetCatmullRomPoint(t);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Показываем контрольные точки
        foreach (var wp in waypoints)
        {
            if (wp != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(wp.position, 0.15f);
            }
        }
    }

    // Один раз при старте или изменении путей — считаем длину
    public void CachePathLength()
    {
        lengths.Clear();
        totalLength = 0f;

        Vector3 prevPos = GetCatmullRomPoint(0);
        lengths.Add(0);

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 pos = GetCatmullRomPoint(t);
            float segmentLength = Vector3.Distance(pos, prevPos);
            totalLength += segmentLength;
            lengths.Add(totalLength);
            prevPos = pos;
        }
    }

    // Получить позицию с равномерной скоростью
    public Vector3 GetPositionAt(float progress)
    {
        if (waypoints == null || waypoints.Length < 2) return transform.position;

        if (lengths.Count == 0)
        {
            CachePathLength();
        }

        float targetLength = progress * totalLength;
        int i = 0;
        while (i < lengths.Count - 1 && lengths[i + 1] < targetLength)
        {
            i++;
        }

        if (i >= lengths.Count - 1) return GetCatmullRomPoint(1f);

        float lengthBefore = lengths[i];
        float lengthAfter = lengths[i + 1];
        float segmentLength = lengthAfter - lengthBefore;
        float tNormalized = segmentLength > 0 ? (targetLength - lengthBefore) / segmentLength : 0;

        float t1 = i / (float)resolution;
        float t2 = (i + 1) / (float)resolution;
        float t = Mathf.Lerp(t1, t2, tNormalized);

        return GetCatmullRomPoint(t);
    }

    public Vector3 GetDirectionAt(float progress)
    {
        float small = 0.001f;
        Vector3 a = GetPositionAt(Mathf.Clamp01(progress - small));
        Vector3 b = GetPositionAt(Mathf.Clamp01(progress + small));
        return (b - a).normalized;
    }

    // Catmull-Rom Spline (гладкая кривая)
    private Vector3 GetCatmullRomPoint(float t)
    {
        int numPoints = waypoints.Length;
        int numSections = isClosed ? numPoints : numPoints - 1;
        float tScaled = t * numSections;
        int section = Mathf.FloorToInt(tScaled);
        float u = tScaled - section;

        if (!isClosed && section >= numPoints - 1)
        {
            return waypoints[numPoints - 1].position;
        }

        int p0 = WrapIndex(section - 1, numPoints);
        int p1 = WrapIndex(section, numPoints);
        int p2 = WrapIndex(section + 1, numPoints);
        int p3 = WrapIndex(section + 2, numPoints);

        Vector3 A = waypoints[p0].position;
        Vector3 B = waypoints[p1].position;
        Vector3 C = waypoints[p2].position;
        Vector3 D = waypoints[p3].position;

        float tt = u * u;
        float ttt = u * tt;

        float x = 0.5f * (
            (2 * B.x) +
            (-A.x + C.x) * u +
            (2 * A.x - 5 * B.x + 4 * C.x - D.x) * tt +
            (-A.x + 3 * B.x - 3 * C.x + D.x) * ttt
        );
        float y = 0.5f * (
            (2 * B.y) +
            (-A.y + C.y) * u +
            (2 * A.y - 5 * B.y + 4 * C.y - D.y) * tt +
            (-A.y + 3 * B.y - 3 * C.y + D.y) * ttt
        );

        return new Vector3(x, y, 0);
    }

    private int WrapIndex(int i, int max)
    {
        return (i + max) % max;
    }
}