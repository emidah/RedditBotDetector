using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Reddit;
using Reddit.Controllers;
using RedditBotDetector.Extensions;
using Comment = Reddit.Things.Comment;
using Post = Reddit.Things.Post;

namespace RedditBotDetector {
    internal static class RepostDetector {
        public static List<RepostPost> GetRepostsForPosts(RedditClient reddit, List<Post> posts) {
            var reposts = posts.Select(post => reddit.Post(post.Name).About())
                .OfType<LinkPost>()
                .Select(post =>
                    new RepostPost {
                        Post = post.Listing,
                        OriginalPost = SearchForPost(reddit, post.Listing)
                    })
                .ToList();
            return reposts;
        }

        private static List<Post> SearchForPosts(RedditClient reddit, Post post) {
            return SearchForPostController(reddit, post).Select(p => p.Listing).ToList();
        }

        private static Post SearchForPost(RedditClient reddit, Post post) {
            return SearchForPostController(reddit, post).Select(p => p.Listing).FirstOrDefault();
        }

        private static List<Reddit.Controllers.Post> SearchForPostController(RedditClient reddit, Reddit.Controllers.Post post) {
            return SearchForPostController(reddit, post.Listing);
        }

        private static List<Reddit.Controllers.Post> SearchForPostController(RedditClient reddit, Post post, bool searchForOne = false) {
            //workaround for reddit's horrible search

            const int phraseLength = 3;
            const int phraseCount = 2;

            var title = NormalizeTitle(post.Title);
            var searchTerms = GenerateAlternativeSearchTerms(title, phraseLength, phraseCount);

            List<Reddit.Controllers.Post> SearchFun(string term) {
                return reddit.Search(term, limit: 25, sort: "relevance").ToList();
            }

            var reposts = SearchFun(title).ToList();
            if (searchForOne) {
                var repostsToReturn = reposts.Where(item => post.IsRepostOf(item.Listing)).ToList();
                if (repostsToReturn.Count > 0) {
                    return repostsToReturn;
                }
            }

            foreach (var searchTerm in searchTerms) {
                var searchAgain = SearchFun(searchTerm);
                if (searchForOne) {
                    var repostsToReturn = searchAgain.Where(item => post.IsRepostOf(item.Listing)).ToList();
                    if (repostsToReturn.Count > 0) {
                        return searchAgain;
                    }
                } else {
                    reposts = reposts.Concat(searchAgain).ToList();
                }
            }
            return searchForOne ? new List<Reddit.Controllers.Post>() : reposts.Where(item => post.IsRepostOf(item.Listing)).ToList();
        }

        private static string[] GenerateAlternativeSearchTerms(string title, int phraseLength, int phraseCount) {
            var words = title.Split(" ");
            var random = GenerateRandomWithSeed(title);
            string[] searchTerms;
            if (words.Length > phraseLength) {
                var randoms = new List<int>();
                while (randoms.Count < Math.Min(phraseCount, words.Length - phraseLength)) {
                    var randomInt = random.Next(words.Length - phraseLength);
                    if (!randoms.Contains(randomInt)) {
                        randoms.Add(randomInt);
                    }
                }
                searchTerms = randoms
                    .Select(i => string.Join(' ', words[new Range(i, i + phraseLength)]))
                    .ToArray();
            } else {
                searchTerms = new string[0];
            }

            return searchTerms;
        }

        private static Random GenerateRandomWithSeed(string title) {
            var hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(title.ToCharArray()));
            var seed = BitConverter.ToInt32(hash.Take(4).ToArray());
            var random = new Random(seed);
            return random;
        }

        //public static List<(LinkPost post, Post)> GetRepostsForPosts(RedditClient reddit, List<Post> posts) {
        //    var reposts = posts
        //        .OfType<LinkPost>()
        //        .Select(post =>
        //            (post, reddit.Search(NormalizeTitle(post.Title), limit: 25, sort: "num_comments")
        //                .FirstOrDefault(post.IsRepostOf)))
        //        .Where(tuple => tuple.Item2 != null)
        //        .ToList();
        //    return reposts;
        //}


        [SuppressMessage("ReSharper", "RedundantEnumerableCastCall")]
        public static List<RepostComment> GetRepostsForComments(List<Comment> comments, RedditClient reddit) {
            // get posts related to comments
            var commentsWithPosts = comments
                .Select(originalComment => (originalComment, reddit.GetPostForCommentListing(originalComment)))
                .Cast<(Comment comment, Reddit.Controllers.Post post)>();
            // get duplicate posts, and from duplicate posts get some top comments
            var commentsWithPostAndCommentsFromDuplicates = commentsWithPosts
                .Select(tuple => (tuple.comment, tuple.post,
                    SearchForPostController(reddit, tuple.post.Listing)
                        .Top(5)
                        .GetTopCommentsFlattened(50, 3)
                        .ToList()))
                .Cast<(Comment comment, Reddit.Controllers.Post post, List<Comment> commentsFromDupes)>().ToList();

            // Only keep comments where the duplicate post comments contain the original comment
            var fakeCommentsWithPosts = commentsWithPostAndCommentsFromDuplicates
                .Select(tuple => new RepostComment {
                    Comment = tuple.comment,
                    CommentPost = tuple.post.Listing,
                    OriginalComment = tuple.commentsFromDupes
                        .FirstOrDefault(comment => NormalizeCommentBody(comment.Body) == NormalizeCommentBody(tuple.comment.Body))
                })
                .ToList();
            return fakeCommentsWithPosts;
        }

        private static string NormalizeCommentBody(string body) {
            var newLineRegex = new Regex(@"\s\s*\n", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            return newLineRegex.Replace(body, "\n").Trim();
        }

        private static string NormalizeTitle(string postTitle) {
            postTitle = postTitle.Replace('*', ' ').Replace('/', ' ');
            if (postTitle.EndsWith(".")) {
                return postTitle.Substring(0, postTitle.Length - 1).Trim();
            }

            return postTitle;
        }
    }
}