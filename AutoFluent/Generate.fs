﻿namespace AutoFluent

open AutoFluent

open System
open System.Reflection

module Generate =

    [<AutoOpen>]
    module Helper = 
        let returnType (t: Type) = 
            t.FullName

        let parameter (t: Type) name = 
            sprintf "%s %s" t.FullName name

        type Code = 
            | Scope of Code
            | Block of Code list
            | Line of string

        type Block = Code list

        let rec toCode (o : obj) = 
            match o with
            | :? string as s -> Line s
            | :? (obj list) as objs -> objs |> List.map toCode |> Block |> Scope
            | :? (string list) as block -> block |> List.map Line |> Block |> Scope
            | :? (Code list) as block -> block |> Block |> Scope
            | _ -> failwithf "invalid type %s in code" (o.GetType().Name)

        let block (block: obj list) =
            block
            |> List.map toCode
            |> Block

        let scope (nested: obj list) =
            nested

        let staticClass (name: string) (blocks: Code list) =
            block [
                sprintf "public static class %s" name
                [ 
                    yield! blocks 
                ]
            ]

    let fluentPropertyExtensionMethod (t: Type) (property: PropertyInfo) = 
        block [
            sprintf "public static %s %s(this %s, %s)" (returnType t) property.Name (parameter t "self") (parameter property.PropertyType  "value")
            [ 
                sprintf "self.%s = value;" property.Name
                sprintf "return self;"
            ]
        ]
    
    let typeProperties (properties: FluentTypeProperties) =
        let methods = 
            properties.properties
            |> List.map (fluentPropertyExtensionMethod properties.t)

        staticClass (properties.t.Name + "FluentProperties") methods

    let assembly (assembly: FluentAssembly) =
        
        let mkNamespace (name: string) (types: FluentTypeProperties list) = 
            let generatedClasses = 
                types 
                |> List.map typeProperties

            block [
                sprintf "namespace %s" name
                [
                    yield! generatedClasses
                ]
            ]
        
        assembly.types
        |> List.groupBy (fun tp -> tp.t.Namespace)
        |> List.map (fun (ns, tp) -> mkNamespace ns tp)
        |> Block

    let code (c: Code) =
        
        let rec lines indent (c: Code) =
            match c with
            | Line l -> Seq.singleton (indent + l)
            | Block block -> 
                block
                |> Seq.map (lines indent)
                |> Seq.collect id
            | Scope scope ->
                let l = 
                    scope |> lines (indent + "\t")

                seq {
                    yield indent + "{"
                    yield! l
                    yield indent + "}"
                }

        let lines = lines "" c
        String.Join("\n", lines)
        

 

