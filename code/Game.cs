﻿using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sandbox.Csg;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Sandbox;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.Game
{
	public CsgSolid CsgWorld { get; private set; }

	public MyGame()
    {
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn();
		client.Pawn = pawn;

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CsgWorld ??= new CsgSolid();

		CsgWorld.Combine(
			CsgConvexSolid.CreateCube(
				new BBox(
					new Vector3( -4096f, -4096f, 1024f ),
					new Vector3( 4096f, 4096f, 2048f ) ) ),
			CsgOperator.Add );

		CsgWorld.Combine(
			CsgConvexSolid.CreateDodecahedron(
				Vector3.Up * 2048f, 512f ),
			CsgOperator.Subtract );
	}
}