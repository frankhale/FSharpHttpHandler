namespace FSharpWeb1

open System
open System.Web

type HelloWorldHandler() =
  interface IHttpHandler with
    member this.IsReusable 
      with get() = false
    member this.ProcessRequest(context:HttpContext) =
      context.Response.ContentType <- "text/html"
      context.Response.Write("<hr><b>I love F# OMG!</b>")