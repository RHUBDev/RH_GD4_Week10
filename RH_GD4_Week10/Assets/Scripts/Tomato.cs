using UnityEngine;

public class Tomato : MonoBehaviour
{
    private float growTime = 10f;
    private float growingTime = 0f;
    public GameObject plant;
    public GameObject tomato;
    public bool grown { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grown = false;
    }

    // Update is called once per frame
    void Update()
    {
        growingTime += Time.deltaTime;

        if (growingTime >= growTime)
        {
            plant.SetActive(false);
            tomato.SetActive(true);
            grown = true;
        }
    }
}
