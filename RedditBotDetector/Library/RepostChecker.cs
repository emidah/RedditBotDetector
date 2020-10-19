using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reddit;
using RedditBotDetector.Extensions;

namespace RedditBotDetector.Library {
    public class RepostChecker {
        private static readonly object LockObject = new object();
        private static readonly IServiceProvider Services = ServiceProviderBuilder.GetServiceProvider();
        private static readonly Secrets Secrets = Services.GetRequiredService<IOptions<Secrets>>().Value;
        private static readonly RedditClient Client = new RedditClient(Secrets.AppId, appSecret: Secrets.ClientSecret, refreshToken: Secrets.RefreshToken,
            userAgent: UserAgent);
        private const string UserAgent = "dotnet:KarmaBotDetector:v0.1";

        public static RepostReport CheckUser(string userName) {
            lock(LockObject) {
                var user = Client.User(userName);
                var postsOrComments = user.GetOverview(limit: 11).ToList();

                // Get which posts are reposts
                var posts = postsOrComments
                    .GetPosts()
                    .ToList();
                IList<RepostPost> reposts = RepostDetector.GetRepostsForPosts(Client, posts);

                // Get which comments are stolen from previous submissions of the parent posts
                var comments = postsOrComments
                    .GetComments()
                    .ToList();

                IList<RepostComment> repostComments = RepostDetector.GetRepostsForComments(comments, Client);

                return new RepostReport {
                    Posts = reposts,
                    Comments = repostComments
                };
            }
        }
    }

    public class RepostReport {
        public IList<RepostPost> Posts {
            get;
            set;
        }

        public IList<RepostComment> Comments {
            get;
            set;
        }

    }
}