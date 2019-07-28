﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clunker.SceneGraph.ComponentInterfaces
{
    public interface IComponentEventListener : IComponent
    {
        void ComponentStarted();
        void ComponentStopped();
    }
}
