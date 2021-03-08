using System;
using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// <summary>
    /// Event argument for <see cref="CompoundTriggerEvents"/> used in <see cref="OverlappingRedirector"/>
    /// </summary>
    public class CollisionEventArgs : EventArgs
    {
        public Collider collider { get; set; }

        public CollisionEventArgs(Collider other)
        {
            collider = other;
        }
    }
}