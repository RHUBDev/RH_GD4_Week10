using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuCam : MonoBehaviour
{
    public Transform enemyPoint;
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        scoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(enemyPoint);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("MyScene");
        }
    }
}
