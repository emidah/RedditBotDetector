using System.Collections.Generic;
using System.Linq;
using Reddit.Controllers;
using Reddit.Things;
using Post = Reddit.Things.Post;

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
                .ToList();
        }

        public static bool IsRepostOf(this Reddit.Controllers.Post self, Reddit.Controllers.Post other) {
            return self.Created > other.Created && self.Subreddit == other.Subreddit;
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