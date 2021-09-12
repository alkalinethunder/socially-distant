using System;
using System.Collections.Generic;
using SociallyDistant.Editor;

namespace SociallyDistant.Core.WorldObjects
{
    [CustomView("SociallyDistant.Editors.CorporateNetworkEditor")]
    public class CompanyData : IAsset
    {
        [EditorHidden]
        public Guid Id { get; set; }
        
        [EditorName("Company name")]
        public string Name { get; set; }
        
        [EditorName("Company type")]
        [EditorDescription("Determines the icon used on the city map for this business.")]
        public CompanyType CompanyType { get; set; }

        [EditorHidden] public List<CorporateNetworkConnection> NetworkConnections { get; set; } = new();
        [EditorHidden] public List<CorporateNetworkNode> Nodes { get; set; } = new();

        [EditorName("Company ISP")]
        [EditorDescription(
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