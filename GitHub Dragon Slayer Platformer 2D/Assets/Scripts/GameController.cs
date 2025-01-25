using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set;}
    int progressAmount;
    //public Slider progressSlider;

    public GameObject player;
    public GameObject loadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex = 0;

    public GameObject gameOverScreen;
    public GameObject gameCompleteScreen;
    public TMP_Text survivedText;
    private int survivedLevelsCount;

    public static event Action OnReset;
    public static event Action OnStartLevel;
    public static event Action OnStopLevel;

    private int levelSelected;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelSelected = LevelSelect.selectedLevel - 1;
        currentLevelIndex = levelSelected;
        levels[levelSelected].SetActive(true);
        LevelSelect.OnLevel += StartLevel;

        progressAmount = 0;
        DoorController.OpenedDoor += LoadNextLevel;
        PlayerHealth.OnPlayerDied += GameOverScreen;
        MenuButtonController.OnMenu += DisableLevels;
        MenuButtonController.OnMenu += DisableGameFinishedScreen;
        MenuButtonController.OnMenu += DisableGameFinishedScreen;
        
        Time.timeScale = 1;
        loadCanvas.SetActive(false);
        gameOverScreen.SetActive(false);  
        Time.timeScale = 1; 
    }

    void Update()
    {
        
    }

    void GameOverScreen()
    {
        OnStopLevel.Invoke();
        gameOverScreen.SetActive(true);
        MusicManager.PauseBackgroundMusic();
        survivedText.text = "YOU SURVIVED " + survivedLevelsCount + " LEVEL";
        if(survivedLevelsCount != 1) survivedText.text += "S";
        Time.timeScale = 0;
    }

    void GameCompleteScreen()
    {
        OnStopLevel.Invoke();
        gameCompleteScreen.SetActive(true);
        MusicManager.PauseBackgroundMusic();
        
        OnReset.Invoke();
        Time.timeScale = 0;

    }

    void DisableGameOverScreen()
    {
        gameOverScreen.SetActive(false);
        survivedLevelsCount = 0;
        Time.timeScale = 1;
    }

    void DisableGameFinishedScreen()
    {
        gameCompleteScreen.SetActive(false);
        survivedLevelsCount = 0;
        Time.timeScale = 1;
    }

    public void ResetGame()
    {
        gameOverScreen.SetActive(false);
        MusicManager.PlayBackgroundMusic(true);
        survivedLevelsCount = 0;
        LoadLevel(levelSelected, false);
        OnReset.Invoke();
        Time.timeScale = 1;
    }

  

    public void StartLevel()
    {
        levelSelected = LevelSelect.selectedLevel - 1;
        currentLevelIndex = levelSelected;
        LoadLevel(levelSelected, false);
        MusicManager.PlayBackgroundMusic(true);
        OnStartLevel.Invoke();
        Time.timeScale = 1;
    }

    public void LoadLevel(int level, bool wantSurvivedIncrease)
    {
        loadCanvas.SetActive(false);

        gameOverScreen.SetActive(false);

        levels[currentLevelIndex].gameObject.SetActive(false);
        levels[level].gameObject.SetActive(true);

        player.transform.position = new Vector3(0,0,0);
        currentLevelIndex = level;
        if(wantSurvivedIncrease) survivedLevelsCount++;
    }

    void LoadNextLevel()
    {
        if(currentLevelIndex == 4)
        {
            GameCompleteScreen();
        }
        else
        {
            int nextLevelIndex = currentLevelIndex + 1;
            LoadLevel(nextLevelIndex, true);
        }
        
    }

    void DisableLevels()
    {
        for(int i = 0; i < levels.Count; i++)
        {
            levels[i].SetActive(false);
        }
    }
}
