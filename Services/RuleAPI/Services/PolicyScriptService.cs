using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCSMqttBus.Producer;
using Models;
using Services.RuleEngine.Services.Base;
using MQTTnet.Client;
using System.Net.WebSockets;
using Microsoft.EntityFrameworkCore;

namespace Services.RuleAPI.Services
{
    public class PolicyScriptService : IPolicyScriptService
    {
        private readonly MqttProducer _mqtt;
        private readonly AppDbContext _dbContext;

        public PolicyScriptService(MqttProducer mqtt, AppDbContext appDbContext)
        {
            _mqtt = mqtt;
            _dbContext = appDbContext;
        }
        public async Task AddPolicyScript(ScriptModels model)
        {
            await _dbContext.AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePolicyScript(Guid id)
        {
            _dbContext.Remove(id);
            _dbContext.SaveChanges();
        }

        public async Task<List<ScriptModels>> GetAllScript()
        {
            var scripts = await _dbContext.Scripts.ToListAsync();
            return scripts;
        }

        public async Task<ScriptModels> GetScriptById(Guid id)
        {
            var script = await _dbContext.Scripts.FirstOrDefaultAsync(x => x.Id == id);
            return script;
        }

        public async Task<ScriptModels> GetScriptByName(string name)
        {
            var script = await _dbContext.Scripts.FirstOrDefaultAsync(x => x.ScriptName == name);
            return script;
        }

        // True ise  controller üzerinden ilgili script mqtt ile business tarafına iletilir
        public async Task<bool> RunPolicyScript(Guid id)
        {
            var policy = await GetScriptById(id);
            if (policy == null) 
            {
                return false;
            }
            _mqtt.PublishMessage("policyScript/start", $"{policy.Id},{policy.ScriptName},{policy.Script}", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            return true;
        }
        // True ise  controller üzerinden ilgili script mqtt ile business tarafına iletilir
        public async Task<bool> StopPolicyScript(Guid id)
        {
            var policy = await GetScriptById(id);
            if (policy == null)
            {
                return false;
            }
            return true;
        }

        public async Task UpdatePolicyScript(Guid id, ScriptModels model)
        {
            var script = await _dbContext.Scripts.FirstOrDefaultAsync(x=>x.Id == id);
            _dbContext.Entry(script).CurrentValues.SetValues(model);
            await _dbContext.SaveChangesAsync();

        }
    }
}
