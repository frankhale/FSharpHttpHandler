namespace FSharpWeb1

open System
open System.Web
open System.Linq
open System.Net
open System.ServiceModel.Syndication
open System.Text
open System.Xml

//type HelloWorldHandler() =
//  interface IHttpHandler with
//    member this.IsReusable 
//      with get() = false
//    member this.ProcessRequest(context:HttpContext) =
//      context.Response.ContentType <- "text/html"
//      context.Response.Write("<b>I love F# OMG!</b>")

type HelloWorld() =
  static member private DownloadFeeds() =
    let rss = new SyndicationFeed("ASP.NET News", "News from the ASP.NET and Visual Studio web team", null)

    let feeds = Map.ofList [ ("feed:aspnetspotlight", "http://www.asp.net/rss/spotlight");
                  ("feed:blog.aspnetnews", "http://www.asp.net/rss/content");
                  ("feed:blog.aspnetteam", "http://blogs.msdn.com/b/webdev/rss.aspx");
                  ("feed:blog.bradygaster", "http://www.bradygaster.com/feed/rss/");
                  ("feed:blog.davidfowler", "http://davidfowl.com/rss/");
                  ("feed:blog.entityframework", "http://blogs.msdn.com/b/adonet/rss.aspx");
                  ("feed:blog.jeffhandley", "http://feeds.jeffhandley.com/jeffhandley");
                  ("feed:blog.jeffreyfritz", "http://www.jeffreyfritz.com/feed/");
                  ("feed:blog.jongalloway", "http://feeds.feedburner.com/jongalloway");
                  ("feed:blog.madskristensen", "http://feeds.feedburner.com/netSlave");
                  ("feed:blog.nuget", "http://blog.nuget.org/feeds/atom.xml");
                  ("feed:blog.sayedhashimi", "http://sedodream.com/SyndicationService.asmx/GetRss");
                  ("feed:blog.scottguthrie", "https://weblogs.asp.net/scottgu/rss?containerid=13");
                  ("feed:blog.scotthanselman", "http://feeds.hanselman.com/ScottHanselman");
                  ("feed:ch9.damianedwards", "http://channel9.msdn.com/Events/Speakers/Damian-Edwards/RSS");
                  ("feed:ch9.danielroth", "http://channel9.msdn.com/Events/Speakers/daniel-roth/RSS");
                  ("feed:ch9.davidfowler", "http://channel9.msdn.com/Events/Speakers/david-fowler/RSS");
                  ("feed:ch9.jongalloway", "http://channel9.msdn.com/Events/Speakers/jon-galloway/RSS");
                  ("feed:ch9.madskristensen", "http://channel9.msdn.com/Niners/Mads%20Kristensen/RSS");
                  ("feed:ch9.pranavrastogi", "http://channel9.msdn.com/Events/Speakers/pranav-rastogi/RSS");
                  ("feed:ch9.sayedhashimi", "http://channel9.msdn.com/Events/Speakers/sayed-hashimi/RSS");
                  ("feed:ch9.scotthanselman", "http://channel9.msdn.com/Niners/Glucose/RSS");
                  ("feed:ch9.scotthunter", "http://channel9.msdn.com/Events/Speakers/scott-hunter/RSS");
                  ("feed:ch9.webcampstv", "http://channel9.msdn.com/Shows/Web+Camps+TV/RSS");
                  ("feed:youtube.madskristensen", "https://gdata.youtube.com/feeds/base/users/madskvistkristensen/uploads");
                  ("feed:youtube.sayedhashimi", "https://gdata.youtube.com/feeds/base/users/sayedihashimi/uploads");
                  ("feed:youtube.scotthanselman", "https://gdata.youtube.com/feeds/base/users/shanselman/uploads") ] 

    feeds 
    |> Map.iter(fun key value -> 
        let feed = HelloWorld.DownloadFeed(value) 
        if feed.Items.Count() > 0 then
          rss.Items <- 
            rss.Items.Union(feed.Items)
                      .GroupBy(fun i -> i.Title.Text)
                      .Select(fun i -> i.First())
                      .OrderByDescending(fun i -> i.PublishDate.Date))

    rss.Items

  static member private DownloadFeed(url:string) : SyndicationFeed =
    using(new WebClient()) 
      (fun client -> 
        try
          let stream = 
            (async {
              return! client.OpenReadTaskAsync(url) |> Async.AwaitTask
            } |> Async.StartAsTask).Result
          SyndicationFeed.Load(XmlReader.Create(stream))
        with
          | _ -> 
              new SyndicationFeed()) 

  static member Handler(ctx:HttpContext) =      
    let feedHtmlBuilder = new StringBuilder()
    let htmlTemplate = "<!DOCTYPE html><html><head><title>ASP.NET News Feeds</title></head><body>{0}</body></html>"

    HelloWorld.DownloadFeeds() 
    |> Seq.iter (fun i -> feedHtmlBuilder.Append(String.Format("<a href='{0}'>{1}</a><br>", i.Links.[0].Uri, i.Title.Text)) |> ignore)
    
    ctx.Response.ContentType <- "text/html"    
    ctx.Response.Write(String.Format(htmlTemplate, feedHtmlBuilder.ToString())) 
    

type HelloWorldAsyncIHttpHandler() =
  static let handler = new Action<HttpContext>(HelloWorld.Handler)

  interface IHttpAsyncHandler with
    member this.IsReusable 
      with get() = true

    member this.BeginProcessRequest(ctx, cb, ed) =
      handler.BeginInvoke(ctx, cb, null)

    member this.EndProcessRequest(result) = ()
    member this.ProcessRequest(ctx) = ()
