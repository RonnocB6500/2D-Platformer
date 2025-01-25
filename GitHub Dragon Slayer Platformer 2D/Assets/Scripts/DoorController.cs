using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    public Transform player;
    private Vector2 playerDirection;
    private Vector2 distance;

    public Transform transform;

    public static event Action OpenedDoor;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        playerDirection = new Vector2(player.position.x - transform.position.x, player.position.y - transform.position.y);
        distance = new Vector2(playerDirection.x, playerDirection.y);
        //print("Distance: " + distance.sqrMagnitude);
    }

    public void OpenDoor(InputAction.CallbackContext context)
    {
        if(context.performed && (distance.sqrMagnitude < 10))
        {
            animator.SetBool("isOpen", true);
            StartCoroutine(WaitToOpen());
        }
    }

    public IEnumerator WaitToOpen()
    {   
        yield return new WaitForSeconds(1);
        OpenedDoor.Invoke();
    }
}
