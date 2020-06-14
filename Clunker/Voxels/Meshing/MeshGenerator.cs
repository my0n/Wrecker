﻿using Clunker.Geometry;
using Clunker.Voxels.Space;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Clunker.Voxels.Meshing
{
    public class MeshGenerator
    {
        public static void GenerateMesh(VoxelGrid grid, VoxelTypes voxelTypes, Action<Voxel, VoxelSide, Quad> quadProcessor)
        {
            FindExposedSides(grid, voxelTypes, (v, x, y, z, side) =>
            {
                AddBlock(quadProcessor, v, x, y, z, side, grid.VoxelSize);
            });
        }

        public static void GenerateGridMesh(VoxelGrid space, Action<Voxel, VoxelSide, Quad> quadProcessor)
        {
            for (int x = 0; x < space.GridSize; x++)
                for (int y = 0; y < space.GridSize; y++)
                    for (int z = 0; z < space.GridSize; z++)
                    {
                        var voxel = space[x, y, z];
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.BOTTOM, space.VoxelSize);
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.EAST, space.VoxelSize);
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.WEST, space.VoxelSize);
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.TOP, space.VoxelSize);
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.NORTH, space.VoxelSize);
                        AddBlock(quadProcessor, voxel, x, y, z, VoxelSide.SOUTH, space.VoxelSize);
                    }
        }

        private static void AddBlock(Action<Voxel, VoxelSide, Quad> quadProcesor, Voxel voxel, float x, float y, float z, VoxelSide side, float voxelSize)
        {
            switch (side)
            {
                case VoxelSide.BOTTOM:
                    quadProcesor(voxel, side, new Quad(new Vector3(x, y, z) * voxelSize, new Vector3(x, y, z + 1) * voxelSize, new Vector3(x + 1, y, z + 1) * voxelSize, new Vector3(x + 1, y, z) * voxelSize, -Vector3.UnitY));
                    break;
                case VoxelSide.EAST:
                    quadProcesor(voxel, side, new Quad(new Vector3(x + 1, y, z + 1) * voxelSize, new Vector3(x + 1, y + 1, z + 1) * voxelSize, new Vector3(x + 1, y + 1, z) * voxelSize, new Vector3(x + 1, y, z) * voxelSize, Vector3.UnitX));
                    break;
                case VoxelSide.WEST:
                    quadProcesor(voxel, side, new Quad(new Vector3(x, y, z) * voxelSize, new Vector3(x, y + 1, z) * voxelSize, new Vector3(x, y + 1, z + 1) * voxelSize, new Vector3(x, y, z + 1) * voxelSize, -Vector3.UnitX));
                    break;
                case VoxelSide.TOP:
                    quadProcesor(voxel, side, new Quad(new Vector3(x, y + 1, z + 1) * voxelSize, new Vector3(x, y + 1, z) * voxelSize, new Vector3(x + 1, y + 1, z) * voxelSize, new Vector3(x + 1, y + 1, z + 1) * voxelSize, Vector3.UnitY));
                    break;
                case VoxelSide.NORTH:
                    quadProcesor(voxel, side, new Quad(new Vector3(x + 1, y, z) * voxelSize, new Vector3(x + 1, y + 1, z) * voxelSize, new Vector3(x, y + 1, z) * voxelSize, new Vector3(x, y, z) * voxelSize, -Vector3.UnitZ));
                    break;
                case VoxelSide.SOUTH:
                    quadProcesor(voxel, side, new Quad(new Vector3(x, y, z + 1) * voxelSize, new Vector3(x, y + 1, z + 1) * voxelSize, new Vector3(x + 1, y + 1, z + 1) * voxelSize, new Vector3(x + 1, y, z + 1) * voxelSize, Vector3.UnitZ));
                    break;
            }
        }

        private static void FindExposedSides(VoxelGrid grid, VoxelTypes types, Action<Voxel, int, int, int, VoxelSide> sideProcessor)
        {
            for (int x = 0; x < grid.GridSize; x++)
                for (int y = 0; y < grid.GridSize; y++)
                    for (int z = 0; z < grid.GridSize; z++)
                    {
                        Voxel voxel = grid[x, y, z];
                        if (voxel.Exists)
                        {
                            if (ShouldRenderSide(grid, types, voxel.BlockType, x, y - 1, z))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.BOTTOM);
                            }

                            if (ShouldRenderSide(grid, types, voxel.BlockType, x + 1, y, z))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.EAST);
                            }

                            if (ShouldRenderSide(grid, types, voxel.BlockType, x - 1, y, z))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.WEST);
                            }

                            if (ShouldRenderSide(grid, types, voxel.BlockType, x, y + 1, z))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.TOP);
                            }

                            if (ShouldRenderSide(grid, types, voxel.BlockType, x, y, z - 1))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.NORTH);
                            }

                            if (ShouldRenderSide(grid, types, voxel.BlockType, x, y, z + 1))
                            {
                                sideProcessor(voxel, x, y, z, VoxelSide.SOUTH);
                            }
                        }
                    }
        }

        private static bool ShouldRenderSide(VoxelGrid grid, VoxelTypes types, ushort otherType, int x, int y, int z)
        {
            var exists = grid.Exists(x, y, z);
            if (!exists) return true;

            var voxel = grid[x, y, z];
            return types[voxel.BlockType].Transparent && voxel.BlockType != otherType;
        }
    }
}
