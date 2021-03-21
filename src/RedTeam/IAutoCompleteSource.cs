using System.Collections.Generic;

namespace RedTeam
{
    public interface IAutoCompleteSource
    {
        bool IsWhiteSpace(char ch)
        {
            return char.IsWhiteSpace(ch);
        }
        
        IEnumerable<string> GetCompletions(string word);
    }
}