using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    /// <summary>
    ///   Simple abtract class to implement primitive naughty words screening from tag input
    /// </summary>
    /// 
    public abstract class UserGeneratedTextBase
    {
        protected const string BANNED_WORD_DETECTED = "Disallowed word detected, please edit your text and try again";
        public IEnumerable<string> BannedWords;

        public UserGeneratedTextBase()
        {
            string relPath = "~/banned_words.xml";
            string absPath = HttpContext.Current.Server.MapPath(relPath);
            XDocument wordsdoc = XDocument.Load(absPath);
            var words = from w in wordsdoc.Root.Elements()
                        select (string)w;

            BannedWords = words;
        }

        public bool IsRudeWord(string word)
        {
            var rude = from bw in BannedWords
                       where bw == word.ToLower()
                       select bw;
            if (rude.Count() == 0)
                return false;
            else
                return true;
        }
        public bool ContainsRudeWord(string text)
        {
            var words = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
                if (IsRudeWord(w)) return true;
            return false;
        }
    }
}