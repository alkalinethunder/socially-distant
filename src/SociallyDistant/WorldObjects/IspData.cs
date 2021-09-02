using System;
using SociallyDistant.ContentEditors;

namespace SociallyDistant.WorldObjects
{
    public class IspData : IAsset
    {
        [EditorHidden]
        public Guid Id { get; set; }
        
        [EditorCategory("Internet Service Provider")]
        [EditorName("ISP Name")]
        public string Name { get; set; }
    }
}