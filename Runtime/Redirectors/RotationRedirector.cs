using System.Collections.Generic;
using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// <summary>
    /// Handles rotation redirection.
    /// Playarea is rotated around a point at the same time the user rotates her head, resulting in a rotation gain.
    /// In order to minimize motionsickness and involuntaray user movement, the user should stay as close as possible to the rotation point of the redirection.
    /// </summary>
    public class RotationRedirector : BaseRedirector
    {
        //custom editor WARNING: before renaming any public / SerializeField variable, check if it is used inside a custom editor. If so, update the name of the FindProperty call inside the custom editor
    

        // Used by custom editor
        /// <summary>
        /// Point in local space, around which the playarea will be rotated.
        /// </summary>
        [SerializeField] private Vector3 rotationPoint = new Vector3(10, 0, 10);
    
        // Used by custom editor
        /// <summary>
        /// Total rotation in degrees the player will be redirected.
        /// </summary>
        [SerializeField] private float rotationDegrees = 0;


        // Used by custom editor
        [SerializeField] private float leftRotationGain = .1f;
        // Used by custom editor
        [SerializeField] private float rightRotationGain = -.1f;
        // Used by custom editor
        [SerializeField] bool velocityDependentGain = true;
        // Used by custom editor
        [SerializeField] float rotationSpeedUpperThreshold = 350.0f;
    
    
        private bool isRedirecting = false;
        private float targetRotationAngle = 0;
        private Quaternion lastHeadRotation = Quaternion.identity;
        private float lastAngleRotation = 0f;
        private float rotationProgress = 0f;

        //Logging Value
        private float totalRealRotation = 0f;
        private float totalTime = 0f;
    
        // Properties ----------------------------------------------------------------------------------------

        /// <summary>
        /// Position around which the playarea will be rotated.
        /// (local space)
        /// </summary>
        /// <value></value>
        public Vector3 RotationPoint{get{return rotationPoint;}}
    
        /// <summary>
        /// Center position of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Vector3 EndPlayAreaPosition{get{return RotatePointAroundPivot(transform.position, transform.TransformPoint(rotationPoint), Quaternion.AngleAxis(rotationDegrees, transform.TransformVector(Vector3.up)));}}

        /// <summary>
        /// Rotation of the end playarea. (worldspace)
        /// </summary>
        /// <returns></returns>
        public override Quaternion EndPlayAreaRotation {get{return transform.rotation * Quaternion.Euler(0, rotationDegrees, 0);}}

        /// <summary>
        /// Current progress of the rotation redirection. [0,1]
        /// </summary>
        public float RotationProgress { get { return rotationProgress; } }

        // end: Properties -----------------------------------------------------------------------------------------

        protected override void Update()
        {
            base.Update();
            RotationRedirection();
        }

        /// <summary>
        /// Calculates and applies the rotation redirection
        /// </summary>
        private void RotationRedirection()
        {
            
            Quaternion hmdRotation;
            // if the current hmd rotation is not avaiable this frame
            if(!TryGetHMDRotation(out hmdRotation))
            {
                hmdRotation = lastHeadRotation;
            }
            if (isRedirecting)
            {
                
                float rotationDelta = GetRotationDelta(lastHeadRotation, hmdRotation);
                UpdateLogging(rotationDelta);
                float gain = GetGain(rotationDelta);

                float normalizedRotation = Mathf.Abs(Mathf.Abs(rotationDelta) * gain) / Mathf.Abs(targetRotationAngle);
                rotationProgress += normalizedRotation;

                rotationProgress = Mathf.Clamp01(rotationProgress);
                float currentAngleRotation = rotationProgress * targetRotationAngle;
                if(redirectionObject)
                {
                    redirectionObject.transform.RotateAround(transform.TransformPoint(rotationPoint), transform.TransformDirection(Vector3.up), currentAngleRotation - lastAngleRotation);
                }
                else
                {
                    Debug.LogWarning("Redirection Object not set! Redirection can not be applied!");
                }

                lastAngleRotation = currentAngleRotation;


                if (rotationProgress == 1f)
                {
                    EndRedirection();
                }
            }

            lastHeadRotation = hmdRotation;
        }

        private float GetGain(float rotationDelta)
        {
            float gain;
            // is user rotating to the right?
            if (rotationDelta > 0)
            {
                gain = rightRotationGain;
            }
            else
            {
                gain = leftRotationGain;
            }

            if(velocityDependentGain)
            {
                gain *= Mathf.InverseLerp(0, rotationSpeedUpperThreshold, Mathf.Abs(rotationDelta / Time.deltaTime));
            }
            return gain;
        }

        /// <summary>
        /// Attempt to retrieve a quaternion representing the current rotation of the hmd.
        /// </summary>
        /// <param name="hmdRotation"></param>
        /// <returns>true if rotation could be retrieved successfully.</returns>
        private bool TryGetHMDRotation(out Quaternion hmdRotation){
            List<UnityEngine.XR.XRNodeState> nodeStates = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);
            var headNodeState = nodeStates.Find(nodeState => nodeState.nodeType.Equals(UnityEngine.XR.XRNode.Head));
            return headNodeState.TryGetRotation(out hmdRotation);
        }

        private float GetRotationDelta(Quaternion rotationA, Quaternion rotationB)
        {
            // get a "forward vector" for each rotation
            var forwardA = rotationA * Vector3.forward;
            var forwardB = rotationB * Vector3.forward;

            // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
            var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

            // get the signed difference in these angles
            var angleDiff = Mathf.DeltaAngle(angleA, angleB);

            return angleDiff;
        }

        /// <summary>
        /// Starts applying rotation redirection.
        /// </summary>
        public override void StartRedirection()
        {
            StartLogging();
            targetRotationAngle = GetRotationDelta(StartPlayAreaRotation, EndPlayAreaRotation);
            rotationProgress = 0f;
            isRedirecting = true;
            base.StartRedirection();
        }

        /// <summary>
        /// Stops applying rotation redirection.
        /// </summary>
        public override void EndRedirection()
        {
            
            isRedirecting = false;
            EndLogging();

            base.EndRedirection();
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) {
            return rotation * (point - pivot) + pivot;
        }

        private void StartLogging()
        {
            if(SpaceExtenderLoggingManager.Instance.LoggingEnabled == true)
                totalTime = Time.time;
        }

        private void UpdateLogging(float rotationDelta)
        {
            if (SpaceExtenderLoggingManager.Instance.LoggingEnabled == true)
                totalRealRotation += Mathf.Abs(rotationDelta);
        }

        private void EndLogging()
        {
            if (SpaceExtenderLoggingManager.Instance.LoggingEnabled == true)
            {
                Debug.Log("Hallo");
                totalTime = Time.time - totalTime;
                SpaceExtenderLoggingManager.Instance.LogData(gameObject.name, totalTime, totalRealRotation);
            }
        }
    }
}
