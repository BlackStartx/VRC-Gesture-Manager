#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.Cache
{
    [Serializable]
    public class OscFile
    {
        public Parameter[] parameters;

        public IEnumerable<Parameter> InputParameters => parameters.Where(parameter => parameter.input.address != null).ToList();
        public IEnumerable<Parameter> OutputParameters => parameters.Where(parameter => parameter.output.address != null).ToList();
    }

    [Serializable]
    public struct Parameter
    {
        public string name;
        public Data input;
        public Data output;
    }

    [Serializable]
    public struct Data
    {
        public string address;
        public string type;

        private AnimatorControllerParameterType? _type;
        internal AnimatorControllerParameterType Type => _type ??= FetchType();

        public AnimatorControllerParameterType FetchType() => type switch
        {
            "Bool" => AnimatorControllerParameterType.Bool,
            "Int" => AnimatorControllerParameterType.Int,
            _ => AnimatorControllerParameterType.Float
        };
    }
}
#endif