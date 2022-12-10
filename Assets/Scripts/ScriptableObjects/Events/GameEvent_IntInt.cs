// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace RoboRyanTron.Unite2017.Events
{
    [CreateAssetMenu]
	public class GameEvent_IntInt : ScriptableObject
    {
        /// <summary>
        /// The list of listeners that this event will notify if it is raised.
        /// </summary>
		private readonly List<GameEventListener_IntInt> eventListeners = 
			new List<GameEventListener_IntInt>();

		public void Raise(int _value, int _value2)
        {
            for(int i = eventListeners.Count -1; i >= 0; i--)
				eventListeners[i].OnEventRaised(_value, _value2);
        }

		public void RegisterListener(GameEventListener_IntInt listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

		public void UnregisterListener(GameEventListener_IntInt listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}