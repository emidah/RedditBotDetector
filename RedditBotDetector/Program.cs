using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reddit;
using RedditBotDetector.Extensions;

namespace RedditBotDetector {
    internal class Program {
        private const string UserAgent = "dotnet:KarmaBotDetector:v0.1";

        private static int Main(string[] args) {
            var userName = args.Length != 1 ? string.Empty : args[0].Trim();

            if (userName == string.Empty) {
                return 1;
            }

            var services = ServiceProviderBuilder.GetServiceProvider();
            var secrets = services.GetRequiredService<IOptions<Secrets>>().Value;

            var reddit = new RedditClient(secrets.AppId, appSecret: secrets.ClientSecret, refreshToken: secrets.RefreshToken,
                userAgent: UserAgent);

            Console.WriteLine($"Checking /u/{userName}");

            var user = reddit.User(userName);
            var postsOrComments = user.GetOverview(limit: 11).ToList();
            var totalPostsOrComments = postsOrComments.Count;
            if (totalPostsOrComments == 0) {
                Console.WriteLine("This user has no posts or comments");
                return 1;
            }

            // Get which posts are reposts
            var posts = postsOrComments
                .GetPosts()
                .ToList();
            List<RepostPost> reposts;
            if (posts.Count > 0) {
                // Check which of the user's posts are reposts
                reposts = RepostDetector.GetRepostsForPosts(reddit, posts);
                Console.WriteLine($"Posts: {reposts.Count} out of {posts.Count} are reposts");
            } else {
                Console.WriteLine("Posts: 0 posts in user's recent 10");
            }

            // Get which comments are stolen from previous submissions of the parent posts
            var comments = postsOrComments
                .GetComments()
                .ToList();

            List<RepostComment> fakeCommentsWithPosts;
            if (comments.Count > 0) {
                fakeCommentsWithPosts = RepostDetector.GetRepostsForComments(comments, reddit)
                    .Where(post => post.OriginalComment != null)
                    .ToList();
                Console.WriteLine($"Comments: {fakeCommentsWithPosts.Count} out of {comments.Count} are reposts");
            } else {
                Console.WriteLine("Comments: 0 comments in user's recent 10");
            }

            return 0;
        }
    }
}