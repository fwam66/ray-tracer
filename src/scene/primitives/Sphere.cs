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

        private Color texture;

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
            this.texture = new Color(0, 0, 0);
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

            double b = ray.Direction.Dot(v);
            double c = v.Dot(v) - this.radius * this.radius;
            double discriminant = b * b - c;

            if (discriminant < 0) return null;
            
            double dist1 = -b - Math.Sqrt(discriminant);
            double dist2 = -b + Math.Sqrt(discriminant);
            double dist;
            if (dist1 >= 0)
            {
                dist = dist1;
            }
            else if (dist2 >= 0)
            {
                dist = dist2;
            }
            else
            {
                return null;
            } 
        
            var position = ray.Origin + dist * ray.Direction;
            var normal = (position - this.center).Normalized();
            return new RayHit(position, normal, ray.Direction, material, dist, texture);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
