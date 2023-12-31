// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

using System;
using ClientApi;
using Newtonsoft.Json.Linq;
using SpacetimeDB;

namespace SpacetimeDB.Types
{
	public enum ReducerType
	{
		None,
		CreatePlayer,
		MovePlayer,
		ObjectSpawnerAgent,
		StopPlayer,
	}

	public partial class ReducerEvent : ReducerEventBase
	{
		public ReducerType Reducer { get; private set; }

		public ReducerEvent(ReducerType reducer, string reducerName, ulong timestamp, SpacetimeDB.Identity identity, SpacetimeDB.Address? callerAddress, string errMessage, ClientApi.Event.Types.Status status, object args)
			: base(reducerName, timestamp, identity, callerAddress, errMessage, status, args)
		{
			Reducer = reducer;
		}

		public CreatePlayerArgsStruct CreatePlayerArgs
		{
			get
			{
				if (Reducer != ReducerType.CreatePlayer) throw new SpacetimeDB.ReducerMismatchException(Reducer.ToString(), "CreatePlayer");
				return (CreatePlayerArgsStruct)Args;
			}
		}
		public MovePlayerArgsStruct MovePlayerArgs
		{
			get
			{
				if (Reducer != ReducerType.MovePlayer) throw new SpacetimeDB.ReducerMismatchException(Reducer.ToString(), "MovePlayer");
				return (MovePlayerArgsStruct)Args;
			}
		}
		public ObjectSpawnerAgentArgsStruct ObjectSpawnerAgentArgs
		{
			get
			{
				if (Reducer != ReducerType.ObjectSpawnerAgent) throw new SpacetimeDB.ReducerMismatchException(Reducer.ToString(), "ObjectSpawnerAgent");
				return (ObjectSpawnerAgentArgsStruct)Args;
			}
		}
		public StopPlayerArgsStruct StopPlayerArgs
		{
			get
			{
				if (Reducer != ReducerType.StopPlayer) throw new SpacetimeDB.ReducerMismatchException(Reducer.ToString(), "StopPlayer");
				return (StopPlayerArgsStruct)Args;
			}
		}

		public object[] GetArgsAsObjectArray()
		{
			switch (Reducer)
			{
				case ReducerType.CreatePlayer:
				{
					var args = CreatePlayerArgs;
					return new object[] {
						args.Username,
					};
				}
				case ReducerType.MovePlayer:
				{
					var args = MovePlayerArgs;
					return new object[] {
						args.Start,
						args.Direction,
					};
				}
				case ReducerType.ObjectSpawnerAgent:
				{
					var args = ObjectSpawnerAgentArgs;
					return new object[] {
						args.PrevTime,
					};
				}
				case ReducerType.StopPlayer:
				{
					var args = StopPlayerArgs;
					return new object[] {
						args.Location,
					};
				}
				default: throw new System.Exception($"Unhandled reducer case: {Reducer}. Please run SpacetimeDB code generator");
			}
		}
	}
}
