using UnityEngine;

public class ShootScript : MonoBehaviour
{
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private float projectileSpeed = 5;
    [SerializeField]
    private float projectileKnockback = 5;
    [SerializeField]
    private float projectileInterval = 0.25f;
    private float projectileCounter = 0;
    private Camera _mainCam;
    
    // Start is called before the first frame update
    void Start()
    {
        _mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (projectileCounter < projectileInterval)
        {
            projectileCounter += Time.deltaTime;
            return;
        }

        if (!Input.GetMouseButtonUp(0)) return;

        var pos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        var delta = (pos - transform.position).normalized;

        var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
    }
}
