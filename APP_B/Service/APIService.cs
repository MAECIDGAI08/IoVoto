using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AppB.Service
{
    #region Session statica
    // Accesso HttpContext
    public static class HttpContext
    {
        private static IHttpContextAccessor _contextAccessor;

        public static Microsoft.AspNetCore.Http.HttpContext Current => _contextAccessor.HttpContext;

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }
    public static class StaticHttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
        {
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            System.Web.HttpContext.Configure(httpContextAccessor);
            return app;
        }
    }
    public class MyService
    {
        private readonly IHttpContextAccessor _accessor;

        public MyService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public void DoWork()
        {
            var context = _accessor.HttpContext;
            // continue with context instance
        }
    }

    #endregion
    public class APIService
    {
        // costruttore
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
              
        public static IRestResponse InvokeChaincodeParameters(string chainCodeName, string methodName, string Endpoint, string Username, params string[] parameters)
        {

            Utility.Utils.LogTrace("InvokeChaincodeParameters", "InvokeChaincodeParameters data: " + chainCodeName);
            Utility.Utils.LogTrace("InvokeChaincodeParameters", "InvokeChaincodeParameters data: " + methodName);
            Utility.Utils.LogTrace("InvokeChaincodeParameters", "InvokeChaincodeParameters data: " + Endpoint);
            Utility.Utils.LogTrace("InvokeChaincodeParameters", "InvokeChaincodeParameters data: " + Username);

            IRestResponse response = null;

            var client = new RestClient(Endpoint);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic " + Username);
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
                variablePart += "\"" + parameters[i].Replace("'","''") + "\"" + separator;
            }
            body += variablePart + "], " + "\"timeout\": 18000, \"sync\" : true";
            body += "}";


            Utility.Utils.LogTrace("InvokeChaincodeParameters", "InvokeChaincodeParameters data: " + body);

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            response = client.Execute(request);
            Console.WriteLine(response.Content);

            return response;
        }


        public static IRestResponse InvokeChaincodeVote(string chainCodeName, string methodName, string Endpoint, string Username, params string[] parameters)
       // public static IRestResponse InvokeChaincodeVote(string chainCodeName, string methodName, params string[] parameters)


        //public static IRestResponse InvokeChaincodeVote(string chainCodeName, string methodName, string[] GuiID, string Lista, string[] Candidati, string Pseudonimo)
        {
            IRestResponse response = null;
            var client = new RestClient(Endpoint);
            //var client = new RestClient("https://comitestest-elettoricomites-fra.blockchain.ocp.oraclecloud.com:7443/restproxy/api/v2/channels/default/transactions");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic " + Username);
            
            String body = @"{" + "\n" + @" ""chaincode"": """ + chainCodeName + "\"," + "\n" +
            @"""args"": [" + "\n" +
            @"""" + methodName + "\"," + "\n";

            string idsString = "";

            // metto l'oggetto degli ids dentro un var
            var test = parameters[0]; 

            // splitto l'oggetto dalle virgole ficcandolo dentro un array di stringhe
            string[]test2 = test.Split(',');

            for (var i = 0; i < test2.Length; i++)
            {
                
                if (test2.Length == 1)
                {
                    idsString += test2[0];                    
                }
                else
                {
                    // cicliamo quello che otteniamo dal codice al fine di porre una virgola ma non nell' ultimo campo (-
                    if (i == test2.Length - 1)
                    {
                        idsString += test2[i];
                    }
                    else
                    {
                        idsString += test2[i] +",";
                    }
                }
                
            }

            string idsStringToken = "{\"ids\":" + idsString + "}";

            body += "\""+idsStringToken.Replace("\"", "\\\"")+"\"" + "," +"\n";

            body += "\"" + parameters[1] + "\"," + "\n"; 
            string candidatiString = "";

            var testB = parameters[2]; 

            string[] CandidatiB = testB.Split(',');

            for (var i = 0; i < CandidatiB.Length; i++)
            {

                if (CandidatiB.Length == 1)
                {
                    candidatiString = CandidatiB[0]; 
                }
                else
                {
                    if (i == CandidatiB.Length - 1)
                    {
                        candidatiString += CandidatiB[i];
                    }
                    else
                    {
                        candidatiString += CandidatiB[i] + ",";
                    }
                }

            }
            string candidatoStringToken = "{\"candidato\":" + candidatiString + "}";

            body += "\"" + candidatoStringToken.Replace("\"", "\\\"") + "\"" + "," + "\n";

            //body += "\"" + parameters[3] + "\"," + "\n" ; // array ids

            //body += "{\"candidato\":" + candidatiString + "}," + "\n";

            body += "\"" + parameters[3] + "\"" + "\n" + "]," + "\n"; 

            body += "\"timeout\": 18000, \"sync\" : true";
            body += "}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);
            response = client.Execute(request);
            Console.WriteLine(response.Content);

            return response;

        }

    }
}
