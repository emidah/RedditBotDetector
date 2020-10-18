using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reddit;
using Reddit.Controllers;

namespace RedditBotDetector {
    internal class Program {
        private static int Main(string[] args) {
            var userName = args.Length != 1 ? string.Empty : args[0];

            if (userName == string.Empty) {
                return 1;
            }

            var services = ServiceProviderBuilder.GetServiceProvider(args);
            var secrets = services.GetRequiredService<IOptions<Secrets>>().Value;
            var reddit = new RedditClient(secrets.AppId, appSecret: secrets.ClientSecret, refreshToken: secrets.RefreshToken);

            var user = reddit.User(userName);
            var submissions = user.GetOverview(limit: 5).ToList();
            var totalSubmissions = submissions.Count;

            if (totalSubmissions == 0) {
                Console.WriteLine("This user has no submissions");
                return 1;
            }

            var posts = submissions.GetPosts();
            var comments = submissions.GetComments();

            var loadedPosts = posts
                .Select(post => post.Load(reddit))
                .OfType<LinkPost>()
                .ToList();
            //var test = loadedPosts.First().GetDuplicates(limit: 5, sort: "num_comments");
            var postsWithReposts = loadedPosts.Select(post =>
                    ValueTuple.Create(post,
                        post.GetDuplicates(limit: 25, sort: "num_comments")
                            .Top(10)
                            .FirstOrDefault(post.IsRepostOf)))
                .Where(tuple => tuple.Item2 != null)
                .ToList();
            var totalPostsWithReposts = postsWithReposts.Count;

            var loadedComments = comments.Select(comment => comment.Load(reddit));
            //loadedComments.Where(comment => comment.Listing.Body)
            
            //var repostedComments = 
            return 0;
        }
    }
}