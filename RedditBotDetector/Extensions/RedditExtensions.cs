using Reddit;
using Reddit.Exceptions;
using Reddit.Things;
using Post = Reddit.Controllers.Post;

namespace RedditBotDetector.Extensions {
    public static class RedditExtensions {

        public static Reddit.Controllers.Post GetPostForComment(this RedditClient client, Reddit.Controllers.Comment comment) {
            return client.Post(comment.Listing.LinkId).About();
        }

        public static Reddit.Controllers.Post GetPostForCommentListing(this RedditClient client, Comment comment) {
            return client.Post(comment.LinkId).About();
        }
    }
}
