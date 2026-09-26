using System;
using UnityEngine;
using UnityEngine.Events;

namespace Crowfox.Util
{
    public class Collision2DBehaviour : MonoBehaviour
    {
        [SerializeField] bool _doDebug = false;
        [SerializeField] bool _doIgnoreOtherTagsInDebug = true;

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

        [Tag]
        [SerializeField] private string _targetTag;
        [SerializeField] private CollisionEvent _events = CollisionEvent.Enter;
        [SerializeField] private CollisionSource _sources = CollisionSource.Trigger;
        [SerializeField] private EventResponse[] _responses = Array.Empty<EventResponse>();
        public bool DoFilterByTag => !(string.IsNullOrEmpty(_targetTag) || _targetTag.Equals("Untagged"));

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var isValid = IsValidTarget(collision.collider.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected collision enter with " + collision.collider.name);

            if (_events.HasFlag(CollisionEvent.Enter) && _sources.HasFlag(CollisionSource.Collision) && isValid)
                Trigger(CollisionEvent.Enter, CollisionSource.Collision, collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            var isValid = IsValidTarget(collision.collider.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected collision stay with " + collision.collider.name);

            if (_events.HasFlag(CollisionEvent.Stay) && _sources.HasFlag(CollisionSource.Collision) && isValid)
                Trigger(CollisionEvent.Stay, CollisionSource.Collision, collision.collider);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            var isValid = IsValidTarget(collision.collider.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected collision exit with " + collision.collider.name);

            if (_events.HasFlag(CollisionEvent.Exit) && _sources.HasFlag(CollisionSource.Collision) && isValid)
                Trigger(CollisionEvent.Exit, CollisionSource.Collision, collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var isValid = IsValidTarget(other.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected trigger enter with " + other.name);

            if (_events.HasFlag(CollisionEvent.Enter) && _sources.HasFlag(CollisionSource.Trigger) && isValid)
                Trigger(CollisionEvent.Enter, CollisionSource.Trigger, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var isValid = IsValidTarget(other.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected trigger stay with " + other.name);

            if (_events.HasFlag(CollisionEvent.Stay) && _sources.HasFlag(CollisionSource.Trigger) && isValid)
                Trigger(CollisionEvent.Stay, CollisionSource.Trigger, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var isValid = IsValidTarget(other.tag);
            if (!isValid && _doIgnoreOtherTagsInDebug)
                return;

            if (_doDebug)
                Debug.Log("Detected trigger exit with " + other.name);

            if (_events.HasFlag(CollisionEvent.Exit) && _sources.HasFlag(CollisionSource.Trigger) && isValid)
                Trigger(CollisionEvent.Exit, CollisionSource.Trigger, other);
        }

        protected virtual void Trigger(CollisionEvent collisionEvent, CollisionSource source, Collider2D other)
        {
            if (_doDebug)
                Debug.Log($"Collision2DBehaviour Triggered: {collisionEvent} from {source} with {other.name}");

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
            public CollisionEvent Events = CollisionEvent.Enter;
            public CollisionSource Sources = CollisionSource.Trigger;
            public UnityEvent Response;

            public bool Matches(CollisionEvent collisionEvent, CollisionSource source)
            {
                return (Events & collisionEvent) != 0
                    && (Sources & source) != 0;
            }
        }

        public bool IsValidTarget(string tag)
        {
            if (_doDebug)
                Debug.Log($"Target with tag '{tag}' is {(tag.Equals(_targetTag) ? "" : "NOT ")}equal to filter tag '{_targetTag}'. DoFilterByTag is {DoFilterByTag}");

            if (!DoFilterByTag)
                return true;

            return tag.Equals(_targetTag);
        }
    }
}
