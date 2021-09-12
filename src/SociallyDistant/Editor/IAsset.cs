using System;

namespace SociallyDistant.Editor
{
    public interface IAsset
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}