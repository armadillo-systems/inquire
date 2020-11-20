using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.Text.RegularExpressions;
using iNQUIRE.Helpers;
using Microsoft.CognitiveServices.ContentModerator;

namespace iNQUIRE.Models
{
    /// <summary>
    ///   Simple abtract class to implement primitive naughty words screening from tag input
    /// </summary>
    /// 
    public abstract class UserGeneratedTextBase
    {
        protected const string BANNED_WORD_DETECTED = "Disallowed text detected, please edit your text and try again";
        public static IEnumerable<string> BannedWords;

        public UserGeneratedTextBase()
        {
            string relPath = "~/banned_words.xml";
            string absPath = HttpContext.Current.Server.MapPath(relPath);
            XDocument wordsdoc = XDocument.Load(absPath);
            var words = from w in wordsdoc.Root.Elements()
                        select (string)w;

            BannedWords = words;
        }

        private static bool IsRudeWord(string word)
        {
            var rude = from bw in BannedWords
                       where bw == word.ToLower()
                       select bw;
            if (rude.Count() == 0)
                return false;
            else
                return true;
        }
        private static bool ContainsRudeWord(string text)
        {
            var words = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
                if (IsRudeWord(w)) return true;
            return false;
        }

        private static bool ContainsUrl(string text)
        {
            return Regex.IsMatch(text, "www|http|https|ftp|mailto", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(text, @"[^\b]\.([a-zA-Z]{2}|aero|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel)[\b/]", RegexOptions.IgnoreCase);
        }

        private static bool ContainsEmail(string text)
        {
            return Regex.IsMatch(text, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase);
        }

        public bool IsTextModerationPassed(string text)
        {
            // do these first as they are Regex based, no request to external service needed
            if (ContainsEmail(text) || ContainsUrl(text) || ContainsRudeWord(text))
                return false;

            var moderated_ok = true;
            if (ContentModeratorHelper.IsContentModeratorEnabled)
            {
                using (var cmclient = ContentModeratorHelper.NewClient())
                {
                    moderated_ok = ContentModeratorHelper.IsTextModerationPassed(cmclient, text);
                }
            }
            return moderated_ok;
        }
    }
}