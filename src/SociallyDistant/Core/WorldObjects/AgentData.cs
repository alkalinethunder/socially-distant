using System;
using System.Collections.Generic;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    [CustomView("SociallyDistant.Editors.SocialPage")]
    public class AgentData : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakCategory("Identity")]
        [TweakName("Full Name")]
        [TweakDescription("Used by the game when displaying the Agent's social media information. Must be non-whitespace.")]
        public string Name { get; set; }
        
        [TweakCategory("Identity")]
        [TweakName("Gender Identity")]
        [TweakDescription("Defines the Agent's preferred pronouns when being referred to in chat logs, emails, and other forms of communication.")]
        public Pronoun Pronouns { get; set; }
        
        [TweakCategory("Identity")]
        [TweakName("UNIX Username")]
        [TweakDescription("A custom, optional UNIX username used by the Agent when referred to in chat logs. Will also be used during Device Generation to create the Agent's device. If left blank then the game will use the all-lowercase first name found in the Full Name property.")]
        public string UserName { get; set; }
        
        [TweakCategory("Pack Behaviour")]
        [TweakName("Is Player Slot?")]
        [TweakDescription("Determines whether this Agent will be used as the Player Agent. All Content Packs must have one and only one Player Slot. If checked, then all values set in the Identity category are irrelevant as they will be set by the player during Agent Creation. All other values set here will be used as the starting stats for the player.")]
        public bool IsPlayer { get; set; }
        
        [TweakCategory("Profile")]
        [TweakName("Social Bio")]
        [TweakDescription("Write about this character - this text displays on the social page. See the social page editor on the left.")]
        public string Bio { get; set; }
        
        [TweakCategory("Profile")]
        [TweakName("Profile Picture")]
        [TweakDescription("A picture displayed on this character's social media posts, emails, and chat messages.")]
        public ImageAssetReference ProfilePicture { get; set; }

        [TweakCategory("Profile")]
        [TweakName("Cover Artwork")]
        [TweakDescription("A picture that's displayed in this character's Social Profile in the header area.")]
        public ImageAssetReference CoverArt { get; set; }

        [TweakCategory("Computer")]
        [TweakName("Owned Computer")]
        [TweakDescription("Select a Computer for this character to use.")]
        public AssetReference<DeviceData> Computer { get; set; } = new();
        
        [TweakHidden] public List<AgentSocialPost> Posts { get; set; } = new();
    }

    public class AgentSocialPost
    {
        public string Text { get; set; }
        public List<ImageAssetReference> Images { get; set; } = new();
    }
}