#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using UnityEngine;

namespace GestureManager.Scripts.Editor.Modules.Vrc3.OpenSoundControl
{
    public class EndpointControl
    {
        internal readonly string HorizontalEndpoint;
        internal readonly List<(float? min, float? value, float? max, object data)> Values = new List<(float? min, float? value, float? max, object data)>();

        private readonly List<VisualEp> _visualElements;

        public EndpointControl(string horizontalEndpoint, IList<EndpointControl> chronological)
        {
            chronological.Insert(0, this);

            _visualElements = new List<VisualEp>();
            HorizontalEndpoint = horizontalEndpoint;
        }

        internal static bool? BoolValue(object value)
        {
            switch (value)
            {
                case bool b: return b;
                case float f: return f > 0.5f;
                case int i: return i == 1;
                default: return null;
            }
        }

        internal static int? IntValue(object value)
        {
            switch (value)
            {
                case bool b: return b ? 1 : 0;
                case float f: return (int)f;
                case int i: return i;
                default: return null;
            }
        }

        internal static float? FloatValue(object value)
        {
            switch (value)
            {
                case bool b: return b ? 1 : 0;
                case float f: return f;
                case int i: return i;
                default: return null;
            }
        }

        public static string StringValue(object value)
        {
            switch (value)
            {
                case string s: return s;
                case char c: return c.ToString();
                default: return "?";
            }
        }

        public void OnMessage(OscPacket.Message message)
        {
            for (var i = 0; i < message.Arguments.Count; i++)
            {
                if (i < Values.Count) UpdateValue(i, message.Arguments[i]);
                else Values.Add((null, FloatValue(message.Arguments[i]), null, message.Arguments[i]));
            }

            foreach (var visual in _visualElements) visual.OnMessage(this);
        }

        private void UpdateValue(int i, object argument)
        {
            var vFloat = FloatValue(argument);
            if (!vFloat.HasValue) Values[i] = (Values[i].min, null, Values[i].max, argument);
            else UpdateValue(i, vFloat.Value);
        }

        private void UpdateValue(int i, float value)
        {
            if (Values[i].value.HasValue && RadialMenuUtility.Is(Values[i].value.Value, value)) return;
            Values[i] = (Mathf.Min(Values[i].min ?? Values[i].value ?? value, value, 0), value, Mathf.Max(Values[i].max ?? Values[i].value ?? value, value, 0), value);
        }

        public VisualEp New()
        {
            var visual = new VisualEp(this);
            _visualElements.Add(visual);
            return visual;
        }
    }
}
#endif