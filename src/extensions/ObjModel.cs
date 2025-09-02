using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        private List<TextureCoord> textures;

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
        private BVHNode root; // root of the BVH

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
            List<Triangle> faces = new List<Triangle>();

            // Here's some code to get you started reading the file...
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.textures = new List<TextureCoord>();

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
                // Texture line,
                if (line.StartsWith("vt"))
                {
                    textures.Add(ParseTexture(line));
                
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
            this.root = BuildBVH(faces);
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // If ray does not intersects bounding box, no need to continue
            if (!RayIntersectsBox(ray, boundMin, boundMax)) return null; 

            // Recursively trace our BVH tree to find nearest hit
            RayHit nearestHit = IntersectBVH(root, ray);
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

        private TextureCoord ParseTexture(string line)
        {
            var coords = line.Split(' ');
            double u = double.Parse(coords[1]);
            double v =  double.Parse(coords[2]);
            return new TextureCoord(u, v);
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
            Vector3 v1 = vertices[faceVertices[0].vertexIndex - 1];
            Vector3 v2 = vertices[faceVertices[1].vertexIndex - 1];
            Vector3 v3 = vertices[faceVertices[2].vertexIndex - 1];

            // Also store textures in the triangles
            if (faceVertices[0].textureIndex.HasValue &&
                faceVertices[1].textureIndex.HasValue &&
                faceVertices[2].textureIndex.HasValue)
            {
                TextureCoord t1 = textures[faceVertices[0].textureIndex.Value - 1];
                TextureCoord t2 = textures[faceVertices[1].textureIndex.Value - 1];
                TextureCoord t3 = textures[faceVertices[2].textureIndex.Value - 1];
                return new Triangle(v1, v2, v3, t1, t2, t3, material);
            }
            return new Triangle(v1, v2, v3, material);
        }


        private bool RayIntersectsBox(Ray ray, Vector3 min, Vector3 max)
        {
            // Checks if ray intersects with bounding box of object as a
            Vector3 invDir = new Vector3(
                                1 / ray.Direction.X,
                                1 / ray.Direction.Y,
                                1 / ray.Direction.Z
            ).Normalized();
            double d1 = (min.X - ray.Origin.X) * invDir.X;
            double d2 = (max.X - ray.Origin.X) * invDir.X;
            double d3 = (min.Y - ray.Origin.Y) * invDir.Y;
            double d4 = (max.Y - ray.Origin.Y) * invDir.Y;
            double d5 = (min.Z - ray.Origin.Z) * invDir.Z;
            double d6 = (max.Z - ray.Origin.Z) * invDir.Z;

            double dMin = Math.Max(Math.Max(Math.Min(d1, d2), Math.Min(d3, d4)), Math.Min(d5, d6)); // find the largest minimium out of the distances
            double dMax = Math.Min(Math.Min(Math.Max(d1, d2), Math.Max(d3, d4)), Math.Max(d5, d6)); // find the smallest maximum out of the distances

            return dMax >= Math.Max(dMin, 0); // if smallest max from each axis is larger than the largest min from each axis we know that the ray intersects the box
        }

        public BVHNode BuildBVH(List<Triangle> triangles)
        {
            int leafSize = 4; // Max 4 triangles per leaf


            // Arbitrary min and max for bounding box
            Vector3 min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            Vector3 max = new Vector3(double.MinValue, double.MinValue, double.MinValue);

            // Compute bounding box for each node
            foreach (Triangle triangle in triangles)
            {
                foreach (Vector3 vertice in triangle.Vertices())
                {
                    min = new Vector3(
                        Math.Min(min.X, vertice.X),
                        Math.Min(min.Y, vertice.Y),
                        Math.Min(min.Z, vertice.Z)
                    );
                    max = new Vector3(
                        Math.Max(max.X, vertice.X),
                        Math.Max(max.Y, vertice.Y),
                        Math.Max(max.Z, vertice.Z)
                    );
                }
            }

            if (triangles.Count <= leafSize) // Reached leaf node no need to split anymore
            {
                return new BVHNode(min, max, null, null, triangles);
            }

            // Decide along which axis we will split, 0 for x, 1 for y, 2 for z
            Vector3 extent = max - min;
            int splitAxis = extent.X > extent.Y
                        ? (extent.X > extent.Z ? 0 : 2)
                        : (extent.Y > extent.Z ? 1 : 2);


            // Sort triangles by their centroid along the chosen axis
            triangles.Sort((a, b) =>
                    GetCentroidValue(a.Centroid(), splitAxis).
                    CompareTo(GetCentroidValue(b.Centroid(), splitAxis)));

            // Split triangles into two groups and recursively call BuildBVH with two groups
            int half = triangles.Count / 2;
            if (half <= 0 || half == triangles.Count) return new BVHNode(min, max, null, null, triangles);

            BVHNode left = BuildBVH(triangles.GetRange(0, half));
            BVHNode right = BuildBVH(triangles.GetRange(half, triangles.Count - half));


            return new BVHNode(min, max, left, right, null);
        }

        public RayHit IntersectBVH(BVHNode node, Ray ray)
        {
            // If ray does not intersect bounding box of node we avoid that subtree
            if (!RayIntersectsBox(ray, node.BoundMin, node.BoundMax))
                return null;

            if (node.IsLeaf) // If node is leaf, we test triangles
            {
                RayHit nearestHit = null;
                foreach (Triangle triangle in node.Triangles)
                {
                    RayHit hit = triangle.Intersect(ray);
                    if (hit != null && (nearestHit == null || hit.Distance < nearestHit.Distance))
                    {
                        nearestHit = hit;
                    }
                }
                return nearestHit;
            }

            // Else if node not leaf then we recurse/traverse the BVH binary tree
            RayHit leftHit = IntersectBVH(node.Left, ray);
            RayHit rightHit = IntersectBVH(node.Right, ray);

            // Handles potential nulls
            if (leftHit == null && rightHit == null) return null;
            if (leftHit == null) return rightHit;
            if (rightHit == null) return leftHit;
            // Return whichever hit is closer
            return leftHit.Distance < rightHit.Distance ? leftHit : rightHit;
        }

        double GetCentroidValue(Vector3 vector, int axis)
        {
            return axis == 0 ? vector.X : (axis == 1 ? vector.Y : vector.Z);
        }
    }

    public class BVHNode
    {
        private Vector3 boundMin;
        private Vector3 boundMax;
        private BVHNode left;
        private BVHNode right;
        private List<Triangle> triangles;
        private bool isLeaf;

        public BVHNode(Vector3 min, Vector3 max, BVHNode left, BVHNode right, List<Triangle> triangles)
        {
            this.boundMax = max;
            this.boundMin = min;
            this.left = left;
            this.right = right;
            this.triangles = triangles;
            this.isLeaf = (this.triangles != null && triangles.Count >= 0);
        }
        public Vector3 BoundMax { get { return this.boundMax; } }
        public Vector3 BoundMin { get { return this.boundMin; } }
        public bool IsLeaf { get { return this.isLeaf; } }

        public List<Triangle> Triangles { get { return this.triangles; } }
        public BVHNode Left { get { return this.left; } }
        public BVHNode Right{get { return this.right; }}
    }
}
