#if VRC_SDK_VRCSDK3
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public static class Vrc3Supporter
    {
        private static bool _checkedForSupporter;

        [Serializable]
        public class Supporter
        {
            [SerializeField] public string name;
            [SerializeField] public Color background;
            [SerializeField] public Color text;
        }

        [Serializable]
        private class SupportersList
        {
            [SerializeField] public Supporter[] early;
            [SerializeField] public Supporter[] supporter;
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static readonly SupportersList SupObject = new()
        {
            early = new[]
            {
                new Supporter { name = "Stack_" },
                new Supporter { name = "Ahri~" },
                new Supporter { name = "Nayu" },

                new Supporter { name = "♡ GaNyan ♡" },
                new Supporter { name = "TheIceDragonz" },
                new Supporter { name = "NinaV2" },

                new Supporter { name = "emymin" },
                new Supporter { name = "Zettai Ryouiki" },
                new Supporter { name = "NekOwneD" },

                new Supporter { name = "lindesu" },
                new Supporter { name = "OptoCloud" },
                new Supporter { name = "lukasong" },

                new Supporter { name = ".Rei." },
                new Supporter { name = "BluWizard" },
                new Supporter { name = "" }
            },
            supporter = new[]
            {
                new Supporter { name = "Hiro N.", text = new Color(1f, 0.69f, 0.02f), background = new Color(1f, 0.4f, 0.82f) },
                new Supporter { name = "Dominhiho", text = new Color(1f, 0.46f, 0f), background = new Color(0f, 1f, 0.69f) },
                new Supporter { name = "maple", text = new Color(0f, 0.89f, 0.03f), background = new Color(0f, 0f, 0.51f) },

                new Supporter { name = "", text = Color.clear, background = Color.clear },
                new Supporter { name = "SashTheDee", text = new Color(0.76f, 0.4f, 1f), background = new Color(0.59f, 0f, 1f) },
                new Supporter { name = "Feldarin", text = Color.red, background = Color.red }
            }
        };

        internal static Supporter SupporterData(bool early, int index) => early ? SupObject.early[index % SupObject.early.Length] : SupObject.supporter[index % SupObject.supporter.Length];

        internal static bool ShouldFade(bool early, int size) => early ? SupObject.early.Length > size : SupObject.supporter.Length > size;

        private static void OnSupporters(string supporters)
        {
            _checkedForSupporter = true;
            JsonUtility.FromJsonOverwrite(supporters, SupObject);
        }

        public static void Check()
        {
            if (_checkedForSupporter) return;
            GestureManagerEditor.CheckSupporters(OnSupporters);
        }
    }
}
#endif