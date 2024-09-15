using Bytes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeIndicator : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> faces;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddEventListener("OnDamageChanged", HandleDamageTaken);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener("OnDamageChanged", HandleDamageTaken);
    }
    void HandleDamageTaken(BytesData data)
    {
        float value = (data as FloatDataBytes).FloatValue;
        int index = Mathf.RoundToInt(value * faces.Count);
        if (index >= faces.Count) return;
        GetComponent<SpriteRenderer>().sprite = faces[index];
    }
}
