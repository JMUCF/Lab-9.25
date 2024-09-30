using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class Player : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float speed;
    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = moveAction.action.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.y);
        transform.Translate(move * speed * Time.deltaTime, Space.World);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}