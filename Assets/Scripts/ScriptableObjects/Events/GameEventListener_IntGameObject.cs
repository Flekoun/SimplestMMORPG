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
	public class GameEventListener_IntGameObject : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent_IntGameObject Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_IntGameObject Response;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_GameObject Response_GameObject;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent_Int Response_Int;


        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

		public void OnEventRaised(int _value, GameObject _value2)
        {
			Response.Invoke(_value, _value2);
            Response_GameObject.Invoke(_value2);
            Response_Int.Invoke(_value);
        }

        
    }
}