using UnityEngine;

namespace CurvatureGames.SpaceExtender
{
    /// <summary>
    /// CustomVector2-propertyAttribute for easily displaying a Vector2 as a CustomVector2.
    /// Use [CustomVector2("x-label-text", "y-label-text")] at the c# decleration of the property.
    /// </summary>
    public class CustomVector2 : PropertyAttribute
    {
        public string xLabel, yLabel, zLabel, wLabel;
        public CustomVector2(string xLabel="X", string yLabel="Y", string zLabel="Z", string wLabel="W")
        {
            this.xLabel = xLabel;
            this.yLabel = yLabel;
            this.zLabel = zLabel;
            this.wLabel = wLabel;
        }
    }
}

