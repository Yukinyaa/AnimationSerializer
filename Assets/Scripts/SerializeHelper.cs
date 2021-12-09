using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Mathf;
static class SerializeHelper
{
    public struct Vector3Serializable
    {
        public float x, y, z;
        public Vector3Serializable(float x, float y, float z) => (this.x, this.y, this.z) = (x, y, z);
        public Vector3Serializable(Vector3 vector3) => (this.x, this.y, this.z) = (vector3.x, vector3.y, vector3.z);

        public static explicit operator Vector3Serializable(Vector3 vector3) => new Vector3Serializable(vector3);
        public static explicit operator Vector3(Vector3Serializable v) => new Vector3(v.x, v.y, v.z);

        public static Vector3Serializable operator +(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        // ZXY rotation according to http://www.songho.ca/opengl/gl_anglestoaxes.html
        public static Vector3Serializable RotateBy(Vector3Serializable a, Vector3Serializable deg)
        {
            float sa = Sin(deg.x), ca = Cos(deg.x);
            float sb = Sin(deg.y), cb = Cos(deg.y);
            float sc = Sin(deg.z), cc = Cos(deg.z);
            return new Vector3Serializable(
                    (cc * cb) * a.x + (sc * cb) * a.y - sb * a.z,
                    (-sc * ca + cc * sb * sa) * a.x + (cc * ca + sc * sb * sa) * a.y + (cb * sa) * a.z,
                    (sc * sa + cc * sb * ca) * a.x + (-cc * sa + sc * sb * ca) * a.y + (cb * ca) * a.z
                );
        }
    }
    public static void FKTest(Dit)
    {

    }
}
