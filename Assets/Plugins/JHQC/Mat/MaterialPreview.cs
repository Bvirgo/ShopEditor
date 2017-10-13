using UnityEngine;
using System.Collections;

namespace Jhqc.EditorCommon
{
    public class MaterialPreview : MonoBehaviour
    {
        public Camera renderCamera;
        public MeshRenderer target;

        private readonly int WIDTH = 256;
        private readonly int HEIGHT = 100;

        public Texture2D GetTexture(ProceduralMaterial mat)
        {
            //gameObject.SetActive(true);

            target.material = mat;

            var rt = new RenderTexture(WIDTH, HEIGHT, 24);

            renderCamera.targetTexture = rt;
            renderCamera.Render();

            RenderTexture.active = rt;

            var tex2d = new Texture2D(WIDTH, HEIGHT);
            tex2d.ReadPixels(new Rect(0, 0, WIDTH, HEIGHT), 0, 0);
            tex2d.Apply();

            RenderTexture.active = null;
            renderCamera.targetTexture = null;

            //gameObject.SetActive(false);

            return tex2d;
        }

        void Start() { }
    }
}