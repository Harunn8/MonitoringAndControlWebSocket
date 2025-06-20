using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.RuleEngine.Repositories
{
    public interface IScriptRepository
    {
        Task<List<ScriptModels>> GetAllScript();
        Task<ScriptModels> GetScriptById(Guid id);
        Task<ScriptModels> GetScriptByName(string name);
        Task<bool> RunPolicyScript(Guid id);
        Task DeletePolicyScript(Guid id);
        Task UpdatePolicyScript(Guid id, ScriptModels model);
        Task<bool> StopPolicyScript(Guid id);
        Task AddPolicyScript(ScriptModels model);

    }
}
