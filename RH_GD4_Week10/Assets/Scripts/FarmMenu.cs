using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = "High Score: " + PlayerPrefs.GetInt("FarmHighScore", 0);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("BasicFarmingSimulator");
    }
}
