using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Harvester : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navagent;
    [SerializeField] private TMP_Text scoreText;
    public int score { get; private set; }
    [SerializeField] private Tractor tractor;
    public TMP_Text statusText;
    private RectTransform statusRect;
    private Camera cam;
    private float statusAboveAmount = 1f;
    private bool harvested = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statusRect = statusText.GetComponent<RectTransform>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                navagent.SetDestination(hit.point);
            }
        }

        if (harvested)
        {
            statusText.text = "Harvested!";
        }
        else if(navagent.remainingDistance <= navagent.stoppingDistance)
        {
            statusText.text = "Idle";
        }
        else
        {

            statusText.text = "Travelling..."; ;
        }

        SetUIPosition();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Tomato"))
        {
            Tomato tom = other.transform.GetComponent<Tomato>();
            if (tom.grown)
            {
                score++;
                scoreText.text = "Score: " + score;
                tractor.plants.Remove(tom.gameObject);
                Destroy(tom.gameObject);
                StartCoroutine(SayHarvested());
            }
        }
    }

    void SetUIPosition()
    {
        statusRect.position = cam.WorldToScreenPoint(transform.position + Vector3.forward * statusAboveAmount);
    }

    IEnumerator SayHarvested()
    {
        harvested = true;
        yield return new WaitForSeconds(1.5f);
        harvested = false;
    }
}
