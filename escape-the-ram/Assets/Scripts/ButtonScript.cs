using Bytes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
        EventManager.AddEventListener("OnButtonStateChanged", HandleButtonStateChanged);
        EventManager.AddEventListener("OnDeath", HandleDeath);
        EventManager.AddEventListener("Cooldown", HandleCooldown);
        var controls = new Controls();
        controls.Player.Enable();
        controls.Player.Button.performed += HandleButtonPressed;
    }

    private void HandleButtonPressed(InputAction.CallbackContext obj)
    {
        if(!button.interactable) return;
        button.onClick.Invoke();
    }

    private void HandleCooldown(BytesData data)
    {
        var value = (data as FloatDataBytes).FloatValue;
        if (value < 0.95) label.text = "Charging..";
        else label.text = "Pull";
        button.GetComponent<Image>().fillAmount = (data as FloatDataBytes).FloatValue;
    }

    private void HandleButtonStateChanged(BytesData data)
    {
        var state = (data as BoolDataBytes).BoolValue;
        button.interactable = state;
    }

    private void HandleDeath(BytesData data)
    {
        EventManager.RemoveEventListener("OnUltimatePull", HandleUltimatePull);
        EventManager.RemoveEventListener("OnUltimatePush", HandleUltimatePush);
        EventManager.RemoveEventListener("OnButtonStateChanged", HandleButtonStateChanged);
        EventManager.RemoveEventListener("OnDeath", HandleDeath);
        SetListener(HandleRestart);
        label.text = "Restart";
        StartCoroutine(DisableFor(3));
    }
    IEnumerator DisableFor(float time) 
    {
        button.interactable = false;
        yield return new WaitForSeconds(time);
        button.interactable = true;
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
        EventManager.RemoveEventListener("OnButtonStateChanged", HandleButtonStateChanged);
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
        EventManager.RemoveEventListener("IntroIsDone", HandleIntroIsDone);
        label.text = "Pull";
        SetListener(Ultimate);
    }

    private void Ultimate()
    {
        EventManager.Dispatch("OnCastUltimate", null);
    }
}
