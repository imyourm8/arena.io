using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

    public static class EditorHelper
    {
        public static void SetEnumValue<T>(this SerializedProperty prop, T value) where T : struct
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            var tp = typeof(T);
            if(tp.IsEnum)
            {
            prop.enumValueIndex = ArrayUtility.IndexOf(prop.enumNames, System.Enum.GetName(tp, value));
            }
            else
            {
            int i = System.Convert.ToInt32(value);
                if (i < 0 || i >= prop.enumNames.Length) i = 0;
                prop.enumValueIndex = i;
            }
        }

        public static void SetEnumValue(this SerializedProperty prop, System.Enum value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

        int i = ArrayUtility.IndexOf(prop.enumNames, System.Enum.GetName(value.GetType(), value));
            if (i < 0) i = 0;
            prop.enumValueIndex = i;
        }

        public static void SetEnumValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if(value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            var tp = value.GetType();
            if (tp.IsEnum)
            {
            int i = ArrayUtility.IndexOf(prop.enumNames, System.Enum.GetName(tp, value));
                if (i < 0) i = 0;
                prop.enumValueIndex = i;
            }
            else
            {
            int i = System.Convert.ToInt32(value);
                if (i < 0 || i >= prop.enumNames.Length) i = 0;
                prop.enumValueIndex = i;
            }
        }

        public static void SetPropertyValue(SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                prop.intValue = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Boolean:
                prop.boolValue = System.Convert.ToBoolean(value);
                    break;
                case SerializedPropertyType.Float:
                prop.floatValue = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = System.Convert.ToString(value);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.LayerMask:
                prop.intValue = (value is LayerMask) ? ((LayerMask)value).value : System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Enum:
                    //prop.enumValueIndex = ConvertUtil.ToInt(value);
                    prop.SetEnumValue(value);
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = (Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                prop.arraySize = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Character:
                prop.intValue = System.Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = value as AnimationCurve;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)value;
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
        }

    }
