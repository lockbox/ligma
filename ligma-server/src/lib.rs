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
pub struct SpawnEntityComponent {
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

