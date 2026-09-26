    using System;
    using UnityEngine;
    using UnityEngine.Events;

    namespace Crowfox.Util
    {
        public class CollisionBehaviour : MonoBehaviour
        {
            [SerializeField] bool _doDebug = false;

            [Flags]
            public enum CollisionEvent
            {
                None = 0,
                Enter = 1 << 0,
                Stay = 1 << 1,
                Exit = 1 << 2,
            }

            [Flags]
            public enum CollisionSource
            {
                None = 0,
                Collision = 1 << 0,
                Trigger = 1 << 1,
            }

            [SerializeField] private CollisionEvent _events = CollisionEvent.None;
            [SerializeField] private CollisionSource _sources = CollisionSource.None;
            [SerializeField] private EventResponse[] _responses = Array.Empty<EventResponse>();

            private void OnCollisionEnter(Collision collision)
            {
                if (_doDebug)
                    Debug.Log("Detected collision enter with " + collision.collider.name);

                if (_events.HasFlag(CollisionEvent.Enter) && _sources.HasFlag(CollisionSource.Collision))
                    Trigger(CollisionEvent.Enter, CollisionSource.Collision, collision.collider);
            }

            private void OnCollisionStay(Collision collision)
            {
                if (_doDebug)
                    Debug.Log("Detected collision stay with " + collision.collider.name);

                if (_events.HasFlag(CollisionEvent.Stay) && _sources.HasFlag(CollisionSource.Collision))
                    Trigger(CollisionEvent.Stay, CollisionSource.Collision, collision.collider);
            }

            private void OnCollisionExit(Collision collision)
            {
                if (_doDebug)
                    Debug.Log("Detected collision exit with " + collision.collider.name);

                if (_events.HasFlag(CollisionEvent.Exit) && _sources.HasFlag(CollisionSource.Collision))
                    Trigger(CollisionEvent.Exit, CollisionSource.Collision, collision.collider);
            }

            private void OnTriggerEnter(Collider other)
            {
                if (_doDebug)
                    Debug.Log("Detected trigger enter with " + other.name);

                if (_events.HasFlag(CollisionEvent.Enter) && _sources.HasFlag(CollisionSource.Trigger))
                    Trigger(CollisionEvent.Enter, CollisionSource.Trigger, other);
            }

            private void OnTriggerStay(Collider other)
            {
                if (_doDebug)
                    Debug.Log("Detected trigger stay with " + other.name);

                if (_events.HasFlag(CollisionEvent.Stay) && _sources.HasFlag(CollisionSource.Trigger))
                    Trigger(CollisionEvent.Stay, CollisionSource.Trigger, other);
            }

            private void OnTriggerExit(Collider other)
            {
                if (_doDebug)
                    Debug.Log("Detected trigger exit with " + other.name);

                if (_events.HasFlag(CollisionEvent.Exit) && _sources.HasFlag(CollisionSource.Trigger))
                    Trigger(CollisionEvent.Exit, CollisionSource.Trigger, other);
            }

            protected virtual void Trigger(CollisionEvent collisionEvent, CollisionSource source, Collider other)
            {
                if (_doDebug)
                    Debug.Log($"CollisionBehaviour Triggered: {collisionEvent} from {source} with {other.name}");

                foreach (var response in _responses)
                {
                    if (!response.Matches(collisionEvent, source))
                        continue;

                    response.Response.Invoke();
                }
            }

            [Serializable]
            public class EventResponse
            {
                public CollisionEvent Events = CollisionEvent.None;
                public CollisionSource Sources = CollisionSource.None;
                public UnityEvent Response;

                public bool Matches(CollisionEvent collisionEvent, CollisionSource source)
                {
                    return (Events & collisionEvent) != 0
                        && (Sources & source) != 0;
                }
            }
        }
    }
