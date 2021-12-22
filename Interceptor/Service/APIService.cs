using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interceptor.Models;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RestSharp;

namespace Interceptor.Service
{
    public static class APIService
    {
        #region InvokeChainCode chiama il chaincode passato come parametro 
        public static IRestResponse InvokeChainCode(string channel, string chainCodeName, string methodName, params string[] parameters)
        {
            var client = new RestClient(Startup.StaticConfig.GetSection("BlockChain").GetSection("URL").Value +"/" + channel + "/transactions");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic " + Startup.StaticConfig.GetSection("BlockChain").GetSection("User").Value);
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
                variablePart += "\"" + parameters[i] + "\"" + separator;
            }
            body += variablePart + "], " + "\"timeout\": 18000, \"sync\" : true";
            body += "}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);

            return response;
        }
        #endregion
    }
}
