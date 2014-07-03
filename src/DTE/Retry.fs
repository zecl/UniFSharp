namespace DTE
open System.Threading

[<AutoOpen>]
module Retry =

  type RetryBuilder(count, seconds) = 
    member x.Return(a) = a
    member x.Delay(f) = f
    member x.Zero() = failwith "Zero" 
    member x.Run(f) =
      let rec loop(n) = 
        if n = 0 then 
          failwith "retry failed"
        else 
          try 
            f()
          with e ->
            Thread.Sleep(seconds * 1000. |> int) 
            loop(n-1)

      loop count

  let retry = RetryBuilder(30,1.)