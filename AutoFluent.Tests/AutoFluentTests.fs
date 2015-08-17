﻿namespace AutoFluent.Tests

open System
open System.IO
open System.Reflection

open NUnit.Framework
open FsUnit

open AutoFluent
open Generate

// Test-Types

type TypeWithGenericProperty() = 
    member val Property : System.Action<bool> = null with get, set

type GenericTypeWithProperty<'T>() = 
    member val Property : bool = false with get, set

type GenericTypeWithConstraintAndProperty<'T when 'T :> Exception>() = 
    member val Property : bool = false with get, set

[<AutoOpen>]
module Helper = 
    let loadLines fn = 
        File.ReadAllLines(fn)

[<TestFixture>]
type AutoFluentTests() =

    [<Test>] 
    member this.xamarinForms() = 
        let assemblyToLoad = "Xamarin.Forms.Core" |> Assembly.Load
        let assembly = AutoFluent.propertiesOfAssembly assemblyToLoad
        assembly
        |> Generate.assembly
        |> Generate.sourceLines
        |> Seq.iter System.Console.WriteLine
    
    [<Test>] 
    member this.WPFPresentationCore() = 
        let assemblyToLoad = "PresentationCore" |> Assembly.Load
        let assembly = AutoFluent.propertiesOfAssembly assemblyToLoad
        assembly
        |> Generate.assembly
        |> Generate.sourceLines
        |> Seq.iter System.Console.WriteLine    
        
    [<Test>] 
    member this.WPFPresentationFramework() = 
        let assemblyToLoad = "PresentationFramework" |> Assembly.Load
        let assembly = AutoFluent.propertiesOfAssembly assemblyToLoad
        assembly
        |> Generate.assembly
        |> Generate.sourceLines
        |> Seq.iter System.Console.WriteLine
    
    [<Test>]
    member this.formatInsertsEmptyLineBetweenBlocks() = 
    
        let c = 
            Block [
                Block [Line "a"]
                Block [Line "b"]
            ]

        let formatted = Generate.format c
        formatted |> should equal (Block [Block[Line "a"]; Line ""; Block[Line "b"]])

    [<Test>]
    member this.canHandleGenericProperties() =
        let t = typeof<TypeWithGenericProperty>
        let fluent = AutoFluent.propertiesOfType t
        let code = Generate.typeProperties fluent
        let code = Generate.sourceLines code
        let file = loadLines "TypeWithGenericProperty.cs"
        code |> should equal file
        
    [<Test>]
    member this.canHandlePropertyInGenericType() = 
        let t = typedefof<GenericTypeWithProperty<_>>
        let fluent = AutoFluent.propertiesOfType t
        let code = Generate.typeProperties fluent
        let code = Generate.sourceLines code
        let file = loadLines "GenericTypeWithProperty.cs"
        code |> should equal file

    [<Test>]
    member this.canHandlePropertyInGenericTypeWithConstraints() = 
        let t = typedefof<GenericTypeWithConstraintAndProperty<_>>
        let fluent = AutoFluent.propertiesOfType t
        let code = Generate.typeProperties fluent
        let code = Generate.sourceLines code
        let file = loadLines "GenericTypeWithConstraintAndProperty.cs"
        code |> should equal file
