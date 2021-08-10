/// File with test cases for the LabApp
module Lab.MainModule

let simpleBinding = 1

///**
let marked1 = 2

/// **
let marked2 = 3

///**
/// Document
let notMarked1 = 6

/// **
/// Document
let notMarked2 = 7

/// Commented alias
type AliasType = List<int>

type FunctionType = unit -> unit

let AString = ""
let InterpolatedString = $""
let InterpolatedStringWithValue = $"{1}"

type Du =
    | Case1
    | Case2 of unit
    | Case3 of (unit -> unit)
    | Case4 of hello:int * world:bool
    | Case5 of Du

type Record =
    {
        Field1: unit
        Field2: (unit -> unit)
    }

type AClass() =
    let LetBinding input = input + 1
    member this.Member (input: List<unit>)= 42
    abstract member AbstractMember: unit -> unit -> unit
    static member StaticMember (input: List<unit>)= 42
    member private this.PrivateMember (input: List<unit>)= 42
    static member private PrivateStaticMember (input: List<unit>)= 42

type AClassWithArgs<'t>(arg1: 't, arg2 : float) =
    member this.Member () = 42

type IInterface1 =
    abstract member AbstractMember1 : unit -> unit -> unit

type IInterface2 =
    inherit IInterface1
    abstract member AbstractMember2 : float -> double -> bool

type InterfaceImpl =
    interface IInterface2 with
        member this.AbstractMember1 () () = ()
        member this.AbstractMember2 a b = true

///**
module MarkedModule =
    let somehting = false

module InnerModule =
    let simpleBinding = 1

    ///**
    let marked1 = 2

    /// **
    let marked2 = 3

    ///**
    /// Document
    let notMarked1 = 6

    /// **
    /// Document
    let notMarked2 = 7

    /// Commented alias
    type AliasType = List<int>

    type FunctionType = unit -> unit

    let AString = ""
    let InterpolatedString = $""
    let InterpolatedStringWithValue = $"{1}"

    type Du =
        | Case1
        | Case2 of unit
        | Case3 of (unit -> unit)
        | Case4 of hello:int * world:bool
        | Case5 of Du

    type Record =
        {
            Field1: unit
            Field2: (unit -> unit)
        }

    type AClass() =
        let LetBinding input = input + 1
        member this.Member (input: List<unit>)= 42
        abstract member AbstractMember: unit -> unit -> unit
        static member StaticMember (input: List<unit>)= 42
        member private this.PrivateMember (input: List<unit>)= 42
        static member private PrivateStaticMember (input: List<unit>)= 42

    type AClassWithArgs<'t>(arg1: 't, arg2 : float) =
        member this.Member () = 42

    type IInterface1 =
        abstract member AbstractMember1 : unit -> unit -> unit

    type IInterface2 =
        inherit IInterface1
        abstract member AbstractMember2 : float -> double -> bool

    type InterfaceImpl =
        interface IInterface2 with
            member this.AbstractMember1 () () = ()
            member this.AbstractMember2 a b = true

