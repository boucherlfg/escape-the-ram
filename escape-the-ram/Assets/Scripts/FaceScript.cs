using Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceScript : MonoBehaviour
{
    int faceIndex = 0;
    [SerializeField]
    private List<Sprite> faces;
    [SerializeField]
    private Sprite ultimateFace;
    private bool isUltimate;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddEventListener("OnDamageChanged", HandleDamageTaken);
        EventManager.AddEventListener("OnUltimatePush", HandlePush);
        EventManager.AddEventListener("OnUltimatePull", HandlePull);
    }

    private void Update()
    {
        if (isUltimate)
        {
            GetComponent<SpriteRenderer>().sprite = ultimateFace;
            return;
        }
        if (faceIndex >= faces.Count) return;
        GetComponent<SpriteRenderer>().sprite = faces[faceIndex];
    }
    private void HandlePull(BytesData data)
    {
        isUltimate = true;
    }

    private void HandlePush(BytesData data)
    {
        isUltimate = false;
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener("OnDamageChanged", HandleDamageTaken);
    }
    void HandleDamageTaken(BytesData data)
    {
        float value = (data as FloatDataBytes).FloatValue;
        faceIndex = Mathf.RoundToInt(value * faces.Count);
    }
}
