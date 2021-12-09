using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
static class SerializeHelper
{
    public class Vector3Serializable
    {
        public float x, y, z;
        public Vector3Serializable(float x, float y, float z) => (this.x, this.y, this.z) = (x, y, z);
        public Vector3Serializable(Vector3 vector3) => (this.x, this.y, this.z) = (vector3.x, vector3.y, vector3.z);

        public static explicit operator Vector3Serializable(Vector3 vector3) => new Vector3Serializable(vector3);
        public static explicit operator Vector3(Vector3Serializable v) => new Vector3(v.x, v.y, v.z);
    }
}
