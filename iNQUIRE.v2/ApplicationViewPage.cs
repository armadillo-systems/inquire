using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iNQUIRE
{
    public abstract class ApplicationViewPage<T> : WebViewPage<T>
    {
        public static string GoogleAnalyticsId { get; set; }
        public static string FacebookAppId { get; set; }
        public static bool FacebookLike { get; set; }
        public static bool FacebookUploadPhoto { get; set; }
        public static string TwitterText { get; set; }
        public static string TwitterHashtag { get; set; }
        public static string TwitterActivityCaption { get; set; }

        protected override void InitializePage()
        {
            SetViewBagDefaultProperties();
            base.InitializePage();
        }

        private void SetViewBagDefaultProperties()
        {
            ViewBag.GoogleAnalyticsId = GoogleAnalyticsId;
            ViewBag.FacebookAppId = FacebookAppId;
            ViewBag.Facebook = (!String.IsNullOrEmpty(FacebookAppId) && (FacebookLike || FacebookUploadPhoto));
            ViewBag.FacebookLike = FacebookLike;
            ViewBag.FacebookUploadPhoto = FacebookUploadPhoto;
            ViewBag.TwitterText = TwitterText;
            ViewBag.TwitterHashtag = TwitterHashtag;
            ViewBag.Twitter = (!String.IsNullOrEmpty(TwitterText) || !String.IsNullOrEmpty(TwitterHashtag) || !String.IsNullOrEmpty(TwitterActivityCaption) );
            ViewBag.TwitterActivityCaption = TwitterActivityCaption;
        }
    }
}