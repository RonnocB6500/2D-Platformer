using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public static int selectedLevel;
    public static event Action OnLevel;
    public int level;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //selectedLevel = this.level;
    }

    public void OpenLevel()
    {
        selectedLevel = level;
        SceneManager.LoadScene("GameScene");
        OnLevel.Invoke();
    }
}
