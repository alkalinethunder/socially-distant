using System;
using System.Collections.Generic;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    [CustomView("SociallyDistant.Editors.CorporateNetworkEditor")]
    public class CompanyData : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakName("Company name")]
        public string Name { get; set; }
        
        [TweakName("Company type")]
        [TweakDescription("Determines the icon used on the city map for this business.")]
        public CompanyType CompanyType { get; set; }

        [TweakHidden] public List<CorporateNetworkConnection> NetworkConnections { get; set; } = new();
        [TweakHidden] public List<CorporateNetworkNode> Nodes { get; set; } = new();

        [TweakName("Company ISP")]
        [TweakDescription(
            "Select a specific internet service provider to connect this corporate network to. If none is selected, a random one will be used when generating the agent. Note that this cannot happen unless the world has an ISP.")]
        public AssetReference<IspData> Isp { get; set; } = new();
    }

    public enum CompanyType
    {
        Office,
        Warehouse,
        Factory,
        Store,
        FinancialInstitution,
        School,
        Restaurant,
        Adult
    }

    public class CorporateNetworkNode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public AssetReference<DeviceData> Device { get; set; } = new();
    }

    public class CorporateNetworkConnection
    {
        public Guid FirstNode { get; set; } 
        public Guid SecondNode { get; set; }
    } 
}