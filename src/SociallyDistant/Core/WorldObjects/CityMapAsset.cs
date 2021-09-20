using System;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    [AutoCreate("City Map")]
    [UserCreatable(false)]
    [CustomView("SociallyDistant.Editors.MapEditor")]
    public class CityMapAsset : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakHidden]
        public string Name { get; set; }
    }
}