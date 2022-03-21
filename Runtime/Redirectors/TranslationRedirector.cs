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
        /// Transform of the players camera
        /// Used to get the current position
        /// </summary>
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

        /// <summary>
        /// The direction of the translation in world space
        /// </summary>
        private Vector3 translationDirectionWorld = Vector3.forward;

        protected override void Awake()
        {
            base.Awake();

            // Use the normalized direction in world space
            translationDirection = translationDirection.normalized;
            translationDirectionWorld = transform.TransformDirection(translationDirection);
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
            // Get the current position of the camera in its local space
            Vector3 hmdPosition = playerCamera.localPosition;

            // Check if the redirection is currently happening
            if (isRedirecting)
            {
                // Calculate the translation delta since last update in world space
                Vector3 translationDelta = playerCamera.parent.TransformVector(hmdPosition - lastHeadPosition);
                // Get the gain vector for that translation delta
                Vector3 gainVector = GetGain(translationDelta);
                // Calculate the gained translation
                Vector3 gainedTranslation = Vector3.Scale(gainVector, translationDelta);

                // Calculate the translation progress for each dimension and nullify a gain if it is already finished for that direction
                if (translationProgress.x.Equals(1.0f))
                {
                    gainedTranslation.x = 0.0f;
                }
                else 
                {
                    translationProgress.x = Mathf.Clamp01(translationProgress.x + Mathf.Abs(gainedTranslation.x) / Mathf.Abs(translationDirectionWorld.x * translationAmount));
                }
                if (translationProgress.y.Equals(1.0f))
                {
                    gainedTranslation.y = 0.0f;
                }
                else
                {
                    translationProgress.y = Mathf.Clamp01(translationProgress.y + Mathf.Abs(gainedTranslation.y) / Mathf.Abs(translationDirectionWorld.y * translationAmount));
                }
                if (translationProgress.z.Equals(1.0f))
                {
                    gainedTranslation.z = 0.0f;
                }
                else
                {
                    translationProgress.z = Mathf.Clamp01(translationProgress.z + Mathf.Abs(gainedTranslation.z) / Mathf.Abs(translationDirectionWorld.z * translationAmount));
                }

                // try to apply redirection
                if (redirectionObject)
                {
                    // Add Gain to position
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
        /// <param name="translationDelta"> the head movement since the last frame </param>
        /// <returns> Gain Vector for the given translation delta </returns>
        private Vector3 GetGain(Vector3 translationDelta)
        {
            // Initialize gain vector as zero-vector
            Vector3 gainVector = Vector3.zero;

            // Check if the translation direction has a x-component
            if (!translationDirectionWorld.x.Equals(0.0f))
            {
                // Set the x-component according to the direction of the delta and the translation direction
                gainVector.x = (translationDirectionWorld.x < 0.0f && translationDelta.x < 0.0f) || (translationDirectionWorld.x > 0.0f && translationDelta.x > 0.0f) 
                        ? forwardTranslationGain : backwardTranslationGain;
            }
            // Check if the translation direction has a y-component
            if (!translationDirectionWorld.y.Equals(0.0f))
            {
                // Set the y-component according to the direction of the delta and the translation direction
                gainVector.y = (translationDirectionWorld.y < 0.0f && translationDelta.y < 0.0f) || (translationDirectionWorld.y > 0.0f && translationDelta.y > 0.0f)
                        ? forwardTranslationGain : backwardTranslationGain;
            }
            // Check if the translation direction has a z-component
            if (!translationDirectionWorld.z.Equals(0.0f))
            {
                // Set the z-component according to the direction of the delta and the translation direction
                gainVector.z = (translationDirectionWorld.z < 0.0f && translationDelta.z < 0.0f) || (translationDirectionWorld.z > 0.0f && translationDelta.z > 0.0f)
                        ? forwardTranslationGain : backwardTranslationGain;
            }

            return gainVector;
        }

        /// <summary>
        /// Starts applying translation redirection.
        /// </summary>
        public override void StartRedirection()
        {
            // Only Start Redirecting if some redirection values are set
            if (!translationAmount.Equals(0.0f) && !translationDirection.magnitude.Equals(0.0f))
            {
                // Check if the translation direction has a component for each dimension
                translationProgress = new Vector3(translationDirectionWorld.x.Equals(0.0f) ? 1.0f : 0.0f,
                                                  translationDirectionWorld.y.Equals(0.0f) ? 1.0f : 0.0f,
                                                  translationDirectionWorld.z.Equals(0.0f) ? 1.0f : 0.0f);
                // Set the flag to true
                isRedirecting = true;
                base.StartRedirection();
            }
        }

        /// <summary>
        /// Stops applying translation redirection.
        /// </summary>
        public override void EndRedirection()
        {
            // Set the flag to false
            isRedirecting = false;
            base.EndRedirection();
        }
    }
}
