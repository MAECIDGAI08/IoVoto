using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppA.Models;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RestSharp;

namespace AppA.Service
{
    public static class APIService
    {
        #region InvokeChainccode chiama il chaincode passato come parametro 
      	public static IRestResponse InvokeChainCode(string channel, string chainCodeName, string methodName, params string[] parameters)
          {
            var BCURL = (channel == "ValidationNumber" || channel == "elections") ? Startup.StaticConfig.GetSection("BlockChain").GetSection("URL_2").Value : Startup.StaticConfig.GetSection("BlockChain").GetSection("URL").Value;
            var BCUser = (channel == "ValidationNumber" || channel == "elections") ? Startup.StaticConfig.GetSection("BlockChain").GetSection("User_2").Value : Startup.StaticConfig.GetSection("BlockChain").GetSection("User").Value;
            var client = new RestClient(BCURL + "/" + channel + "/transactions");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic " + BCUser);
            var body = @"{" + "\n" + @" ""chaincode"": """ + chainCodeName + "\"," + "\n" +
            @"    ""args"": [" + "\n" +
            @"""" + methodName + "\"," + "\n";

            var variablePart = "";

            for (var i = 0; i < parameters.Length; i++)
            {
                var separator = "";

                if (i < parameters.Length - 1)
                {
                    separator = ",";
                }
                variablePart += "\"" + parameters[i].Replace("'", "''") + "\"" + separator;
            }
            body += variablePart + "], " + "\"timeout\": 18000, \"sync\" : true";
            body += "}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return response;
        }
        #endregion
    }
}
