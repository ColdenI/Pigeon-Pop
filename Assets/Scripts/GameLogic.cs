using CGL;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance;

    [SerializeField] private GameObject BirdPrefab;
    [SerializeField] private Text text_Score;
    [SerializeField] private Text text_ScoreRecord;
    [SerializeField] private GameObject SlingshotBase;

    [SerializeField] private Level[] Levels;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SlingshotJoystick bread;

    private GameObject thisPath;
    private Level thisLevel;

    public int Score = 0;

    private void Awake()
    {
        TranslationData.Init();
        SaveLogic.Load();
    }
    private void Start()
    {
        Instance = this;

        text_Score.text = Score.ToString();
        text_ScoreRecord.text = SaveLogic.SaveData.Score.ToString();

        //StartLevel();
    }

    [ContextMenu("Start")]
    public void StartLevel() => StartCoroutine(startLevel());
    private IEnumerator startLevel()
    {
        if (thisPath != null)
        {
            yield return new WaitForSeconds(.7f);
            Destroy(thisPath);
            yield return new WaitForSeconds(Random.Range(.7f, 3f));
        }
        else
        {
            yield return new WaitForSeconds(.7f);
        }


        thisLevel = Levels[Random.Range(0, Levels.Length)];
        thisPath = Instantiate(thisLevel.FlightPath);

        yield return new WaitForSeconds(Random.Range(.7f, 1.7f));

        BirdController bird = Instantiate(BirdPrefab, new Vector3(-100, -100, 0), Quaternion.identity).GetComponent<BirdController>();
        bird.OnHit += Bird_OnHit;
        bird.OnFinished += Bird_OnFinished;
        bird.flightPath = thisPath.GetComponent<FlightPath>();

        bird.speed = thisLevel.BirdSpeed + Score * .01f;
    }

    private void Bird_OnFinished(BirdController obj)
    {
        obj.OnFinished -= Bird_OnFinished;
        obj.OnHit += Bird_OnHit;

        //SlingshotBase.transform.position = new Vector3(Random.Range(-7.5f, 7.5f), SlingshotBase.transform.position.y, SlingshotBase.transform.position.z);
        //bread.originalPosition = SlingshotBase.transform.position;
        StartLevel();
    }

    private void Bird_OnHit(BirdController obj)
    {
        obj.OnFinished -= Bird_OnFinished;
        obj.OnHit += Bird_OnHit;
        audioSource.Play();

        StartCoroutine(AddScore());

        StartLevel();
    }

    public void BackButton()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator AddScore()
    {
        Score++;
        bool isNewRecord = false;
        if (SaveLogic.SaveData.Score < Score)
        {
            SaveLogic.SaveData.Score = Score;
            SaveLogic.Save();
            isNewRecord = true;
        }

        text_Score.text = Score.ToString();
        text_ScoreRecord.text = SaveLogic.SaveData.Score.ToString();

        const float delay = .004f;
        const float speed = .01f;
        const float size = 1.4f;

        for (float i = 1f; i < size; i += speed)
        {
            text_Score.gameObject.transform.localScale = Vector3.one * i;
            if(isNewRecord) text_ScoreRecord.gameObject.transform.localScale = Vector3.one * i;
            yield return new WaitForSeconds(delay);
        }
        for (float i = size; i > 1f; i -= speed)
        {
            text_Score.gameObject.transform.localScale = Vector3.one * i;
            if(isNewRecord) text_ScoreRecord.gameObject.transform.localScale = Vector3.one * i;
            yield return new WaitForSeconds(delay);
        }

        text_Score.gameObject.transform.localScale = Vector3.one;
        text_ScoreRecord.gameObject.transform.localScale = Vector3.one;
    }


    [System.Serializable]
    public struct Level
    {
        public GameObject FlightPath;
        public float BirdSpeed;
    }
}
