using System.Collections.Generic;
using System.Linq;
using Reddit;
using Reddit.Things;

namespace RedditBotDetector {
    public static class CommentExtensions {
        public static IEnumerable<Comment> GetComments(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Comment != null).Select(item => item.Comment);
        }
        public static Reddit.Controllers.Comment Load(this Comment comment, RedditClient client) {
            return client.Comment(comment.Name).About();
        }
    }
}