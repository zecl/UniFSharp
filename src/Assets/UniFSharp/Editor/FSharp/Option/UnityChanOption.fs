namespace UniFSharp
open System.Xml.Serialization

[<AllowNullLiteral>]
type UnityChanOption () =

    // Starting Voice
    member val startingVoice = true with get, set

    // Build Voice
    member val buildVoice = true with get, set

    // Progress Voice
    member val progressVoice = true with get, set

    // Progress Interval
    member val progressInterval = 30. with get, set

    // TimeSignal Voice
    member val timeSignalVoice = true with get, set

    // Event Voice
    member val eventVoice = true with get, set

    // Starting Voice
    member val birthday = "2014/06/17" with get, set
