using Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreIndicator : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddEventListener("OnScoreChanged", HandleScoreChanged);
    }

    private void HandleScoreChanged(BytesData data)
    {
        var score = (data as IntDataBytes).IntValue;
        label.text = "" + score;
    }
}
