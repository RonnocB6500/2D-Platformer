using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 7;
    private int currentHealth;

    public HealthUI healthUI;

    private SpriteRenderer spriteRenderer;

    public static event Action OnPlayerDied;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetHealth();

        spriteRenderer = GetComponent<SpriteRenderer>();
        GameController.OnReset += ResetHealth;
        PlayerMovement.PlayerReset += ResetHealth;
        DoorController.OpenedDoor += ResetHealth;
        HealthItem.OnHealthCollect += Heal;
        LevelSelect.OnLevel += GameReset;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Dragon dragon = collision.GetComponent<Dragon>();
        // if(dragon)
        // {   
        //     TakeDamage(dragon.damage);
        //     SoundEffectManager.Play("PlayerHit");
        // }
        Trap trap = collision.GetComponent<Trap>();
        if(trap && trap.damage > 0)
        {
            TakeDamage(trap.damage);
            SoundEffectManager.Play("PlayerHit");
        }
        else if(trap)
        {
            SoundEffectManager.Play("BounceTrap");
        }
        SpikeTile spike = collision.GetComponent<SpikeTile>();
        if(spike)
        {
            TakeDamage(spike.damage);
            SoundEffectManager.Play("PlayerHit");
        }
        Fireball fireball = collision.GetComponent<Fireball>();
        if(fireball)
        {
            TakeDamage(fireball.damage);
            SoundEffectManager.Play("PlayerHit");
            Destroy(fireball.gameObject);
        }
    }

    void Heal(int amount)
    {
        currentHealth += amount;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthUI.UpdateHearts(currentHealth);
    }

    void ResetHealth()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        if(currentHealth <= 0)
        {
            //player dead
            OnPlayerDied.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    void GameReset()
    {
        spriteRenderer.color = Color.white;
    }
}
