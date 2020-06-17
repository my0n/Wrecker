﻿using Clunker.Core;
using Clunker.Editor.FilePicker;
using Clunker.Editor.SelectedEntity;
using Clunker.Geometry;
using Clunker.Graphics;
using Clunker.Physics;
using Clunker.Physics.Voxels;
using Clunker.Voxels;
using Clunker.Voxels.Serialization;
using Clunker.Voxels.Space;
using DefaultEcs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Clunker.Editor.VoxelSpaceLoader
{
    public class VoxelSpaceLoader : Editor
    {
        public override string Name => "Voxel Space Loader";
        public override string Category => "Voxels";
        public override char? HotKey => 'L';

        private World _world;
        private EntitySet _selectedEntities;
        private Transform _transform;
        private MaterialInstance _voxelMaterialInstance;

        private string _fileLocation = "C:\\Clunker";

        public VoxelSpaceLoader(World world, Transform transform, MaterialInstance voxelMaterialInstance)
        {
            _world = world;
            _selectedEntities = _world.GetEntities().With<SelectedEntityFlag>().AsSet();
            _transform = transform;
            _voxelMaterialInstance = voxelMaterialInstance;
        }

        public override void DrawEditor(double delta)
        {
            foreach(var entity in _selectedEntities.GetEntities())
            {
                if(entity.Has<VoxelSpace>())
                {
                    ImGui.Text($"Entity {entity}");
                    ImGui.SameLine();
                    if (ImGui.Button("Save"))
                    {
                        FilePicker.FilePicker.Open("save-voxel-space");
                    }

                    if(FilePicker.FilePicker.Window("save-voxel-space", ref _fileLocation, new[] { ".cvx" }))
                    {
                        if(!Directory.Exists(_fileLocation))
                        {
                            ref var space = ref entity.Get<VoxelSpace>();
                            var voxelSpaceData = new VoxelSpaceData()
                            {
                                VoxelSize = space.VoxelSize,
                                GridSize = space.GridSize,
                                Grids = space.Members.Select(kvp => (kvp.Key, kvp.Value.Get<VoxelGrid>().Voxels)).ToArray()
                            };
                            VoxelSpaceDataSerializer.Serialize(voxelSpaceData, File.OpenWrite(_fileLocation));
                        }
                    }

                    ImGui.Separator();
                }
            }

            if (ImGui.Button("Load File"))
            {
                FilePicker.FilePicker.Open("load-voxel-space");
            }

            if (FilePicker.FilePicker.Window("load-voxel-space", ref _fileLocation, new[] { ".cvx" }))
            {
                if(File.Exists(_fileLocation))
                {
                    var voxelSpaceData = VoxelSpaceDataSerializer.Deserialize(File.OpenRead(_fileLocation));
                    LoadAsDynamic(voxelSpaceData);
                }
            }

            if(ImGui.Button("Load Empty"))
            {
                var voxels = new Voxel[8 * 8 * 8];
                voxels[0] = new Voxel() { Exists = true };
                LoadAsDynamic(new VoxelSpaceData()
                {
                    VoxelSize = 1,
                    GridSize = 8,
                    Grids = new[]
                    {
                        (new Vector3i(0, 0, 0), voxels)
                    }
                });
            }
        }

        private void LoadAsDynamic(VoxelSpaceData voxelSpaceData)
        {
            var spaceEntity = _world.CreateEntity();
            var spaceTransform = new Transform()
            {
                WorldPosition = _transform.WorldPosition
            };

            var members = voxelSpaceData.Grids
                .Select(t =>
                {
                    var gridEntity = _world.CreateEntity();

                    var gridTransform = new Transform();
                    gridTransform.Position = Vector3.One * voxelSpaceData.GridSize * voxelSpaceData.VoxelSize * t.Index;
                    spaceTransform.AddChild(gridTransform);
                    gridEntity.Set(gridTransform);
                    gridEntity.Set(_voxelMaterialInstance);
                    gridEntity.Set(new ExposedVoxels());
                    gridEntity.Set(new VoxelSpaceExpander());
                    gridEntity.Set(new VoxelGrid(voxelSpaceData.VoxelSize, voxelSpaceData.GridSize, spaceEntity, t.Index, t.Voxels));

                    return (t.Item1, gridEntity);
                })
                .ToDictionary(t => t.Item1, t => t.gridEntity);

            spaceEntity.Set(spaceTransform);
            spaceEntity.Set(new VoxelSpaceDynamicBody());
            spaceEntity.Set(new DynamicBody());
            spaceEntity.Set(new VoxelSpace()
            {
                GridSize = voxelSpaceData.GridSize,
                VoxelSize = voxelSpaceData.VoxelSize,
                Members = members
            });

            _world.Publish(new SelectEntityRequest() { Entity = spaceEntity });
        }
    }
}
