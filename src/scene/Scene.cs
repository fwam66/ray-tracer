using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using ImageMagick;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private Camera camera;
        private Color ambientLightColor;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        private ISet<Animation> animations;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.camera = new Camera(Transform.Identity);
            this.ambientLightColor = new Color(0, 0, 0);
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
            this.animations = new HashSet<Animation>();
        }

        /// <summary>
        /// Set the camera for the scene.
        /// </summary>
        /// <param name="camera">Camera object</param>
        public void SetCamera(Camera camera)
        {
            this.camera = camera;
        }

        /// <summary>
        /// Set the ambient light color for the scene.
        /// </summary>
        /// <param name="color">Color object</param>
        public void SetAmbientLightColor(Color color)
        {
            this.ambientLightColor = color;
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Add an animation to the scene.
        /// </summary>
        /// <param name="animation">Animation object</param>
        public void AddAnimation(Animation animation)
        {
            this.animations.Add(animation);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds</param>
        public void Render(Image outputImage, double time = 0)
        {
            // Begin writing your code here...
            int fov = 60;
            var origin = new Vector3(0, 0, 1e-4);
            float aspect = outputImage.Width / outputImage.Height;
            float scale = MathF.Tan(fov * 0.5f * MathF.PI / 180f);

            for (int i = 0; i < outputImage.Width; i++)
            {
                for (int j = 0; j < outputImage.Height; j++)
                {
                    // 2D pixel coord to 3D ray
                    // First, convert 2D coord to range of [-1, 1]
                    float x_converted = (i + 0.5f) * 2 / outputImage.Width - 1;
                    float y_converted = 1 - (j + 0.5f) * 2 / outputImage.Height;

                    // Then, calculate x, y, z for direction from origin
                    float x = x_converted * aspect * scale;
                    float y = y_converted * scale;
                    float z = 1;

                    // Create Ray with normalized direction vector
                    Ray ray = new Ray(origin, new Vector3(x, y, z).Normalized());

                    // Find color for each pixel
                    Color color = Trace(ray, 100); 
                    outputImage.SetPixel(i, j, color);
                }
            }
        }

        public Color FindDiffuse(RayHit hit, PointLight light)
        {
            Color materialDiffuseColor = hit.Material.DiffuseColor;
            Vector3 normal = hit.Normal;
            Color lightColor = light.Color;
            Vector3 lightDirection = (light.Position - hit.Position).Normalized();
            return materialDiffuseColor * lightColor * Math.Max(0, normal.Dot(lightDirection));
        }

        public Color FindSpecular(RayHit hit, PointLight light)
        {
            Color materialSpecularColor = hit.Material.SpecularColor;
            Vector3 directionToCamera = (-hit.Position).Normalized();
            double shininess = hit.Material.Shininess;
            Color lightColor = light.Color;
            Vector3 lightDirection = (light.Position - hit.Position).Normalized();
            Vector3 reflectionDirection = (2 * hit.Normal.Dot(lightDirection) * hit.Normal - lightDirection).Normalized();
            return materialSpecularColor * lightColor * Math.Pow(Math.Max(0, reflectionDirection.Dot(directionToCamera)), shininess);
        }

        public Color FindAmbient(RayHit hit)
        {
            return hit.Material.AmbientColor * this.ambientLightColor;
        }

        public bool FindShadow(RayHit nearestHit, PointLight light)
        {
            Vector3 hitToLight = light.Position - nearestHit.Position;
            double lightDistance = hitToLight.Length();
            Vector3 hitToLightDirection = hitToLight.Normalized();
            Ray shadowRay = new Ray(nearestHit.Position + nearestHit.Normal * 1e-4, hitToLightDirection);
            bool isShadow = false;
            foreach (SceneEntity entity in this.entities)
            {
                RayHit shadowHit = entity.Intersect(shadowRay);
                // Checks if shadowRay intersects and that object is between surface and lights
                if (shadowHit != null && shadowHit.Distance < lightDistance)
                {
                    isShadow = true;
                }
            }
            return isShadow;
        }

        public Ray? FindRefRay(RayHit nearestHit, Ray ray) {
            Vector3 D = ray.Direction;
            Vector3 N = nearestHit.Normal;
            double etai = 1.0;
            double etat = nearestHit.Material.RefractiveIndex;
            double eta = etai / etat; // from air to inside
            double cosThetaI = -D.Dot(N);

            if (-cosThetaI > 0) // ray is exiting, normal must be flipped
            {
                N = -N;
                eta = 1 / eta; // flipped
                cosThetaI = -cosThetaI;
                
            }
            double discriminant = 1.0f - eta * eta * (1.0f - cosThetaI * cosThetaI);
            if (discriminant < 0) // no real solution, total internal reflection
            {
                return null;
            }
            Vector3 T = (eta * D + N * (eta * cosThetaI - Math.Sqrt(discriminant))).Normalized();
            return new Ray(nearestHit.Position - N * 1e-4, T);
        }

        public Color FindLocalColor(RayHit nearestHit)
        {
            Color localColor = new Color(0,0,0);
            foreach (PointLight light in this.lights)
            {
                // Find if there is shadow
                bool isShadow = FindShadow(nearestHit, light);
                if (!isShadow)
                {
                    localColor += FindDiffuse(nearestHit, light);
                    localColor += FindSpecular(nearestHit, light);
                }
            }
            localColor += FindAmbient(nearestHit);
            return localColor;
        }

        public Color Trace(Ray ray, int depth)
        {
            if (depth <= 0) // Prevents infinite recursion
            {
                return new Color(0, 0, 0);
            }
            RayHit nearestHit = FindNearestHit(ray);
            if (nearestHit == null)
            {
                return new Color(0, 0, 0);
            }

            Vector3 D = ray.Direction;
            Vector3 N = nearestHit.Normal;
            double transmissivity = nearestHit.Material.Transmissivity;
            double reflectivity = nearestHit.Material.Reflectivity;

            // Calculate diffuse, specular, ambient and shadow
            Color localColor = FindLocalColor(nearestHit);

            // Handle reflectionor transmissive then recursively trace
            Color reflectionColor = new Color(0,0,0);
            double offset = 1e-4;
            if (reflectivity > 0.0f) {
                Vector3 reflectedDirection = (D - 2 * D.Dot(N) * N).Normalized();
                Ray reflectionRay = new Ray(nearestHit.Position + N * offset, reflectedDirection);
                reflectionColor = Trace(reflectionRay, depth - 1);
            }

            Color refractionColor = new Color(0,0,0);
            // Handle refraction
            if (transmissivity > 0.0f)
            {
                Ray? refRay = FindRefRay(nearestHit, ray);
                if (refRay.HasValue) // has solution
                {
                    refractionColor = Trace(refRay.Value, depth - 1);
                }
                else
                {
                    refractionColor = reflectionColor;
                }
            }
            return localColor + reflectionColor * reflectivity + refractionColor * transmissivity;
        }

        public RayHit FindNearestHit(Ray ray)
        {
            // Checks for closest RaycastHits
            RayHit nearestHit = null;
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit != null && (nearestHit == null || hit.Distance < nearestHit.Distance))
                {
                    nearestHit = hit;
                }
            }
            return nearestHit;
        }
    }
}
