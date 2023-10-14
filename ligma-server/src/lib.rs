use log;
use rand::Rng;
use spacetimedb::{spacetimedb, Identity, ReducerContext, SpacetimeType, Timestamp};

#[spacetimedb(table)]
#[derive(Clone)]
pub struct Config {
    /// Always 0, used to store a singleton global state
    #[primarykey]
    pub version: u32,

    pub message_of_the_day: String,

    /// The limits of the map, (-`map_extents` to +`map_extents`)
    pub map_extents: u32,

    /// Max objects
    pub num_object_nodes: u32,
}

/// Allows us to access any spawnable entity in the world by its `entity_id`
#[spacetimedb(table)]
#[derive(Clone, Debug)]
pub struct SpawnableEntityComponent {
    #[primarykey]
    #[autoinc]
    pub entity_id: u64,
}

#[derive(Clone, Debug)]
#[spacetimedb(table)]
pub struct PlayerComponent {
    #[primarykey]
    pub entity_id: u64,

    #[unique]
    pub owner_id: Identity,

    /// Username that is provided to the `create_player` reducer
    pub username: String,

    /// Reflects the login state of the user
    pub logged_in: bool,
}

#[derive(SpacetimeType, Clone, Debug)]
pub enum ObjectNodeType {
    Asteroid,
}

#[spacetimedb(table)]
#[derive(Clone, Debug)]
pub struct ObjectNodeComponent {
    #[primarykey]
    pub entity_id: u64,

    /// Object type of this [`ObjectNodeComponent`]
    pub object_type: ObjectNodeType,
}

/// Stores 2D positions
#[derive(SpacetimeType, Clone, Debug)]
pub struct StdbVector2 {
    pub x: f32,
    pub z: f32,
}

impl StdbVector2 {
    /// Zero implementation of the 2D vector
    pub const ZERO: StdbVector2 = StdbVector2 { x: 0.0, z: 0.0 };
}

/// Created for all world objects that can move throughout the world.
///
/// This keeps track of the position the last time the component was
/// updated and the direction the mobile object is currently moving
#[spacetimedb(table)]
#[derive(Clone, Debug)]
pub struct MobileLocationComponent {
    #[primarykey]
    pub entity_id: u64,

    /// The last known location of this entity
    pub location: StdbVector2,
    /// Movement direction, {0,0} if not moving
    pub direction: StdbVector2,
    /// Timestamp then movement started, Timestamp::UNIX_EPOCH if not moving
    pub move_start_timestamp: Timestamp,
}

/// Entity component that stores only position and rotation
#[spacetimedb(table)]
#[derive(Clone, Debug)]
pub struct StaticLocationComponent {
    #[primarykey]
    pub entity_id: u64,

    pub location: StdbVector2,
    pub rotation: f32,
}

/// Called when the user logs in for the first time and enters a username
#[spacetimedb(reducer)]
pub fn create_player(ctx: ReducerContext, username: String) -> Result<(), String> {
    let owner_id = ctx.sender;
    // We check to see if there is already a PlayerComponent with this identity.
    // this should never happen because the client only calls it if no player
    // is found.
    if PlayerComponent::filter_by_owner_id(&owner_id).is_some() {
        log::info!("Player already exists");
        return Err("Player already exists".to_string());
    }

    // Next we create the SpawnableEntityComponent. The entity_id for this
    // component automatically increments and we get it back from the result
    // of the insert call and use it for all components.

    let entity_id = SpawnableEntityComponent::insert(SpawnableEntityComponent { entity_id: 0 })
        .expect("Failed to create player spawnable entity component.")
        .entity_id;

    // The PlayerComponent uses the same entity_id and stores the identity of
    // the owner, username, and whether or not they are logged in.
    PlayerComponent::insert(PlayerComponent {
        entity_id,
        owner_id,
        username: username.clone(),
        logged_in: true,
    })
    .expect("Failed to insert player component.");

    // The MobileLocationComponent is used to calculate the current position
    // of an entity that can move smoothly in the world. We are using 2d
    // positions and the client will use the terrain height for the y value.
    MobileLocationComponent::insert(MobileLocationComponent {
        entity_id,
        location: StdbVector2::ZERO,
        direction: StdbVector2::ZERO,
        move_start_timestamp: Timestamp::UNIX_EPOCH,
    })
    .expect("Failed to insert player mobile entity component.");

    log::info!("Player created: {}({})", username, entity_id);

    Ok(())
}

/// Module initializer, craetes global config
#[spacetimedb(init)]
pub fn init() {
    Config::insert(Config {
        version: 0,
        message_of_the_day: "How do you do, fellow kids?".to_string(),
        map_extents: 50,
        num_object_nodes: 30,
    })
    .expect("Failed to insert config");

    // scheduler our repeating spawner to start after 1 second of module being loaded
    spacetimedb::schedule!("1000ms", object_spawner_agent(_, Timestamp::now()));
}

/// called when the client connects, updates the logged_in state to true
#[spacetimedb(connect)]
pub fn identity_connected(ctx: ReducerContext) {
    update_player_login_state(ctx, true);
}

/// Called when the client disconnects, updates the logged_in state to false
#[spacetimedb(disconnect)]
pub fn identity_disconnected(ctx: ReducerContext) {
    update_player_login_state(ctx, false);
}

/// This helper function gets the PlayerComponent, sets the logged
/// in variable and updates the SpacetimeDB table row.
pub fn update_player_login_state(ctx: ReducerContext, logged_in: bool) {
    if let Some(player) = PlayerComponent::filter_by_owner_id(&ctx.sender) {
        let entity_id = player.entity_id;
        // We clone the PlayerComponent so we can edit it and pass it back.
        let mut player = player.clone();
        player.logged_in = logged_in;
        log::debug!("Player: {:?} has logged in", player);
        PlayerComponent::update_by_entity_id(&entity_id, player);
    }
}

/// Update the MobileLocationComponent with the current movement
/// values. The client will call this regularly as the direction of movement
/// changes.
///
/// TODO: validate moves before commiting the movement to the DB
#[spacetimedb(reducer)]
pub fn move_player(
    ctx: ReducerContext,
    start: StdbVector2,
    direction: StdbVector2,
) -> Result<(), String> {
    let owner_id = ctx.sender;
    // First, look up the player using the sender identity, then use that
    // entity_id to retrieve and update the MobileLocationComponent
    if let Some(player) = PlayerComponent::filter_by_owner_id(&owner_id) {
        if let Some(mut mobile) = MobileLocationComponent::filter_by_entity_id(&player.entity_id) {
            mobile.location = start;
            mobile.direction = direction;
            mobile.move_start_timestamp = ctx.timestamp;
            log::debug!(
                "Updating player location to: {:?}, direction to: {:?}",
                mobile.location,
                mobile.direction
            );
            MobileLocationComponent::update_by_entity_id(&player.entity_id, mobile);

            return Ok(());
        }
    }

    // If we can not find the PlayerComponent for this user something went wrong.
    // This should never happen.
    return Err("Player not found".to_string());
}

/// Update the MobileLocationComponent when a player comes to a stop. We set
/// the location to the current location and the direction to {0,0}
#[spacetimedb(reducer)]
pub fn stop_player(ctx: ReducerContext, location: StdbVector2) -> Result<(), String> {
    let owner_id = ctx.sender;
    if let Some(player) = PlayerComponent::filter_by_owner_id(&owner_id) {
        if let Some(mut mobile) = MobileLocationComponent::filter_by_entity_id(&player.entity_id) {
            mobile.location = location;
            mobile.direction = StdbVector2::ZERO;
            mobile.move_start_timestamp = Timestamp::UNIX_EPOCH;
            log::debug!("Stopping player: {:?}", player);
            MobileLocationComponent::update_by_entity_id(&player.entity_id, mobile);

            return Ok(());
        }
    }

    return Err("Player not found".to_string());
}

#[spacetimedb(reducer, repeat=1sec)]
pub fn object_spawner_agent(_ctx: ReducerContext, _prev_time: Timestamp) -> Result<(), String> {
    let config = Config::filter_by_version(&0).ok_or("Failed to find valid config".to_string())?;

    // get maximum number of nodes to spawn
    let object_limit = config.num_object_nodes as usize;

    // get number of nodes, exit if the limit is already met
    let spawned_object_count = ObjectNodeComponent::iter().count();
    if spawned_object_count >= object_limit {
        log::info!("Maximum objects spawn, skipping spawn.");
        return Ok(());
    }

    // pick random coordinates inside the map limits
    let mut rng = rand::thread_rng();
    let map_limits = config.map_extents as f32;
    let location = StdbVector2 {
        x: rng.gen_range(-map_limits..map_limits),
        z: rng.gen_range(-map_limits..map_limits),
    };

    // pick random rotation
    let rotation = rng.gen_range(0.0..360.0);

    // insert new `SpawnableEntityComponent`, getting the new `entity_id`
    let entity_id =
        SpawnableEntityComponent::insert(SpawnableEntityComponent { entity_id: 0 })?.entity_id;

    // from the new `entity_id`, create a `StaticEntityComponent`
    StaticLocationComponent::insert(StaticLocationComponent {
        entity_id,
        location: location.clone(),
        rotation,
    })?;

    // insert the new object
    ObjectNodeComponent::insert(ObjectNodeComponent {
        entity_id,
        object_type: ObjectNodeType::Asteroid,
    })?;

    log::info!(
        "Object spawned: {} at ({}, {})",
        entity_id,
        location.x,
        location.z
    );

    Ok(())
}


