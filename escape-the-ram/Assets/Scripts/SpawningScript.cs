using Bytes;
using UnityEngine;

public class SpawningScript : MonoBehaviour
{
    [SerializeField]
    private GameObject enemy;
    [SerializeField]
    private float minimumInterval = 1f;
    [SerializeField]
    private float maximumInterval = 3f;
    [SerializeField]
    private int maximumDeadEnemy = 200;
    private float spawnCounter;
    Rect cameraRect;
    Camera mainCam;

    int deadEnemies = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        EventManager.AddEventListener("OnEnemyDeath", HandleEnemyDeath);
        EventManager.AddEventListener("OnDeath", HandleDeath);
    }

    private void HandleDeath(BytesData data)
    {
        EventManager.Dispatch("GetScore", new IntDataBytes(deadEnemies));
        EventManager.RemoveEventListener("OnDeath", HandleDeath);
    }

    void HandleEnemyDeath(BytesData data) 
    {
        deadEnemies++;
        EventManager.Dispatch("OnScoreChanged", new IntDataBytes(deadEnemies));
    }

    float EnemySpawnInterval
    {
        get
        {
            deadEnemies = Mathf.Min(maximumDeadEnemy, deadEnemies); 
            return maximumInterval - (maximumInterval - minimumInterval) * (deadEnemies / (float)maximumDeadEnemy);
        }
    }
    // Update is called once per frame
    void Update()
    {
        spawnCounter += Time.deltaTime;
        if(spawnCounter < EnemySpawnInterval) return;
        spawnCounter = 0;

        var min = mainCam.ScreenToWorldPoint(Vector2.zero);
        var max = mainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        cameraRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        
        Vector2 center = min + (max - min) / 2;
        var screenSize = Vector2.Distance(max, center);

        var direction = Random.insideUnitCircle.normalized;
        direction.x = direction.x * (cameraRect.width / 2 + cameraRect.width / 10);
        direction.y = direction.y * (cameraRect.height / 2 + cameraRect.height / 10);
        var spawnPosition = center + direction;

        var enemyInstance = Instantiate(enemy, spawnPosition, Quaternion.identity, transform);
    }
}
