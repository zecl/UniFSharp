namespace UniFSharp
open System
open UnityEngine

[<AutoOpen>]
module Util = 
  type DisposableBuilder () =

    member x.Delay(f : unit -> IDisposable) = 
      { new IDisposable with 
          member x.Dispose() = f().Dispose() }

    member x.Bind(d1:IDisposable, f:unit -> IDisposable) = 
      let d2 = f()
      { new IDisposable with 
          member x.Dispose() = d1.Dispose(); d2.Dispose() }

    member x.Return(()) = x.Zero()

    member x.Zero() =
      { new IDisposable with 
          member x.Dispose() = () }

  let disposable = DisposableBuilder()