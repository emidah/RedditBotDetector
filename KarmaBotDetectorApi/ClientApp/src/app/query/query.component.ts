import { Component, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";

@Component({
  selector: "query",
  templateUrl: "./query.component.html",
  styleUrls: ["./query.component.css"]
})
export class FetchDataComponent {
  posts = null;
  comments = null;
  isFetching = false;
  username = "";
  lastUsername = "";
  constructor(private http: HttpClient, @Inject("BASE_URL") private baseUrl: string) {
  }

  static get redditUrl(): string { return "https://reddit.com" };

  onSubmit() {
    this.lastUsername = this.username;
    this.isFetching = true;
    this.http.get(this.baseUrl + "BotDetector/" + this.username).subscribe(result => {
      this.posts = result["posts"].map((rawPost: any): IPost => {
        this.isFetching = false;
        const post: IPost = {
          url: FetchDataComponent.redditUrl + rawPost.post.permalink,
          originalUrl: FetchDataComponent.redditUrl + rawPost.originalPost.permalink
        }
        return post;
      });
      this.comments = result["comments"].map((rawPost: any): IPost => {
        const post: IComment = {
          url: FetchDataComponent.redditUrl + rawPost.comment?.permalink,
          postUrl: FetchDataComponent.redditUrl + rawPost.commentPost.permalink,
          originalUrl: FetchDataComponent.redditUrl + rawPost.originalComment.permalink
        }
        return post;
      });
      console.log(result);
    }, error => {
      console.error(error);
      this.isFetching = false;
    });
    
  }

}

interface IPost {
  url: string;
  originalUrl: string;
}

interface IComment {
  url: string;
  postUrl: string;
  originalUrl: string;
}

