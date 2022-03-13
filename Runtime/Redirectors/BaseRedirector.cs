using UnityEngine;
using UnityEngine.Events;

namespace CurvatureGames.SpaceExtender
{
    /// <summary>
    /// Base class for all Redirectors.
    /// Creates start and end playarea as well as event calls.
    /// </summary>
    abstract public class BaseRedirector : MonoBehaviour
    {
        //custom editor WARNING: before renaming any public / SerializeField variable, check if it is used inside a custom editor. If so, update the name of the FindProperty call inside the custom editor
    
        // used inside custom editor
        /// <summary>
        /// Dimensions of the vr playarea in meters.
        /// x = width, y = depth
        /// </summary>
    
        [CustomVector2("Width", "Depth")]
        [SerializeField]  private Vector2 playAreaDimensions = new Vector2(4, 3);
    
        // Used by custom editor
        /// <summary>
        /// All redirection gains are applied to this gameobject. 
        /// The VR playarea must be one of it's children for redirection to work.
        /// </summary>
        public Transform redirectionObject;

        // used inside custom editor
        /// <summary>
        /// used for drawing the playarea UI inside custom editor
        /// </summary>
        [SerializeField] private Mesh gizmoPlayAreaMesh;


        // properties --------------------------------------------------------------------------------
        public Vector2 PlayAreaDimensions{ get { return playAreaDimensions; } protected set { playAreaDimensions = value; } }
        /// <summary>
        /// Center position of the start playarea. (worldspace)
        /// </summary>
        /// <value></value>
        public Vector3 StartPlayAreaPosition{get{return transform.position;}}
        /// <summary>
        /// Rotation of the start playarea. (worldspace)
        /// </summary>
        /// <value></value>
        public Quaternion StartPlayAreaRotation{get{return transform.rotation;}}
        /// <summary>
        /// Center position of the end playarea. (worldspace)
        /// </summary>
        /// <value></value>
        abstract public Vector3 EndPlayAreaPosition{get;}
        /// <summary>
        /// Rotation of the end playarea. (worldspace)
        /// </summary>
        /// <value></value>
        abstract public Quaternion EndPlayAreaRotation{get;}
        /// <summary>
        /// used for drawing the playarea UI inside custom editor.
        /// (1m x 1m plane)
        /// </summary>
        /// <value></value>
        public Mesh GizmoPlayAreaMesh{get {return gizmoPlayAreaMesh;}}

        // events ------------------------------------------------------------------
        public UnityEvent OnRedirectionStarted;
        public UnityEvent OnRedirectionEnded;


        public virtual void StartRedirection()
        {
            OnRedirectionStarted.Invoke();
        }
        public virtual void EndRedirection()
        {
            OnRedirectionEnded.Invoke();
        }

        protected virtual void Reset()
        {
            gizmoPlayAreaMesh = Resources.Load("Space Extender/Meshes/1x1planeMesh") as Mesh;
        }

        protected virtual void Awake()
        {
            // already declared protected virtual for possible future use.
        }

        protected virtual void Update()
        {
            // already declared protected virtual for possible future use.
        }

        protected virtual void OnEnable()
        {
            // already declared protected virtual for possible future use.
        }

        protected virtual void OnDisable()
        {
            // already declared protected virtual for possible future use.
        }

        /// <summary>
        /// Old way of initializing the mesh. Now it is loaded from resources in Reset()
        /// Initializes the gizmoMesh to a 1x1 plane mesh. gizmoMesh is used by the custom editor to visualize the playArea.
        /// </summary>
        private void InitGizmoMesh()
        {
            gizmoPlayAreaMesh = new Mesh();

            Vector3[] verts = new Vector3[4];
   
            verts[0] = new Vector3(-.5f, 0, .5f);
            verts[1] = new Vector3(.5f, 0, .5f);
            verts[2] = new Vector3(.5f, 0, -.5f);
            verts[3] = new Vector3(-.5f, 0, -.5f);

            gizmoPlayAreaMesh.vertices = verts;


            var tris = new int[6]
            {
                // lower left triangle
                0, 1, 3,
                // upper right triangle
                2, 3, 1
            };
            gizmoPlayAreaMesh.triangles = tris;

            var normals = new Vector3[4]
            {
                -Vector3.up,
                -Vector3.up,
                -Vector3.up,
                -Vector3.up
            };
            gizmoPlayAreaMesh.normals = normals;

            var uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            gizmoPlayAreaMesh.uv = uv;

        }
    }
}