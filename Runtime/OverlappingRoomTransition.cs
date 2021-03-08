using System;
using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// Coordinates transitions of a room from activation to deactivation and vice versa.
    /// Used for e.g. animating room doors, playing sounds etc.
    public class OverlappingRoomTransition : MonoBehaviour
    {
        public event EventHandler<bool> RaiseTransitionStart;
        public event EventHandler<bool> RaiseTransitionEnd;

        public bool targetStateActive;

        /// Called by the redirector when a transition should begin.
        /// Register a handler for the `RaiseTransitionStart` event from
        /// another script to react to this.
        public void OnTransitionStart(bool active)
        {
            targetStateActive = active;
            RaiseTransitionStart?.Invoke(this, targetStateActive);
        }

        /// Notify the redirector that the transition is finished.
        /// This will make the redirector actually show or hide the room, depending on the 
        /// target state last passed to `OnTransitionStart`.
        public void EndTransition()
        {
            RaiseTransitionEnd?.Invoke(this, targetStateActive);
        }

    }
}