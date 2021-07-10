#if VRC_SDK_VRCSDK3
namespace GestureManager.Scripts.Editor.Modules.Vrc3
{
    public class RadialDescription
    {
        public readonly string Text;
        public readonly string Link;
        public readonly string Url;

        public RadialDescription(string text, string link, string url)
        {
            Text = text;
            Link = link;
            Url = url;
        }
    }
}
#endif