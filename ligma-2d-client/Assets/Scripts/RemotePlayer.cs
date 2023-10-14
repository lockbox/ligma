using SpacetimeDB.Types;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public ulong EntityId;

    public TMP_Text UsernameElement;

    public string Username { set { UsernameElement.text = value; } }

    void Start()
    {
        // initialize overhead name
        UsernameElement = GetComponentInChildren<TMP_Text>();
        var canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;

        // get the username from the PlayerComponent for this object and set it in the UI
        PlayerComponent playerComp = PlayerComponent.FilterByEntityId(EntityId);
        Username = playerComp.Username;

        // get the last location for this player and set the initial
        // position
        MobileLocationComponent mobPos = MobileLocationComponent.FilterByEntityId(EntityId);
        Vector3 playerPos = new Vector3(mobPos.Location.X, 0.0f, mobPos.Location.Z);
        transform.position = new Vector3(playerPos.x, 0.0f, playerPos.z);

        // register for a callback that is called when the client gets an
        // update for a row in the MobileLocationComponent table
        MobileLocationComponent.OnUpdate += MobileLocationComponent_OnUpdate;
    }

    private void MobileLocationComponent_OnUpdate(MobileLocationComponent oldObj, MobileLocationComponent obj, ReducerEvent callInfo)
    {
        // if the update was made to this object
        if (obj.EntityId == EntityId)
        {
            // update the DirectionVec in the PlayerMovementController component with the updated values
            var movementController = GetComponent<PlayerController>();
            //movementController.DirectionVec = new Vector3(obj.Direction.X, 0.0f, obj.Direction.Z);
            // if DirectionVec is {0,0,0} then we came to a stop so correct our position to match the server
            //if (movementController.DirectionVec == Vector3.zero)
            //{
             ///   Vector3 playerPos = new Vector3(obj.Location.X, 0.0f, obj.Location.Z);
                //transform.position = new Vector3(playerPos.x, 0.0f, playerPos.z);
            //}
        }
    }
}
