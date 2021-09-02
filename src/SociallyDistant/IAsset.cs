using System;

namespace SociallyDistant.Core
{
    public interface IAsset
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}