using System.Collections.Generic;
using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    public class OverlappingRedirector : MonoBehaviour
    {
        public GameObject objectToHide;

        public bool playerInInterior;

        public bool playerInTransition;

        public bool playerStartsInThisRoom;

        public OverlappingRoomTransition transition;

        public HashSet<GameObject> objectsInRoom = new HashSet<GameObject>();

        public bool showAreaGizmos;
        public CompoundTriggerEvents transitionArea;
        public CompoundTriggerEvents interiorArea;

        protected void Start()
        {
            var transitionArea = transform.Find("TransitionArea").gameObject.GetComponent<CompoundTriggerEvents>();
            transitionArea.RaiseTriggerEnter += OnColliderEnterTransition;
            transitionArea.RaiseTriggerExit += OnColliderExitTransition;
            var interiorArea = transform.Find("InteriorArea").gameObject.GetComponent<CompoundTriggerEvents>();
            interiorArea.RaiseTriggerEnter += OnColliderEnterInterior;
            interiorArea.RaiseTriggerExit += OnColliderExitInterior;
            if (playerStartsInThisRoom)
            {
                interiorArea.RaiseTriggerEnter += OnStartInRoomEnterInterior;
            }
            playerInInterior = false;
            SetRoomActiveInstantly(false);
            if (transition != null) {
                transition.RaiseTransitionEnd += OnTransitionEnd;
            }
        }

        /// <summary>
        /// Activate this room at the start of the game by simulating the player entering it through
        /// the transition area.
        /// </summary>
        private void OnStartInRoomEnterInterior(object sender, CollisionEventArgs args)
        {
            if (!args.collider.gameObject.TryGetComponent<OverlappingRedirectorPlayer>(out var _)) {
                return;
            }

            // simulate going into the room
            OnColliderEnterTransition(sender, args);
            OnColliderEnterInterior(sender, args);
            OnColliderExitTransition(sender, args);
            // only do it once
            interiorArea.RaiseTriggerEnter -= OnStartInRoomEnterInterior;
        }

        private void OnColliderEnterInterior(object sender, CollisionEventArgs args)
        {
            var obj = args.collider.gameObject;
            if (obj.TryGetComponent<OverlappingRedirectorPlayer>(out var player))
            {
                playerInInterior = true;
                player.OnPlayerEnterInterior(this);
            }
            else
            {
                if (obj.GetComponent<Rigidbody>() != null && 
                    !obj.isStatic &&
                    (!obj.TryGetComponent<IsInRoom>(out var isInRoom) || isInRoom.willBeRemoved) &&
                    objectToHide.activeInHierarchy)
                {
                    obj.AddComponent<IsInRoom>();
                    objectsInRoom.Add(obj);
                }
            }
        }

        private void OnColliderExitInterior(object sender, CollisionEventArgs args)
        {
            var obj = args.collider.gameObject;
            if (obj.TryGetComponent<OverlappingRedirectorPlayer>(out var player))
            {
                playerInInterior = false;
                player.OnPlayerExitInterior(this);
            }
            else
            {
                if (obj.TryGetComponent<IsInRoom>(out var isInRoom) && objectToHide.activeInHierarchy)
                {
                    isInRoom.willBeRemoved = true;
                    Destroy(isInRoom);
                    objectsInRoom.Remove(obj);
                }
            }
        }

        private void OnColliderEnterTransition(object sender, CollisionEventArgs args)
        {
            if (args.collider.gameObject.TryGetComponent<OverlappingRedirectorPlayer>(out var player))
            {
                playerInTransition = true;
                player.OnPlayerEnterTransition(this);
            }
        }

        private void OnColliderExitTransition(object sender, CollisionEventArgs args)
        {
            if (args.collider.gameObject.TryGetComponent<OverlappingRedirectorPlayer>(out var player))
            {
                playerInTransition = false;
                player.OnPlayerExitTransition(this);
            }
        }

        public void SetRoomActive(bool active)
        {
            if (transition != null) {
                transition.OnTransitionStart(active);

                // Wait for the transition to finish before doing anything
                return;
            }

            SetRoomActiveInstantly(active);
        }

        public void SetRoomActiveInstantly(bool active)
        {
            objectToHide.SetActive(active);

            foreach (GameObject obj in objectsInRoom)
            {
                if (!obj.GetComponent<Rigidbody>().isKinematic)
                {
                    obj.SetActive(active);
                }
            }
        }

        public void OnTransitionEnd(object sender, bool active)
        {
            SetRoomActiveInstantly(active);
        }

        private void OnDrawGizmos()
        {
            if (showAreaGizmos)
            {
                if (transitionArea != null)
                    DrawAreaGizmo(transitionArea.gameObject, new Color(0, 0, 0, 0.3f));
                if (interiorArea != null)
                    DrawAreaGizmo(interiorArea.gameObject, new Color(0, 0, 0, 0.8f));
            }
        }

        private void DrawAreaGizmo(GameObject area, Color color)
        {
            if(area != null)
            {
                List<Collider> colliders = GetChildColliders(area.transform);
                foreach (Collider c in colliders)
                {
                    Gizmos.color = color;
                    Gizmos.DrawCube(c.bounds.center, c.bounds.extents * 2);
                    Gizmos.DrawWireCube(c.bounds.center, c.bounds.extents * 2);
                }
            }
        }

        private List<Collider> GetChildColliders(Transform transformToSearch)
        {
            List<Collider> result = new List<Collider>();
            result.AddRange(transformToSearch.GetComponents<Collider>());        
            for (int i = 0; i < transformToSearch.childCount; i++)
            {
                result.AddRange(GetChildColliders(transformToSearch.GetChild(i)));
            }
            return result;
        }
    }
}