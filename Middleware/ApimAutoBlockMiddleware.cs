using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Security
{
    public class ApimAutoBlock
    {
        private readonly ILogger<ApimAutoBlock> _logger;

        public ApimAutoBlock(ILogger<ApimAutoBlock> logger)
        {
            _logger = logger;
        }

        [Function("BlockAttackerIp")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# APIM Auto-Block trigger processing a request.");

            try
            {
                // 1. Parse the Azure Monitor Alert Payload
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                using var doc = JsonDocument.Parse(requestBody);
                
                // Extract ClientIP from the common Azure Alert schema dimensions
                var dimensions = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("context")
                    .GetProperty("dimensions");

                string attackerIp = null;
                foreach (var dim in dimensions.EnumerateArray())
                {
                    if (dim.GetProperty("name").GetString() == "ClientIP" || 
                        dim.GetProperty("name").GetString() == "ClientTlsIpAddress")
                    {
                        attackerIp = dim.GetProperty("value").GetString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(attackerIp))
                {
                    var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("No ClientIP found in alert dimensions.");
                    return badResponse;
                }

                // 2. Define Azure Architecture Resource Coordinates
                string subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
                string resourceGroupName = Environment.GetEnvironmentVariable("AZURE_RESOURCE_GROUP");
                string apimServiceName = Environment.GetEnvironmentVariable("AZURE_APIM_NAME");

                // 3. Connect to Azure Resource Manager using Managed Identity
                var armClient = new ArmClient(new DefaultAzureCredential());
                
                // Get a reference to the Global Policy resource of your APIM instance
                ResourceIdentifier policyResourceId = ApiManagementPolicyResource.CreateResourceIdentifier(
                    subscriptionId, resourceGroupName, apimServiceName, PolicyName.Policy);

                ApiManagementPolicyResource policyResource = armClient.GetApiManagementPolicyResource(policyResourceId);
                
                // Fetch the current XML policy data
                PolicyContractData policyData = (await policyResource.GetAsync()).Value.Data;
                string currentXml = policyData.Value;

                // 4. Safely modify the XML policy using LINQ-to-XML
                XDocument xDoc = XDocument.Parse(currentXml);
                XElement inbound = xDoc.Root.Element("inbound");

                if (inbound == null)
                {
                    inbound = new XElement("inbound");
                    xDoc.Root.AddFirst(inbound);
                }

                XElement ipFilter = inbound.Element("ip-filter");

                // If <ip-filter action="forbid"> doesn't exist, create it at the top of inbound
                if (ipFilter == null)
                {
                    ipFilter = new XElement("ip-filter", new XAttribute("action", "forbid"));
                    inbound.AddFirst(ipFilter);
                }

                // Check if this IP is already banned to prevent duplicates
                bool alreadyBanned = ipFilter.Elements("address").Any(x => x.Value == attackerIp);

                if (!alreadyBanned)
                {
                    ipFilter.Add(new XElement("address", attackerIp));
                    
                    // Update policy payload
                    policyData.Value = xDoc.ToString(SaveOptions.DisableFormatting);
                    
                    // Push the updated XML policy configuration back to APIM
                    await policyResource.UpdateAsync(WaitUntil.Completed, policyData);
                    
                    _logger.LogWarning($"Successfully banned attacker IP: {attackerIp}");
                    
                    var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await successResponse.WriteStringAsync($"IP {attackerIp} blocked successfully.");
                    return successResponse;
                }

                _logger.LogInformation($"IP {attackerIp} was already on the blocklist.");
                var noOpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await noOpResponse.WriteStringAsync($"IP {attackerIp} was already blocked.");
                return noOpResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating APIM Policy: {ex.Message}");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal processing error occurred.");
                return errorResponse;
            }
        }
    }
}
