﻿using Built.Emit.DynamicProxy;

using System;
using System.Diagnostics;

namespace Built.Emit.DynamicProxy.Demo
{
    public class ActionAttribute : ActionBaseAttribute
    {
        public override void Before(string @method, object[] parameters)
        {
            Trace.WriteLine($"Action Before  1111111 ,method:{method},parameters:{parameters}");
        }

        public override object After(string method, object result)
        {
            Trace.WriteLine($"Action After  2222222 ,method:{method},parameters:{result}");
            return base.After(method, result);
        }
    }
}