using System;
using SociallyDistant.Editor;

namespace SociallyDistant.Core.WorldObjects
{
    [AutoCreate("City Map")]
    [UserCreatable(false)]
    [CustomView("SociallyDistant.Editors.MapEditor")]
    public class CityMapAsset : IAsset
    {
        [EditorHidden]
        public Guid Id { get; set; }
        
        [EditorHidden]
        public string Name { get; set; }
    }
}