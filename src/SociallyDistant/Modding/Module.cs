using System.Collections.Generic;
using System.Linq;
using SociallyDistant.Editor;

namespace SociallyDistant.Modding
{
    public abstract class Module
    {
        public virtual IEnumerable<AssetInfo> GetAssetTypes() => Enumerable.Empty<AssetInfo>();
    }
}