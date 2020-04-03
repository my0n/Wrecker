﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Utilities;

namespace Clunker.Graphics
{
    public struct MeshGeometry
    {
        public VertexPositionTextureNormal[] Vertices;
        public ushort[] Indices;
        public Vector3 BoundingSize;
    }
}