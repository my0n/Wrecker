﻿using BepuPhysics;
using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Clunker.Physics.Bepu
{

    //Note that the engine does not require any particular form of gravity- it, like all the contact callbacks, is managed by a callback.
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        private Vector3 _gravityDt;
        private float _dt;
        private PhysicsSystem _physicsSystem;

        /// <summary>
        /// Gets how the pose integrator should handle angular velocity integration.
        /// </summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving; //Don't care about fidelity in this demo!


        public PoseIntegratorCallbacks(PhysicsSystem physicsSystem, Vector3 gravity) : this()
        {
            _physicsSystem = physicsSystem;
            Gravity = gravity;
        }

        /// <summary>
        /// Called prior to integrating the simulation's active bodies. When used with a substepping timestepper, this could be called multiple times per frame with different time step values.
        /// </summary>
        /// <param name="dt">Current time step duration.</param>
        public void PrepareForIntegration(float dt)
        {
            //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            _gravityDt = Gravity * dt;
            _dt = dt;
        }

        /// <summary>
        /// Callback called for each active body within the simulation during body integration.
        /// </summary>
        /// <param name="bodyIndex">Index of the body being visited.</param>
        /// <param name="pose">Body's current pose.</param>
        /// <param name="localInertia">Body's current local inertia.</param>
        /// <param name="workerIndex">Index of the worker thread processing this body.</param>
        /// <param name="velocity">Reference to the body's current velocity to integrate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            //Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
            if (localInertia.InverseMass > 0)
            {
                var gravityDtThisObj = _gravityDt;
                var context = _physicsSystem.GetDynamicContext(new BodyHandle(bodyIndex));
                if (context != null && context is Entity entity && entity.Has<DynamicBody>())
                {
                    var dynamicBody = entity.Get<DynamicBody>();
                    if(dynamicBody.Gravity.HasValue)
                    {
                        gravityDtThisObj = dynamicBody.Gravity.Value * _dt;
                    }
                }
                velocity.Linear = velocity.Linear + gravityDtThisObj;
            }
        }

    }
}
