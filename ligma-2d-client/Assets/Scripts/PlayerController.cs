using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float userSpeed = 2;
    public float rotateSpeed = 3;
    public Transform player_sprite;
    float _horizontal_input, _vertical_input;
    Vector2 horizontal_vector, vertical_vector, move_vector;

    float diff_angle;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _horizontal_input = Input.GetAxis("Horizontal");
        _vertical_input = Input.GetAxis("Vertical");
        horizontal_vector = Vector2.right * _horizontal_input;
        vertical_vector = Vector2.up * _vertical_input;
        move_vector = (horizontal_vector + vertical_vector) * userSpeed * Time.deltaTime;
        transform.Translate(move_vector);

        if (move_vector != Vector2.zero)
        {
            diff_angle = Mathf.Atan2(move_vector.y - Vector2.up.y, move_vector.x - Vector2.up.x) * 180 / Mathf.PI;
            Debug.Log(diff_angle);

            player_sprite.Rotate(0.0f, 0.0f, diff_angle * rotateSpeed * Time.deltaTime);
        }
    }
}
