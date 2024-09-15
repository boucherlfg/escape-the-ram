using Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonScript : SingletonBehaviour<ButtonScript>
{
    private Button button;
    private TMP_Text label;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return null;

        Time.timeScale = 0;
        button = GetComponent<Button>();
        label = GetComponentInChildren<TMP_Text>();
        label.text = "Continue";

        EventManager.AddEventListener("IntroIsDone", HandleIntroIsDone);
        EventManager.AddEventListener("OnUltimatePull", HandleUltimatePull);
        EventManager.AddEventListener("OnUltimatePush", HandleUltimatePush);
        EventManager.AddEventListener("OnDeath", HandleDeath);
    }

    private void HandleDeath(BytesData data)
    {
        SetListener(HandleRestart);
        label.text = "Restart";
    }

    void HandleRestart() {
        SceneManager.LoadScene(gameObject.scene.name);
    }

    public void SetListener(UnityAction listener)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
        button.onClick.AddListener(listener);
    }
    private void OnDestroy()
    {
        EventManager.RemoveEventListener("IntroIsDone", HandleIntroIsDone);
        EventManager.RemoveEventListener("OnUltimatePull", HandleUltimatePull);
        EventManager.RemoveEventListener("OnUltimatePush", HandleUltimatePush);
        EventManager.RemoveEventListener("OnDeath", HandleDeath);
    }

    private void HandleUltimatePush(BytesData data)
    {
        label.text = "Pull";
    }

    private void HandleUltimatePull(BytesData data)
    {
        label.text = "Push";
    }

    private void HandleIntroIsDone(BytesData data)
    {
        label.text = "Pull";
        SetListener(Ultimate);
    }

    private void Ultimate()
    {
        EventManager.Dispatch("OnCastUltimate", null);
    }
}
