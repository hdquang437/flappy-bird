using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float maxTime = 1.2f;
    [SerializeField] private float heightRange = 0.45f;
    [SerializeField] private GameObject pipeTop;
    [SerializeField] private GameObject pipeBottom;

    private float timer;

    private GameManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    }

    private void FixedUpdate()
    {
        if (manager.isStarted)
        {
            if (timer > maxTime)
            {
                SpawnPipe();
                timer = 0;
            }

            timer += Time.fixedDeltaTime;
        }
        else if (timer != 0)
        {
            timer = 0;
        }

    }

    public void SpawnPipe()
    {
        Vector3 spawnPos = transform.position + new Vector3(0, Random.Range(-heightRange, heightRange), 0);

        GameObject pipeT = Instantiate(pipeTop, spawnPos + new Vector3(0, 1.1f, 0), Quaternion.identity);
        GameObject pipeB = Instantiate(pipeBottom, spawnPos + new Vector3(0, -1.1f, 0), Quaternion.identity);

        manager.objects.Add(pipeT);
        manager.objects.Add(pipeB);
    }
}
