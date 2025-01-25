using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour
{
    public static event Action OnMenu;

    public GameObject ui;

    void Start()
    {
        LevelSelect.OnLevel += enable;
    }

    public void OnMenuClick()
    {
        MusicManager.PauseBackgroundMusic();
        OnMenu.Invoke();
        disable();
        SceneManager.LoadScene("StartScene");
    }

    void disable()
    {
        ui.SetActive(false);
    }

    void enable()
    {
        ui.SetActive(true);
    }
}
