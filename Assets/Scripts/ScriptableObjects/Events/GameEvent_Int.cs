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
	public class GameEvent_Int : ScriptableObject
    {
        /// <summary>
        /// The list of listeners that this event will notify if it is raised.
        /// </summary>
		private readonly List<GameEventListener_Int> eventListeners = 
			new List<GameEventListener_Int>();

		public void Raise(int _value)
        {
            for(int i = eventListeners.Count -1; i >= 0; i--)
				eventListeners[i].OnEventRaised(_value);
        }

		public void RegisterListener(GameEventListener_Int listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

		public void UnregisterListener(GameEventListener_Int listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}