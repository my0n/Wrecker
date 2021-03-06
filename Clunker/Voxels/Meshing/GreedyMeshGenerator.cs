﻿using Clunker.Graphics;
using Clunker.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Clunker.Voxels.Meshing
{
    public class GreedyMeshGenerator
    {
        public static System.Collections.Concurrent.ConcurrentBag<double> Times = new System.Collections.Concurrent.ConcurrentBag<double>();

        public static void GenerateMesh(VoxelGrid voxels, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var stopwatch = Stopwatch.StartNew();
            var plane = new VoxelPoint[voxels.GridSize, voxels.GridSize];
            MeshPosZ(voxels, plane, quadProcessor);
            MeshNegZ(voxels, plane, quadProcessor);
            MeshPosX(voxels, plane, quadProcessor);
            MeshNegX(voxels, plane, quadProcessor);
            MeshPosY(voxels, plane, quadProcessor);
            MeshNegY(voxels, plane, quadProcessor);
            Times.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MeshPosZ(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int z = 0; z < gridSize; z++)
            {
                bool someFacesExist = false;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var open = z + 1 == gridSize || !voxels[x, y, z + 1].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[x, y] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                if(someFacesExist)
                {
                    var zPos = (z + 1);
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(rect.Left, rect.Top, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Left, rect.Bottom, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Right, rect.Bottom, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Right, rect.Top, zPos) * voxels.VoxelSize,
                            Vector3.UnitZ);
                        quadProcessor(typeNum, orientation, VoxelSide.SOUTH, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void MeshNegZ(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int z = 0; z < gridSize; z++)
            {
                bool someFacesExist = false;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var open = z == 0 || !voxels[x, y, z - 1].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[x, y] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                if(someFacesExist)
                {
                    var zPos = z;
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(rect.Right, rect.Top, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Right, rect.Bottom, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Left, rect.Bottom, zPos) * voxels.VoxelSize,
                            new Vector3(rect.Left, rect.Top, zPos) * voxels.VoxelSize,
                            -Vector3.UnitZ);
                        quadProcessor(typeNum, orientation, VoxelSide.NORTH, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void MeshPosX(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int x = 0; x < gridSize; x++)
            {
                bool someFacesExist = false;
                for (int z = 0; z < gridSize; z++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var open = x + 1 == gridSize || !voxels[x + 1, y, z].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[z, y] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                var xPos = (x + 1);
                if(someFacesExist)
                {
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(xPos, rect.Top, rect.Right) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Bottom, rect.Right) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Bottom, rect.Left) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Top, rect.Left) * voxels.VoxelSize,
                            Vector3.UnitX);
                        quadProcessor(typeNum, orientation, VoxelSide.EAST, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void MeshNegX(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int x = 0; x < gridSize; x++)
            {
                bool someFacesExist = false;
                for (int z = 0; z < gridSize; z++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var open = x == 0 || !voxels[x - 1, y, z].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[z, y] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                var xPos = x;
                if (someFacesExist)
                {
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(xPos, rect.Top, rect.Left) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Bottom, rect.Left) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Bottom, rect.Right) * voxels.VoxelSize,
                            new Vector3(xPos, rect.Top, rect.Right) * voxels.VoxelSize,
                            -Vector3.UnitX);
                        quadProcessor(typeNum, orientation, VoxelSide.WEST, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void MeshPosY(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int y = 0; y < gridSize; y++)
            {
                bool someFacesExist = false;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        var open = y + 1 == gridSize || !voxels[x, y + 1, z].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[x, z] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                var yPos = (y + 1);
                if (someFacesExist)
                {
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(rect.Left, yPos, rect.Top) * voxels.VoxelSize,
                            new Vector3(rect.Right, yPos, rect.Top) * voxels.VoxelSize,
                            new Vector3(rect.Right, yPos, rect.Bottom) * voxels.VoxelSize,
                            new Vector3(rect.Left, yPos, rect.Bottom) * voxels.VoxelSize,
                            Vector3.UnitY);
                        quadProcessor(typeNum, orientation, VoxelSide.TOP, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void MeshNegY(VoxelGrid voxels, VoxelPoint[,] plane, Action<ushort, VoxelSide, VoxelSide, Quad, Vector2i> quadProcessor)
        {
            var gridSize = voxels.GridSize;

            for (int y = 0; y < gridSize; y++)
            {
                bool someFacesExist = false;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        var open = y == 0 || !voxels[x, y - 1, z].Exists;
                        var voxel = voxels[x, y, z];
                        someFacesExist = (open && voxel.Exists) || someFacesExist;
                        plane[x, z] = new VoxelPoint() { Voxel = voxels[x, y, z], Processed = !open };
                    }
                }
                var yPos = y;
                if(someFacesExist)
                {
                    FindPlaneRects(plane, (typeNum, orientation, rect) =>
                    {
                        var quad = new Quad(
                            new Vector3(rect.Left, yPos, rect.Top) * voxels.VoxelSize,
                            new Vector3(rect.Left, yPos, rect.Bottom) * voxels.VoxelSize,
                            new Vector3(rect.Right, yPos, rect.Bottom) * voxels.VoxelSize,
                            new Vector3(rect.Right, yPos, rect.Top) * voxels.VoxelSize,
                            -Vector3.UnitY);
                        quadProcessor(typeNum, orientation, VoxelSide.BOTTOM, quad, new Vector2i(rect.Width, rect.Height));
                    });
                }
            }
        }

        private static void FindPlaneRects(VoxelPoint[,] plane, Action<ushort, VoxelSide, Rectangle> rectProcessor)
        {
            var xLength = plane.GetLength(0);
            var yLength = plane.GetLength(1);

            for (int y = 0; y < yLength; y++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    if (!plane[x, y].Processed && plane[x, y].Voxel.Exists)
                    {
                        var (newX, typeNum, orientation, rect) = FindRectangle(plane, xLength, yLength, x, y);
                        rectProcessor(typeNum, orientation, rect);
                        x = newX;
                    }
                }
            }
        }

        private static (int, ushort, VoxelSide, Rectangle) FindRectangle(VoxelPoint[,] plane, int xLength, int yLength, int startX, int startY)
        {
            var type = plane[startX, startY].Voxel.BlockType;
            var orientation = plane[startX, startY].Voxel.Orientation;
            var rect = new Rectangle(startX, startY, 1, 1);

            var x = startX + 1;
            while (x < xLength && plane[x, startY].Voxel.Exists && !plane[x, startY].Processed && plane[x, startY].Voxel.BlockType == type && plane[x, startY].Voxel.Orientation == orientation)
            {
                rect.Width++;
                x++;
            }

            var endX = x;

            var y = startY + 1;
            while (y < yLength)
            {
                x = startX;
                while (x < endX && plane[x, y].Voxel.Exists && !plane[x, y].Processed && plane[x, y].Voxel.BlockType == type && plane[x, y].Voxel.Orientation == orientation)
                {
                    x++;
                }

                if (x == endX)
                {
                    rect.Height++;
                    y++;
                }
                else
                {
                    break;
                }
            }

            for (var px = rect.Left; px < rect.Right; px++)
            {
                for (var py = rect.Top; py < rect.Bottom; py++)
                {
                    plane[px, py].Processed = true;
                }
            }

            return (endX, type, orientation, rect);
        }

        struct VoxelPoint
        {
            public Voxel Voxel;
            public bool Processed;
        }
    }
}
