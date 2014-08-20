rmdir /S /Q bin
rmdir /S /Q obj

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild SimpleInjector.Advanced.csproj /t:Build /p:Configuration="Release net45" /p:Platform="AnyCPU"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild SimpleInjector.Advanced.csproj /t:Build /p:Configuration="Release net40" /p:Platform="AnyCPU"

nuget pack SimpleInjector.Advanced.csproj -Prop Configuration="Release net45" -Prop Platform=AnyCPU
nuget pack SimpleInjector.Advanced.csproj -Prop Configuration="Release net45" -Prop Platform=AnyCPU -Symbols

pause