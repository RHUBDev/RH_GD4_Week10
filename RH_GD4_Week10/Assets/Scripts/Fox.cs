using System.Collections;
using UnityEngine;

public class Fox : MonoBehaviour
{
    private float fastWaitTime = 3f;
    private float tomatoWaitTime = 1f;
    private float timer = 0f;
    private float moveSpeed = 1f;
    private float slowSpeed = 1f;
    private float fastSpeed = 3f;
    public GameObject tomato;
    public Tractor tractor;
    private bool waitForTomatoDone = false;
    [SerializeField] CapsuleCollider coll;
    [SerializeField] SkinnedMeshRenderer rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        timer = 0f;
        moveSpeed = slowSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < fastWaitTime)
        {
            moveSpeed = slowSpeed;
        }
        else
        {
            moveSpeed = fastSpeed;
        }
        if (tomato != null)
        {
            Vector3 vect = tomato.transform.position - transform.position;
            vect.y = 0;
            if (vect.magnitude > 1)
            {
                transform.forward = vect;
                transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            }
            else
            {
                tractor.plants.Remove(tomato);
                Destroy(tomato);
                tractor.CropEaten();
            }
        }
        else
        {
            if (tractor.plants.Count > 0)
            {
                if (!waitForTomatoDone)
                {
                    waitForTomatoDone = true;
                    WaitForTomato();
                    //StartCoroutine(WaitForTomato());
                }
            }
        }
    }

    void WaitForTomato()
    {
        if (tractor.plants.Count > 0)
        {
            tomato = tractor.plants[Random.Range(0, tractor.plants.Count)];
            moveSpeed = slowSpeed;
            timer = 0;
            waitForTomatoDone = false;
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("Vehicle"))
        {
            if (collision.transform.GetComponent<StatusMonitor>().status.text == "Travelling...")
            {
                gameObject.SetActive(false);
            }
        }
    }
}
