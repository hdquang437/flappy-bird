using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    public GameObject bird;
    public List<GameObject> objects = new();

    [SerializeField] GameObject gameOverCanvas;

    public float gravity = -0.12f;
    public float speed = -1.2f;
    public float birdJump = 1.6f;

    public bool isStarted = false;

    public bool gameOver = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        objects.Add(GameObject.FindWithTag("Ground"));
        objects.Add(GameObject.FindWithTag("Ceil"));
    }

    // Game over
    public void EndGame()
    {
        gameOver = true;
        isStarted = false;
        gameOverCanvas.SetActive(true);
    }

    // New game
    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
