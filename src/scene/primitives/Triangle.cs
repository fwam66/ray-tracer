using System;
using System.Collections.Generic;
using System.Numerics;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            Vector3 e1 = this.v1 - this.v0;
            Vector3 e2 = this.v2 - this.v0;
            Vector3 plane = e1.Cross(e2);
            Vector3 normal = plane.Normalized();

            if (plane.Dot(ray.Direction) != 0) // Not parallel to triangle plane
            {
                double dist = plane.Dot(this.v0 - ray.Origin) / plane.Dot(ray.Direction);
                var position = ray.Origin + dist * ray.Direction;

                // Compute barycentric coordinates
                double totalArea = ComputeArea(this.v0, this.v1, this.v2, normal);
                double u = ComputeArea(this.v1, this.v2, position, normal) / totalArea;
                double v = ComputeArea(this.v2, this.v0, position, normal) / totalArea;
                double w = ComputeArea(this.v0, this.v1, position, normal) / totalArea;

                if (u >= 0 && v >= 0 && w >= 0 && dist >= 0) // Position P is within the triangle means intersection
                {
                    return new RayHit(position, normal, ray.Direction, material, dist);
                }
            }
            return null;
        }

        public double ComputeArea(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            var eBA = b - a;
            var eCA = c - a;
            return 0.5 * eBA.Cross(eCA).Dot(normal);

        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }

        public List<Vector3> Vertices()
        {
            List<Vector3> vertices = new List<Vector3> { v0, v1, v2 };
            return vertices;
        }

        public Vector3 Centroid()
        {
            return (this.v0 + this.v1 + this.v2) / 3f;
        }
    }
}
