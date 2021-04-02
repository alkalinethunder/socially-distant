using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class HackStartInfo
    {
        private SaveManager _saveManager;
        private Hackable _hackable;
        
        public HackStartInfo(SaveManager saveManager, Hackable hackable)
        {
            _saveManager = saveManager;
            _hackable = hackable;
        }

        public Difficulty Difficulty => _hackable.Difficulty;
        public bool IsTraced => _hackable.HackableFlags.IsTraced;
        
        
    }
}