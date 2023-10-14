using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SpacetimeDB.Types;

public class Username : Singleton<Username>
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _usernameField;

    private bool _initialized = false;

    public void Show()
    {
        Debug.Log("showing");
        if (!_initialized)
        {
            _initialized = true;
            _panel.SetActive(true);
        }
    }

    private void OnEnable()
    {
        _panel.SetActive(false);
    }

    public void ButtonPressed()
    {
        _panel.SetActive(false);

        // Call the SpacetimeDB CreatePlayer reducer 
        Reducer.CreatePlayer(_usernameField.text);
    }
}
