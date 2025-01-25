using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class FallingPlatform : MonoBehaviour
{
    public GameObject fallingPlatform;
    public Transform platform;
    public float fallWait = 1f;
    public float destroyWait = 5f;

    bool isFalling;
    private Vector3 ogPosition;

    
    public Rigidbody2D rb;
    public GameObject gameObject;
    public int levelContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.GetComponent<Rigidbody2D>();
        ogPosition = platform.position;

        MenuButtonController.OnMenu += PlatformKill;
        GameController.OnReset += PlatformReset;
        GameController.OnStopLevel += PlatformKill;
        GameController.OnStartLevel += PlatformReset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isFalling && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall()
    {
        isFalling = true;
        yield return new WaitForSeconds(fallWait);
        rb.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(destroyWait);
        fallingPlatform.SetActive(false);
    }

    public void PlatformReset()
    {
        if((LevelSelect.selectedLevel == levelContainer) && (gameObject != null))
        {
            gameObject.SetActive(true);
            rb.bodyType = RigidbodyType2D.Static;
            transform.position = ogPosition;
            isFalling = false;
        }
    }

    public void PlatformKill()
    {
        if(gameObject != null)
        {
            //transform.position = ogPosition;
            rb.bodyType = RigidbodyType2D.Static;
            gameObject.SetActive(false);
        }
    }
}
