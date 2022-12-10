// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using PrisonGlobals;

namespace RoboRyanTron.Unite2017.Events
{
	public class GameEventListener_IntInt : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent_IntInt Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_IntInt Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

		public void OnEventRaised(int _value, int _value2)
        {
			Response.Invoke(_value, _value2);
        }
    }
}