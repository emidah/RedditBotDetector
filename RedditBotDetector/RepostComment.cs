using Reddit.Things;

namespace RedditBotDetector {
    public class RepostComment {
        public Comment Comment {
            get;
            set;
        }

        public Post CommentPost {
            get;
            set;
        }

        public Comment OriginalComment {
            get;
            set;
        }
    }
}