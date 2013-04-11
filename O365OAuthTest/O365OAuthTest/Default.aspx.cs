using O365OAuthTest.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace O365OAuthTest
{
    public partial class Default : System.Web.UI.Page
    {
        public const string targetPrincipalName = "00000003-0000-0ff1-ce00-000000000000";

        public string clientId = "";
        public string clientSecret = "";
        public string redirectUrl = "https://localhost:44316/Default.aspx";
        public Uri targetUrl = new Uri("https://<mydomain>.sharepoint.com/");

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var loginUrl = new StringBuilder();
                loginUrl.Append(targetUrl.ToString());
                loginUrl.Append("_layouts/15/OAuthAuthorize.aspx?");
                loginUrl.Append(string.Format("client_id={0}&", clientId));
                loginUrl.Append(string.Format("client_secret={0}&", clientSecret));
                loginUrl.Append(string.Format("scope={0}.{1}&", ScopeAlias.AllProfiles.Name, Rights.Manage.Name));
                loginUrl.Append("response_type=code&");
                loginUrl.Append(string.Format("redirect_uri={0}", redirectUrl));

                loginHyperlink.NavigateUrl = loginUrl.ToString();

                var requestCode = Page.Request.QueryString["code"];
                if (requestCode != null)
                {
                    var accessToken = GetAccessToken(requestCode);
                    GetSharePointMyProperties(accessToken);
                }
            }
            catch (WebException ex)
            {
                Response.Write(ex.InnerException.ToString());
            }
        }

        public void GetSharePointMyProperties(string accessToken)
        {
            var spRequest = HttpWebRequest.Create(targetUrl.ToString() + "_api/SP.UserProfiles.PeopleManager/GetMyProperties");
            spRequest.Headers.Add("Authorization: Bearer " + accessToken);

            using (WebResponse spResponse = spRequest.GetResponse())
            {
                var dataStream = spResponse.GetResponseStream();
                var reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Response.Write(Server.HtmlEncode(responseFromServer));
                reader.Close();
                dataStream.Close();
            }
        }

        #region Helper Functions

        private string GetAccessToken(string requestToken)
        {
            var realm = GetRealmFromTargetUrl(targetUrl);
            var acsAuth2Url = string.Format("https://accounts.accesscontrol.windows.net/{0}/tokens/OAuth/2", realm);

            var request = HttpWebRequest.Create(acsAuth2Url);
            request.Method = "POST";

            var postDataString = new StringBuilder();
            postDataString.Append("grant_type=authorization_code");
            postDataString.Append("&client_id=" + Server.UrlEncode(clientId + "@" + realm));
            postDataString.Append("&client_secret=" + Server.UrlEncode(clientSecret));
            postDataString.Append("&code=" + requestToken);
            postDataString.Append("&redirect_uri=" + Server.UrlEncode(redirectUrl));
            postDataString.Append("&resource=" + Server.UrlEncode(targetPrincipalName + "/" + targetUrl.Authority + "@" + realm));

            var postDataArray = Encoding.UTF8.GetBytes(postDataString.ToString());
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataArray.Length;
            
            var dataStream = request.GetRequestStream();
            dataStream.Write(postDataArray, 0, postDataArray.Length);
            dataStream.Close();

            var currentACSData = new ACSData();
            using (WebResponse response = request.GetResponse())
            {
                dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var serializer = new JavaScriptSerializer();
                currentACSData = serializer.Deserialize<ACSData>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return currentACSData.access_token;
        }

        private string GetRealmFromTargetUrl(Uri targetApplicationUri)
        {
            var request = HttpWebRequest.Create(targetApplicationUri.ToString() + "/_vti_bin/client.svc");
            request.Headers.Add("Authorization: Bearer ");
            try
            {
                using (WebResponse response = request.GetResponse()) { }
            }
            catch (WebException e)
            {
                var bearerResponseHeader = e.Response.Headers["WWW-Authenticate"];
                var realm = bearerResponseHeader.Substring(bearerResponseHeader.IndexOf("Bearer realm=\"") + 14, 36);

                Guid realmGuid;
                if (Guid.TryParse(realm, out realmGuid)){
                    return realm;
                }
            }
            return null;
        } 

        #endregion
    }
}