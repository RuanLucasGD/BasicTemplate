using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FlaxEditor;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace BasicTemplate;

/// <summary>
/// RoadTerrain Script.
/// </summary>
public unsafe class RoadTerrain : Script
{
    public struct BasePath
    {
        public int Index;
        public Vector3 Position;
    }

    public struct FillPath
    {
        public int Index;
        public Vector3 Position;
    }

    public Spline RoadPath;

    private float* _heightMap;
    private Terrain _terrain;
    private float _pathLength;

    public int ChunkSize => _terrain.ChunkSize;
    public int HeightMapSize => ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
    public int HeightMapLength => HeightMapSize * HeightMapSize;

    public BasePath[] DefaultPath;
    public HashSet<FillPath> FullPath;

    /// <inheritdoc/>
    public override void OnStart()
    {
        Int2 pathCoord = Int2.Zero;
        _terrain = Level.FindActor<Terrain>();
        _heightMap = TerrainTools.GetHeightmapData(_terrain, ref pathCoord);

        _pathLength = 0f;
        for (int i = 0; i < RoadPath.SplinePointsCount - 1; i++)
        {
            _pathLength += Vector3.Distance(RoadPath.GetSplinePoint(i), RoadPath.GetSplinePoint(i + 1));
        }

        DefaultPath = new BasePath[(int)(_pathLength / 200)];

        var point = RoadPath.GetSplinePoint(0);

        DefaultPath[0] = new BasePath
        {
            Index = PositionToIndex(point),
            Position = point
        };

        for (int i = 0; i < DefaultPath.Length; i++)
        {
            var time = RoadPath.GetSplineTimeClosestToPoint(point);
            point = RoadPath.GetSplinePointClosestToPoint(point + (RoadPath.GetSplineDirection(time) * 200));

            DefaultPath[i] = new BasePath
            {
                Index = PositionToIndex(point),
                Position = point
            };
        }

        FullPath = new HashSet<FillPath>();
        for (int i = 0; i < DefaultPath.Length; i++)
        {
            FullPath.Add(new FillPath
            {
                Index = DefaultPath[i].Index,
                Position = DefaultPath[i].Position
            });

            for (int x = -5; x < 5; x++)
            {
                for (int z = -5; z < 5; z++)
                {
                    var position = DefaultPath[i].Position + new Vector3(x * 100, 0, z * 100);
                    var index = PositionToIndex(position);
                    FullPath.Add(new FillPath
                    {
                        Index = index,
                        Position = position
                    });
                }
            }
        }

        Debug.Log(DefaultPath.Length);
        Debug.Log(FullPath.Count);

        float* h = _heightMap;

        foreach (var item in FullPath)
        {
            h[item.Index] = item.Position.Y;
        }

        var patchCoord = new Int2(0, 0);
        _terrain.SetupPatchHeightMap(ref patchCoord, HeightMapLength, h, null, true);

    }

    public override void OnDestroy()
    {
        Marshal.FreeHGlobal((IntPtr)_heightMap);
    }

    public override void OnUpdate()
    {
        for (int i = 0; i < DefaultPath.Length; i++)
        {
            DebugDraw.DrawSphere(new BoundingSphere(DefaultPath[i].Position, 100), Color.Yellow);
        }
    }

    private int PositionToIndex(Vector3 position)
    {
        return Mathf.RoundToInt((position.X / 100f) + (Mathf.RoundToInt(position.Z / 100f) * HeightMapSize));
    }

    // Método para converter um índice para uma posição (Vector3) no terreno
    public void IndexToPosition(int index, out float x, out float z)
    {
        x = (index / HeightMapSize) * 100;
        z = (index % HeightMapSize) * 100;
    }
}
