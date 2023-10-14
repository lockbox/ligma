// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

using System;
using ClientApi;
using Newtonsoft.Json.Linq;
using SpacetimeDB;

namespace SpacetimeDB.Types
{
	public static partial class Reducer
	{
		public delegate void ObjectSpawnerAgentHandler(ReducerEvent reducerEvent, ulong prevTime);
		public static event ObjectSpawnerAgentHandler OnObjectSpawnerAgentEvent;

		public static void ObjectSpawnerAgent(ulong prevTime)
		{
			var _argArray = new object[] {prevTime};
			var _message = new SpacetimeDBClient.ReducerCallRequest {
				fn = "object_spawner_agent",
				args = _argArray,
			};
			SpacetimeDBClient.instance.InternalCallReducer(Newtonsoft.Json.JsonConvert.SerializeObject(_message, _settings));
		}

		[ReducerCallback(FunctionName = "object_spawner_agent")]
		public static bool OnObjectSpawnerAgent(ClientApi.Event dbEvent)
		{
			if(OnObjectSpawnerAgentEvent != null)
			{
				var args = ((ReducerEvent)dbEvent.FunctionCall.CallInfo).ObjectSpawnerAgentArgs;
				OnObjectSpawnerAgentEvent((ReducerEvent)dbEvent.FunctionCall.CallInfo
					,(ulong)args.PrevTime
				);
				return true;
			}
			return false;
		}

		[DeserializeEvent(FunctionName = "object_spawner_agent")]
		public static void ObjectSpawnerAgentDeserializeEventArgs(ClientApi.Event dbEvent)
		{
			var args = new ObjectSpawnerAgentArgsStruct();
			var bsatnBytes = dbEvent.FunctionCall.ArgBytes;
			using var ms = new System.IO.MemoryStream();
			ms.SetLength(bsatnBytes.Length);
			bsatnBytes.CopyTo(ms.GetBuffer(), 0);
			ms.Position = 0;
			using var reader = new System.IO.BinaryReader(ms);
			var args_0_value = SpacetimeDB.SATS.AlgebraicValue.Deserialize(SpacetimeDB.SATS.AlgebraicType.CreatePrimitiveType(SpacetimeDB.SATS.BuiltinType.Type.U64), reader);
			args.PrevTime = args_0_value.AsU64();
			dbEvent.FunctionCall.CallInfo = new ReducerEvent(ReducerType.ObjectSpawnerAgent, "object_spawner_agent", dbEvent.Timestamp, Identity.From(dbEvent.CallerIdentity.ToByteArray()), Address.From(dbEvent.CallerAddress.ToByteArray()), dbEvent.Message, dbEvent.Status, args);
		}
	}

	public partial class ObjectSpawnerAgentArgsStruct
	{
		public ulong PrevTime;
	}

}