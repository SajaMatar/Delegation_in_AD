using System;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

public partial class Upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Get the authenticated user's identity
            WindowsIdentity userIdentity = (WindowsIdentity)HttpContext.Current.User.Identity;

            // Determine the department based on the user's group membership
            string department = GetUserDepartment(userIdentity);

            // Redirect admin users to the admin panel
            if (department == "Admins")
            {
                Response.Redirect("AdminPanel.aspx");
            }
            else
            {
                LoadFiles();
            }
        }
    }


    protected void UploadButton_Click(object sender, EventArgs e)
    {
        if (FileUploadControl.HasFile)
        {
            // Check file size (10 MB limit)
            if (FileUploadControl.PostedFile.ContentLength > 10 * 1024 * 1024)
            {
                StatusLabel.Text = "File size exceeds the limit of 10 MB.";
                return;
            }

            // Declare networkPath outside the try block to make it accessible in the catch block
            string networkPath = null;

            try
            {
                // Get the authenticated user's identity
                WindowsIdentity userIdentity = (WindowsIdentity)HttpContext.Current.User.Identity;

                // Determine the department based on the user's group membership
                string department = GetUserDepartment(userIdentity);

                if (string.IsNullOrEmpty(department))
                {
                    StatusLabel.Text = "You do not have permission to upload files.";
                    return;
                }

                // Impersonate the user
                using (userIdentity.Impersonate())
                {
                    // Specify the network path based on the department
                    networkPath = string.Format(@"\\FS\shares\{0}", department);

                    // Ensure the network path exists
                    if (!Directory.Exists(networkPath))
                    {
                        StatusLabel.Text = "Network path does not exist: " + networkPath;
                        return;
                    }

                    // Get the file name
                    string fileName = Path.GetFileName(FileUploadControl.FileName);

                    // Combine the network path and file name
                    string fullPath = Path.Combine(networkPath, fileName);

                    // Save the file
                    FileUploadControl.SaveAs(fullPath);

                    StatusLabel.Text = "File uploaded successfully!";
                    LoadFiles(); // Refresh the file list
                }
            }
            catch (Exception ex)
            {
                // Include the network path in the error message
                StatusLabel.Text = "Error accessing " + networkPath + ": " + ex.Message;
            }
        }
        else
        {
            StatusLabel.Text = "Please select a file to upload.";
        }
    }


    private void LoadFiles()
    {
        try
        {
            // Get the authenticated user's identity
            WindowsIdentity userIdentity = (WindowsIdentity)HttpContext.Current.User.Identity;

            // Determine the department based on the user's group membership
            string department = GetUserDepartment(userIdentity);

            if (string.IsNullOrEmpty(department))
            {
                StatusLabel.Text = "You do not have permission to view files.";
                return;
            }

            // Impersonate the user
            using (userIdentity.Impersonate())
            {
                // Specify the network path based on the department
                string networkPath = string.Format(@"\\FS\shares\{0}", department);

                // Ensure the network path exists
                if (!Directory.Exists(networkPath))
                {
                    StatusLabel.Text = "Network path does not exist: " + networkPath;
                    return;
                }

                // Get the list of files in the directory
                var files = Directory.GetFiles(networkPath).Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    FilePath = f
                }).ToList();

                // Bind the files to the GridView
                FilesGrid.DataSource = files;
                FilesGrid.DataBind();
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error loading files: " + ex.Message;
        }
    }


    protected void FilesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Download")
        {
            string filePath = e.CommandArgument.ToString();
            DownloadFile(filePath);
        }
    }


    protected void FilesGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        try
        {
            // Log RowIndex and DataKeys count
            StatusLabel.Text = string.Format("RowIndex: {0}, DataKeys Count: {1}", e.RowIndex, FilesGrid.DataKeys.Count);

            // Validate RowIndex
            if (e.RowIndex < 0 || e.RowIndex >= FilesGrid.DataKeys.Count)
            {
                StatusLabel.Text = "Invalid row index.";
                return;
            }

            // Get the file path from the DataKeys collection
            string filePath = FilesGrid.DataKeys[e.RowIndex].Value.ToString();

            // Ensure the file path is not null or empty
            if (string.IsNullOrEmpty(filePath))
            {
                StatusLabel.Text = "File path is invalid.";
                return;
            }

            // Delete the file
            DeleteFile(filePath);

            // Refresh the file list
            LoadFiles();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error deleting file: " + ex.Message;
        }
    }


    private void DownloadFile(string filePath)
    {
        try
        {
            // Impersonate the user
            WindowsIdentity userIdentity = (WindowsIdentity)HttpContext.Current.User.Identity;
            using (userIdentity.Impersonate())
            {
                // Ensure the file exists
                if (File.Exists(filePath))
                {
                    // Set the response headers for the download
                    Response.Clear();
                    Response.ContentType = "application/octet-stream";
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
                    Response.WriteFile(filePath); // Use WriteFile instead of TransmitFile
                    Response.Flush(); // Flush the response
                    Response.End(); // Terminate the response
                }
                else
                {
                    StatusLabel.Text = "File not found: " + filePath;
                }
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error downloading file: " + ex.Message;
        }
    }


    private void DeleteFile(string filePath)
    {
        try
        {
            // Impersonate the user
            WindowsIdentity userIdentity = (WindowsIdentity)HttpContext.Current.User.Identity;
            using (userIdentity.Impersonate())
            {
                // Ensure the file exists
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    StatusLabel.Text = "File deleted successfully!";
                }
                else
                {
                    StatusLabel.Text = "File not found: " + filePath;
                }
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error deleting file: " + ex.Message;
        }
    }


    private string GetUserDepartment(WindowsIdentity userIdentity)
    {
        // Define the security groups and their corresponding departments
        var departmentGroups = new Dictionary<string, string>
        {
            { "HR", "HR" },
            { "IT", "IT" },
            { "Marketing", "Marketing" },
            { "Admins", "Admins" } // Add Admins group
        };

        // Check which group the user belongs to
        foreach (var group in userIdentity.Groups)
        {
            // Translate the group to an NTAccount and get its value
            var groupName = group.Translate(typeof(NTAccount)).Value;

            // Split the group name by backslash and handle cases where there is no domain
            var parts = groupName.Split('\\');
            if (parts.Length > 1)
            {
                groupName = parts[1]; // Take the group name after the domain
            }
            else
            {
                groupName = parts[0]; // Use the entire name if there is no domain
            }

            // Check if the group name matches a department
            if (departmentGroups.ContainsKey(groupName))
            {
                return departmentGroups[groupName];
            }
        }

        return null;
    }
}