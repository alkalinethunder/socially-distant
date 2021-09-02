using System;

namespace SociallyDistant
{
    public interface IAsset
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}