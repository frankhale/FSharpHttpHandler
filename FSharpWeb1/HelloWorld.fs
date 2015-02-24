namespace FSharpWeb1

open System
open System.Web

//type HelloWorldHandler() =
//  interface IHttpHandler with
//    member this.IsReusable 
//      with get() = false
//    member this.ProcessRequest(context:HttpContext) =
//      context.Response.ContentType <- "text/html"
//      context.Response.Write("<b>I love F# OMG!</b>")

type HelloWorld() =
  static member Handler(ctx:HttpContext) =
    ctx.Response.ContentType <- "text/html"
    ctx.Response.Write("<b>I love <a href='http://fsharp.org/' target='_blank'>F#</a> OMG!</b>")

type HelloWorldAsyncIHttpHandler() =
  static let handler = new Action<HttpContext>(HelloWorld.Handler)

  interface IHttpAsyncHandler with
    member this.IsReusable 
      with get() = true

    member this.BeginProcessRequest(ctx, cb, ed) =
      handler.BeginInvoke(ctx, cb, null)

    member this.EndProcessRequest(result) = ()
    member this.ProcessRequest(ctx) = ()
