using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Local;

    public Transform playerSprite;
    public GameObject exhaustSprite;

    [SerializeField]
    private float moveSpeed = 150.0f;
    [SerializeField]
    private float rotationSpeed = 0.65f;
    [SerializeField]
    private float maxMoveSpeed = 1.5f;

    private Rigidbody2D r;

    private float horizontal, vertical;
    private Vector2 moveDirection;
    
    private float targetRotation, currentRotation;
    private float _rotation_v;

    private void Awake()
    {
        r = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        moveDirection = Vector2.up * vertical + Vector2.right * horizontal;
        moveDirection.Normalize();
        r.AddForce(moveSpeed * Time.deltaTime * moveDirection);
         
        r.velocity = Vector2.ClampMagnitude(r.velocity, maxMoveSpeed);

        if (moveDirection.magnitude > 0)
        {
            targetRotation = Vector2.SignedAngle(Vector2.up, moveDirection);
            currentRotation = Mathf.SmoothDampAngle(currentRotation, targetRotation, ref _rotation_v, rotationSpeed);
            playerSprite.eulerAngles = new Vector3(0.0f, 0.0f, currentRotation);
            exhaustSprite.SetActive(true);
        }
        else
        {
            exhaustSprite.SetActive(false);
        }      
    }
}
