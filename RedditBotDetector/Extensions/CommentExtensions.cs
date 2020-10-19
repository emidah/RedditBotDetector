using System.Collections.Generic;
using System.Linq;
using Reddit.Things;

namespace RedditBotDetector.Extensions {
    public static class CommentExtensions {
        public static IEnumerable<Comment> GetComments(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Comment != null).Select(item => item.Comment);
        }

        public static IEnumerable<Comment> Get(this IEnumerable<CommentOrPost> corp) {
            return corp.Where(item => item.Comment != null).Select(item => item.Comment);
        }

        public static List<Reddit.Things.Comment> GetCommentsRecursive(this List<Reddit.Things.Comment> comments) {
            // ReSharper disable PossibleMultipleEnumeration
            return comments.Concat(comments
                .Where(c => c.Replies?.Comments != null)
                .SelectMany(c => c.Replies.Comments.GetCommentsRecursive())).ToList();
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}