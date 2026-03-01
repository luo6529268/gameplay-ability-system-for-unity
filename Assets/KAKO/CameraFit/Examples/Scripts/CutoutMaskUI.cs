using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Kako.CameraFit.Examples
{
    public class CutoutMaskUI : Image
    {
        private static readonly int Comp = Shader.PropertyToID(StencilComp);
        private const string StencilComp = "_StencilComp";
        
        public override Material materialForRendering
        {
            get
            {
                Material cutoutMaterial = new Material(base.materialForRendering);
                cutoutMaterial.SetInt(Comp, (int)CompareFunction.NotEqual);
                return cutoutMaterial;
            }
        }
    }
}