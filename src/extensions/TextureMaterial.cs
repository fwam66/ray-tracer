
using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a texture material that is associated with an entity
    /// that is to be rendered by the ray tracer. This material uses an image
    /// as a texture map for the color and optionally a normal map for surface
    /// details. It extends the base Material class to include texture properties.
    /// </summary>
    public class TextureMaterial : Material
    {
        private Image colorMap;
        private Image normalMap;

        /// <summary>
        /// Construct a new material object.
        /// </summary>
        /// <param name="colorMap">The color map (texture) of the material</param>
        /// <param name="normalMap">The normal map of the material (optional)</param>
        public TextureMaterial(Image colorMap, Image normalMap = null)
            : base(new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0), 0, 0, 0, 1)
        {
            this.colorMap = colorMap;
            this.normalMap = normalMap;
        }
        public Image ColorMap { get { return this.colorMap; } }
        public Image NormalMap { get{ return this.normalMap; }}
    }
}
