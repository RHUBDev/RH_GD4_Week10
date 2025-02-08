using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Tractor : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navagent;
    public GameObject plantPrefab;
    public GameObject foxPrefab;
    public GameObject currentFox;
    public List<GameObject> plants = new List<GameObject>();
    private float sowDistance = 2f;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text message2;
    private RectTransform message2Rect;
    public TMP_Text statusText;
    private bool planted = false;
    public LayerMask layerMask;
    private RectTransform statusRect;
    private Camera cam;
    private float statusAboveAmount = 1f;
    private Vector3 message2AboveVector = new Vector3(0,30,0);
    private int lives = 1;
    [SerializeField] Harvester harvester;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timeText;
    private bool dead = false;
    private bool spawningFox = true;
    [SerializeField] Transform[] foxLocations;
    [SerializeField] Fox fox;
    private float statusWait = 1.5f;
    private float statusTimer = 1.5f;
    private float gameTimer = 100f;
    //private float gameTime = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1f;
        statusRect = statusText.GetComponent<RectTransform>();
        message2Rect = message2.GetComponent<RectTransform>();
        cam = Camera.main;
        SpawnFox();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            if (gameTimer > 0)
            {
                gameTimer -= Time.deltaTime;
                timeText.text = "" + Mathf.CeilToInt(gameTimer).ToString();
            }
            else
            {
                timeText.text = "" + 0;
                CropEaten();
            }
            if (statusTimer < 1.5f)
            {
                statusTimer += Time.deltaTime;
            }
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 200f, layerMask))
                {
                    bool tooclose = false;
                    foreach (GameObject plant in plants)
                    {
                        if ((plant.transform.position - hit.point).magnitude < sowDistance)
                        {
                            //if new plant location is within 2 metres of another, don't accept it
                            tooclose = true;
                            StartCoroutine(ShowMessage());
                            break;
                        }
                    }
                    if (!tooclose)
                    {
                        statusText.text = "Travelling...";
                        //if not too close, set nav agent destination
                        navagent.destination = hit.point;
                        planted = false;
                    }
                    /*else
                    {
                        statusText.text = "Idle";
                    }*/
                }
            }
            if ((navagent.destination - transform.position).magnitude < 1 && !planted)
            {
                statusTimer = 0;
                statusText.text = "Planted!";
                planted = true;
                GameObject theplant = Instantiate(plantPrefab, navagent.destination, Quaternion.identity);
                plants.Add(theplant);
            }
            else if ((navagent.destination - transform.position).magnitude < 1 && statusTimer > statusWait)
            {
                statusText.text = "Idle";
            }
        }

        if(!currentFox.activeInHierarchy && !spawningFox)
        {
            spawningFox = true;
            StartCoroutine(WaitSpawnFox());
        }
        SetUIPosition();
    }

    IEnumerator ShowMessage()
    {
        message2.text = "Too close to another plant";
        yield return new WaitForSeconds(2);
        message2.text = "";
    }

    void SetUIPosition()
    {
        statusRect.position = cam.WorldToScreenPoint(transform.position + Vector3.forward * statusAboveAmount);
        message2Rect.position = Input.mousePosition + message2AboveVector;
    }

    public void CropEaten()
    {
        if (!dead)
        {
            //Take damage if enemy hit us
            lives--;
            livesText.text = "Lives: " + lives;
            if (lives <= 0)
            {
                dead = true;

                int highScore = PlayerPrefs.GetInt("FarmHighScore", 0);
                if (harvester.score > highScore)
                {
                    endText.text = "Congrats!\nYou scored: " + harvester.score + "\nOld score: " + highScore;
                    PlayerPrefs.SetInt("FarmHighScore", harvester.score);
                }
                else
                {
                    endText.text = "You scored: " + harvester.score + "\nHigh score: " + highScore;
                }

                StartCoroutine(DoDie());
                Time.timeScale = 0f;
            }
        }
    }

    IEnumerator DoDie()
    {
        yield return new WaitForSecondsRealtime(3);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator WaitSpawnFox()
    {
        yield return new WaitForSeconds(3);
        SpawnFox();
    }

    void SpawnFox()
    {
        //currentFox = Instantiate(foxPrefab, foxLocations[Random.Range(0, foxLocations.Length)].position, Quaternion.identity);
        currentFox.transform.position = foxLocations[Random.Range(0, foxLocations.Length)].position;
        currentFox.SetActive(true);
        spawningFox = false;
        //Fox fox = currentFox.GetComponent<Fox>();
        fox.tractor = this;
        if (plants.Count > 0)
        {
            fox.tomato = plants[Random.Range(0, plants.Count)];
        }
    }
}
