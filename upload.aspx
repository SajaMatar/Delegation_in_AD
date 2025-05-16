<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Upload.aspx.cs" Inherits="Upload" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>File Upload - University of Jordan</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            width: 80%;
            margin: 50px auto;
            background-color: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }
        h1, h2 {
            color: #006400; /* Dark green */
        }
        .status-label {
            color: #d9534f; /* Red for error messages */
            font-weight: bold;
        }
        .file-upload {
            margin-bottom: 20px;
        }
        .file-upload input[type="file"] {
            margin-bottom: 10px;
        }
        .grid-view {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        .grid-view th, .grid-view td {
            padding: 10px;
            border: 1px solid #ddd;
            text-align: left;
        }
        .grid-view th {
            background-color: #006400; /* Dark green */
            color: #fff;
        }
        .grid-view tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        .grid-view tr:hover {
            background-color: #f1f1f1;
        }
        .button {
            background-color: #006400; /* Dark green */
            color: #fff;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
        }
        .button:hover {
            background-color: #004d00; /* Darker green on hover */
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h1>Upload File to Your Department Share</h1>
            <div class="file-upload">
                <asp:FileUpload ID="FileUploadControl" runat="server" CssClass="file-upload-control" />
                <br /><br />
                <asp:Button ID="UploadButton" runat="server" Text="Upload" OnClick="UploadButton_Click" CssClass="button" />
            </div>
            <br />
            <asp:Label ID="StatusLabel" runat="server" Text="" CssClass="status-label"></asp:Label>
            <br /><br />

            <h2>Files in Your Department Share</h2>
            <asp:GridView ID="FilesGrid" runat="server" AutoGenerateColumns="false" OnRowCommand="FilesGrid_RowCommand" OnRowDeleting="FilesGrid_RowDeleting" DataKeyNames="FilePath" CssClass="grid-view">
                <Columns>
                    <asp:BoundField DataField="FileName" HeaderText="File Name" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="DownloadButton" runat="server" Text="Download" CommandName="Download" CommandArgument='<%# Eval("FilePath") %>' CssClass="button" />
                            <asp:Button ID="DeleteButton" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("FilePath") %>' OnClientClick="return confirm('Are you sure you want to delete this file?');" CssClass="button" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>