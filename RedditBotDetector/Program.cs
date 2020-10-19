using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reddit;
using Reddit.Controllers;
using RedditBotDetector.Extensions;
using Comment = Reddit.Things.Comment;

namespace RedditBotDetector {
    internal class Program {
        private static int Main(string[] args) {
            var userName = args.Length != 1 ? string.Empty : args[0].Trim();

            if (userName == string.Empty) {
                return 1;
            }

            Console.WriteLine($"Checking /u/{userName}");

            var services = ServiceProviderBuilder.GetServiceProvider(args);
            var secrets = services.GetRequiredService<IOptions<Secrets>>().Value;
            var reddit = new RedditClient(secrets.AppId, appSecret: secrets.ClientSecret, refreshToken: secrets.RefreshToken);
            var user = reddit.User(userName);
            var postsOrComments = user.GetOverview(limit: 10).ToList();
            var totalPostsOrComments = postsOrComments.Count;

            if (totalPostsOrComments == 0) {
                Console.WriteLine("This user has no submissions");
                return 1;
            }

            var posts = postsOrComments
                .GetPosts()
                .Select(post => reddit.Post(post.Name).About())
                .OfType<LinkPost>()
                .ToList();

            // Check which of the user's posts are reposts
            var reposts = posts.Select(post =>
                    (post, reddit.Search(q: post.Title, limit: 25, sort: "num_comments")
                        .Top(10)
                        .FirstOrDefault(oldPost => post.Listing.IsRepostOf(oldPost.Listing))))
                .Where(tuple => tuple.Item2 != null)
                .ToList();
            Console.WriteLine($"Posts: {reposts.Count} out of {posts.Count} are reposts");

            // Get which comments are stolen from previous submissions of the parent posts:

            var comments = postsOrComments
                .GetComments()
                .ToList();

            // ReSharper disable RedundantEnumerableCastCall

            // get posts related to comments
            var commentsWithPosts = comments
                .Select(originalComment => (originalComment, reddit.GetPostForCommentListing(originalComment)))
                .Cast<(Comment originalComment, Post post)>();

            // get duplicate posts, and from duplicate posts get some top comments
            var commentsWithPostAndCommentsFromDuplicates = commentsWithPosts
                .Select(tuple => (tuple.originalComment, tuple.post, 
                    reddit.Search(q: tuple.post.Title, limit: 25, sort: "num_comments")
                    .Top(10)
                    .GetTopCommentsFlattened(50, 3)
                    .ToList()))
                .Cast<(Comment originalComment, Post post, List<Comment> commentsFromDupes)>().ToList();

            // Only keep comments where the duplicate post comments contain the original comment
            var fakeCommentsWithPosts = commentsWithPostAndCommentsFromDuplicates
                .Where(tuple => tuple.commentsFromDupes.Any(comment => comment.Body == tuple.originalComment.Body))
                .Select(tuple => (tuple.originalComment, tuple.post))
                .ToList();

            // ReSharper restore RedundantEnumerableCastCall

            Console.WriteLine($"Comments: {fakeCommentsWithPosts.Count} out of {comments.Count} are reposts");
            return 0;
        }
    }
}