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
        [SerializeField] private Transform playerCamera;

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
        public override Vector3 EndPlayAreaPosition { get { return transform.TransformPoint(translationDirection.normalized * translationAmount); } } //normalized is zero vector if used on zero vector

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

            // Use the normalized direction
            translationDirection = translationDirection.normalized;
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
            Vector3 hmdPosition = playerCamera.localPosition;
            /*if (!TryGetHMDPosition(out hmdPosition))
            {
                hmdPosition = lastHeadPosition;
            }*/

            if (isRedirecting)
            {
                Vector3 translationDelta = playerCamera.TransformVector(hmdPosition - lastHeadPosition);
                Vector3 gainVector = GetGain(translationDelta);
                Vector3 gainedTranslation = Vector3.Scale(gainVector, translationDelta);

                // TODO: Currently leads to a translation that is slightly further than intended
                if (translationProgress.x == 1.0f)
                {
                    gainedTranslation.x = 0.0f;
                }
                else 
                {
                    translationProgress.x = Mathf.Clamp01(translationProgress.x + Mathf.Abs(gainedTranslation.x) / Mathf.Abs(translationDirection.x * translationAmount));
                }
                if (translationProgress.y == 1.0f)
                {
                    gainedTranslation.y = 0.0f;
                }
                else
                {
                    translationProgress.y = Mathf.Clamp01(translationProgress.y + Mathf.Abs(gainedTranslation.y) / Mathf.Abs(translationDirection.y * translationAmount));
                }
                if (translationProgress.z == 1.0f)
                {
                    gainedTranslation.z = 0.0f;
                }
                else
                {
                    translationProgress.z = Mathf.Clamp01(translationProgress.z + Mathf.Abs(gainedTranslation.z) / Mathf.Abs(translationDirection.z * translationAmount));
                }

                // try to apply redirection
                if (redirectionObject)
                {
                    // Add Gain to position   TODO: Check if local or world coordinates
                    redirectionObject.position += gainedTranslation;
                }
                else
                {
                    Debug.LogWarning("Redirection Object not set! Redirection can not be applied!");
                }

                // Redirection is done
                if (translationProgress.sqrMagnitude.Equals(3.0f))
                {
                    EndRedirection();
                }
            }

            // Save the current head position for the next frame
            lastHeadPosition = hmdPosition;
        }

        /// <summary>
        /// Get the Gain Vector for the given translation delta
        /// </summary>
        /// <param name="translationDelta"> the head movement since the last frame</param>
        /// <returns> Gain Vector for the given translation delta </returns>
        private Vector3 GetGain(Vector3 translationDelta)
        {
            Vector3 gainVector = Vector3.zero;

            Vector3 translationdirectionWorld = transform.TransformDirection(translationDirection);
            if (!translationdirectionWorld.x.Equals(0.0f))
            {
                gainVector.x = (translationdirectionWorld.x < 0.0f && translationDelta.x < 0.0f) || (translationdirectionWorld.x > 0.0f && translationDelta.x > 0.0f) 
                        ? forwardTranslationGain : backwardTranslationGain;
            }
            if (!translationdirectionWorld.y.Equals(0.0f))
            {
                gainVector.y = (translationdirectionWorld.y < 0.0f && translationDelta.y < 0.0f) || (translationdirectionWorld.y > 0.0f && translationDelta.y > 0.0f)
                        ? forwardTranslationGain : backwardTranslationGain;
            }
            if (!translationdirectionWorld.z.Equals(0.0f))
            {
                gainVector.z = (translationdirectionWorld.z < 0.0f && translationDelta.z < 0.0f) || (translationdirectionWorld.z > 0.0f && translationDelta.z > 0.0f)
                        ? forwardTranslationGain : backwardTranslationGain;
            }

            return gainVector;
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
            // Only Start Redirecting if some redirection values are set
            if (!translationAmount.Equals(0.0f) && !translationDirection.magnitude.Equals(0.0f))
            {
                translationProgress = new Vector3(translationDirection.x.Equals(0.0f) ? 1.0f : 0.0f,
                                                  translationDirection.y.Equals(0.0f) ? 1.0f : 0.0f,
                                                  translationDirection.z.Equals(0.0f) ? 1.0f : 0.0f);
                isRedirecting = true;
                base.StartRedirection();
            }
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
