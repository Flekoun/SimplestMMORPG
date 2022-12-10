using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using RoboRyanTron.Unite2017.Events;

namespace PrisonGlobals{


    [Serializable]
	public class UnityEvent_Transform : UnityEvent<Transform>{}

	[Serializable]
	public class UnityEvent_Vector2 : UnityEvent<Vector2>{}

    [Serializable]
    public class UnityEvent_Vector2Int : UnityEvent<Vector2,int> { }

    [Serializable]
	public class UnityEvent_Bool : UnityEvent<bool>{}

	[Serializable]
	public class UnityEvent_Int : UnityEvent<int>{}

    [Serializable]
    public class UnityEvent_IntGameObject : UnityEvent<int,GameObject> { }


    [Serializable]
    public class UnityEvent_String : UnityEvent<string> { }

    [Serializable]
    public class UnityEvent_IntInt : UnityEvent<int,int> { }

    [Serializable]
	public class UnityEvent_Float : UnityEvent<float>{}

    [Serializable]
    public class UnityEvent_FloatInt : UnityEvent<float,int> { }



	[Serializable]
	public class UnityEvent_Collision2D : UnityEvent<Collision2D>{}

	[Serializable]
	public class UnityEvent_GameObject : UnityEvent<GameObject>{}

   
    [Serializable]
    public class UnityEvent_Collision2DInt : UnityEvent<Collision2D, int> { }

    [Serializable]
    public class UnityEvent_Collider2DInt : UnityEvent<Collider2D, int> { }



  




    public static class PrisonGlobals
    {



    }



}