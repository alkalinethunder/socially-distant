using System;
using System.Collections.Generic;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Editor;

namespace SociallyDistant.Core.WorldObjects
{
    [CustomView("SociallyDistant.Editors.SocialPage")]
    public class AgentData : IAsset
    {
        [EditorHidden]
        public Guid Id { get; set; }
        
        [EditorCategory("Identity")]
        [EditorName("Full Name")]
        [EditorDescription("Used by the game when displaying the Agent's social media information. Must be non-whitespace.")]
        public string Name { get; set; }
        
        [EditorCategory("Identity")]
        [EditorName("Gender Identity")]
        [EditorDescription("Defines the Agent's preferred pronouns when being referred to in chat logs, emails, and other forms of communication.")]
        public Pronoun Pronouns { get; set; }
        
        [EditorCategory("Identity")]
        [EditorName("UNIX Username")]
        [EditorDescription("A custom, optional UNIX username used by the Agent when referred to in chat logs. Will also be used during Device Generation to create the Agent's device. If left blank then the game will use the all-lowercase first name found in the Full Name property.")]
        public string UserName { get; set; }
        
        [EditorCategory("Pack Behaviour")]
        [EditorName("Is Player Slot?")]
        [EditorDescription("Determines whether this Agent will be used as the Player Agent. All Content Packs must have one and only one Player Slot. If checked, then all values set in the Identity category are irrelevant as they will be set by the player during Agent Creation. All other values set here will be used as the starting stats for the player.")]
        public bool IsPlayer { get; set; }
        
        [EditorCategory("Profile")]
        [EditorName("Social Bio")]
        [EditorDescription("Write about this character - this text displays on the social page. See the social page editor on the left.")]
        public string Bio { get; set; }
        
        [EditorCategory("Profile")]
        [EditorName("Profile Picture")]
        [EditorDescription("A picture displayed on this character's social media posts, emails, and chat messages.")]
        public ImageAssetReference ProfilePicture { get; set; }

        [EditorCategory("Profile")]
        [EditorName("Cover Artwork")]
        [EditorDescription("A picture that's displayed in this character's Social Profile in the header area.")]
        public ImageAssetReference CoverArt { get; set; }

        [EditorCategory("Computer")]
        [EditorName("Owned Computer")]
        [EditorDescription("Select a Computer for this character to use.")]
        public AssetReference<DeviceData> Computer { get; set; } = new();
        
        [EditorHidden] public List<AgentSocialPost> Posts { get; set; } = new();
    }

    public class AgentSocialPost
    {
        public string Text { get; set; }
        public List<ImageAssetReference> Images { get; set; } = new();
    }
}