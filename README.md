# F# File Structure Toolwindow for Visual Studio
Visual Studio Extension for VS 2019 that provides a File Structure Toolwindow for F#

Rider has it, Ionide has it, now Visual Studio has it too.

Available at https://marketplace.visualstudio.com/items?itemName=snuup.FSharpFileStructure or inside the VS extension manager.

![preview](https://github.com/snuup/FSharp-File-Structure-for-Visual-Studio/blob/master/preview.PNG)

The project holds 3 assemblies in 3 projects:

### FSharpFileAst (F#)
An F# project that computes the abstract syntax tree (AST) from a file and converts it into a TreeModel, which is another tree acting as the model of the XAML control that displays the file structure.

### LabApp (C#)
This project references the user interface code inside VSIXSharpStruct and hosts the file structure control as a simple XAML App. This is useful when working and debugging the ui part of the toolwindow.

### VSIXFSharpStruct (C#)
The Visual Studio extension providing the toolwindow.

### The tricky thing
Visual Studio extensions must be signed, including all referenced assemblies. Signing FSharpFileAst and VSIXSharpStruct is simple.
The problem is that FSharp.Compiler.Service.dll assembly comes unsigned. To get around that, I converted the assembly with ilasm as can be seen in tools/sign.bat. The signed assembly is in the lib folder together with the dependencies of FSharp.Compiler among which only the System.Buffers assembly is really needed. Instead of nuget packages, the projects refernce those assembly files so they get the signed versions.
