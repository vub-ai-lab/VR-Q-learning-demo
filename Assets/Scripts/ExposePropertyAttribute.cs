using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// See http://wiki.unity3d.com/index.php?title=Expose_properties_in_inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExposePropertyAttribute : Attribute
{

}