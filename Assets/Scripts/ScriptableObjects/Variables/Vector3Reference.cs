// ----------------------------------------------------------------------------
// Unite 2017 - Game Architecture with Scriptable Objects
// 
// Author: Ryan Hipple
// Date:   10/04/17
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

namespace RoboRyanTron.Unite2017.Variables
{
    [Serializable]
    public class Vector3Reference
    {
        public bool UseConstant = true;
		public Vector3 ConstantValue;
		public Vector3Variable Variable;

		public Vector3Reference()
        { }

		public Vector3Reference(Vector3 value)
        {
            UseConstant = true;
            ConstantValue = value;
        }

        public Vector3 Value
        {
            get { return UseConstant ? ConstantValue : Variable.Value; }
			set { if(UseConstant) ConstantValue = value; else Variable.Value= value; }
        }

		public static implicit operator Vector3(Vector3Reference reference)
        {
            return reference.Value;
        }
    }
}