using System.Collections.Generic;

namespace RedTeam
{
    public interface IAutoCompleteSource
    {
        IEnumerable<string> GetCompletions();
    }
}