use spacetimedb::{spacetimedb, Identity, SpacetimeType, Timestamp, ReducerContext};
use log;

#[spacetimedb(table)]
#[derive(Clone)]
pub struct Config {
    #[primarykey]
    pub version: u32,

    pub message_of_the_day: String,
}

/// Allows us to access any spawnable entity in the world by its `entity_id`
#[spacetimedb(table)]
pub struct SpawnableEntityComponent {
    #[primarykey]
    #[autoinc]
    pub entity_id: u64,
}

#[derive(Clone)]
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

/// Stores 2D positions
#[derive(SpacetimeType, Clone)]
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
#[derive(Clone)]
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
    }).expect("Failed to insert player mobile entity component.");

    log::info!("Player created: {}({})", username, entity_id);

    Ok(())
}

/// Module initializer, craetes global config
#[spacetimedb(init)]
pub fn init() {
    Config::insert(Config { version: 0, message_of_the_day: "How do you do, fellow kids?".to_string() }).expect("Failed to insert config");
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
            MobileLocationComponent::update_by_entity_id(&player.entity_id, mobile);

            return Ok(());
        }
    }

    return Err("Player not found".to_string());
}