#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl.VisualElements;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class EndpointControl
    {
        internal readonly string HorizontalEndpoint;
        internal readonly List<(float? min, object value, float? max)> Values = new();

        private readonly List<VisualEp> _visualElements;

        public EndpointControl(string horizontalEndpoint, IList<EndpointControl> chronological)
        {
            chronological.Insert(0, this);

            _visualElements = new List<VisualEp>();
            HorizontalEndpoint = horizontalEndpoint;
        }

        internal static bool? BoolValue(object value) => value switch
        {
            bool b => b,
            float f => f > 0.5f,
            int i => i == 1,
            _ => null
        };

        internal static int? IntValue(object value) => value switch
        {
            bool b => b ? 1 : 0,
            float f => (int)f,
            int i => i,
            _ => null
        };

        internal static float? FloatValue(object value) => value switch
        {
            bool b => b ? 1 : 0,
            float f => f,
            int i => i,
            _ => null
        };

        public void OnMessage(OscPacket.Message message)
        {
            for (var i = 0; i < message.Arguments.Count; i++)
            {
                if (i < Values.Count) UpdateValue(i, message.Arguments[i]);
                else Values.Add((null, message.Arguments[i], null));
            }

            foreach (var visual in _visualElements) visual.OnMessage(this);
        }

        private void UpdateValue(int i, object argument)
        {
            var vFloat = FloatValue(argument);
            if (vFloat.HasValue) UpdateValue(i, vFloat.Value, argument);
            else Values[i] = (Values[i].min, argument, Values[i].max);
        }

        private void UpdateValue(int i, float fValue, object value)
        {
            var vFloat = FloatValue(Values[i].value);
            if (vFloat.HasValue && RadialMenuUtility.Is(vFloat.Value, fValue)) return;
            Values[i] = (Mathf.Min(Values[i].min ?? vFloat ?? fValue, fValue, 0), value, Mathf.Max(Values[i].max ?? vFloat ?? fValue, fValue, 0));
        }

        public VisualEp New()
        {
            var visual = new VisualEp(this);
            _visualElements.Add(visual);
            return visual;
        }

        public void Clear()
        {
            foreach (var visual in _visualElements) visual.RemoveFromHierarchy();
        }
    }
}
#endif