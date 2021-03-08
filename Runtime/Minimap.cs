using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace CurvatureGames.SpaceExtender
{
    [ExecuteAlways]
    public class Minimap : MonoBehaviour
    {
        public Material material;

        public Transform head;

        public Transform playerPin;

        public bool northIsAlwaysUp;

        public Transform handTransform;

        public bool onlyShowWhenPlayerLooksAtHand;

        [SerializeField] 
        public GameObject meshContainer;

        public List<OverlappingRedirector> redirectors;

        public Dictionary<Transform, GameObject> rooms;

        [SerializeField]
        private List<string> minimapGuids = new List<string>();

        [SerializeField]
        private List<GameObject> minimapRoomGameObjects = new List<GameObject>();

        private Dictionary<string, GameObject> minimapRooms;

        public void Start()
        {
            if (!Application.IsPlaying(this.gameObject)) {
                return;
            }

            foreach (var obj in GetAllRootObjects()) {
                redirectors = FindAllRedirectors();
            }
        }

        public void Update()
        {
            // todo re-calculcate minimap room diffs here to correctly map playerpin position 
            // when rooms in minimap get modified without re-generating the map

            if (!Application.IsPlaying(this.gameObject)) {
                return;
            }

            var playerOffset = head.position;

            foreach (var redirector in redirectors) {
                if (redirector.playerInInterior && 
                    redirector.objectToHide.activeInHierarchy && 
                    redirector.objectToHide.TryGetComponent<MinimapRoom>(out var minimapRoom)) {
                    playerOffset = minimapRoom.TransformRoomPosition(playerOffset, redirector.objectToHide.transform);
                }
            }

            // center the minimap vertically around the player position indicator
            meshContainer.transform.localPosition = new Vector3(0, -playerOffset.y, 0);

            playerOffset.y = 0;
            playerPin.localPosition = playerOffset;

            if (handTransform == null) {
                return;
            }

            var handUpDirection = -handTransform.right;

            if (onlyShowWhenPlayerLooksAtHand) {
                var visionAngleToHead = Vector3.Angle(head.forward, transform.position - head.position);
                var handAngleToHead = Vector3.Angle(head.forward, -handUpDirection);
                ToggleMapVisibility(visionAngleToHead <= 25 && handAngleToHead <= 90);
            }

            Quaternion targetRotation;
            // somehow, the head position is not exactly in the center between the user's eyes,
            // so we shift it to the right a little bit
            var headToHand = handTransform.position - (head.position + head.right.normalized * 0.15f);
            if (northIsAlwaysUp) {
                var pitch = Vector3.Angle(head.up, handUpDirection) / 90;
                targetRotation = Quaternion.Lerp(NorthAwayFromUser(handUpDirection, headToHand), NorthUp(handUpDirection), pitch);
            } else {
                var pitch = Vector3.Angle(Vector3.up, handUpDirection) / 90;
                targetRotation = Quaternion.Lerp(ViewDirectionAwayFromUser(handUpDirection), ViewDirectionUp(handUpDirection, headToHand), pitch);
            }

            transform.rotation = targetRotation;
        }

        private Quaternion NorthUp(Vector3 handUpDirection)
        {
            var projected = Vector3.ProjectOnPlane(head.up, handUpDirection);
            return Quaternion.LookRotation(projected, handUpDirection);
        }

        private Quaternion NorthAwayFromUser(Vector3 handUpDirection, Vector3 headToHand)
        {
            var projected = Vector3.ProjectOnPlane(headToHand, handUpDirection);
            return Quaternion.LookRotation(projected, handUpDirection);
        }
    
        private Quaternion ViewDirectionAwayFromUser(Vector3 handUpDirection)
        {
            var projected = Vector3.ProjectOnPlane(Vector3.forward, handUpDirection);
            return Quaternion.LookRotation(projected, handUpDirection);
        }

        private Quaternion ViewDirectionUp(Vector3 handUpDirection, Vector3 headToHand)
        {
            var projected = Vector3.ProjectOnPlane(headToHand, Vector3.up);
            var projectedRotation = Quaternion.FromToRotation(projected, Vector3.forward);
            return NorthUp(handUpDirection) *
                   projectedRotation;
        }

        private void ToggleMapVisibility(bool show)
        {
            this.meshContainer.SetActive(show);
            this.playerPin.gameObject.SetActive(show);
        }

        /// <summary>
        /// toggle rotating minimap (rotate vs north is up)
        /// </summary>
        public void ToggleMapOrientation()
        {
            northIsAlwaysUp = !northIsAlwaysUp;
        }

        /// <summary>
        /// this will generate a new minimap based on static game objects in the scene
        /// </summary>
        public void RegenerateMap()
        {
            // create a dictionary from the two serialized lists
            minimapRooms = minimapGuids.Zip(minimapRoomGameObjects, (k, v) => new {k, v})
                .ToDictionary(x => x.k, x => x.v);

            foreach (var obj in GetAllRootObjects()) {
                var rooms = obj.GetComponentsInChildren<MinimapRoom>(true);
                foreach (var minimapRoom in rooms) {
                    if (minimapRooms.TryGetValue(minimapRoom.guid, out var room) && room != null) {
                        var minimapTransform = room.transform;

                        if (minimapTransform != null && minimapTransform.hasChanged) {
#if UNITY_EDITOR
                            Undo.RecordObject(minimapRoom, "Update minimap room transform");
#endif
                            minimapRoom.MinimapTransformChanged(minimapTransform);
                            minimapTransform.hasChanged = false;
                        }
                    }
                }
            }

            ClearChildren();
            rooms = new Dictionary<Transform, GameObject>(); 
            redirectors = new List<OverlappingRedirector>();

            // walk the whole scene graph once and clone all objects that should 
            // be shown on the minimap as well as their parents.
            this.meshContainer = new GameObject("MinimapMeshContainer");
            this.meshContainer.transform.SetParent(transform, false);
            /// Find all redirectors and remember their assigned rooms, 
            /// so in <see cref="Clone"/> we know when we are cloning a room 
            redirectors = FindAllRedirectors();
            foreach (var redirector in redirectors) {
                rooms[redirector.objectToHide.transform] = null;
            }

            foreach (var obj in GetAllRootObjects()) {
                RecursiveClone(obj)?.transform.SetParent(meshContainer.transform, false);
            }

            // Save our minimapRoom dictionary in the two lists
            // that unity is able to serialize
            minimapGuids = new List<string>(minimapRooms.Keys);
            minimapRoomGameObjects = new List<GameObject>(minimapRooms.Values);

            // MoveRoomsApart();
        }

        private List<OverlappingRedirector> FindAllRedirectors()
        {
            var results = new List<OverlappingRedirector>();
            foreach (var obj in GetAllRootObjects()) {
                var redirectorChildren = obj.GetComponentsInChildren<OverlappingRedirector>();
                results.AddRange(redirectorChildren);
            }

            return results;
        }

        private List<GameObject> GetAllRootObjects()
        {
            var results = new List<GameObject>();
            var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++) {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                results.AddRange(scene.GetRootGameObjects());
            }

            return results;
        }

        private void MoveRoomsApart()
        {
            var transformedRooms = new HashSet<OverlappingRedirector>();
            foreach (var redirectorA in redirectors) {
                foreach (var redirectorB in redirectors) {
                    var boundsA = GetRedirectorBounds(redirectorA);
                    var boundsB = GetRedirectorBounds(redirectorB);
                    if (boundsA != boundsB && 
                        !transformedRooms.Contains(redirectorA) && 
                        !transformedRooms.Contains(redirectorB) &&
                        boundsA.Intersects(boundsB)
                    ) {
                        var intersection = GetIntersectionVector(boundsA, boundsB) / 2;
                        var cloneA = rooms[redirectorA.objectToHide.transform];
                        var cloneB = rooms[redirectorB.objectToHide.transform];

                        var intersectionAxis = AbsoluteVector(Vector3.Normalize(intersection));
                        var intersectionAmount = Vector3.Magnitude(intersection);

                        var lengthA = Vector3.Magnitude(Vector3.Scale(boundsA.extents, intersectionAxis)) * 2;
                        var scaleA = intersectionAmount / lengthA;
                        var lengthB = Vector3.Magnitude(Vector3.Scale(boundsB.extents, intersectionAxis)) * 2;
                        var scaleB = intersectionAmount / lengthB;

                        cloneA.transform.Translate(- AbsoluteVector(cloneA.transform.localRotation * Vector3.Scale((intersection / 2), cloneA.transform.parent.lossyScale)));
                        cloneA.transform.localScale -= AbsoluteVector(cloneA.transform.localRotation * intersectionAxis) * scaleA;
                        cloneB.transform.Translate(AbsoluteVector(cloneB.transform.localRotation * Vector3.Scale((intersection / 2), cloneB.transform.parent.lossyScale)));
                        cloneB.transform.localScale -= AbsoluteVector(cloneB.transform.localRotation * intersectionAxis) * scaleB;


                        transformedRooms.Add(redirectorA);
                        transformedRooms.Add(redirectorB);
                    }
                }
            }
        }

        private Vector3 AbsoluteVector(Vector3 vector) {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));;
        }

        private Bounds GetRedirectorBounds(OverlappingRedirector redirector) 
        {
            var bounds = redirector.transform.Find("InteriorArea").GetComponent<Collider>().bounds;
            bounds.center = redirector.transform.TransformPoint(bounds.center);
            bounds.extents = redirector.transform.TransformVector(bounds.extents);

            return bounds;
        }

        private Vector3 GetIntersectionVector(Bounds a, Bounds b) 
        {
            var x = ResolveCollisionAxis(a.center.x, b.center.x, a.extents.x, b.extents.x);
            var y = ResolveCollisionAxis(a.center.y, b.center.y, a.extents.y, b.extents.y);
            var z = ResolveCollisionAxis(a.center.z, b.center.z, a.extents.z, b.extents.z);

            var absX = Mathf.Abs(x);
            var absY = Mathf.Abs(y);
            var absZ = Mathf.Abs(z);

            if (absX < absY) {
                if (absX < absZ) {
                    return new Vector3(x, 0f, 0f);
                } else {
                    return new Vector3(0f, 0f, z);
                }
            } else {
                if (absY < absZ) {
                    return new Vector3(0f, y, 0f);
                } else {
                    return new Vector3(0f, 0f, z);
                }
            }
        }

        private float ResolveCollisionAxis(float centerA, float centerB, float extentsA, float extentsB) {
            var resolveRight = (centerA + extentsA) - (centerB - extentsB);
            var resolveLeft = (centerA - extentsA) - (centerB + extentsB);

            return MinAbs(resolveRight, resolveLeft);
        }

        private float MinAbs(float a, float b) {
            if (Mathf.Abs(a) < Mathf.Abs(b)) {
                return a;
            }
            return b;
        }

        private void ClearChildren()
        {
            if (Application.IsPlaying(this.gameObject)) {
                Destroy(this.meshContainer);
            } else {
                DestroyImmediate(this.meshContainer);
            }
        }

        /// <summary>
        /// Objects should be displayed on the minimap if they
        /// - have a Collider that is not a trigger
        /// - and have a MeshFilter component
        /// - and are static
        /// </summary>
        private bool ShouldBeDisplayedOnMinimap(GameObject obj)
        {
            return (
                obj.TryGetComponent<Collider>(out var collider) &&
                !collider.isTrigger && 
                obj.TryGetComponent<MeshFilter>(out var _ ) &&
                obj.isStatic &&
                !obj.TryGetComponent<DontShowOnMinimap>(out var _)
            );
        }

        ///<summary> 
        /// Look at every transitive child of the GameObject given.
        /// if any child should be included in the minimap as indicated by
        /// <see cref="ShouldBeDisplayedOnMinimap"/>, clone that child as well as all of it's
        /// parents, preserving the Transform hierarchy of the original scene.
        /// </summary>
        private GameObject RecursiveClone(GameObject src)
        {
            GameObject ownClone = null;

            foreach (Transform child in src.transform) {
                var clonedChild = RecursiveClone(child.gameObject);
                if (clonedChild != null) {
                    if (ownClone == null) {
                        ownClone = Clone(src);
                    }
                    clonedChild.transform.SetParent(ownClone.transform, false);
                }
            }

            if (ownClone == null && ShouldBeDisplayedOnMinimap(src)) {
                ownClone = Clone(src);
            }

            return ownClone;
        }

        private GameObject Clone(GameObject src)
        {
            GameObject clone = new GameObject("Minimap" + src.name, typeof(MeshFilter), typeof(MeshRenderer));

            clone.transform.localPosition = src.transform.localPosition;
            clone.transform.localRotation = src.transform.localRotation;
            clone.transform.localScale = src.transform.localScale;
            clone.GetComponent<MeshRenderer>().material = material;

            if (src.TryGetComponent<MeshFilter>(out var mesh)) {
                clone.GetComponent<MeshFilter>().mesh = mesh.sharedMesh;
            }

            if (rooms.ContainsKey(src.transform)) {
                if (!src.TryGetComponent<MinimapRoom>(out var room)) {
                    // Debug.Log("adding " + src.name);
#if UNITY_EDITOR
                    Undo.AddComponent<MinimapRoom>(src);
#endif
                    src.GetComponent<MinimapRoom>().SetOriginalTransform(clone.transform);
                } else {
                    // Debug.Log("updating " + src.name);

                    src.GetComponent<MinimapRoom>().SetOriginalTransform(clone.transform);
                    src.GetComponent<MinimapRoom>().ApplyMinimapTransform(clone.transform);
                }

                minimapRooms[src.GetComponent<MinimapRoom>().guid] = clone;
                rooms[src.transform] = clone;

            }

            return clone;
        }
    }
}