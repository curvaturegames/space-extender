using System;
using UnityEngine;
using System.Collections.Generic;

namespace CurvatureGames.SpaceExtender
{
    // used to access trigger events from scripts only,
    // using c# standard events as those are more efficient
    // at the cost of not being configurable via the unity editor.
    // event filtering is deliberately left out of this,
    // as opposed to TriggerEvents.cs which is used as an editor tool.
    // this also manages compound colliders, making sure enter and exit
    // callbacks are only fired for the whole collider and not for individual children.
    [RequireComponent(typeof(Collider))]
    public class CompoundTriggerEvents : MonoBehaviour
    {
        public event EventHandler<CollisionEventArgs> RaiseTriggerEnter;

        public event EventHandler<CollisionEventArgs> RaiseTriggerExit;

        public Dictionary<int, int> colliderEnterCounts = new Dictionary<int, int>();

        public void OnTriggerEnter(Collider other)
        {
            colliderEnterCounts.TryGetValue(other.GetInstanceID(), out var timesEntered);
            if (timesEntered <= 0 && RaiseTriggerEnter != null) {
                RaiseTriggerEnter(this, new CollisionEventArgs(other));
            }

            colliderEnterCounts[other.GetInstanceID()] = timesEntered + 1;
        }

        public void OnTriggerExit(Collider other)
        {
            colliderEnterCounts.TryGetValue(other.GetInstanceID(), out var timesEntered);

            if (timesEntered <= 1 && RaiseTriggerExit != null) {
                RaiseTriggerExit(this, new CollisionEventArgs(other));
            }

            colliderEnterCounts[other.GetInstanceID()] = timesEntered - 1;
        }
    }
}

