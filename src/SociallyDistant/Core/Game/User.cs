namespace SociallyDistant.Core.Game
{
    public class User
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string HomeDirectory { get; set; }
        public UserType Type { get; set; }

        public static User Root => new User
        {
            UserId = 0,
            UserName = "root",
            HomeDirectory = "/root",
            Type = UserType.Root
        };
        
        public static User Nobody => new User
        {
            UserId = int.MaxValue,
            UserName = "nobody",
            HomeDirectory = "/tmp",
            Type = UserType.Nobody
        };
    }
}