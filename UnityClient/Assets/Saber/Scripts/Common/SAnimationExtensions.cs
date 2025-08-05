using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Saber
{
    public static class SAnimationExtensions
    {
        /// <summary> Invoke with Parameters </summary>
        public static bool InvokeWithParams(this MonoBehaviour sender, string method, object args)
        {
            Type argType = null;

            if (args != null) argType = args.GetType();

            MethodInfo methodPtr;

            if (argType != null)
            {
                methodPtr = sender.GetType().GetMethod(method, new Type[] { argType });
            }
            else
            {
                try
                {
                    methodPtr = sender.GetType().GetMethod(method);
                }
                catch (Exception)
                {
                    //methodPtr = sender.GetType().GetMethods().First
                    //(m => m.Name == method && m.GetParameters().Count() == 0);

                    //Debug.Log("OTHER");

                    throw;
                }
            }

            if (methodPtr != null)
            {
                if (args != null)
                {
                    var arguments = new object[1] { args };
                    methodPtr.Invoke(sender, arguments);
                    return true;
                }
                else
                {
                    methodPtr.Invoke(sender, null);
                    return true;
                }
            }

            PropertyInfo property = sender.GetType().GetProperty(method);

            if (property != null)
            {
                property.SetValue(sender, args, null);
                return true;
            }

            return false;
        }
    }
}