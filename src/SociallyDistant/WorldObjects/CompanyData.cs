using System;
using SociallyDistant.ContentEditors;

namespace SociallyDistant.WorldObjects
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
}