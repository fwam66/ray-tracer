using System;

namespace RayTracer
{
    /// <summary>
    /// Represents a camera in the ray tracing scene.
    /// </summary>
    public class Camera : SceneEntity
    {
        public Transform Transform { get; private set; }

        
        /// <summary>
        /// Construct a new camera with a specified transformation.
        /// </summary>
        /// <param name="transform">Transformation to apply to the camera</param>
        public Camera(Transform transform)
        {
            this.Transform = transform;
        }

        /// <summary>
        /// Cameras do not intersect with rays in the same way as other entities.
        /// This method can is left unimplemented.
        /// </summary>
        public RayHit Intersect(Ray ray)
        {
            throw new NotImplementedException("Camera does not support intersection checks.");
        }

        /// <summary>
        /// Cameras do not have a material in the same way as other entities.
        /// This property is left unimplemented.
        /// </summary>
        public Material Material
        {
            get { throw new NotImplementedException("Camera does not have a material."); }
        }
    }
}
