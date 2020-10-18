using System.Collections.Generic;
using System.Linq;
using Reddit;
using Reddit.Things;

namespace RedditBotDetector {
    public static class PostExtensions {
        public static IEnumerable<Post> GetPosts(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Post != null).Select(item => item.Post);
        }

        public static Reddit.Controllers.Post Load(this Post post, RedditClient client) {
            return client.Post(post.Name).About();
        }

        public static IEnumerable<Reddit.Controllers.Post> Top(this IEnumerable<Reddit.Controllers.Post> posts, int count) {
            return posts.OrderBy(post => post.UpVotes).Take(count);
        }

        public static IEnumerable<Reddit.Controllers.Comment> GetTopCommentsMany(this IEnumerable<Reddit.Controllers.Post> posts, int count) {
            return posts.SelectMany(post => post.Comments.GetTop(5));
        }

        public static bool IsRepostOf(this Reddit.Controllers.Post self, Reddit.Controllers.Post other) {
            return self.Created > other.Created && self.Title == other.Title && self.Subreddit == other.Subreddit;
        }
    }
}