using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            var v = ray.Origin - this.center;
            double discriminant = 2 * ray.Direction.Dot(v) * (2 * ray.Direction.Dot(v)) - 4 * (v.Dot(v) - this.radius * this.radius);
        
            if (discriminant >= 0) // if intersects
            {
                double dist = (-2 * ray.Direction.Dot(v) - Math.Sqrt(discriminant)) / 2;
                if (dist >= 0)
                {
                    var position = ray.Origin + dist * ray.Direction;
                    var normal = (position - this.center).Normalized();
                    return new RayHit(position, normal, ray.Direction, material, dist);
                }
            }
            return null;    
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
