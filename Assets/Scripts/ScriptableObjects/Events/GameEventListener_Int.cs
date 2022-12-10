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
	public class GameEventListener_Int : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent_Int Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_Int Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

		public void OnEventRaised(int _value)
        {
			Response.Invoke(_value);
        }
    }
}