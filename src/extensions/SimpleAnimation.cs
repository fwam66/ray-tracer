
namespace RayTracer
{
    public class SimpleAnimation : Animation
    {
        private SceneEntity entity;
        public Vector3 TranslationPerFrame { get; private set; }
        public Quaternion RotationPerFrame { get; private set; }

        /// <summary>
        /// Construct a new simple animation.
        /// </summary>
        /// <param name="entity">The entity to animate</param>
        /// <param name="translationPerFrame">Translation vector applied each frame</param>
        /// <param name="rotationPerFrame">Rotation quaternion applied each frame</param>
        public SimpleAnimation(SceneEntity entity, Vector3 translationPerFrame, Quaternion rotationPerFrame)
        {
            this.entity = entity;
            this.TranslationPerFrame = translationPerFrame;
            this.RotationPerFrame = rotationPerFrame;
        }

        /// <summary>
        /// Get the entity associated with this animation.
        /// </summary>
        public SceneEntity Entity
        {
            get { return entity; }
        }
        public void Update(double time){ return; }
    }
}

