using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CurvatureGames.SpaceExtender
{
    /// This Behaviour receives events from OverlappingRedirectors when players
    /// enter and exit their interior and transition areas, and coordinates
    /// the activation and deactivation of the rooms the player is visiting.
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class OverlappingRedirectorPlayer : MonoBehaviour
    {
        /// The room the player is currently visiting in their perception,
        /// meaning the player is in the room's interior and it is visible to them.
        /// When the player is in a transition area or elsewhere, this will be null.
        [HideInInspector]
        public OverlappingRedirector currentRoom;

        /// All rooms in whose transition area the player currently resides
        public HashSet<OverlappingRedirector> roomsInTransition = new HashSet<OverlappingRedirector>();

        /// All rooms in whose interior area the player currently resides
        public HashSet<OverlappingRedirector> roomsInInterior = new HashSet<OverlappingRedirector>();

        public HashSet<OverlappingRedirector> activeRooms = new HashSet<OverlappingRedirector>();

        private void Reset()
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }

        public void OnPlayerEnterInterior(OverlappingRedirector room)
        {
            roomsInInterior.Add(room);

            // if this room is not visible to the player
            // or the player is re-entering the current room, don't do anything
            if (!room.objectToHide.activeInHierarchy || room == currentRoom)
            {
                return;
            }

            // should the player be able to enter a room through overlapping transition
            // areas, overlapping rooms will be visible at the time of entering. To prevent
            // this, we deactivate the previous one. While the player will probably
            // notice this, it is better than showing the overlapping rooms the whole time
            if (currentRoom != null && currentRoom.objectToHide.activeInHierarchy)
            {
                SetRoomActive(currentRoom, false);
            }

            currentRoom = room;
        }

        public void OnPlayerExitInterior(OverlappingRedirector room)
        {
            roomsInInterior.Remove(room);

            // if the player is leaving the current room through the transition
            // area, it is not the current room anymore.
            // if they are leaving a room that is not the current one,
            if (room != currentRoom 
                // or not leaving but e.g. peeking through a wall,
                || !room.playerInTransition)
            {
                // the current room should remain set.
                return;
            }

            currentRoom = null;

            // activate all other rooms in whose transition areas the player
            // currently resides, because these could not be shown before
            // when the player was in a room overlapping with them
            foreach (var roomInTransition in roomsInTransition)
            {
                if (!roomInTransition.objectToHide.activeInHierarchy) {
                    SetRoomActive(roomInTransition, true);
                }
            }
        }

        public void OnPlayerEnterTransition(OverlappingRedirector room)
        {
            roomsInTransition.Add(room);

            // if the player is inside a room, do nothing
            if (currentRoom != null) {
                return;
            }

            // deactivate other rooms that might still be active from before,
            // but where the player is not in transition or interior anymore
            var roomsToDeactivate = activeRooms.Except(roomsInTransition).Except(roomsInInterior);
            foreach(var activeRoom in roomsToDeactivate.ToList()) {
                this.SetRoomActive(activeRoom, false, true);
            }

            SetRoomActive(room, true);
        }

        public void OnPlayerExitTransition(OverlappingRedirector room)
        {
            roomsInTransition.Remove(room);

            // if the player is not inside this room
            if (!room.playerInInterior 
                // and not in an area overlapping other rooms (peeking through a wall),
                && roomsInInterior.Count == 0)
            {
                // he has gone elsewhere and it can be deactivated
                SetRoomActive(room, false);
            }
        }

        public void SetRoomActive(OverlappingRedirector room, bool active, bool willActivateAnother = false)
        {
            // make sure at least one room is always visible
            if (!active && activeRooms.Count <= 1 && !willActivateAnother) {
                return;
            }

            if (active) {
                activeRooms.Add(room);
            } else {
                activeRooms.Remove(room);
            }

            room.SetRoomActive(active);
        }
    }
}