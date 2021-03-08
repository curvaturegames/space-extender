using UnityEngine;
using System;

namespace CurvatureGames.SpaceExtender
{
    [ExecuteAlways]
    public class MinimapRoom : MonoBehaviour
    {
        [SerializeField]
        private TransformOperation originalTransform = new TransformOperation();

        [SerializeField]
        private TransformOperation minimapChanges = new TransformOperation();

        public string guid;

        public MinimapRoom()
        {
            // todo somehow, transform operations are not persisted properly between editor restarts
            if (guid == null) {
                guid = System.Guid.NewGuid().ToString();
            }
        }

        public void MinimapTransformChanged(Transform changed)    
        {
            minimapChanges = TransformOperation.Diff(originalTransform, changed);
        }

        public void SetOriginalTransform(Transform transform)
        {
            originalTransform = new TransformOperation(transform);
        }

        public void ApplyMinimapTransform(Transform other)
        {
            minimapChanges.Apply(other);
        }

        public Vector3 TransformRoomPosition(Vector3 playerPosition, Transform roomPosition)
        {
            return minimapChanges.TransformRoomPosition(playerPosition, roomPosition);
        }

        [Serializable]
        private class TransformOperation {
            [SerializeField]
            private Vector3 position = Vector3.zero;
        
            [SerializeField]
            private Quaternion rotation = Quaternion.identity;

            [SerializeField]
            private Vector3 scale = Vector3.one;

            public void Apply(Transform other) {
                other.localPosition += position;
                other.localRotation = other.localRotation * rotation;
                other.localScale = Vector3.Scale(other.localScale, scale);
            }

            public Vector3 TransformRoomPosition(Vector3 playerPosition, Transform roomPosition) {
                var minimapRoomPosition = roomPosition.position + position;
                var localPlayerPosition = playerPosition - roomPosition.position;
                var positionInRoom = rotation * Vector3.Scale(localPlayerPosition, scale);
                return minimapRoomPosition + positionInRoom;
            }

            public TransformOperation (Transform transform) 
            {
                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;
            }

            public TransformOperation()
            {
            }

            public static TransformOperation Diff(TransformOperation from, Transform to)
            {
                var operation = new TransformOperation();

                operation.position = to.localPosition - from.position;
                operation.rotation = Quaternion.Inverse(from.rotation) * to.localRotation;
                operation.scale = Vector3.Scale(to.localScale, new Vector3(1f/from.scale.x, 1f/from.scale.y, 1f/from.scale.z));

                return operation;
            }
        }
    }
}