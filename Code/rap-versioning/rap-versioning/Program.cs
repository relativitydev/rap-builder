using System;
using System.IO;
using System.Xml;

namespace rap_versioning
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string returnValue = String.Empty;
				string path = args[1];
				//need checks for valid path

				if (args[0].ToLower() == "get")
				{
					returnValue = GetVersion(path);
				}
				else if (args[0].ToLower() == "increment")
				{
					returnValue = IncrementVersion(path);
				}
				else if (args[0].ToLower() == "addref")
				{
					returnValue = AddAssemblies(path);
				}

				Console.WriteLine(returnValue);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}

		//TODO: Do not open document multiple times, thought I needed to because we were going to multiple times with different command options
		//Should figure out a way to allow option specification for one run only and reduce open and close.
		static string GetVersion(string fileName)
		{
			//should be second parameter
			string buildFile = fileName.Replace("application.xml", "build.xml");
			string returnValue = String.Empty;
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);
			XmlNode versionNode = doc.SelectSingleNode("/Application/Version");
			string versionNumber = (versionNode.InnerText).Trim();
			return versionNumber;
		}

		static string IncrementVersion(string fileName)
		{
			string versionNumber = GetVersion(fileName);
			Version version = new Version(versionNumber);
			int revisionNumber = version.Revision;
			revisionNumber++;
			string newVersionNumber = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.MajorRevision.ToString() + "." + revisionNumber.ToString();

			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);
			doc.SelectSingleNode("/Application/Version").InnerText = newVersionNumber;
			doc.Save(fileName);
			AddAssemblies(fileName);
			AddCustomPages(fileName);
			return newVersionNumber;
		}

		static string AddAssemblies(string fileName)
		{
			XmlDocument applicationXML = new XmlDocument();
			applicationXML.Load(fileName);
			XmlNodeList assemblyNodes = applicationXML.SelectNodes("/Application/Assemblies/Assembly/Name");

			XmlDocument buildXML = new XmlDocument();
			string buildXMLPath = fileName.Replace("application.xml", "build.xml");

			buildXML.Load(buildXMLPath);

			XmlNodeList buildAssemblyNodesList = buildXML.GetElementsByTagName(@"Assembly");
			XmlNodeList buildAssembliesNodeList = buildXML.GetElementsByTagName(@"Assemblies");
			XmlNode buildAssembliesNode = buildAssembliesNodeList[0];
			if (assemblyNodes != null)
			{
				//remove existing children
				buildAssembliesNode.InnerXml = "";

				//Test for Null
				foreach (XmlNode assemblyNode in assemblyNodes)
				{
					string assemblyName = assemblyNode.InnerText;
					string projPath = @"bin\" + assemblyName;
					XmlNode newAssemblyNode = buildXML.CreateElement("Assembly");
					newAssemblyNode.InnerText = projPath;
					buildAssembliesNode.AppendChild(newAssemblyNode);
				}

				buildXML.Save(buildXMLPath);
			}

			return "something";
		}

		static string AddCustomPages(string fileName)
		{
			NameTable nt = new NameTable();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
			nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003"); //default namespace

			XmlDocument applicationXML = new XmlDocument();
			applicationXML.Load(fileName);
			XmlNodeList applicationCustomPageNodes = applicationXML.SelectNodes("/Application/CustomPages");



			XmlDocument buildXML = new XmlDocument();
			string buildXMLPath = fileName.Replace("application.xml", "build.xml");
			buildXML.Load(buildXMLPath);

			XmlDocument rapBuilderCsprojFile = new XmlDocument();
			string rapBuilderCsprojFileName = Path.GetDirectoryName(fileName) + @"\" + Path.GetFileName(Path.GetDirectoryName(fileName)) + ".csproj";
			rapBuilderCsprojFile.Load(rapBuilderCsprojFileName);
			string rapBuilderOutputRelativityDirectory = rapBuilderCsprojFile.SelectSingleNode(@"ns:Project/ns:PropertyGroup/ns:OutputPath", nsmgr).InnerText;
			string rapBuilderOutputFullDirectory = Path.Combine(Path.GetDirectoryName(fileName), rapBuilderOutputRelativityDirectory);

			//XmlNodeList customPageNodesList = buildXML.GetElementsByTagName(@"CustomPage");
			XmlNodeList customPagesNodeList = buildXML.GetElementsByTagName(@"CustomPages");
			XmlNode customPagesNode = customPagesNodeList[0];

			string customPageZipFileName;
			string customPageGuid;
			string customPageVersion;
			string customPageProjectName;
			string customPageProjectLocation;
			string customPageProjectLocationPath;
			string publishingProfileName;
			string publishProfilePath;
			string customPageOutputLocation;
			string rapBuilderOutputPathForCustomPage;

			XmlNode customPageProjectReferenceNode;

			if (applicationCustomPageNodes != null)
			{
				//remove existing children
				customPagesNode.InnerXml = "";

				//Test for Null
				foreach (XmlNode customPage in applicationCustomPageNodes[0].SelectNodes("CustomPage"))
				{
					customPageGuid = customPage.SelectSingleNode("Guid").InnerText;
					//todo: increment and save it back to the application.xml
					customPageVersion = customPage.SelectSingleNode("ApplicationVersion").InnerText;
					customPageProjectName = customPage.SelectSingleNode("Name").InnerText;

					customPageProjectReferenceNode = rapBuilderCsprojFile.SelectSingleNode(@"ns:Project/ns:ItemGroup/ns:ProjectReference[ns:Name[text()='" + customPageProjectName + @"']]", nsmgr);
					customPageProjectLocation = customPageProjectReferenceNode.Attributes["Include"].Value;
					customPageProjectLocationPath = Path.GetFullPath(Path.GetFullPath(fileName) + @"\..\.." + customPageProjectLocation);

					XmlDocument customPageProjectCsprojFileXML = new XmlDocument();
					customPageProjectCsprojFileXML.Load(customPageProjectLocationPath);
					publishingProfileName = customPageProjectCsprojFileXML.SelectSingleNode(@"/ns:Project/ns:PropertyGroup/ns:AutoDeployPublishProfileName", nsmgr).InnerText;

					publishProfilePath = Path.GetDirectoryName(customPageProjectLocationPath) + @"\Properties\PublishProfiles\" + publishingProfileName + @".pubxml";
					XmlDocument publishProfileXML = new XmlDocument();
					publishProfileXML.Load(publishProfilePath);
					customPageOutputLocation = publishProfileXML.SelectSingleNode(@"//ns:Project/ns:PropertyGroup/ns:publishUrl", nsmgr).InnerText;
					customPageOutputLocation = Path.Combine(Path.GetDirectoryName(customPageProjectLocationPath), customPageOutputLocation);
					rapBuilderOutputPathForCustomPage = Path.Combine(rapBuilderOutputFullDirectory, customPageProjectName);

					//consider wiping out all files in rapBuilderOutputPathForCustomPage first
					DirectoryCopy(customPageOutputLocation, rapBuilderOutputPathForCustomPage, true);

					XmlElement newAssemblyNode = buildXML.CreateElement("CustomPage");
					newAssemblyNode.SetAttribute("guid", customPageGuid);
					newAssemblyNode.InnerText = Path.Combine(rapBuilderOutputRelativityDirectory, customPageProjectName);
					customPagesNode.AppendChild(newAssemblyNode);
				}

				buildXML.Save(buildXMLPath);
			}

			return "something";
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
	}

}
