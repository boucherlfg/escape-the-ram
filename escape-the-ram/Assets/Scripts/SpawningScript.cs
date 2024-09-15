using Bytes;
using UnityEngine;

public class SpawningScript : MonoBehaviour
{
    [SerializeField]
    private float minimumRandomSize = 0.5f;
    [SerializeField]
    private float maximumRanomSize = 2f;
    [SerializeField]
    private GameObject enemy;
    [SerializeField]
    private float minimumInterval = 1f;
    [SerializeField]
    private float maximumInterval = 3f;
    [SerializeField]
    private float maximumDeadEnemy = 100;
    private float spawnCounter;
    Rect cameraRect;
    Camera mainCam;

    int deadEnemies = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        EventManager.AddEventListener("OnEnemyDeath", HandleEnemyDeath);
    }

    void HandleEnemyDeath(BytesData data) 
    {
        deadEnemies++;
    }

    float EnemySpawnInterval
    {
        get
        {
            var intervalDelta = maximumInterval - minimumInterval;
            return deadEnemies > maximumDeadEnemy ? minimumInterval : maximumInterval - deadEnemies * intervalDelta / maximumDeadEnemy;
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
        enemyInstance.transform.localScale *= Random.Range(minimumRandomSize, maximumRanomSize);
    }
}
