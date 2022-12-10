// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using PrisonGlobals;
using UnityEngine;
using UnityEngine.Events;

namespace RoboRyanTron.Unite2017.Events
{
	public class GameEventListener_GameObject : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent_GameObject Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_GameObject Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

		public void OnEventRaised(GameObject _gameObject)
        {
			Response.Invoke(_gameObject);
        }
    }
}