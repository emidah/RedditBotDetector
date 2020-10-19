using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Reddit;
using Reddit.Controllers;
using RedditBotDetector.Extensions;
using Comment = Reddit.Things.Comment;
using Post = Reddit.Things.Post;

namespace RedditBotDetector {
    public static class RepostDetector {
        public static List<(LinkPost post, Reddit.Controllers.Post)> GetRepostsForPosts(RedditClient reddit, List<Post> posts) {
            var reposts = posts.Select(post => reddit.Post(post.Name).About())
                .OfType<LinkPost>()
                .Select(post =>
                    (post, reddit.Search(post.Title, limit: 25, sort: "num_comments")
                        .FirstOrDefault(post.IsRepostOf)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();
            return reposts;
        }

        public static List<(LinkPost post, Reddit.Controllers.Post)> GetRepostsForPosts(RedditClient reddit, List<Reddit.Controllers.Post> posts) {
            var reposts = posts
                .OfType<LinkPost>()
                .Select(post =>
                    (post, reddit.Search(post.Title, limit: 25, sort: "num_comments")
                        .FirstOrDefault(post.IsRepostOf)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();
            return reposts;
        }

       
        [SuppressMessage("ReSharper", "RedundantEnumerableCastCall")]
        public static List<(Comment originalComment, Reddit.Controllers.Post post)> GetRepostsForComments(List<Comment> comments, RedditClient reddit) {
            // get posts related to comments
            var commentsWithPosts = comments
                .Select(originalComment => (originalComment, reddit.GetPostForCommentListing(originalComment)))
                .Cast<(Comment originalComment, Reddit.Controllers.Post post)>();
            // get duplicate posts, and from duplicate posts get some top comments
            var commentsWithPostAndCommentsFromDuplicates = commentsWithPosts
                .Select(tuple => (tuple.originalComment, tuple.post,
                    reddit.Search(NormalizeTitle(tuple.post.Title), limit: 25, sort: "num_comments")
                        .Where(foundPost => foundPost.HasSameLink(tuple.post) && foundPost.Id != tuple.post.Id)
                        .Top(5)
                        .GetTopCommentsFlattened(50, 3)
                        .ToList()))
                .Cast<(Comment originalComment, Reddit.Controllers.Post post, List<Comment> commentsFromDupes)>().ToList();

            // Only keep comments where the duplicate post comments contain the original comment
            var fakeCommentsWithPosts = commentsWithPostAndCommentsFromDuplicates
                .Where(tuple => tuple.commentsFromDupes.Any(comment => comment.Body == tuple.originalComment.Body))
                .Select(tuple => (tuple.originalComment, tuple.post))
                .ToList();
            return fakeCommentsWithPosts;
        }

        private static string NormalizeTitle(string postTitle) {
            if (postTitle.EndsWith(".")) {
                return postTitle.Substring(0, postTitle.Length - 1).Trim();
            }
            return postTitle;
        }
    }
}