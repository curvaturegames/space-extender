using CurvatureGames.SpaceExtender;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{

    /// This file defines new uxml elements for customizing the labels and a corresponding propertyDrawer for easily using the new uxml-element. 
    /// You can use the CustomVectorField-element directly inside uxml and use it's traits (x-label, y-label...),
    /// or you can use the CustomVector-property-attribute at the c# decleration of the vector and just use the normal 
    /// PropertyField inside uxml.


    /// <summary>
    /// CustomVector2Field UIElement that lets you customize the x- and y-labels.
    /// </summary>
    public class CustomVector2Field : Vector2Field
    {
        /// <summary>
        /// Factory for uxml
        /// </summary>
        public new class UxmlFactory : UxmlFactory<CustomVector2Field, UxmlTraits>
        {
        }

        /// <summary>
        /// Used to define applicable uxml traits and link them to the c# properties
        /// </summary>
        public new class UxmlTraits : Vector2Field.UxmlTraits
        {
            UxmlStringAttributeDescription m_xLabel = new UxmlStringAttributeDescription {name = "x-label"};
            UxmlStringAttributeDescription m_yLabel = new UxmlStringAttributeDescription {name = "y-label"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var customVec2 = ve as CustomVector2Field;

                customVec2.xLabel = m_xLabel.GetValueFromBag(bag, cc);
                customVec2.yLabel = m_yLabel.GetValueFromBag(bag, cc);
            }
        }



        public CustomVector2Field() : base()
        {
            Init();
        }

        public CustomVector2Field(string label) : base(label)
        {
            Init();
        }

        private FloatField xFloatField;
        private FloatField yFloatField;

        public string xLabel
        {
            get { return xFloatField.label; }
            set { xFloatField.label = value; }
        }

        public string yLabel
        {
            get { return yFloatField.label; }
            set { yFloatField.label = value; }
        }

        private void Init()
        {
            var floatFields = contentContainer.Query<FloatField>().ToList();
            xFloatField = floatFields[0];
            yFloatField = floatFields[1];

            labelElement.style.paddingLeft = 0;
            xFloatField.labelElement.style.flexBasis = StyleKeyword.Auto;
            yFloatField.labelElement.style.flexBasis = StyleKeyword.Auto;
        }
    }

    /// <summary>
    /// Draws properties that use the CustomVector2-attribute 
    /// </summary>
    [CustomPropertyDrawer(typeof(CustomVector2))]
    public class CustomVectorDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var customVec2 = new CustomVector2Field();
                customVec2.BindProperty(property);

                var customVecAttribute = attribute as CustomVector2;
                customVec2.xLabel = customVecAttribute.xLabel;
                customVec2.yLabel = customVecAttribute.yLabel;
                customVec2.label = property.displayName;
                return customVec2;
            }
            //else if (property.propertyType == SerializedPropertyType.Vector3)
            //{
            //    var customVec3 = new CustomVector3Field();
            //    customVec3.BindProperty(property);

            //    var customVecAttribute = attribute as CustomVector;
            //    customVec3.xLabel = customVecAttribute.xLabel;
            //    customVec3.yLabel = customVecAttribute.yLabel;
            //    customVec3.zLabel = customVecAttribute.zLabel;
            //    customVec3.label = property.displayName;
            //    return customVec3;
            //}
            //else if (property.propertyType == SerializedPropertyType.Vector4)
            //{
            //    var customVec4 = new CustomVector4Field();
            //    customVec4.BindProperty(property);

            //    var customVecAttribute = attribute as CustomVector;
            //    customVec4.xLabel = customVecAttribute.xLabel;
            //    customVec4.yLabel = customVecAttribute.yLabel;
            //    customVec4.zLabel = customVecAttribute.zLabel;
            //    customVec4.wLabel = customVecAttribute.wLabel;
            //    customVec4.label = property.displayName;
            //    return customVec4;
            //}
            return new Label("Use CustomVector attribute with Vector2, Vector3, Vector4 only.");

        }
    }
}
