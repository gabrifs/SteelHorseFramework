using SteelHorse.Framework.Tags;

namespace SteelHorse.Framework.Services.Tags
{
    public interface ITagManager
    {
        bool TryGetTag(string key, out TagDefinition tag);
        bool TryGetTag(TagReference reference, out TagDefinition tag);
    }
}
