using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraMover : MonoBehaviour
    {
        public float speed = 1;

        private void Update()
        {
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
        }
    }
}