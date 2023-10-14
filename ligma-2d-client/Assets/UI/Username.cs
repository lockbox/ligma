using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB.Types;
using TMPro;

public class Username : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _usernameField;

    private bool _initialized = false;

    public void Show()
    {
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
