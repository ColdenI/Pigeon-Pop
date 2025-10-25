using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingshotJoystick : MonoBehaviour
{
    [Header("Settings")]
    public float maxStretchDistance = 1f;
    public float returnSpeed = 8f;
    public float launchForceMin = 5f;
    public float launchForceMax = 15f;
    public float resetDelay = 2f;
    public float rubberReturnTime = 0.3f;

    [Header("References")]
    public Transform centerPoint;
    public Transform leftRubberAnchor;
    public Transform rightRubberAnchor;
    public LineRenderer leftRubberLine;
    public LineRenderer rightRubberLine;
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip stretchSound;
    public AudioClip launchSound;

    public Vector3 originalPosition;
    private bool isDragging = false;
    private Camera cam;
    private bool isLaunched = false;
    private float currentStretch = 0f;
    private bool isPlayingStretchSound = false;
    private bool isReturningRubber = false;
    private Vector3 rubberReturnStartLeft;
    private Vector3 rubberReturnStartRight;
    private float rubberReturnTimer = 0f;

    public GameObject trailDotPrefab; // Префаб кружка
    private List<TrailDot> activeDots = new List<TrailDot>();
    private float trailInterval = 0.1f; // Как часто ставить точку
    private float lastTrailTime;
    private int trailDotCount = 0; // Сколько точек уже создано

    void Start()
    {
        cam = Camera.main;
        originalPosition = transform.position;

        // Настройка резинок
        if (leftRubberLine != null)
        {
            leftRubberLine.positionCount = 2;
            leftRubberLine.enabled = true;
            leftRubberLine.sortingLayerName = "Default";
            leftRubberLine.sortingOrder = 8; // Поверх фона, но под всеми объектами
            leftRubberLine.material = new Material(Shader.Find("Sprites/Default"));
        }

        if (rightRubberLine != null)
        {
            rightRubberLine.positionCount = 2;
            rightRubberLine.enabled = true;
            rightRubberLine.sortingLayerName = "Default";
            rightRubberLine.sortingOrder = 8;
            rightRubberLine.material = new Material(Shader.Find("Sprites/Default"));
        }


        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        rubberReturnStartLeft = transform.position;
        rubberReturnStartRight = transform.position;
    }

    void Update()
    {
        if (transform.position.y < -10) ResetSlingshot();

        if (isLaunched)
        {
            transform.position += (Vector3)GetComponent<Rigidbody2D>().velocity * Time.deltaTime;
            // Генерация следа
            if (Time.time - lastTrailTime > trailInterval)
            {                
                CreateTrailDot();
                lastTrailTime = Time.time;
            }
            return;
        }

        if (isDragging)
        {
            Drag();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, centerPoint.position, returnSpeed * Time.deltaTime);
        }

        DrawRubberBands();
        PlayStretchSound();

    }

    private void CreateTrailDot()
    {
        if (trailDotPrefab == null) return;

        GameObject dot = Instantiate(trailDotPrefab, transform.position, Quaternion.identity);

        if (dot.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            // Устанавливаем начальную прозрачность
            Color startColor = sr.color;
            startColor.a = 1f;
            sr.color = startColor;

            TrailDot td = new TrailDot(dot, sr);

            // 🔽 Устанавливаем размер ТОЛЬКО ОДИН РАЗ при создании
            float totalExpected = 15; // Примерно столько точек будет
            float normalizedSize = Mathf.InverseLerp(0, totalExpected - 1, trailDotCount); // 1...0

            // Чем раньше создана — тем больше: scale от 0.1 до 0.03
            float scale = Mathf.Lerp(0.1f, 0.03f, normalizedSize); // первая: 0.1, далее: меньше

            dot.transform.localScale = Vector3.one * scale;

            activeDots.Add(td);
            trailDotCount++; // Увеличиваем счётчик
            StartCoroutine(DestroyTrailDot(td));
        }
        else
        {
            var srNew = dot.AddComponent<SpriteRenderer>();
            srNew.color = new Color(1, 1, 1, 1f);

            TrailDot td = new TrailDot(dot, srNew);

            float totalExpected = 15;
            float normalizedSize = Mathf.InverseLerp(0, totalExpected - 1, trailDotCount);
            float scale = Mathf.Lerp(0.1f, 0.03f, normalizedSize);

            dot.transform.localScale = Vector3.one * scale;

            activeDots.Add(td);
            trailDotCount++;

            StartCoroutine(DestroyTrailDot(td));
        }
    }

    void ResetSlingshot()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        transform.position = originalPosition;
        isLaunched = false;
        currentStretch = 0f;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }


        trailDotCount = 0; // 🔁 Сброс счётчика

        // Сброс резинок
        isReturningRubber = false;
        transform.eulerAngles = Vector3.zero;
        rubberReturnTimer = 0f;

        if (leftRubberLine != null && leftRubberAnchor != null)
        {
            leftRubberLine.SetPosition(1, leftRubberAnchor.position);
        }
        if (rightRubberLine != null && rightRubberAnchor != null)
        {
            rightRubberLine.SetPosition(1, rightRubberAnchor.position);
        }
    }

    private IEnumerator DestroyTrailDot(TrailDot obj)
    {
        for (float i = 1f; i > 0f; i -= .01f)
        {
            yield return new WaitForSeconds(.1f);
            Color c = obj.renderer.color;
            c.a = i;
            obj.renderer.color = c;
        }
        Destroy(obj.obj);
    }

    void OnMouseDown()
    {
        isDragging = true;
        isPlayingStretchSound = false;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Drag();
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        Vector3 direction = centerPoint.position - transform.position;
        currentStretch = direction.magnitude / maxStretchDistance;

        if (currentStretch > 0.5f)
        {
            Launch(direction.normalized, currentStretch);
        }
        else
        {
            transform.position = centerPoint.position;
        }

        if (audioSource.isPlaying && audioSource.clip == stretchSound)
        {
            audioSource.Stop();
        }
    }

    void Drag()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = mousePos - centerPoint.position;
        float clampedDistance = Mathf.Clamp(direction.magnitude, 0, maxStretchDistance);
        Vector3 clampedDirection = direction.normalized;

        transform.position = centerPoint.position + clampedDirection * clampedDistance;
        currentStretch = clampedDistance / maxStretchDistance;
    }

    void DrawRubberBands()
    {
        if (leftRubberLine != null && leftRubberAnchor != null)
        {
            leftRubberLine.SetPosition(0, leftRubberAnchor.position);
            leftRubberLine.SetPosition(1, transform.position);

            // 🔁 Принудительно устанавливаем слой КАЖДЫЙ КАДР
            leftRubberLine.sortingLayerName = "Default";
            leftRubberLine.sortingOrder = 8;
        }

        if (rightRubberLine != null && rightRubberAnchor != null)
        {
            rightRubberLine.SetPosition(0, rightRubberAnchor.position);
            rightRubberLine.SetPosition(1, transform.position);

            // 🔁 То же самое для правой резинки
            rightRubberLine.sortingLayerName = "Default";
            rightRubberLine.sortingOrder = 8;
        }
    }

    void PlayStretchSound()
    {
        if (isDragging && currentStretch > 0.1f && !isPlayingStretchSound)
        {
            if (stretchSound != null)
            {
                audioSource.clip = stretchSound;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingStretchSound = true;
            }
        }
        else if (!isDragging || currentStretch <= 0.1f)
        {
            if (isPlayingStretchSound && audioSource.clip == stretchSound)
            {
                audioSource.Stop();
                isPlayingStretchSound = false;
            }
        }
    }

    void Launch(Vector3 direction, float stretchFactor)
    {
        isLaunched = true;

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.drag = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        float launchPower = Mathf.Lerp(launchForceMin, launchForceMax, stretchFactor);
        rb.velocity = direction * launchPower;

        // 🔁 ДОБАВЬ ЭТИ СТРОКИ: вращение хлеба
        rb.angularVelocity = Random.Range(100f, 300f); // градусов в секунду
                                                       // Можно сделать направление случайным:
                                                       // если хочешь, чтобы вращалось против часовой стрелки: rb.angularVelocity = -200f;

        if (launchSound != null)
        {
            audioSource.clip = launchSound;
            audioSource.loop = false;
            audioSource.Play();
        }

        StartCoroutine(ReturnRubberLine());

        lastTrailTime = Time.time;

        //Invoke(nameof(ResetSlingshot), resetDelay);
    }

    private IEnumerator ReturnRubberLine()
    {
        Vector3 startPosLeft = leftRubberLine.GetPosition(1);
        Vector3 startPosRight = rightRubberLine.GetPosition(1);
        Vector3 endPos = centerPoint.position;

        float duration = 0.1f; // Время анимации возврата (сек)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; // 0 → 1

            // Плавное перемещение конца резинок к центру
            leftRubberLine.SetPosition(1, Vector3.Lerp(startPosLeft, endPos, t));
            rightRubberLine.SetPosition(1, Vector3.Lerp(startPosRight, endPos, t));

            yield return null; // ждём следующий кадр
        }

        // Гарантированно ставим в конечную точку
        leftRubberLine.SetPosition(1, endPos);
        rightRubberLine.SetPosition(1, endPos);
    }


    private class TrailDot
    {
        public GameObject obj;
        public SpriteRenderer renderer;
        public float spawnTime;

        public TrailDot(GameObject go, SpriteRenderer r)
        {
            obj = go;
            renderer = r;
            spawnTime = Time.time;
        }
    }
}



