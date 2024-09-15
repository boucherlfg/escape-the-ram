using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField]
    float timer = 1;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
