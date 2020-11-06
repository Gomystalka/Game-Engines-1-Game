using System;
using System.Reflection;
[AttributeUsage(AttributeTargets.Field)]
public class SynchronizedVarAttribute : Attribute
{
    public string Target { get; set; }
    public bool HideTargetInInspector { get; set; }
}