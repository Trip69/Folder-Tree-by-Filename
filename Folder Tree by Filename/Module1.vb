Module Module1

    Private video_ext() As String = {".mp4", ".mov", ".gif"}
    Private picture_ext() As String = {".jpg", ".jpeg", ".png", ".JPG"}

    Sub Main()
        Console.WriteLine("Folder Tree by Filename.exe v1.1")
        Console.WriteLine("Utils for auto moving movies into Photos video tree by Trip")
        Console.WriteLine("wwww.somedodgywebsite.com")

        Dim CommandArgs As List(Of String) = My.Application.CommandLineArgs.ToList
#If DEBUG Then
        CommandArgs.Clear()
        'CommandArgs.Add("C:\Users\Chris\Google Drive\Programming\Visual Studio 2012\Projects\Folder Tree by Filename\Folder Tree by Filename\bin\Debug\2014_01_18_125606.mp4")
        CommandArgs.Add("C:\Users\Chris\Google Drive\Google Photos\20180512_163627.jpg")
        'CommandArgs.Add("C:\Users\Chris\Pictures\ToSort\2013-05-21-12-43-50.jpg")
        'CommandArgs.Add("C:\Users\Chris\Pictures\ToSort\2016_12_30_154140.mp4")
#End If
        Dim PhotosPath As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\Pictures"
        Dim CurrentPath = ""
        Dim DestinationDirectoryExists As Boolean = False
        Dim Patterns As New List(Of String)
        Patterns.Add("(\d{4})-*_*(\d{2})-*_*(\d{2})(.*)")
        Patterns.Add("(\d{4})(\d{2})(\d{2})(.*)")
        Dim matches As MatchCollection = Nothing
        Dim FileToSort As FileInfo = Nothing
        Dim Count As Integer = 0
        For Each argument As String In CommandArgs
            Count = Count + 1
            FileToSort = New FileInfo(argument)
            Console.WriteLine("Processing (" + Count.ToString + "/" + CommandArgs.Count.ToString + ") " + FileToSort.FullName)
            For Each pat As String In Patterns
                If Regex.IsMatch(FileToSort.Name, pat) Then
                    matches = Regex.Matches(FileToSort.Name, pat)
                    Dim NewName As String = matches(0).Groups(1).Value + "_" + matches(0).Groups(2).Value + "_" + matches(0).Groups(3).Value + matches(0).Groups(4).Value
                    If DeleteSmallerDuplicate(FileToSort, New FileInfo(NewName)) = (DuplicateResults.NotDuplcaite Or DuplicateResults.DestinationDeleted) Then
                        Try
                            FileToSort.MoveTo(FileToSort.DirectoryName + "\" + NewName)
                        Catch ex As Exception
                            Console.WriteLine("Failed to rename " + FileToSort.Name + " to " + NewName)
                            Continue For
                        End Try
                    End If
                End If
            Next
            If matches Is Nothing Then Continue For
            If matches(0).Groups.Count < 4 Then 'First item is the string to be searched
                matches = Nothing
                Continue For
            End If
            CurrentPath = PhotosPath
            For a = 1 To 3
                CurrentPath = CurrentPath + "\" + matches(0).Groups(a).Value
                If MakeDir(CurrentPath) Then
                    DestinationDirectoryExists = True
                Else
                    Console.WriteLine("Failed making directory " + CurrentPath + " for " + FileToSort.Name)
                    DestinationDirectoryExists = False
                    Exit For
                End If
            Next

            If video_ext.Contains(FileToSort.Extension) Then
                CurrentPath = CurrentPath + "\video"
                If Not MakeDir(CurrentPath) Then
                    Console.WriteLine("Failed making directory " + CurrentPath + " for " + FileToSort.Name)
                    Continue For
                End If
            ElseIf Not picture_ext.Contains(FileToSort.Extension) Then
                Console.WriteLine("Not Video or jpg")
                Continue For
            End If
            If DeleteSmallerDuplicate(FileToSort, New FileInfo(CurrentPath + "\" + FileToSort.Name)) = (DuplicateResults.DestinationDeleted Or DuplicateResults.NotDuplcaite) Then
                Try
                    FileToSort.MoveTo(CurrentPath + "\" + FileToSort.Name)
                    Console.WriteLine("Moved " + FileToSort.Name + " to " + CurrentPath + "\" + FileToSort.Name)
                Catch ex As Exception
                    Console.WriteLine("Failed moving file " + FileToSort.Name)
                    Continue For
                End Try
            End If
        Next
        Console.WriteLine("Press whatever the fuck you want to exit...")
        Console.ReadKey()
    End Sub

    Private Function MakeDir(Path As String) As Boolean
        If Directory.Exists(Path) Then
            Return True
        Else
            Try
                Directory.CreateDirectory(Path)
            Catch ex As Exception
                Return False
            End Try
            Return True
        End If
    End Function

    Private Enum DuplicateResults
        SourceDeleted
        DestinationDeleted
        DeleteFailed
        NotDuplcaite
    End Enum

    Private Function DeleteSmallerDuplicate(Src As FileInfo, Dst As FileInfo) As DuplicateResults
        If Src.FullName = Dst.FullName Or Dst.Exists Then
            If Src.Length > Dst.Length Then
                Try
                    Dst.Delete()
                    Console.WriteLine("Deleted smaller duplicate " + Dst.FullName)
                Catch ex As Exception
                    Console.WriteLine("Failed to delete " + Dst.FullName)
                    Return DuplicateResults.DeleteFailed
                End Try
            ElseIf Src.Length <= Dst.Length Then
                Try
                    Src.Delete()
                    Console.WriteLine("Deleted smaller duplicate " + Src.FullName)
                    Return DuplicateResults.SourceDeleted
                Catch ex As Exception
                    Console.WriteLine("Failed to delete " + Src.FullName)
                    Return DuplicateResults.DeleteFailed
                End Try
            End If
        End If
        Return DuplicateResults.NotDuplcaite
    End Function

End Module
