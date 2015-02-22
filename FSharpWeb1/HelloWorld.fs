namespace FSharpWeb1

open System
open System.Web

type HelloWorldHandler() =
  interface IHttpHandler with
    member this.IsReusable 
      with get() = false
    member this.ProcessRequest(context:HttpContext) =
      context.Request.Headers.Add("text", "html")
      context.Response.Write("<b>Hello, F# World!</b>")