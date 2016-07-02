Dim dte
Dim project

Set dte = CreateObject("VisualStudio.DTE.11.0")
dte.Solution.Open("C:\Users\imyuorm8\Documents\prototype_tapper\TapServer\TapServer.sln")

For Each project In dte.Solution.Projects
 If project.Name = "ProtoLib" Then
  project.ProjectItems.AddFromFile(WScript.Arguments(0))
  project.Save(project.FullName)
 End If
Next

dte.Solution.Close()