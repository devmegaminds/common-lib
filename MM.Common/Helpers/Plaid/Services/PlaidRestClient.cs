/* ------------------------------------------------------------------------------------

CREATED BY          :       Variya Sandip.
CREATED DATE        :       15-Fab-2021.
DESCRIPTION         :       This file for the plaid only, we are connecting all pliad RestClient API here.

 -------------------------------------------------------------------------------------- */

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MM.Common.Helpers.Plaid.Services
{
    public static class PlaidRestClient
    {

        public static string BaseUrl()
        {
            return "https://sandbox.plaid.com";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ResT"></typeparam>
        /// <typeparam name="ReqT"></typeparam>
        /// <param name="strUrl"></param>
        /// <param name="RequestParameters"></param>
        /// <returns></returns>
        public static ResT PostRequest<ResT, ReqT>(string strUrl, ReqT RequestParameters)
        {
            strUrl = BaseUrl() + strUrl;
            try
            {
                string RequestBodyJson = JsonConvert.SerializeObject(RequestParameters);
                var client = new RestClient(strUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", RequestBodyJson, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<ResT>(response.Content);
                else
                    return JsonConvert.DeserializeObject<ResT>(null);

            }
            catch (Exception ex)
            {
                return default(ResT);
            }

        }
    }
}
