using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace DevTraining101Net.Controllers
{
    public class SignController : Controller
    {

        // Enter your info here:
        string email = "*****";			// your account email
        string password = "******";			// your account password
        string integratorKey = "*****";		// your account Integrator Key (found on Preferences -> API page)
        string templateId = "*****";		// valid templateId from a template in your account
        string templateRole = "****";		// template role that exists on above referenced template
        string baseURL = "";			// - we will retrieve this
        //
        // GET: /Sign/
        public ActionResult Index()
        {
            try
            {

                string recipientName = Request.Form["name"];		// provide a recipient (signer) name
                string recipientEmail = Request.Form["email"];
                //============================================================================
                //  STEP 1 - Login API Call (used to retrieve your baseUrl)
                //============================================================================

                // Endpoint for Login api call (in demo environment):
                string url = "https://demo.docusign.net/restapi/v2/login_information";

                // set request url, method, and headers.  No body needed for login api call
                HttpWebRequest request = initializeRequest(url, "GET", null, email, password);

                // read the http response
                string response = getResponseBody(request);

                // parse baseUrl from response body
                baseURL = parseDataFromResponse(response, "baseUrl");

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));

                //============================================================================
                //  STEP 2 - Create an Envelope from Template and Send
                //============================================================================

                // append "/envelopes" to baseURL and use for signature request api call
                url = baseURL + "/envelopes";

                // construct an outgoing XML formatted request body (JSON also accepted)
                string requestBody =
                    "<envelopeDefinition xmlns=\"http://www.docusign.com/restapi\">" +
                    "<status>sent</status>" +
                    "<emailSubject>DocuSign API - Embedded Signing example</emailSubject>" +
                    "<templateId>" + templateId + "</templateId>" +
                    "<templateRoles>" +
                    "<templateRole>" +
                    "<email>" + recipientEmail + "</email>" +
                    "<name>" + recipientName + "</name>" +
                    "<roleName>" + templateRole + "</roleName>" +
                    "<clientUserId>1</clientUserId>" +	// user-configurable
                    "</templateRole>" +
                    "</templateRoles>" +
                    "</envelopeDefinition>";

                // set request url, method, body, and headers
                request = initializeRequest(url, "POST", requestBody, email, password);

                // read the http response
                response = getResponseBody(request);

                // parse the envelope uri from response body
                string uri = parseDataFromResponse(response, "uri");

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));

                //============================================================================
                //  STEP 3 - Launch the Embedded Signing view (aka recipient view)
                //============================================================================

                // append "/views/sender" to the uri that was returned from step 2 and append to baseURL
                url = baseURL + uri + "/views/recipient";

                Debug.WriteLine("return url:" + Url.Action("Index", "Final", null, Request.Url.Scheme, null));

                // construct another outgoing XML request body
                requestBody = "<recipientViewRequest xmlns=\"http://www.docusign.com/restapi\">" +
                        "<authenticationMethod>email</authenticationMethod>" +
                        "<email>" + recipientEmail + "</email>" +
                        "<returnUrl>" + Url.Action("Index", "Final", null, Request.Url.Scheme, null) + "</returnUrl>" +
                        "<clientUserId>1</clientUserId>" + 	// must match clientUserId set in step 2!
                        "<userName>" + recipientName + "</userName>" +
                        "</recipientViewRequest>";

                // set request url, method, body, and headers
                request = initializeRequest(url, "POST", requestBody, email, password);

                // read the http response
                response = getResponseBody(request);

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));
                string signUrl = parseDataFromResponse(response, "url");
                return Redirect(signUrl);

            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        Console.WriteLine(prettyPrintXml(text));
                    }
                }
            }

            return View();
        }


		//***********************************************************************************************
		// --- HELPER FUNCTIONS ---
		//***********************************************************************************************
		public  HttpWebRequest initializeRequest(string url, string method, string body, string email, string password)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);
			request.Method = method;
			addRequestHeaders( request, email, password );
			if( body != null )
				addRequestBody(request, body);
			return request;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public  void addRequestHeaders(HttpWebRequest request, string email, string password)
		{
			// authentication header can be in JSON or XML format.  XML used for this walkthrough:
			string authenticateStr = 
				"<DocuSignCredentials>" + 
					"<Username>" + email + "</Username>" +
					"<Password>" + password + "</Password>" + 
					"<IntegratorKey>" + integratorKey + "</IntegratorKey>" + // global (not passed)
					"</DocuSignCredentials>";
			request.Headers.Add ("X-DocuSign-Authentication", authenticateStr);
			request.Accept = "application/xml";
			request.ContentType = "application/xml";
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public  void addRequestBody(HttpWebRequest request, string requestBody)
		{
			// create byte array out of request body and add to the request object
			byte[] body = System.Text.Encoding.UTF8.GetBytes (requestBody);
			Stream dataStream = request.GetRequestStream ();
			dataStream.Write (body, 0, requestBody.Length);
			dataStream.Close ();
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public  string getResponseBody(HttpWebRequest request)
		{
			// read the response stream into a local string
			HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse ();
			StreamReader sr = new StreamReader(webResponse.GetResponseStream());
			string responseText = sr.ReadToEnd();
			return responseText;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public  string parseDataFromResponse(string response, string searchToken)
		{
			// look for "searchToken" in the response body and parse its value
			using (XmlReader reader = XmlReader.Create(new StringReader(response))) {
				while (reader.Read()) {
					if((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
						return reader.ReadString();
				}
			}
			return null;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public  string prettyPrintXml(string xml)
		{
			// print nicely formatted xml
			try {
				XDocument doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (Exception) {
				return xml;
			}
		}
	}
}


