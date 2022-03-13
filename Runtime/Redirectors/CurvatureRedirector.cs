using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CurvatureGames.SpaceExtender
{

    public class CurvatureRedirector : BaseRedirector
    {
        /// <summary>
        /// Minimal value that is added to the progress, since 0.0 progress results in an interruption
        /// </summary>
        private const float MIN_PROGRESS = 1e-5f;

        // Used by custom editor
        /// <summary>
        /// Event that is invoked when the user walks back to the starting position
        /// </summary>
        public UnityEvent OnRedirectionInterrupted;

        // Used by custom editor
        /// <summary>
        /// Transform of the players camera
        /// Used to get the current position
        /// </summary>
        [SerializeField] private Transform playerCamera;

        // Used by custom editor
        /// <summary>
        /// Direction in local space in which the player needs to walk to apply redirection
        /// </summary>
        [SerializeField] private Vector3 virtualDirection = Vector3.forward;

        // Used by custom editor
        /// <summary>
        /// How many meters the redirection should be applied
        /// </summary>
        [SerializeField, Min(0.0f)] private float redirectionLength = 1.0f;

        // Used by custom editor
        /// <summary>
        /// How many degrees the player should be rotated per meter walked
        /// </summary>
        [SerializeField] private float degreesPerMeter = 10.0f;

        // Used by custom editor
        /// <summary>
        /// How often should the gizmo line be sampled (0.1 means that every 10cm the line will be sampled)
        /// redirectionLength % gizmoLineDistance should result in 0.0 for correct results
        /// </summary>
        [SerializeField, Min(0.001f)] private float gizmoLineDistance = 0.1f;

        /// <summary>
        /// Center position of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Vector3 EndPlayAreaPosition { get { return transform.TransformPoint(virtualDirection.normalized * redirectionLength); } } //normalized is zero vector if used on zero vector

        /// <summary>
        /// Rotation of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Quaternion EndPlayAreaRotation { get { return transform.rotation; } }

        /// <summary>
        /// Center position of the real end playarea. (worldspace)
        /// </summary>
        public Vector3 RealEndPlayAreaPosition { 
            get
            {
                // Initialize Rotation Matrix
                float degrees = Mathf.Deg2Rad * (degreesPerMeter * gizmoLineDistance);
                Matrix4x4 rotationMatrix = new Matrix4x4();
                rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(degrees), 0.0f, Mathf.Sin(degrees), 0.0f));
                rotationMatrix.SetColumn(1, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                rotationMatrix.SetColumn(2, new Vector4(-Mathf.Sin(degrees), 0.0f, Mathf.Cos(degrees), 0.0f));
                rotationMatrix.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

                // Initialize position and direction
                Vector3 currentDirection = transform.TransformDirection(virtualDirection.normalized);
                Vector3 currentPosition = transform.position;
                
                // Move along the path to the final position
                for (float f = gizmoLineDistance; f <= redirectionLength; f += gizmoLineDistance)
                {
                    currentPosition += gizmoLineDistance * currentDirection;
                    currentDirection = rotationMatrix * currentDirection;
                }

                // return that position
                return currentPosition;
            }
        }

        /// <summary>
        /// Rotation of the real end playarea (worldspace)
        /// </summary>
        public Quaternion RealEndPlayAreaRotation { get { return transform.rotation * Quaternion.Euler(0.0f, -degreesPerMeter * redirectionLength, 0.0f); } }

        /// <summary>
        /// Array of sampled real path positions (worldspace)
        /// </summary>
        public Vector3[] RealPathPositions
        {
            get
            {
                // Initialize rotation matrix
                float degrees = Mathf.Deg2Rad * (degreesPerMeter * gizmoLineDistance);
                Matrix4x4 rotationMatrix = new Matrix4x4();
                rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(degrees), 0.0f, Mathf.Sin(degrees), 0.0f));
                rotationMatrix.SetColumn(1, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
                rotationMatrix.SetColumn(2, new Vector4(-Mathf.Sin(degrees), 0.0f, Mathf.Cos(degrees), 0.0f));
                rotationMatrix.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

                // Initialize position and direction
                Vector3 currentDirection = transform.TransformDirection(virtualDirection.normalized);
                Vector3 currentPosition = transform.position;

                // Initialize list of sampled positions and add the first position
                List<Vector3> pathPositions = new List<Vector3>();
                pathPositions.Add(currentPosition);

                // Move along the path and add positions to the list
                for (float f = gizmoLineDistance; f <= redirectionLength; f += gizmoLineDistance)
                {
                    currentPosition += gizmoLineDistance * currentDirection;
                    pathPositions.Add(currentPosition);
                    currentDirection = rotationMatrix * currentDirection;
                }

                // return the list as an array
                return pathPositions.ToArray();
            }
        }

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
        private Vector3 redirectionProgress = Vector3.zero;

        /// <summary>
        /// Direction in world space in which the player needs to walk to apply redirection
        /// </summary>
        private Vector3 virtualDirectionWorld = Vector3.forward;

        /// <summary>
        /// Value to check if the user walked back to the starting position
        /// </summary>
        private float interruptValue = 0.0f;

        protected override void Awake()
        {
            base.Awake();

            // Use the normalized direction in world space
            virtualDirection = virtualDirection.normalized;
            virtualDirectionWorld = transform.TransformDirection(virtualDirection);
        }

        protected override void Update()
        {
            base.Update();

            CurvatureRedirection();
        }

        /// <summary>
        /// Calculates and applies the curvature redirection
        /// </summary>
        private void CurvatureRedirection()
        {
            // Get the current position of the camera in its local space
            Vector3 hmdPosition = playerCamera.localPosition;

            // Check if the redirection is currently happening
            if (isRedirecting)
            {
                // Calculate the translation delta since last update in world space
                Vector3 translationDelta = playerCamera.parent.TransformVector(hmdPosition - lastHeadPosition);
                // Calculate the vector with the translation delta in the right direction
                Vector3 translationDeltaInVirtualDirection = GetVectorInVirtualDirection(translationDelta);
                // Calculate the vector with the translation delta in the opposite direction
                Vector3 translationDeltaInOppositeDirection = GetVectorInOppositeDirection(translationDelta);

                // Update the progress and change the deltas if neccessary
                UpdateRedirectionProgress(ref translationDeltaInVirtualDirection, ref translationDeltaInOppositeDirection);

                // Calculate the degrees of the rotation
                float rotationDegrees = translationDeltaInVirtualDirection.magnitude * degreesPerMeter - translationDeltaInOppositeDirection.magnitude * degreesPerMeter;

                // try to apply redirection
                if (redirectionObject)
                {
                    redirectionObject.transform.RotateAround(playerCamera.position, transform.TransformDirection(Vector3.up), rotationDegrees);
                }
                else
                {
                    Debug.LogWarning("Redirection Object not set! Redirection can not be applied!");
                }

                // Redirection is done
                if (redirectionProgress.sqrMagnitude.Equals(3.0f))
                {
                    EndRedirection();
                }
                else if (redirectionProgress.sqrMagnitude.Equals(interruptValue))
                {
                    InterruptRedirection();
                }
            }

            // Save the current head position for the next frame
            lastHeadPosition = hmdPosition;
        }

        /// <summary>
        /// Calculate the translation progress for each dimension and nullify a translation delta if it is already finished for that direction
        /// </summary>
        /// <param name="translationDeltaInVirtualDirection"> translation delta in the virtual direction </param>
        /// <param name="translationDeltaInOppositeDirection"> translation delta in the opposite direction </param>
        private void UpdateRedirectionProgress(ref Vector3 translationDeltaInVirtualDirection, ref Vector3 translationDeltaInOppositeDirection)
        {

            // X in virtual direction
            if (!translationDeltaInVirtualDirection.x.Equals(0.0f))
            {
                if (redirectionProgress.x.Equals(1.0f))
                {
                    translationDeltaInVirtualDirection.x = 0.0f;
                }
                else
                {
                    redirectionProgress.x = Mathf.Clamp01(redirectionProgress.x + Mathf.Abs(translationDeltaInVirtualDirection.x) / Mathf.Abs(virtualDirectionWorld.x * redirectionLength));
                }
            }
            // X in opposite direction
            else if (!translationDeltaInOppositeDirection.x.Equals(0.0f))
            {
                if (redirectionProgress.x.Equals(0.0f))
                {
                    translationDeltaInOppositeDirection.x = 0.0f;
                }
                else
                {
                    redirectionProgress.x = Mathf.Clamp01(redirectionProgress.x - Mathf.Abs(translationDeltaInOppositeDirection.x) / Mathf.Abs(virtualDirectionWorld.x * redirectionLength));
                }
            }

            // Y in virtual direction
            if (!translationDeltaInVirtualDirection.y.Equals(0.0f))
            {
                if (redirectionProgress.y.Equals(1.0f))
                {
                    translationDeltaInVirtualDirection.y = 0.0f;
                }
                else
                {
                    redirectionProgress.y = Mathf.Clamp01(redirectionProgress.y + Mathf.Abs(translationDeltaInVirtualDirection.y) / Mathf.Abs(virtualDirectionWorld.y * redirectionLength));
                }
            }
            // Y in opposite direction
            else if (!translationDeltaInOppositeDirection.y.Equals(0.0f))
            {
                if (redirectionProgress.y.Equals(0.0f))
                {
                    translationDeltaInOppositeDirection.y = 0.0f;
                }
                else
                {
                    redirectionProgress.y = Mathf.Clamp01(redirectionProgress.y - Mathf.Abs(translationDeltaInOppositeDirection.y) / Mathf.Abs(virtualDirectionWorld.y * redirectionLength));
                }
            }

            // Z in virtual direction
            if (!translationDeltaInVirtualDirection.z.Equals(0.0f))
            {
                if (redirectionProgress.z.Equals(1.0f))
                {
                    translationDeltaInVirtualDirection.z = 0.0f;
                }
                else
                {
                    redirectionProgress.z = Mathf.Clamp01(redirectionProgress.z + Mathf.Abs(translationDeltaInVirtualDirection.z) / Mathf.Abs(virtualDirectionWorld.z * redirectionLength));
                }
            }
            // Z in opposite direction
            else if (!translationDeltaInOppositeDirection.z.Equals(0.0f))
            {
                if (redirectionProgress.z.Equals(0.0f))
                {
                    translationDeltaInOppositeDirection.z = 0.0f;
                }
                else
                {
                    redirectionProgress.z = Mathf.Clamp01(redirectionProgress.z - Mathf.Abs(translationDeltaInOppositeDirection.z) / Mathf.Abs(virtualDirectionWorld.z * redirectionLength));
                }
            }
        }

        /// <summary>
        /// Get the translation delta with components that are in the same direction as the virtual direction
        /// </summary>
        /// <param name="translationDelta"> the head movement since the last frame </param>
        /// <returns> translation delta with components that are in the same direction as the virtual direction </returns>
        private Vector3 GetVectorInVirtualDirection(Vector3 translationDelta)
        {
            // Initialize the direction vector
            Vector3 directionVector = Vector3.zero;

            // Check if the virtual direction has a x-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.x.Equals(0.0f)
                && (virtualDirectionWorld.x < 0.0f && translationDelta.x < 0.0f) || (virtualDirectionWorld.x > 0.0f && translationDelta.x > 0.0f))
            {
                directionVector.x = translationDelta.x;
            }
            // Check if the virtual direction has a y-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.y.Equals(0.0f) 
                && (virtualDirectionWorld.y < 0.0f && translationDelta.y < 0.0f) || (virtualDirectionWorld.y > 0.0f && translationDelta.y > 0.0f))
            {
                directionVector.y = translationDelta.y;
            }
            // Check if the virtual direction has a z-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.z.Equals(0.0f)
                && (virtualDirectionWorld.z < 0.0f && translationDelta.z < 0.0f) || (virtualDirectionWorld.z > 0.0f && translationDelta.z > 0.0f))
            {
                directionVector.z = translationDelta.z;
            }

            return directionVector;
        }

        /// <summary>
        /// Get the translation delta with components that are in the opposite direction of the virtual direction
        /// </summary>
        /// <param name="translationDelta"> the head movement since the last frame </param>
        /// <returns> translation delta with components that are in the opposite direction of the virtual direction </returns>
        private Vector3 GetVectorInOppositeDirection(Vector3 translationDelta)
        {
            // Initialize the direction vector
            Vector3 directionVector = Vector3.zero;

            // Check if the virtual direction has a x-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.x.Equals(0.0f)
                && (virtualDirectionWorld.x > 0.0f && translationDelta.x < 0.0f) || (virtualDirectionWorld.x < 0.0f && translationDelta.x > 0.0f))
            {
                directionVector.x = translationDelta.x;
            }
            // Check if the virtual direction has a y-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.y.Equals(0.0f)
                && (virtualDirectionWorld.y > 0.0f && translationDelta.y < 0.0f) || (virtualDirectionWorld.y < 0.0f && translationDelta.y > 0.0f))
            {
                directionVector.y = translationDelta.y;
            }
            // Check if the virtual direction has a z-component and if the translation delta is in the right direction
            if (!virtualDirectionWorld.z.Equals(0.0f)
                && (virtualDirectionWorld.z > 0.0f && translationDelta.z < 0.0f) || (virtualDirectionWorld.z < 0.0f && translationDelta.z > 0.0f))
            {
                directionVector.z = translationDelta.z;
            }

            return directionVector;
        }

        /// <summary>
        /// Interrupts the curvature redirection.
        /// </summary>
        private void InterruptRedirection()
        {
            isRedirecting = false;
            OnRedirectionInterrupted.Invoke();
        }

        /// <summary>
        /// Starts applying curvature redirection.
        /// </summary>
        public override void StartRedirection()
        {
            // Only Start Redirecting if some redirection values are set
            if (!redirectionLength.Equals(0.0f) && !virtualDirection.magnitude.Equals(0.0f))
            {
                // Check if the virtual direction has a component for each dimension
                redirectionProgress = new Vector3(virtualDirectionWorld.x.Equals(0.0f) ? 1.0f : MIN_PROGRESS,
                                                  virtualDirectionWorld.y.Equals(0.0f) ? 1.0f : MIN_PROGRESS,
                                                  virtualDirectionWorld.z.Equals(0.0f) ? 1.0f : MIN_PROGRESS);

                // Set the interrupt value, which is used when walking back to the start position
                interruptValue = 0.0f;
                interruptValue += virtualDirectionWorld.x.Equals(0.0f) ? 1.0f : 0.0f;
                interruptValue += virtualDirectionWorld.y.Equals(0.0f) ? 1.0f : 0.0f;
                interruptValue += virtualDirectionWorld.z.Equals(0.0f) ? 1.0f : 0.0f;

                // Set the flag to true
                isRedirecting = true;
                base.StartRedirection();
            }
        }

        /// <summary>
        /// Stops applying curvature redirection.
        /// </summary>
        public override void EndRedirection()
        {
            // Set the flag to false
            isRedirecting = false;
            base.EndRedirection();
        }


        /// <summary>
        /// Creates another CurvatureRedirector that counters this redirector
        /// </summary>
        public void CreateOpposingRedirector()
        {
            GameObject opposingRedirectorObject = new GameObject("Opposing_" + gameObject.name);
            CurvatureRedirector opposingRedirector = opposingRedirectorObject.AddComponent<CurvatureRedirector>();
            
            opposingRedirector.transform.position = EndPlayAreaPosition;
            opposingRedirector.transform.rotation = Quaternion.AngleAxis(180.0f, transform.up) * EndPlayAreaRotation;

            opposingRedirector.playerCamera = playerCamera;
            opposingRedirector.virtualDirection = virtualDirection;
            opposingRedirector.redirectionLength = redirectionLength;
            opposingRedirector.degreesPerMeter = -degreesPerMeter;
            opposingRedirector.gizmoLineDistance = gizmoLineDistance;
            opposingRedirector.PlayAreaDimensions = PlayAreaDimensions;
            opposingRedirector.redirectionObject = redirectionObject;
        }
    }
}
