REM del .\Signed\FSharp.Compiler.Service.dll /F
ildasm.exe .\FSharp.Compiler.Service.dll /out:m:\FSharp.Compiler.Service.il
ilasm.exe m:\FSharp.Compiler.Service.il /dll /key=snuup.snk /output=m:\FSharp.Compiler.Service.s.dll
pause