using Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField]
    TMP_Text score;
    [SerializeField]
    TMP_Text highscore;
    [SerializeField]
    GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.AddEventListener("OnDeath", HandleDeath);
        EventManager.AddEventListener("GetScore", GetScore);
    }

    private void GetScore(BytesData data)
    {
        int score = (data as IntDataBytes).IntValue;
        this.score.text = "" + score;

        int highscore = PlayerPrefs.GetInt("highscore", 0);
        if (score > highscore) highscore = score;
        PlayerPrefs.SetInt("highscore", highscore);
        this.highscore.text = "" + highscore;
    }

    private void HandleDeath(BytesData data)
    {

        panel.SetActive(true);
        EventManager.RemoveEventListener("OnDeath", HandleDeath);
    }
}
