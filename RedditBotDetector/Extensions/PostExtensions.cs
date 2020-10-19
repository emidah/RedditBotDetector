using System;
using System.Collections.Generic;
using System.Linq;
using Reddit.Controllers;
using Reddit.Inputs.Wiki;
using Reddit.Things;
using Post = Reddit.Things.Post;
using Subreddit = Reddit.Controllers.Subreddit;

namespace RedditBotDetector.Extensions {
    public static class PostExtensions {
        public static IEnumerable<Post> GetPosts(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Post != null).Select(item => item.Post);
        }

        public static IEnumerable<Reddit.Controllers.Post> Top(this IEnumerable<Reddit.Controllers.Post> posts, int count) {
            return posts.OrderByDescending(post => post.UpVotes).Take(count);
        }

        public static IEnumerable<Reddit.Things.Comment> GetTopCommentsFlattened(this IEnumerable<Reddit.Controllers.Post> posts, int count, int depth = 1) {
            return posts
                .SelectMany(post => post.Comments
                    .GetTop(0, limit: count, depth: depth)
                    .Select(comment => comment.Listing)
                    .ToList()
                    .GetCommentsRecursive())
                .Where(item => item.Body != null)
                .ToList();
        }

        public static bool IsRepostOf(this Reddit.Controllers.Post self, Reddit.Controllers.Post other) {
            if (self.Id == other.Id) {
                return false;
            }
            // This is method is completely based on observed bot patterns. It might not make sense to the untrained eye.
            // Bots often crosspost between nononono and wcgw because of the similarity of the subreddits. They often edit the title when doing so.
            var subredditsToMatch = new List<string>(){ self.Subreddit.ToUpperInvariant() };
            var shortenedTitle = self.Title.EndsWith('.') ? self.Title.Substring(0, self.Title.Length - 1).Trim() : self.Title;
            switch (self.Subreddit.ToUpperInvariant()){
                case "NONONONO":
                    subredditsToMatch.Add("WHATCOULDGOWRONG");
                    break;
                case "WHATCOULDGOWRONG":
                    shortenedTitle = shortenedTitle.Replace("wcgw", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                    subredditsToMatch.Add("NONONONO");
                    break;
                case "PETTHEDAMNCOW":
                    subredditsToMatch.Add("HAPPYCOWGIFS");
                    break;
                case "HAPPYCOWGIFS":
                    subredditsToMatch.Add("PETTHEDAMNCOW");
                    break;
            }
            var isClonedTitle = self.Title == other.Title || other.Title.Contains(shortenedTitle);
            return self.Created > other.Created && subredditsToMatch.Contains(other.Subreddit.ToUpperInvariant()) && isClonedTitle;
        }

        public static bool HasSameLink(this Reddit.Controllers.Post self, Reddit.Controllers.Post other) {
            if (self is LinkPost lp && other is LinkPost lp2) {
                var url1 = NormalizeUrl(lp.URL);
                var url2 = NormalizeUrl(lp2.URL);
                return url1 == url2;
            }
            return false;
        }

        private static string NormalizeUrl(string url1) {
            if (url1.StartsWith("https")) {
                return url1;
            }
            if (url1.StartsWith("http")) {
                return "https" + url1.Substring(4);
            }
            return url1;
        }

        /// <summary>
        /// Return a list of other submissions of the same URL.
        /// </summary>
        /// <param name="after">fullname of a thing</param>
        /// <param name="before">fullname of a thing</param>
        /// <param name="crosspostsOnly">boolean value</param>
        /// <param name="sort">one of (num_comments, new)</param>
        /// <param name="sr">subreddit name</param>
        /// <param name="count">a positive integer (default: 0)</param>
        /// <param name="limit">the maximum number of items desired (default: 25, maximum: 100)</param>
        /// <param name="show">(optional) the string all</param>
        /// <param name="srDetail">(optional) expand subreddits</param>
        /// <returns>A list of matching posts.</returns>
        public static List<LinkPost> GetDuplicates(this Reddit.Controllers.Post post, string after = "", string before = "", bool crosspostsOnly = false, string sort = "new", string sr = "",
            int count = 0, int limit = 25, string show = "all", bool srDetail = false) {
            if (post is LinkPost linkPost) {
                return linkPost.GetDuplicates(after, before, crosspostsOnly, sort, sr, count, limit, show, srDetail);
            } else {
                return new List<LinkPost>();
            }
        }
    }
}