using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
					returnValue = AddRefrences(path);
				}

				Console.WriteLine(returnValue);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}

		//TODO: Do not open document twice, thought I needed to because we were going to run twice.
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
			string newVersionNumber = version.Major.ToString() + "."  + version.Minor.ToString() + "." + version.MajorRevision.ToString() + "." + revisionNumber.ToString();
	
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);
			doc.SelectSingleNode("/Application/Version").InnerText = newVersionNumber;
			doc.Save(fileName);
			AddRefrences(fileName);
			return newVersionNumber;
		}

		static string AddRefrences(string fileName)
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
	}
}
