using System;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    public class IspData : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakCategory("Internet Service Provider")]
        [TweakName("ISP Name")]
        public string Name { get; set; }
    }
}