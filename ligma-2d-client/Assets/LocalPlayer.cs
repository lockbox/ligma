using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static LocalPlayer instance;

    void Start()
    {
        instance = this;
        PlayerController.Local = GetComponent<PlayerController>();
    }
}
