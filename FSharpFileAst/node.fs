namespace FSharpFileAst

open System

open FSharp.Compiler.Text

type SynType =
    | File = 7
    | Namespace = 6
    | Module = 0
    | Class = 1
    | Member = 3
    | StaticValField = 11
    | Decl = 4
    | Unknown = 5
    | ValField = 9
    | Interface = 10
    | Ctor = 8
    | CCtor = 12
    | Property = 13
    | DUCase = 14
    | DU = 15

type Node =
    val Caption : string
    val Type : SynType
    val Children : Node list
    val Range : range
    val Compilernode : Object
    val IsInterface : bool
    val Tag : string
    #if DEBUG
    new(caption, typ, cn, r, co, ?isInterface, ?tag) = { Caption=caption; Type=typ; Children=cn; Range=r; Compilernode=co; IsInterface=defaultArg isInterface false; Tag=defaultArg tag ""}
    #else
    new(caption, typ, cn, r, co, ?isInterface, ?tag) = { Caption=caption; Type=typ; Children=cn; Range=r; Compilernode=null; IsInterface=defaultArg isInterface false; Tag=defaultArg tag ""}
    #endif
    override o.ToString() = sprintf "%s [%d]" o.Caption o.Children.Length
    member o.WithChildren cn = new Node(o.Caption, o.Type, cn, o.Range, o.Compilernode, o.IsInterface, o.Tag)
    member o.ChildCount = o.Children.Length
    member o.ChildrenEnum : seq<Node> = o.Children :> seq<Node>
    member o.IsInterfaced = o.IsInterface

type Nodes = Node list
