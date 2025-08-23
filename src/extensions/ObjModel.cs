using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using ImageMagick;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private string objFilePath;
        private Transform transform;
        private Material material;

        private List<Vector3> vertices;
        private List<Vector3> normals;

        private Vector3 boundMin;
        private Vector3 boundMax;

        private struct FaceVertex
        {
            public int vertexIndex;
            public int? textureIndex;
            public int normalIndex;
            public FaceVertex(int vi, int? ti, int ni)
            {
                this.vertexIndex = vi;
                this.textureIndex = ti;
                this.normalIndex = ni;
            }
        }
        private List<Triangle> faces; // list of all faces of the object



        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Transform transform, Material material)
        {
            this.objFilePath = objFilePath;
            this.transform = transform;
            this.material = material;
            this.faces = new List<Triangle>();

            // Here's some code to get you started reading the file...
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            
            foreach (string line in File.ReadLines(objFilePath))
            {
                // The current line is line  
               
                // If invalid line skip line
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                // Vertex line
                if (line.StartsWith("v "))
                {
                    vertices.Add(ParseVector(line));
                    continue;
                }
                // Normal line
                if (line.StartsWith("vn"))
                {
                    normals.Add(ParseVector(line).Normalized());
                    continue;
                }
                // Face line, find a face's indices, then triangulate
                if (line.StartsWith("f"))
                {
                    faces.Add(Triangulate(ParseFace(line)));
                    continue;
                }
            }

            boundMax = vertices[0];
            boundMin = vertices[0];
            foreach (var v in vertices)
            {
                this.boundMin = new Vector3(
                    Math.Min(boundMin.X, v.X),
                    Math.Min(boundMin.Y, v.Y),
                    Math.Min(boundMin.Z, v.Z)
                );

                this.boundMax = new Vector3(
                    Math.Max(boundMax.X, v.X),
                    Math.Max(boundMax.Y, v.Y),
                    Math.Max(boundMax.Z, v.Z)
                );
            }
            Console.WriteLine(faces.Count);
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            int intersections = 0;
            if (!RayIntersectsBox(ray)) return null; // If ray does not intersects bounding box, no need to continue

            RayHit nearestHit = null;
            foreach (Triangle face in faces)
            {
                RayHit hit = face.Intersect(ray);
                intersections += 1;
                Console.WriteLine(intersections);
                if (hit != null && (nearestHit == null || hit.Distance < nearestHit.Distance))
                {
                    nearestHit = hit;
                }
            }
            return nearestHit;
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }

        private Vector3 ParseVector(string line)
        {
            var coords = line.Split(' ');
            double x = double.Parse(coords[1]);
            double y = double.Parse(coords[2]);
            double z = double.Parse(coords[3]);
            return transform.Apply(new Vector3(x, y, z));
        }

        private List<FaceVertex> ParseFace(string line)
        {
            List<FaceVertex> faceVertices = new List<FaceVertex>();
            var token = line.Split(' ');
            for (int i = 1; i < token.Length; i++)
            {
                faceVertices.Add(ParseFaceVertex(token[i]));
            }
            return faceVertices;
        }
        private FaceVertex ParseFaceVertex(string token)
        {
            var part = token.Split('/');
            int vertexIndex = int.Parse(part[0]);
            int? textureIndex = (part.Length > 1 && !string.IsNullOrEmpty(part[1]))
                                ? int.Parse(part[1]) : null;
            int normalIndex = int.Parse(part[2]);

            return new FaceVertex(vertexIndex, textureIndex, normalIndex);
        }

        private Triangle Triangulate(List<FaceVertex> faceVertices)
        {
            Vector3 v1 =  vertices[faceVertices[0].vertexIndex - 1];
            Vector3 v2 =  vertices[faceVertices[1].vertexIndex - 1];
            Vector3 v3 =  vertices[faceVertices[2].vertexIndex - 1];
            return new Triangle(v1, v2, v3, material);
        }


        private bool RayIntersectsBox(Ray ray)
        {
            // Checks if ray intersects with bounding box
            Vector3 invDir = new Vector3(
                                1 / ray.Direction.X,
                                1 / ray.Direction.Y,
                                1 / ray.Direction.Z
            ).Normalized();
            double d1 = (boundMin.X - ray.Origin.X) * invDir.X;
            double d2 = (boundMax.X - ray.Origin.X) * invDir.X;
            double d3 = (boundMin.Y - ray.Origin.Y) * invDir.Y;
            double d4 = (boundMax.Y - ray.Origin.Y) * invDir.Y;
            double d5 = (boundMin.Z - ray.Origin.Z) * invDir.Z;
            double d6 = (boundMax.Z - ray.Origin.Z) * invDir.Z;

            double dMin = Math.Max(Math.Max(Math.Min(d1, d2), Math.Min(d3, d4)), Math.Min(d5, d6)); // find the largest minimium out of the distances
            double dMax = Math.Min(Math.Min(Math.Max(d1, d2), Math.Max(d3, d4)), Math.Max(d5, d6)); // find the smallest maximum out of the distances

            return dMax >= Math.Max(dMin, 0); // if smallest max from each axis is larger than the largest min from each axis we know that the ray intersects the box
        }
    }
}
