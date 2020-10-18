using System.Collections.Generic;
using System.Linq;
using Reddit;
using Reddit.Things;

namespace RedditBotDetector.Extensions {
    public static class CommentExtensions {
        public static IEnumerable<Comment> GetComments(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Comment != null).Select(item => item.Comment);
        }
    }
}