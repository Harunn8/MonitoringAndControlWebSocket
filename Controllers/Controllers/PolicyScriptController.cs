using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Threading.Tasks;
using Services.RuleAPI.Services;
using MCSMqttBus.Producer;
using System;
using AutoMapper.Configuration.Conventions;

namespace Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyScriptController : ControllerBase
    {
        private readonly PolicyScriptService _policyService;
        private readonly MqttProducer _mqtt;

        public PolicyScriptController(PolicyScriptService policyService, MqttProducer mqtt)
        {
            _policyService = policyService;
            _mqtt = mqtt;
        }

        [HttpGet("GetAllScript")]
        public async Task<IActionResult> GetAllScripts()
        {
            var scripts = await _policyService.GetAllScript();
            if(scripts == null || scripts.Count == 0) 
            { 
                return NotFound("Scripts Not Found"); 
            }
            return Ok(scripts);
        }

        [HttpGet("GetScriptById")]
        public async Task<IActionResult> GetScriptById(Guid id)
        {
            var script = await _policyService.GetScriptById(id);
            if(script == null) 
            {
                return NotFound("Script Not Found");
            }
            return Ok(script);
        }

        [HttpGet("GetScriptByName")]
        public async Task<IActionResult> GetScriptByName(string name)
        {
            var script = await _policyService.GetScriptByName(name);
            if(script == null)
            {
                return NotFound("Scipt Not Found");
            }
            return Ok(script);
        }

        [HttpPost("AddPolicyScript")]
        public async Task<IActionResult> AddPolicyScript(ScriptModels model)
        {
            await _policyService.AddPolicyScript(model);
            return Ok(model);
        }

        [HttpPut("UpdatePolicyScript")]
        public async Task<IActionResult> UpdatePolicyScript(Guid id, ScriptModels model)
        {
            var script = _policyService.GetScriptById(id);
            if (script == null)
            {
               return NotFound("Script Not Found");
            }
            await _policyService.UpdatePolicyScript(id, model);
            return Ok(model);
        }

        [HttpDelete("DeletePolicyScript/{id}")]
        public async Task<IActionResult> DeletePolicyScript(Guid id)
        {
            var script = await _policyService.GetScriptById(id);
            if( script == null)
            {
                return NotFound("Script Not Found");
            }
            await _policyService.DeletePolicyScript(id);
            return Ok();
        }

        [HttpPut("RunPolicy/{id}")]
        public async Task<IActionResult> RunPolicyScript(Guid id)
        {
            var script = await _policyService.GetScriptById(id);
            if(script == null)
            {
                return NotFound("Script Not Found");
            }
            var result = await _policyService.RunPolicyScript(id);
            if (!result)
            {
                return BadRequest("This process not running");
            }

            _mqtt.PublishMessage("policyScript/start",$"{id}", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            return Ok();
        }

        [HttpPut("DisablePolicy/{id}")]
        public async Task<IActionResult> DisablePolicyScript(Guid id)
        {
            var script = await _policyService.GetScriptById(id);
            if (script == null)
            {
                return NotFound("Script Not Found");
            }
            var result = await _policyService.StopPolicyScript(id);
            if (!result)
            {
                return BadRequest("This process not running");
            }

            _mqtt.PublishMessage("policyScript/stop", $"{id}", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            return Ok();
        }
    }
}
