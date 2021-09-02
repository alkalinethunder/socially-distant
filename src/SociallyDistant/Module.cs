using System.Collections.Generic;
using System.Linq;

namespace SociallyDistant.Core
{
    public abstract class Module
    {
        public virtual IEnumerable<AssetInfo> GetAssetTypes() => Enumerable.Empty<AssetInfo>();
    }
}