using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Движение")]
    public FlightPath flightPath;
    public float speed = 1f; // Скорость движения по траектории
    public bool loop = true; // Зациклить путь?

    [Header("Анимация полёта")]
    public float bobSpeed = 2f;       // Скорость покачивания
    public float bobHeight = 0.15f;   // Амплитуда вертикального колебания

    [Header("Попадание")]
    public GameObject hitEffect; // Эффект при попадании

    private float progress = 0f;
    private bool isHit = false;
    private Vector3 initialOffset;

    public delegate void simple(BirdController obj);
    public event simple OnFinished;
    public event simple OnHit;

    void Start()
    {
        if (flightPath == null)
        {
            Debug.LogError("FlightPath не назначен на " + name);
            enabled = false;
            return;
        }

        if (flightPath.lengths.Count == 0)
        {
            flightPath.CachePathLength();
        }

        initialOffset = transform.position;

        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        GetComponent<SpriteRenderer>().sortingOrder = 10; // выше всех

        InvokeRepeating(nameof(FlapWings), 0f, 0.2f);
    }

    void Update()
    {
        if (isHit || flightPath == null) return;

        // Двигаемся по траектории
        progress += speed * Time.deltaTime;
        if (loop)
        {
            progress %= 1f;
        }
        else if (progress >= 1f)
        {
            progress = 1f;
            Destroy(gameObject);
            OnFinished?.Invoke(this);
            return;
        }

        // Базовая позиция по траектории
        Vector3 basePosition = flightPath.GetPositionAt(progress);

        // 🔁 Вертикальное покачивание (как в Flappy Bird)
        float yOffset = Mathf.Sin(Time.timeSinceLevelLoad * bobSpeed) * bobHeight;
        Vector3 bobbedPosition = basePosition + new Vector3(0, yOffset, 0);

        transform.position = bobbedPosition;

        // 🔄 Поворот: направление движения
        Vector3 direction = flightPath.GetDirectionAt(progress);

        ApplyDynamicRotation(direction);
    }

    void ApplyDynamicRotation(Vector3 direction)
    {
        direction.z = 0;
        if (direction.sqrMagnitude < 0.01f) return;

        // Основной поворот: +90° чтобы птица летела головой вперёд
        Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, direction);
        baseRotation *= Quaternion.Euler(0, 0, 90); // Поворот на 90° по Z
        transform.rotation = baseRotation;

        // 🔁 Переворачиваем по оси Y ТОЛЬКО если движется влево (x < 0)
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(1, -1, 1); // Переворот вверх ногами
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // Нормальное положение
        }
    }

    void FlapWings()
    {
        if (isHit) return;
        // Здесь можно добавить эффект взмаха крыльев
        // Например: изменение scale или rotation крыльев
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit || other.gameObject.tag != "Projectile") return;

        isHit = true;
        _OnHit();
    }

    void _OnHit()
    {
        // Увеличиваем счёт
        OnHit?.Invoke(this);

        // Эффект попадания
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        StartCoroutine(Die());
    }

    private IEnumerator Die()
    {      
        for(float i = 1f; i > 0; i -= 0.05f)
        {
            yield return new WaitForSeconds(.01f);
            transform.localScale = new Vector3(i, i, i);
            transform.Rotate(Vector3.forward, 5);
        }
        Destroy(this.gameObject);
    }
}