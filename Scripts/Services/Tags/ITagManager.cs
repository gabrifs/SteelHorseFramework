using SteelHorse.Framework.Tags;

namespace SteelHorse.Framework.Services.Tags
{
    public interface ITagManager
    {
        void Setup();

        bool TryGetTag(string key, out TagDefinition tag);
        bool TryGetTag(TagReference reference, out TagDefinition tag);
    }
}
