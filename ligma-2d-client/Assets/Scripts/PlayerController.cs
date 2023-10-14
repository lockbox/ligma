using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int userSpeed = 2;
    float _horizontal_input, _vertical_input;
    Vector2 horizontal_vector, vertical_vector;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _horizontal_input = Input.GetAxis("Horizontal");
        _vertical_input = Input.GetAxis("Vertical");
        horizontal_vector = Vector2.right * _horizontal_input * userSpeed * Time.deltaTime;
        vertical_vector = Vector2.up * _vertical_input * userSpeed * Time.deltaTime;
        transform.Translate(horizontal_vector + vertical_vector);
    }
}
