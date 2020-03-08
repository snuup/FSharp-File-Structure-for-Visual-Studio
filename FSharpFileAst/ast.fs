module FSharpFileAst.TreeModel

open System
open System.IO
open System.Linq
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Ast
open FSharp.Compiler
open FSharp.Compiler.Text

let checker = FSharpChecker.Create()

let sp = sprintf

let getRangeText (lines : string []) (r : Range.range) =
    if r.StartLine <> r.EndLine then "linesrange"
    else
        try
            let line = lines.[r.StartLine - 1]
            //let endo = min (line.Length-1) (r.EndColumn-1)
            line.Substring(r.StartColumn, r.EndColumn - r.StartColumn)
        with e -> sp "problem %s" e.Message

let isMarkedAsInterface (xmldoc :PreXmlDoc) =
    match xmldoc with
    | PreXmlDoc.PreXmlDoc(pos, collector) ->
        match collector.LinesBefore(pos).LastOrDefault() with
        | null -> false
        | lastline ->
            //System.Diagnostics.Trace.WriteLine(lastline)
            let s = lastline.Trim()
            s.StartsWith("fsi interface") || s.StartsWith("**")
    | _ -> false

module String =

    let joined (separator : string) os : string =
        let ss = os |> Seq.map (fun o -> o.ToString()) |> Seq.toArray
        String.Join(separator, ss)

    let fromIdent (id : Ident) =
        id.idText

    let fromLongIdent (idents : LongIdent) = // LongIdent = Ident list
        String.Join(".", idents |> Seq.map fromIdent)

    let fromLongIdentWithDots (LongIdentWithDots(lid, r)) =
        lid |> fromLongIdent

    let rec fromSynType lines =
        function
        | SynType.LongIdent(lidwd) -> lidwd |> fromLongIdentWithDots
        | SynType.Var(syntypar,r) -> getRangeText lines r
        | SynType.App(typeName, leftAngleRange, typeArgs, commaRanges, rightAngleRange, isPostfix, r) -> getRangeText lines r
        | SynType.Fun(arg,ret,r) -> getRangeText lines r
        | SynType.Tuple(false, tupleitems, r) -> // isStruct = false ... this might be too simple
            tupleitems
            |> List.map (fun (somebool, syntype) -> syntype |> fromSynType lines)
            |> joined ", "
        | SynType.Array (someint, elementsyntype, r) -> elementsyntype |> fromSynType lines |> sp "%s[]"
        | st -> sp "unknown syntype %A" st

    let rec fromSynsImplePat lines =
        function
        | SynSimplePat.Id(id, altNameRefCell, isCompilerGenerated, isThisVar, isOptArg, range) -> id |> fromIdent
        | SynSimplePat.Typed(ssp, typeName, r) -> (ssp |> fromSynsImplePat lines) + ": " + (typeName |> (fromSynType lines))
        | SynSimplePat.Attrib(ssp, attributes, r) -> "attr" + (ssp |> fromSynsImplePat lines)

    let fromSynArgInfo =
        function
        | SynArgInfo(attrs, optional, Some(id)) -> id |> fromIdent
        | _ -> ""

    /// visit pattern - this is for example used in 'let <pat> = <expr>' or in the 'match' expression
    let rec fromPattern (pat : SynPat) : string =
        match pat with
        | SynPat.Wild(r) -> "wild"
        | SynPat.Named(pp, id, isThisVar, accessiblity, r) ->
            id |> fromIdent
        //| SynPat.LongIdent(dotId=LongIdentWithDots(lid, _)) -> lid |> getCaption |> sp "lid(%s)"
        //| SynPat.LongIdent(idswdts, (* holds additional ident for tooling *) ido, synvaltypardeclsoption, (* usually None: temporary used to parse "f<'a> x = x"*) synconstructorargs, synaccessoption, range) ->
        //    pam "synvaltypardeclsoption=" synvaltypardeclsoption
        //    pam "synconstructorargs=" synconstructorargs
        //    pam "synaccessoption=" synaccessoption
        //    idswdts
        //    |> fromlongidentwithdots
        //    |> sp "lid(%s)"
        | p -> sp "unrecognized pattern %A" p

    let fromValSig (vs : SynValSig) =
        match vs with
        | SynValSig.ValSpfn(ident=id) -> fromIdent id

    let rec fromSynPat lines synpat =
        let rec f =
            function
            | SynPat.Const(synconst,r) -> getRangeText lines r
            | SynPat.Wild(r) -> getRangeText lines r
            | SynPat.Named(pat, id, isThisVar (* true if 'this' variable *), accessiblity:SynAccess option, r) ->
                //printfn "pat = %A" pat - gives the same. always?
                id |> fromIdent
            | SynPat.Typed(pat, typeName, r) -> sprintf "%s : %s" (pat |> f) (typeName |> fromSynType lines)
            // | SynPat.Attrib of  SynPat * attributes:SynAttributes * range:range
            // | SynPat.Or of  SynPat * SynPat * range:range
            // | SynPat.Ands of  SynPat list * range:range
            | SynPat.LongIdent(lidwds (* holds additional ident for tooling *), identoption, synvaltypardeclsoption (* usually None: temporary used to parse "f<'a> x = x"*), synconstructorargs, synaccessoption, r) ->
                lidwds |> fromLongIdentWithDots
            | SynPat.Tuple(false, pats, r) -> pats |> List.map f |> joined ", " // struct = false might be too simple
            | SynPat.Paren (pat, r) -> pat |> f // |> sprintf "(%s)"
            // | SynPat.ArrayOrList of  bool * SynPat list * range:range
            // | SynPat.Record of fields:((LongIdent * Ident) * SynPat) list * range:range
            // /// 'null'
            // | Null of range:range
            // /// '?id' -- for optional argument names
            // | OptionalVal of Ident * range:range
            // /// ':? type '
            // | IsInst of typeName:SynType * range:range
            // /// &lt;@ expr @&gt;, used for active pattern arguments
            // | QuoteExpr of expr:SynExpr * range:range
            // /// Deprecated character ranges
            // | DeprecatedCharRange of char * char * range:range
            // /// Used internally in the type checker
            // | InstanceMember of  Ident * Ident * (* holds additional ident for tooling *) Ident option * accesiblity:SynAccess option * range:range (* adhoc overloaded method/property *)
            | p -> sp "missing pattern: %A" p
        f synpat

let getBinding (lines : string []) (Binding(access, kind, inlin, mutabl, attrs, xmlDoc, SynValData(memberflago, SynValInfo(argss, args), ido), pat, retinfo, body, r, sp) as co) =
    let syntype =
        match memberflago with
        | Some(memberflags) ->
            match memberflags.MemberKind with
            | MemberKind.ClassConstructor -> SynType.CCtor
            | MemberKind.Constructor -> SynType.Ctor
            | MemberKind.Member -> SynType.Member
            | MemberKind.PropertyGet -> SynType.Property
            | MemberKind.PropertySet -> SynType.Property
            | MemberKind.PropertyGetSet -> SynType.Property
        | None -> SynType.Member
    let syntypeflags =
        match memberflago with
        | Some(memberflags) ->
            match memberflags.IsInstance with
            | true -> "instance"
            | _ -> ""
        | None -> ""
    let caption =
        match pat with
        | SynPat.LongIdent(idswdts, (* holds additional ident for tooling *) ido, synvaltypardeclsoption, (* usually None: temporary used to parse "f<'a> x = x"*) synconstructorargs, synaccessoption, range) ->
            let membername = idswdts |> String.fromLongIdentWithDots
            let pats =
                match synconstructorargs with
                | SynConstructorArgs.Pats(pats) -> pats
                | _ -> failwith "$missing SynConstructorArgs"
            let parameters = pats |> List.map (String.fromSynPat lines) |> String.joined " "
            sprintf "%s %s" membername parameters
        | _ -> getRangeText lines pat.Range // simple return the text for therange
    let markedintf = isMarkedAsInterface xmlDoc
    Node(caption, syntype, [], pat.Range, co, markedintf, syntypeflags)

let rec getMember lines (m : SynMemberDefn) : Node list =
    match m with
    | SynMemberDefn.ValField(Field(attrs, isstatic, identoption, typeName, b, xmlDoc, accessiblity, rr), r) ->
        let t = if isstatic then SynType.StaticValField else SynType.ValField
        let caption =
            match identoption with
            | Some(id) -> id |> String.fromIdent
            | _ -> "_"
        [Node(caption, SynType.ValField, [], m.Range, m)]
    | SynMemberDefn.Member(binding, r) -> [binding |> getBinding lines]
    | SynMemberDefn.Interface(typeName, members, r) ->
        let n =
            let caption = typeName |> String.fromSynType lines
            Node(caption, SynType.Interface, [], m.Range, m)
        match members with
        | Some(ms) -> [n.WithChildren(ms |> List.collect (getMember lines))]
        | _ -> [n]
    | SynMemberDefn.ImplicitCtor(accessiblity, attributes, ctorArgs, selfIdentifier, r) ->
        let createnode (sargs : SynSimplePat list) =
            let args = sargs |> List.map (String.fromSynsImplePat lines) |> String.joined ", "
            Node(sp "(%s)" args, SynType.Ctor, [], r, m)
        match ctorArgs with
        | SynSimplePats.SimplePats(sargs,_)
        | SynSimplePats.Typed(SynSimplePats.SimplePats(sargs,_),_,_) -> [createnode sargs]
        | _ -> []
    | SynMemberDefn.LetBindings(bindings, isstatic, isrec, range) ->
        //let createnode(b : SynBinding) =
        //    Node("boinding", SynType.Member, [], range, m) |> Some
        bindings |> List.map (getBinding lines)
    | SynMemberDefn.AutoProperty(ident=id;range=r) -> [Node(String.fromIdent id, SynType.Property, [], r, m)]
    | SynMemberDefn.AbstractSlot(valSig,_,r) -> [Node(String.fromValSig valSig, SynType.Member, [], r, m)]
    | m -> [Node(sp "?member? %O" (m.GetType().Name), SynType.Member, [], m.Range, m)]

let getUnionCaseField lines =
    function
    | SynField.Field(attributes, isStatic, ido, typeName, somebool, xmlDoc, accessiblity, r) ->
        typeName |> String.fromSynType lines

let getUnionCaseType lines =
    function
    /// Normal style declaration
    | SynUnionCaseType.UnionCaseFields(fields) ->
        fields |> List.map (getUnionCaseField lines)
    /// Full type spec given by 'UnionCase : ty1 * tyN -> rty'. Only used in FSharp.Core, otherwise a warning.
    | SynUnionCaseType.UnionCaseFullType(_) -> []

let getSynUnionCase lines c =
    match c with
    | UnionCase(attributes, id, caseType, xmlDoc, accessibility, r) ->
        let caseTypestring =
            match caseType |> getUnionCaseType lines with
            | [] -> ""
            | cases -> cases |> String.joined " * " |> sp " of %s"
        Node(id.idText + caseTypestring, SynType.DUCase, [], r, c)

let getSynEnumCase c =
    match c with
    | EnumCase(attributes, id, synconst, xmlDoc, r) ->
        Node(id.idText, SynType.DUCase, [], r, c)

let getRecordField lines =
    function
    | SynField.Field(attributes, isStatic, ido, typeName, somebool, xmlDoc, accessiblity, r) ->
        match ido with
        | Some(id) -> id.idText
        | None     -> typeName |> String.fromSynType lines // if we ever encounter a field with no name (how could that be?), we use the typename

let getType lines (SynTypeDefn.TypeDefn(ComponentInfo(_, _, _, lid, xmlDoc, _, _, _), typedefn, members, r) as co) : Node =
    let caption = lid |> String.fromLongIdent
    let markedintf = isMarkedAsInterface xmlDoc

    let createtypenode members =
        let membernodes = members |> List.collect (getMember lines)
        new Node(caption, SynType.Class, membernodes, r, co, markedintf)

    match typedefn with
    | SynTypeDefnRepr.ObjectModel(SynTypeDefnKind.TyconAugmentation, ms, r) -> members |> createtypenode
    | SynTypeDefnRepr.ObjectModel(SynTypeDefnKind.TyconUnion, ms, r) -> members |> createtypenode
    | SynTypeDefnRepr.ObjectModel(SynTypeDefnKind.TyconClass, ms, r) -> ms |> createtypenode
    | SynTypeDefnRepr.ObjectModel(kind, ms, r) -> ms  |> createtypenode
    | SynTypeDefnRepr.Simple(simplrep, _) ->
        match simplrep with
        | SynTypeDefnSimpleRepr.Union(accessiblity, cases, _) ->
            let casenodes = cases |> List.map (getSynUnionCase lines)
            let membernodes = members |> List.collect (getMember lines)
            Node(caption, SynType.DU, casenodes @ membernodes, r, co, markedintf)
        | SynTypeDefnSimpleRepr.Enum(cases, _) ->
            let casenodes = cases |> List.map getSynEnumCase
            Node(caption, SynType.DU, casenodes, r, co, markedintf)
        | SynTypeDefnSimpleRepr.Record(access, fields, r) ->
            let fieldnodes =
                fields
                |> List.map (getRecordField lines)
                |> List.map (fun s -> Node(s, SynType.ValField, [], r, null))
            let membernodes = members |> List.collect (getMember lines)
            new Node(caption, SynType.Class, fieldnodes @ membernodes, r, co, markedintf)
        | _ -> [] |> createtypenode
    | SynTypeDefnRepr.Exception(ex) -> [] |> createtypenode

/// walk over the top declarations of a module.
let rec getDeclarations lines decls : Nodes =
    let getdeclaration d : Nodes =
        match d with
        | SynModuleDecl.Let(isRec, bindings, range) ->
            // Let binding as a declaration is similar to let binding as an expression (in visitExpression), but has no body
            bindings |> List.map (getBinding lines)
        | SynModuleDecl.NestedModule(ComponentInfo(_, _, _, lid, xmlDoc, _, _, _), isrec, decls, x, r) ->
            let caption = lid |> String.fromLongIdent
            let cn = getDeclarations lines decls
            [ Node(caption, SynType.Module, cn, r, d, isMarkedAsInterface xmlDoc) ]
        | SynModuleDecl.Types(types, _) -> types |> List.map (getType lines)
        | _ -> []
    decls |> List.collect getdeclaration

let getModulesAndNamespaces lines modulesOrNss : Nodes =
    let getmoduleorns (SynModuleOrNamespace(lid, isRec, isMod, decls, xml, attrs, _, r) as con) : Node =
        let caption = String.fromLongIdent lid
        let t =
            if isMod.IsModule then SynType.Module
            else SynType.Namespace
        Node(caption, t, (decls |> getDeclarations lines), r, con)
    modulesOrNss |> List.map getmoduleorns

let getUntypedTree (filename : string, text : string) =
    let text = SourceText.ofString(text)
    // get compiler options for the 'project' implied by a single script file
    // filename is used as a key inside the checker - but must be a serious correct filename otherwise this method fails.
    let pro,_ = checker.GetProjectOptionsFromScript(filename, text) |> Async.RunSynchronously
    // run the first phase (untyped parsing) of the compiler
    let po,_ = checker.GetParsingOptionsFromProjectOptions(pro)
    let parseFileResults = checker.ParseFile(filename, text, po) |> Async.RunSynchronously
    match parseFileResults.ParseTree with
    | Some tree -> tree
    | None -> failwith "parser error"

let getModel (filename, text : string) : Node =
    let ast = getUntypedTree (filename, text)
    match ast with
    | ParsedInput.ImplFile(ParsedImplFileInput(filename, isscript, name, fullfilename, _, msns, _)) ->
        let lines = text.Split('\n') |> Seq.toArray
        let cn = getModulesAndNamespaces lines msns
        let totalrange = cn |> List.map (fun n -> n.Range) |> List.reduce Range.unionRanges
        Node((Path.GetFileName filename), SynType.File, cn, totalrange, msns)
    | _ -> Node("parsing failed", SynType.Unknown, [], Range.range.Zero, null)
