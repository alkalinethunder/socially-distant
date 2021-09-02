using System.Collections.Generic;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.WorldObjects;

namespace SociallyDistant.Core
{
    public sealed class CoreModule : Module
    {
        public override IEnumerable<AssetInfo> GetAssetTypes()
        {
            yield return AssetInfo.Create<CityMapAsset>("World");
            yield return AssetInfo.Create<IspData>("ISP");
            yield return AssetInfo.Create<AgentData>("Character");
            yield return AssetInfo.Create<DeviceData>("Computer");
            yield return AssetInfo.Create<CompanyData>("Corporate Network");
            yield return AssetInfo.Create<ChatConversation>("Chat Conversation");
        }
    }
}