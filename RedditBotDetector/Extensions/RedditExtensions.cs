using Reddit;
using Reddit.Exceptions;
using Reddit.Things;

namespace RedditBotDetector.Extensions {
    public static class RedditExtensions {
        public static Reddit.Controllers.Post Load(this RedditClient client, Post post) {
            try {
                return client.Post(post.Name).About();
            } catch (RedditNotFoundException) {
                return null;
            }
        }

        public static Reddit.Controllers.Comment Load(this RedditClient client, Comment post) {
            try {
                var comm = client.Comment(post.Name);
                return comm.About();
            } catch (RedditNotFoundException) {
                return null;
            }
        }
    }
}
