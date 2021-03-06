﻿using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace GestureManager.Scripts.Core.Editor
{
    /**
     * Hi, you're a curious one!
     * 
     * What you're looking at are some of the methods of my Unity Libraries.
     * They do not contains all the methods otherwise the UnityPackage would have been so much bigger.
     * 
     * P.S: Gmg stands for GestureManager~
     */
    public static class GmgVisualElementExtensions
    {
        public static T MyAdd<T>(this VisualElement visualElement, T child) where T : VisualElement
        {
            visualElement.Add(child);
            return child;
        }

        public static T With<T>(this T visualElement, VisualElement child) where T : VisualElement
        {
            visualElement.Add(child);
            return visualElement;
        }

        public static T MyBorder<T>(this T visualElement, float width, float radius, Color color) where T : VisualElement
        {
            visualElement.style.borderColor = color;
            visualElement.style.borderRadius = radius;
            visualElement.style.borderTopWidth = width;
            visualElement.style.borderLeftWidth = width;
            visualElement.style.borderRightWidth = width;
            visualElement.style.borderBottomWidth = width;
            return visualElement;
        }

        public static T OnClickUpEvent<T>(this T visualElement, EventCallback<MouseUpEvent> action) where T : VisualElement
        {
            visualElement.RegisterCallback(action);
            return visualElement;
        }

        public static T OnClickDownEvent<T>(this T visualElement, EventCallback<MouseDownEvent> action) where T : VisualElement
        {
            visualElement.RegisterCallback(action);
            return visualElement;
        }
    }
}