using System.Collections.Generic;

namespace RayTracer
{
    public class KeyFrameAnimation : Animation
    {
        public struct KeyFrame
        {
            public Transform Transform { get; set; }
            public double Time { get; set; }

            public KeyFrame(Transform transform, double time)
            {
                this.Transform = transform;
                this.Time = time;
            }
        }

        private SceneEntity entity;
        public List<KeyFrame> KeyFrames { get; private set; }

        /// <summary>
        /// Create a new KeyFrameAnimation for a specific entity.
        /// </summary>
        /// <param name="entity">The entity to animate</param>
        public KeyFrameAnimation(SceneEntity entity)
        {
            this.entity = entity;
            KeyFrames = new List<KeyFrame>();

        }

        /// <summary>
        /// Add a keyframe to the animation.
        /// </summary>
        /// <param name="transform">Transform to apply at this keyframe</param>
        /// <param name="time">Time at which this keyframe occurs</param>
        public void AddKeyFrame(Transform transform, double time)
        {
            KeyFrames.Add(new KeyFrame(transform, time));
            KeyFrames.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        /// <summary>
        /// Get the entity associated with this animation.
        /// </summary>
        public SceneEntity Entity
        {
            get { return entity; }
        }

        public void Update(double time)
        {
            if (KeyFrames.Count == 0) return; // No keyframes so we skip
            if (KeyFrames.Count == 1) // Just one keyframe 
            {
                
            }
            double timesqr = time * time;
            double timecubed = timesqr * time;
        }
    }
}

