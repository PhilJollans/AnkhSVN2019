' VB Script Document
option explicit

Const ForReading = 1
Const BaseOutputDirectory = "C:\AnkhSVN2019Builds"

'-------------------------------------------------------------------------------
'Global variables
'-------------------------------------------------------------------------------

'Utility objects
Dim fso
Dim TextStream
Dim FileText
Dim re
Dim WshShell

'Values to patch into the files 
Dim Build_Version
Dim Assembly_Version
Dim Vsix_Version
Dim RootDir

Dim OutputDirectory
Dim ExitCode

'-------------------------------------------------------------------------------
'Function: PatchAssemblyFileVersion
'-------------------------------------------------------------------------------
Private Sub PatchAssemblyFileVersion ( ConfigFile, AssemblyVersion )

  Set TextStream  = fso.OpenTextFile ( ConfigFile, ForReading )
  FileText = TextStream.ReadAll
  TextStream.Close
  Set TextStream = Nothing
         
  re.Pattern = "AssemblyFileVersion\s*\(\s*""[^""]*""\s*\)"
  FileText = re.Replace ( FileText, "AssemblyFileVersion(""" & AssemblyVersion & """)" )

  re.Pattern = "AssemblyVersion\s*\(\s*""[^""]*""\s*\)"
  FileText = re.Replace ( FileText, "AssemblyVersion(""" & AssemblyVersion & """)" )

  set TextStream = fso.CreateTextFile ( ConfigFile, True, False )
  TextStream.Write FileText
  TextStream.Close
  Set TextStream = Nothing

End Sub

'-------------------------------------------------------------------------------
'Function: PatchVsixManifest
'-------------------------------------------------------------------------------
Private Sub PatchVsixManifest ( ManifestFile, AssemblyVersion )
                            
  Set TextStream  = fso.OpenTextFile ( ManifestFile, ForReading )
  FileText = TextStream.ReadAll
  TextStream.Close
  Set TextStream = Nothing
         
  're.Pattern = "<Version>.*</Version>" 
  'FileText = re.Replace ( FileText, "<Version>" & AssemblyVersion & "</Version>" )

  'This assumes the exact format <Identity Version="6.1" ...
  '(i.e. Version is the first attribute 
  re.Pattern = "Identity Version=""[^""]*""" 
  FileText = re.Replace ( FileText, "Identity Version=""" & AssemblyVersion & """" )

  set TextStream = fso.CreateTextFile ( ManifestFile, True, False )
  TextStream.Write FileText
  TextStream.Close
  Set TextStream = Nothing

End Sub

'-------------------------------------------------------------------------------
'Function: PatchPackageDefinition
'-------------------------------------------------------------------------------
Private Sub PatchPackageDefinition ( PackageSourceFile, AssemblyVersion )

  Set TextStream  = fso.OpenTextFile ( PackageSourceFile, ForReading )
  FileText = TextStream.ReadAll
  TextStream.Close
  Set TextStream = Nothing
         
  Dim matches
  Dim SubMatch
  Dim FullMatch
  Dim NewText

  re.Pattern = "InstalledProductRegistration(\s*\(\s*""[^""]*""\s*,\s*""[^""]*""\s*,\s*"")[^""]*" 
  set matches = re.execute ( FileText )
  
  'msgbox matches.Count
  'msgbox matches(0)
  'msgbox matches(0).Submatches(0)
  
  FullMatch = matches(0)
  SubMatch  = matches(0).Submatches(0)  
  NewText   = "InstalledProductRegistration" & SubMatch & AssemblyVersion 

  FileText = Replace ( FileText, FullMatch, NewText )

  set TextStream = fso.CreateTextFile ( PackageSourceFile, True, False )
  TextStream.Write FileText
  TextStream.Close
  Set TextStream = Nothing

End Sub

'-------------------------------------------------------------------------------
'Function: BuildSolution
'-------------------------------------------------------------------------------
Private Sub BuildSolution ( SolutionFile )

  Dim MSBuildCommand
  
  MSBuildCommand = """C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"" " & SolutionFile & " /t:Build /p:Configuration=Release /p:Platform=x86 /p:TargetFramework=v4.7.2 /fl /flp:logfile=AnkhSVN2019Build.log /nodeReuse:false"
  
  ExitCode = WshShell.Run ( MSBuildCommand, 1, True )
  If ExitCode <> 0 Then
    MsgBox "Build error in " & SolutionFile
    WScript.Quit
  End if  
  
End Sub

'===============================================================================
'MAIN CODE
'===============================================================================

'-------------------------------------------------------------------------------
'Get some objects
'-------------------------------------------------------------------------------
set re = New RegExp
Set fso = CreateObject("Scripting.FileSystemObject")
Set WshShell = WScript.CreateObject("WScript.Shell")

'-------------------------------------------------------------------------------
'Get some values for patching
'-------------------------------------------------------------------------------

Build_Version     = InputBox ( "Enter the build version" )
Build_Version     = Right ( "0000" & Build_Version, 4 )
Assembly_Version  = "1.00.0." & Build_Version
Vsix_Version      = "1.00." & Build_Version      

RootDir = fso.GetParentFolderName(WScript.ScriptFullName)
'MsgBox RootDir

'-------------------------------------------------------------------------------
'Create the output directory
'-------------------------------------------------------------------------------
OutputDirectory = BaseOutputDirectory & "\1_00_" & Build_Version & "\"
if Not fso.FolderExists ( OutputDirectory ) Then
  fso.CreateFolder OutputDirectory 
End If

'Patch the assembly config files
PatchAssemblyFileVersion "src\Ankh.Diff\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.ExtensionPoints\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.GitScc\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.Ids\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.ImageCatalog\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.ImageCatalogTest\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.Package\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.Scc\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.Services\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.Tests\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.UI\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS.IntegrationTest\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS.Interop\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS.UnitTest\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS.VersionThunk\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS.WpfServices\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.VS\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh.WpfUI\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\Ankh\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\TestUtils\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\tools\Ankh.Chocolatey\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\tools\Ankh.DotNet2Test\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\tools\Ankh.GenerateVSIXManifest\Properties\AssemblyInfo.cs", Assembly_Version
PatchAssemblyFileVersion "src\tools\Ankh.GenPkgDef\Properties\AssemblyInfo.cs", Assembly_Version

'Patch the version in the vsixmanifest file
PatchVsixManifest "src\Ankh.Package\source.extension.VsixManifest", Vsix_Version 
                                                                                                              
'Patch the version in the package defintion file for the visual studio about dialog
PatchPackageDefinition "src\Ankh.Package\AnkhSvnPackage.About.cs", Assembly_Version

'Compile the projects
BuildSolution "src\AnkhSvn.sln" 

'Copy output file
fso.CopyFile "src\Ankh.Package\bin\Release\AnkhSVN2019.vsix", OutputDirectory 

MsgBox "Done"
 