using UnityEngine;

public class Levels : MonoBehaviour
{
    public GameObject levels;
    void Start()
    {
        MenuButtonController.OnMenu += disable;
        LevelSelect.OnLevel += enable;
    }
    void disable()
    {
        levels.SetActive(false);
    }

    void enable()
    {
        levels.SetActive(true);
    }
}
