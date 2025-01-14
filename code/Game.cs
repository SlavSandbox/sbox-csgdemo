﻿using Sandbox;
using Sandbox.UI.Construct;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sandbox.Csg;

namespace CsgDemo;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class CsgDemoGame : GameManager
{
    public new static CsgDemoGame Current => GameManager.Current as CsgDemoGame;

    public CsgBrush CubeBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/cube.csg" );
    public CsgBrush DodecahedronBrush { get; } = ResourceLibrary.Get<CsgBrush>( "brushes/dodecahedron.csg" );

    public CsgMaterial DefaultMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csgdemo/default.csgmat" );
    public CsgMaterial RedMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csgdemo/red.csgmat" );
    public CsgMaterial ScorchedMaterial { get; } = ResourceLibrary.Get<CsgMaterial>( "materials/csgdemo/scorched.csgmat" );

    [Net]
    public CsgSolid CsgWorld { get; private set; }

    public CsgDemoGame()
    {
    }

    /// <summary>
    /// A client has joined the server. Make them a pawn to play with
    /// </summary>
    public override void ClientJoined( IClient client )
    {
        base.ClientJoined( client );

        if ( CsgWorld == null )
        {
            SpawnWorld();
        }

        // Create a pawn for this client to play with
        var pawn = new Player( client );
        client.Pawn = pawn;
        pawn.Respawn();

        // Get all of the spawnpoints
        var spawnpoints = All.OfType<SpawnPoint>();

        // chose a random one
        var randomSpawnPoint = spawnpoints.OrderBy( x => System.Guid.NewGuid() ).FirstOrDefault();

        // if it exists, place the pawn there
        if ( randomSpawnPoint != null )
        {
            var tx = randomSpawnPoint.Transform;
            tx.Position = tx.Position + Vector3.Up * 2048.0f; // raise it up
            pawn.Transform = tx;
        }
    }

    private void SpawnWorld()
    {
        Assert.True( IsServer );

        CsgWorld = new CsgSolid( 1024f );

        CsgWorld.Add( CubeBrush,
            DefaultMaterial,
            scale: new Vector3( 8192f, 8192f, 1024f ) );

        for ( var i = -3; i <= 3; ++i )
        {
            BuildHouse( new Vector3( i * 512f, 512f, 512f ), Random.Int( 2, 10 ) );
            BuildHouse( new Vector3( i * 512f, -512f, 512f ), Random.Int( 2, 10 ) );
        }
    }

    private void AddCube( Vector3 min, Vector3 max )
    {
        CsgWorld.Add( CubeBrush, DefaultMaterial, (min + max) * 0.5f, max - min );
    }

    private void SubtractCube( Vector3 min, Vector3 max )
    {
        CsgWorld.Subtract( CubeBrush, (min + max) * 0.5f, max - min );
    }

    private void BuildHouse( Vector3 floorPos, int floorCount )
    {
        const float width = 384f;
        const float depth = 256f;
        const float floorHeight = 128f;
        const float windowHeight = 64f;
        const float windowWidth = 128f;
        const float windowFloorOffset = 32f;
        const float wallThickness = 16f;

        AddCube(
            floorPos - new Vector3( width * 0.5f, depth * 0.5f, 0f ),
            floorPos + new Vector3( width * 0.5f, depth * 0.5f, floorHeight * floorCount ) );

        Vector3 windowPos;

        for ( var i = 0; i < floorCount; ++i )
        {
            SubtractCube(
                floorPos - new Vector3( width * 0.5f - wallThickness, depth * 0.5f - wallThickness, 0f ),
                floorPos + new Vector3( width * 0.5f - wallThickness, depth * 0.5f - wallThickness, floorHeight - wallThickness ) );

            windowPos = floorPos + new Vector3( -width * 0.25f, (depth - wallThickness) * 0.5f, windowFloorOffset );

            SubtractCube(
                windowPos - new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, 0f ),
                windowPos + new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, windowHeight ) );

            windowPos = floorPos + new Vector3( width * 0.25f, (depth - wallThickness) * 0.5f, windowFloorOffset );

            SubtractCube(
                windowPos - new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, 0f ),
                windowPos + new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, windowHeight ) );

            windowPos = floorPos + new Vector3( -width * 0.25f, -(depth - wallThickness) * 0.5f, windowFloorOffset );

            SubtractCube(
                windowPos - new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, 0f ),
                windowPos + new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, windowHeight ) );

            windowPos = floorPos + new Vector3( width * 0.25f, -(depth - wallThickness) * 0.5f, windowFloorOffset );

            SubtractCube(
                windowPos - new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, 0f ),
                windowPos + new Vector3( windowWidth * 0.5f, wallThickness * 0.5f, windowHeight ) );

            floorPos += Vector3.Up * floorHeight;
        }
    }
}
