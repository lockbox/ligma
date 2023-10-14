using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB;
using SpacetimeDB.Types;

public class GameManager : MonoBehaviour
{
    // These are connection variables that are exposed on the GameManager
    // inspector. The cloud version of SpacetimeDB needs sslEnabled = true
    [SerializeField] private string moduleAddress = "YOUR_MODULE_DOMAIN_OR_ADDRESS";
    [SerializeField] private string hostName = "localhost:3000";
    [SerializeField] private bool sslEnabled = false;

    // This is the identity for this player that is automatically generated
    // the first time you log in. We set this variable when the
    // onIdentityReceived callback is triggered by the SDK after connecting
    private Identity local_identity;
    private Address local_address;

    public static GameManager instance;


    public GameObject playerPrefab;

    private void Start()
    {
        instance = this;

        // When we connect to SpacetimeDB we send our subscription queries
        // to tell SpacetimeDB which tables we want to get updates for.
        SpacetimeDBClient.instance.onConnect += () =>
        {
            Debug.Log("Connected.");

            SpacetimeDBClient.instance.Subscribe(new List<string>()
            {
                "SELECT * FROM Config",
                "SELECT * FROM SpawnableEntityComponent",
                "SELECT * FROM PlayerComponent",
                "SELECT * FROM MobileLocationComponent",
            });
        };

        // called when we have an error connecting to SpacetimeDB
        SpacetimeDBClient.instance.onConnectError += (error, message) =>
        {
            Debug.LogError($"Connection error: " + message);
        };

        // called when we are disconnected from SpacetimeDB
        SpacetimeDBClient.instance.onDisconnect += (closeStatus, error) =>
        {
            Debug.Log("Disconnected.");
        };


        // called when we receive the client identity from SpacetimeDB
        SpacetimeDBClient.instance.onIdentityReceived += (token, identity, address) => {
            AuthToken.SaveToken(token);
            local_identity = identity;
            local_address = address;
        };


        // called after our local cache is populated from a Subscribe call
        SpacetimeDBClient.instance.onSubscriptionApplied += OnSubscriptionApplied;

        // now that we’ve registered all our callbacks, lets connect to
        // spacetimedb
        SpacetimeDBClient.instance.Connect(AuthToken.Token, hostName, moduleAddress);
    }

    void OnSubscriptionApplied()
    {
        Debug.Log("test");
        // If we don't have any data for our player, then we are creating a
        // new one. Let's show the username dialog, which will then call the
        // create player reducer
        var player = PlayerComponent.FilterByOwnerId(local_identity);
        Debug.Log(player);
        if (player == null)
        {
            Debug.Log("is null");
            // Show username selection
            Username.instance.Show();
        }

        // Now that we've done this work we can unregister this callback
        SpacetimeDBClient.instance.onSubscriptionApplied -= OnSubscriptionApplied;
    }

    public void StartGame()
    {
        return;
    }
}
