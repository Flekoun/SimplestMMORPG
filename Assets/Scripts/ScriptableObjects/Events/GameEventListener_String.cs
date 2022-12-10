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
	public class GameEventListener_String : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent_String Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_String Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

		public void OnEventRaised(string _value)
        {
			Response.Invoke(_value);
        }
    }
}