using System;
using SociallyDistant.Core.ContentEditors;

namespace SociallyDistant.Core.WorldObjects
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