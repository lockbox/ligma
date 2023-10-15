using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float userSpeed = 1.25;
    public float rotateSpeed = 1.5;
    public float maxSpeed = 1.5;
    public Transform player_sprite;
    float _horizontal_input, _vertical_input;

    Vector2 player_movement_vector = Vector2.zero;
    Vector2 horizontal_vector, vertical_vector, move_vector;

    float target_angle;

    public static PlayerController Local;

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
        move_vector = horizontal_vector + vertical_vector;

        player_movement_vector += move_vector / 500;
        player_movement_vector = Vector2.ClampMagnitude(player_movement_vector, maxSpeed);

        transform.Translate(player_movement_vector * userSpeed * Time.deltaTime);

        if (move_vector != Vector2.zero)
        {
            target_angle = Vector2.Angle(Vector2.up, move_vector);
            // need to figure out how tf to do these angles
            if (_horizontal_input > 0)
                target_angle += 180;
            player_sprite.transform.eulerAngles = new Vector3(0.0f, 0.0f, Mathf.Lerp(player_sprite.transform.eulerAngles.z, target_angle, Time.deltaTime * rotateSpeed));
        }
    }
}
