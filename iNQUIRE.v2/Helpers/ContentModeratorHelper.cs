using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.CognitiveServices.ContentModerator;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Configuration;

namespace iNQUIRE.Helpers
{
    public static class ContentModeratorHelper
    {
        private static readonly string AzureBaseURL = ConfigurationManager.AppSettings["AzureContentModeratorURL"];
        private static readonly string CMSubscriptionKey = ConfigurationManager.AppSettings["AzureContentModeratorKey"];

        public static string TestTest = @"Is this a grabage email abcdef@abcd.com, phone: 4255550111, IP: 255.255.255.255, 1234 Main Boulevard, Panapolis WA 96555.
Crap is the profanity here. Is this information PII? phone 4255550111";

        public static bool IsContentModeratorEnabled
        {
            get { return !string.IsNullOrEmpty(AzureBaseURL) && !string.IsNullOrEmpty(CMSubscriptionKey); }
        }

        public static bool IsTextModerationPassed(ContentModeratorClient client, string text)
        {
            // Remove carriage returns
            text = text.Replace(Environment.NewLine, " ");
            // Screen the input text: check for profanity, classify the text into three categories,
            // do autocorrect text, and check for personally identifying information (PII)
            var screen_result = client.TextModeration.ScreenText("text/plain", new MemoryStream(Encoding.UTF8.GetBytes(text)), null, true, true, null, true);
            // outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));

            if (!string.IsNullOrEmpty(screen_result.Status.Exception))
                throw new Exception(string.Format("Unable to moderate text: {0}", screen_result.Status.Exception));

            bool review = (screen_result?.Classification?.ReviewRecommended) ?? false;

            if (screen_result.PII != null || screen_result.Terms != null || review)
                return false;
            return true;
        }


        /// <summary>
        /// Returns a new Content Moderator client for your subscription.
        /// </summary>
        /// <returns>The new client.</returns>
        /// <remarks>The <see cref="ContentModeratorClient"/> is disposable.
        /// When you have finished using the client,
        /// you should dispose of it either directly or indirectly. </remarks>
        public static ContentModeratorClient NewClient()
        {
            // Create and initialize an instance of the Content Moderator API wrapper.
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(CMSubscriptionKey));
            client.Endpoint = AzureBaseURL;
            return client;
        }
    }
}