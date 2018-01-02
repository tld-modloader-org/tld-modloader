using System;
using System.Runtime.InteropServices;
using Loader;
using UnityEngine;

namespace TestScripts
{
    [Script]
    public class TestScript : MonoBehaviour
    {
        [DllImport("kernel32.dll")]
        private static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags,  uint nNumberOfArguments, IntPtr lpArguments);
        // RaiseException(13, 0, 0, new IntPtr(1));

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                new GameObject().AddComponent<uConsole>();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                // uConsole.RunCommand("god");
            }
        }
    }
}