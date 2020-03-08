# F# File Structure Toolwindow for Visual Studio
Visual Studio Extension for VS 2019 that provides a File Structure Toolwindow for F#

![preview](https://github.com/snuup/FSharp-File-Structure-for-Visual-Studio/blob/master/preview.PNG)
![preview-dark](https://github.com/snuup/FSharp-File-Structure-for-Visual-Studio/blob/master/preview-dark.PNG)

An extension for Visual Studio 2019. Provides a Toolwindow showing the outline or file structure of a F# code file.

The project holds 3 assemblies in 3 projects:

## FSharpFileAst (F#)
An f# project that computes the abstract syntax tree (AST) from a file and converts it into a TreeModel, that is another tree used to display the file structure.

## LabApp (C#)
This project references the UI code inside VSIXSharpStruct and hosts the file structure control as a simple XAML App. This is useful whe working on the ui part 
of the toolwindow.

## VSIXFSharpStruct (C#)
The Visual Studio extension providing the toolwindow.

## Tricky things: Signing
Visual Studio extensions must be signed, including there referenced assemblies. Therefore FSharpFileAst and VSIXSharpStruct are signed.
The problem is that FSharp.Compiler.Service.dll assembly comes unsigned. The get around that, I converted the assembly with ilasm as can be seen in tools/sign.bat.
The signed assembly is in the lib folder together with dependencies of FSharp.Compiler among which only the System.Buffers assembly is really needed. 
Instead of nuget packages the projects refernce those assembly files so they get the signed versions.
