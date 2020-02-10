using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MPUIKIT {
    [AddComponentMenu("UI/MPUI/MPImage")]
    public class MPImage : Image {
        private const string MP_SHADER_NAME = "MPUI/Sprite Basic";

        [SerializeField] private bool m_OverrideSize;
        [SerializeField] private Vector2 m_Size;

        [SerializeField] private DrawMethod m_DrawMethod;
        [SerializeField] private DrawShape m_DrawShape;
        [SerializeField] private float m_LineWeight;
        [SerializeField] private float m_FalloffDistance;

        [SerializeField] private Material m_SharedMaterial;

        //Rectangle Settings
        [SerializeField] private Vector4 m_RectangleCornerRadius;

        //circle Settings
        [SerializeField] private float m_CircleRadius;

        //Pentagon Settings
        [SerializeField] private Vector4 m_PentagonRectangleRadius;
        [SerializeField] private float m_PentagonTriangleRadius;
        [SerializeField] private float m_PentagonTriangleSize;

        public Vector4 PentagonRectangleRadius => m_PentagonRectangleRadius;
        public float PentagonTriangleRadius {
            get => m_PentagonTriangleRadius;
            set => m_PentagonTriangleRadius = Mathf.Max(value, 0.001f);
        }
        public float PentagonTriangleSize => m_PentagonTriangleSize;
        
        //Triangle Settings
        [SerializeField] private Vector3 m_TriangleRadius;
        public Vector3 TriangleRadius => m_TriangleRadius;
        
        
        
        private Material materialInstance = null;
        public DrawMethod DrawMethod {
            get => m_DrawMethod;
            set {
                m_DrawMethod = value;
                if (value != DrawMethod.Regular) {
                    useSpriteMesh = false;
                    preserveAspect = false;
                }

                base.SetMaterialDirty();
            }
        }
        public DrawShape DrawShape {
            get => m_DrawShape;
            set => m_DrawShape = value;
        }
        public Vector4 RectangleCornerRadius {
            get => m_RectangleCornerRadius;
            set {
                m_RectangleCornerRadius = new Vector4(
                    Mathf.Clamp(value.x, 0f, rectTransform.sizeDelta.x),
                    Mathf.Clamp(value.y, 0f, rectTransform.sizeDelta.y),
                    Mathf.Clamp(value.z, 0f, rectTransform.sizeDelta.y),
                    Mathf.Clamp(value.w, 0f, rectTransform.sizeDelta.x)
                );
                base.SetMaterialDirty();
            }
        }
        public float CircleRadius {
            get => m_CircleRadius;
            set => m_CircleRadius = value;
        }
        public float LineWeight {
            get => m_LineWeight;
            set {
                m_LineWeight = Mathf.Clamp(value, 0, Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y));
                base.SetMaterialDirty();
            }
        }
        public float FalloffDistance {
            get => m_FalloffDistance;
            set {
                m_FalloffDistance = value;
                base.SetMaterialDirty();
            }
        }
        public Material SharedMaterial {
            get => m_SharedMaterial;
            set {
                if (value != null && value.shader.name == MP_SHADER_NAME) {
                    m_SharedMaterial = value;
                }
                else {
                    m_SharedMaterial = null;
                }

                base.SetMaterialDirty();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            AssignMaterial();
        }
        
        private void AssignMaterial() {
            if (m_SharedMaterial == null) {
                if(materialInstance == null) materialInstance = new Material(Shader.Find(MP_SHADER_NAME));
                base.material = materialInstance;
            }
            else {
                base.material = m_SharedMaterial;
            }
        }

        public override Material GetModifiedMaterial(Material baseMaterial) {
            Debug.Log("Reinitializing baseMaterial");
            baseMaterial.SetTexture("_MainTex", sprite ? sprite.texture : null);
            baseMaterial.SetFloat("_LineWeight", m_LineWeight);
            baseMaterial.SetVector("_RectangleCornerRadius", m_RectangleCornerRadius);
            baseMaterial.SetFloat("_CircleRadius", m_CircleRadius);
            
            baseMaterial.SetVector("_PentagonRectangleRadius", m_PentagonRectangleRadius);
            baseMaterial.SetFloat("_PentagonTriangleRadius", m_PentagonTriangleRadius);
            baseMaterial.SetFloat("_PentagonTriangleSize", m_PentagonTriangleSize);
            
            baseMaterial.SetVector("_TriangleRadius", m_TriangleRadius);

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float pixelSize = Vector3.Distance(corners[1], corners[2]) / rectTransform.rect.width;
            pixelSize = pixelSize / m_FalloffDistance;

            if (m_OverrideSize) {
                baseMaterial.SetFloat("_Width", m_Size.x);
                baseMaterial.SetFloat("_Height", m_Size.y);
            }
            else {
                baseMaterial.SetFloat("_Width", rectTransform.rect.width + FalloffDistance);
                baseMaterial.SetFloat("_Height", rectTransform.rect.height + FalloffDistance);
            }

            baseMaterial.SetFloat("_PixelWorldScale", Mathf.Clamp(pixelSize, 0f, 999999f));

            if (DrawMethod == DrawMethod.Regular) {
                baseMaterial.EnableKeyword("BASE_IMAGE");
                baseMaterial.DisableKeyword("PROCEDURAL_CUT");
            }
            else if (DrawMethod == DrawMethod.Procedural) {
                baseMaterial.DisableKeyword("BASE_IMAGE");
                baseMaterial.EnableKeyword("PROCEDURAL_CUT");
            }
            else {
                baseMaterial.EnableKeyword("BASE_IMAGE");
                baseMaterial.EnableKeyword("PROCEDURAL_CUT");
            }

            baseMaterial.DisableKeyword("CIRCLE");
            baseMaterial.DisableKeyword("TRIANGLE");
            baseMaterial.DisableKeyword("RECTANGLE");
            baseMaterial.DisableKeyword("PENTAGON");
            baseMaterial.DisableKeyword("HEXAGON");
            baseMaterial.DisableKeyword("OCTAGON");

            switch (DrawShape) {
                case DrawShape.Circle:
                    baseMaterial.EnableKeyword("CIRCLE");
                    break;
                case DrawShape.Triangle:
                    baseMaterial.EnableKeyword("TRIANGLE");
                    break;
                case DrawShape.Rectangle:
                    baseMaterial.EnableKeyword("RECTANGLE");
                    break;
                case DrawShape.Pentagon:
                    baseMaterial.EnableKeyword("PENTAGON");
                    break;
                case DrawShape.Hexagon:
                    baseMaterial.EnableKeyword("HEXAGON");
                    break;
                case DrawShape.Octagon:
                    baseMaterial.EnableKeyword("OCTAGON");
                    break;
            }

            if (m_LineWeight > 0f) baseMaterial.EnableKeyword("OUTLINE");
            else baseMaterial.DisableKeyword("OUTLINE");

            return base.GetModifiedMaterial(baseMaterial);
        }

        public new void UpdateMaterial() {
            base.UpdateMaterial();
        }
    }

    public enum DrawMethod {
        Regular,
        Procedural,
        Hybrid
    }

    public enum DrawShape {
        Circle,
        Triangle,
        Rectangle,
        Pentagon,
        Hexagon,
        Octagon
    }
}