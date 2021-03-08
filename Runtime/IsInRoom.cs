using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// used to track which objects are already associated with an
    /// overlapping room, see OverlappingRedirector for more
    public class IsInRoom : MonoBehaviour
    {
        /// When using `Destroy` to remove this component, it will not get
        /// removed until the current frame has completed.
        /// if there are an `exit` and `enter` collider event in the same
        /// frame, the redirector will mistakenly think that the object with 
        /// this component is already in another room, although it's just
        /// the components removal being delayed. To work around this,
        /// the redirector sets this flag to true on removal and ignores
        /// this component if it is true.
        public bool willBeRemoved = false;
    }
}