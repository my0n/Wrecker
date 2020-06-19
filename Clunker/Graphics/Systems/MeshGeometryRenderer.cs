﻿using Clunker.Core;
using Clunker.Graphics;
using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace Clunker.Graphics
{
    public class MeshGeometryRenderer : AEntitySystem<RenderingContext>
    {
        // World Transform
        public ResourceSet WorldTransformResourceSet;
        public DeviceBuffer WorldMatrixBuffer { get; private set; }

        // Scene Inputs
        public ResourceSet SceneInputsResourceSet;
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ViewMatrixBuffer { get; private set; }
        public DeviceBuffer SceneLightingBuffer { get; private set; }

        public MeshGeometryRenderer(GraphicsDevice device, MaterialInputLayouts materialInputLayouts, World world) : base(world.GetEntities()
            .With<Material>()
            .With<MaterialTexture>()
            .With<RenderableMeshGeometry>()
            .With<RenderableMeshGeometryResources>()
            .With<Transform>().AsSet())
        {
            var factory = device.ResourceFactory;

            WorldMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            WorldTransformResourceSet = factory.CreateResourceSet(new ResourceSetDescription(materialInputLayouts.ResourceLayouts["WorldTransform"], WorldMatrixBuffer));

            ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            SceneLightingBuffer = factory.CreateBuffer(new BufferDescription(SceneLighting.Size, BufferUsage.UniformBuffer));
            SceneInputsResourceSet = factory.CreateResourceSet(new ResourceSetDescription(materialInputLayouts.ResourceLayouts["SceneInputs"], ProjectionMatrixBuffer, ViewMatrixBuffer, SceneLightingBuffer));
        }

        protected override void Update(RenderingContext context, ReadOnlySpan<Entity> entities)
        {
            var commandList = context.CommandList;
            var cameraTransform = context.CameraTransform;

            commandList.UpdateBuffer(ProjectionMatrixBuffer, 0, context.ProjectionMatrix);

            var viewMatrix = cameraTransform.GetViewMatrix();
            commandList.UpdateBuffer(ViewMatrixBuffer, 0, viewMatrix);

            var frustrum = new BoundingFrustum(viewMatrix * context.ProjectionMatrix);

            var transparents = new List<(Material mat, MaterialTexture texture, DeviceBuffer vertices, DeviceBuffer indices, uint numIndices, Transform transform)>();

            var materialInputs = new MaterialInputs();
            materialInputs.ResouceSets["SceneInputs"] = SceneInputsResourceSet;

            foreach(var entity in entities)
            {
                ref var material = ref entity.Get<Material>();
                ref var texture = ref entity.Get<MaterialTexture>();
                ref var geometry = ref entity.Get<RenderableMeshGeometry>();
                ref var resources = ref entity.Get<RenderableMeshGeometryResources>();
                ref var transform = ref entity.Get<Transform>();

                var shouldRender = geometry.BoundingSize.HasValue ?
                    frustrum.Contains(GetBoundingBox(transform, geometry.BoundingSize.Value)) != ContainmentType.Disjoint :
                    true;

                if (shouldRender)
                {
                    RenderObject(commandList, materialInputs, material, texture, resources.VertexBuffer, resources.IndexBuffer, (uint)geometry.Indices.Length, transform);

                    if (geometry.TransparentIndices.Length > 0)
                    {
                        transparents.Add((material, texture, resources.VertexBuffer, resources.TransparentIndexBuffer, (uint)geometry.TransparentIndices.Length, transform));
                    }
                }
            }

            var sorted = transparents.OrderByDescending(t => Vector3.Distance(cameraTransform.WorldPosition, t.transform.WorldPosition));

            foreach(var (material, texture, vertices, indices, numIndices, transform) in sorted)
            {
                RenderObject(commandList, materialInputs, material, texture, vertices, indices, numIndices, transform);
            }
        }

        private void RenderObject(CommandList commandList, MaterialInputs inputs, Material material, MaterialTexture texture, DeviceBuffer vertices, DeviceBuffer indices, uint numIndices, Transform transform)
        {
            commandList.UpdateBuffer(SceneLightingBuffer, 0, new SceneLighting()
            {
                AmbientLightColour = RgbaFloat.White,
                AmbientLightStrength = 0.4f,
                DiffuseLightColour = RgbaFloat.White,
                DiffuseLightDirection = Vector3.Normalize(Vector3.Transform(new Vector3(2, 5, -1), Quaternion.Inverse(transform.WorldOrientation)))
            });

            commandList.UpdateBuffer(WorldMatrixBuffer, 0, transform.WorldMatrix);

            inputs.ResouceSets["WorldTransform"] = WorldTransformResourceSet;
            inputs.ResouceSets["Texture"] = texture.ResourceSet;

            inputs.VertexBuffers["Model"] = vertices;
            inputs.IndexBuffer = indices;

            material.RunPipeline(commandList, inputs, (uint)numIndices);
        }

        private BoundingBox GetBoundingBox(Transform transform, Vector3 size)
        {
            var positions = new Vector3[]
            {
                transform.GetWorld(new Vector3(0, 0, 0)),
                transform.GetWorld(new Vector3(size.X, 0, 0)),
                transform.GetWorld(new Vector3(size.X, 0, size.Z)),
                transform.GetWorld(new Vector3(0, 0, size.Z)),

                transform.GetWorld(new Vector3(0, size.Y, 0)),
                transform.GetWorld(new Vector3(size.X, size.Y, 0)),
                transform.GetWorld(new Vector3(size.X, size.Y, size.Z)),
                transform.GetWorld(new Vector3(0, size.Y, size.Z)),
            };

            var min = new Vector3(positions.Min(p => p.X), positions.Min(p => p.Y), positions.Min(p => p.Z));
            var max = new Vector3(positions.Max(p => p.X), positions.Max(p => p.Y), positions.Max(p => p.Z));

            return new BoundingBox(min, max);
        }
    }
}
