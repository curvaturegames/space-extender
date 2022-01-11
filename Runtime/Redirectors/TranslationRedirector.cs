using System.Collections.Generic;
using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// <summary>
    /// Handles translation redirection
    /// Playarea is translated along a direction at the same time the user moves her head, resulting in a translation gain.
    /// </summary>
    public class TranslationRedirector : BaseRedirector
    {
        // Used by custom editor
        /// <summary>
        /// The direction of the translation in local space
        /// </summary>
        [SerializeField] private Vector3 translationDirection = Vector3.forward;

        // Used by custom editor
        /// <summary>
        /// The translation in meters the player will be redirected
        /// </summary>
        [SerializeField] private float translationAmount = 0.0f;

        // Used by custom editor
        /// <summary>
        /// Gain if moving in the direction of translationDirection
        /// </summary>
        [SerializeField] private float forwardTranslationGain = 0.1f;

        // Used by custom editor
        /// <summary>
        /// Gain if moving in the negative direction of translationDirection
        /// </summary>
        [SerializeField] private float backwardTranslationGain = -0.1f;

        /// <summary>
        /// Center position of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Vector3 EndPlayAreaPosition { get { return transform.TransformPoint(translationDirection.normalized * translationAmount); } }

        /// <summary>
        /// Rotation of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Quaternion EndPlayAreaRotation { get { return transform.rotation; } }

        /// <summary>
        /// Whether a redirection is currently in progress
        /// </summary>
        private bool isRedirecting = false;

        /// <summary>
        /// The last position of the hmd
        /// </summary>
        private Vector3 lastHeadPosition = Vector3.zero;

        /// <summary>
        /// Progress of the current redirection process splitted in x, y and z components
        /// </summary>
        private Vector3 translationProgress = Vector3.zero;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            TranslationRedirection();
        }

        /// <summary>
        /// Calculates and applies the translation redirection
        /// </summary>
        private void TranslationRedirection()
        {
            Vector3 hmdPosition;
            if (!TryGetHMDPosition(out hmdPosition))
            {
                hmdPosition = lastHeadPosition;
            }
            if (isRedirecting)
            {
                Vector3 translationDelta = hmdPosition - lastHeadPosition;
                Vector3 gainVector = GetGain(translationDelta);

                // TODO: translationProgress

                if (redirectionObject)
                {
                    // Add Gain to position       TODO: Check if local or world coordinates
                    redirectionObject.position += Vector3.Scale(gainVector, translationDelta) - translationDelta;
                }
                else
                {
                    Debug.LogWarning("Redirection Object not set! Redirection can not be applied!");
                }

                if (translationProgress.x == 1.0f)
                {
                    //TODO: End redirection in x direction
                }
                if (translationProgress.y == 1.0f)
                {
                    //TODO: End redirection in y direction
                }
                if (translationProgress.z == 1.0f)
                {
                    //TODO: End redirection in z direction
                }

                if (translationProgress.magnitude == 3.0f)
                {
                    EndRedirection();
                }
            }

            lastHeadPosition = hmdPosition;
        }

        private Vector3 GetGain(Vector3 translationDelta)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Attempt to retrieve a Vector3 representing the current position of the hmd.
        /// </summary>
        /// <param name="hmdPosition"> output parameter of the position of the hmd </param>
        /// <returns>true if position could be retrieved successfully.</returns>
        private bool TryGetHMDPosition (out Vector3 hmdPosition)
        {
            List<UnityEngine.XR.XRNodeState> nodeStates = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);
            var headNodeState = nodeStates.Find(nodeState => nodeState.nodeType.Equals(UnityEngine.XR.XRNode.Head));
            return headNodeState.TryGetPosition(out hmdPosition);
        }

        /// <summary>
        /// Starts applying translation redirection.
        /// </summary>
        public override void StartRedirection()
        {
            translationProgress = Vector3.zero;
            isRedirecting = true;
            base.StartRedirection();
        }

        /// <summary>
        /// Stops applying translation redirection.
        /// </summary>
        public override void EndRedirection()
        {
            isRedirecting = false;
            base.EndRedirection();
        }
    }
}
