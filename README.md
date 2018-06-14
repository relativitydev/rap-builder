# rap-builder
Alpha version of a Hackathon project aimed at allowing the creation of RAP files within the Visual Studio build. At this time there is no support for this project. If you are interested, reach out to the Developer Experience team at Relativity.

The rap-builder project uses the sample-application project to test with which can be cloned here: git@github.com:relativitydev/sample-application.git .

When set up properly this will create a rap file for a custom solution that contains applicaiton schema, event handlers, and agents.  Other types of extensibility points may work but have not been tested.  If you find one that does not work, please create an issue and feel free to jump in and fix. 

**Note:** We are not open-sourcing the source code for kCura.RAPBuilder.exe

## Sample Usage (I have gone through this once.  Will repeat and update)
(Using visual studio 2017. If this works for you in 2012 or 2015 let us know)
* Install **RapCreator** Visual Studio Template
	* Go to **Tools** --> **Extensions & Updates**
  		* Search for **Relativity**
  		* Select **RapCreator**
  		* Select **Download**
    		* You will have to restart Visual Studio to finish the install.
			* Make sure the VSIX Installer window is around (it likes to hide) and follow the prompt to **Modify**

[Add Screen shot of template install
 
* Open Sample Application 
	* Clone the **sample-application** project on teh RelativityDev github account
		* git clone git@github.com:relativitydev/sample-application.git
	* Open **Source\BasicSampleApplication.sln** in Visual Studio
		* There should be 3 projects (Can you guess what they do by their name?  If not, you should probably reach out to the Developer Experience team @ Relativity before you attempt this example.)
			* SampleAgent
			* SampleCustomPage
			* SamplePreSaveEventHandler
[Add Solution Explorer Screenshot]
	
* Add **BasicSampleApplication** RapCreator Project
	* Right-Click the **BasicSampleApplication** solution in Solution Explorer
	* Select **Add** --> **New Project**
	* Select **RapCreator** template
		* You should find the template at the following location
			* Installed --> Visual C# --> Relativity --> RAP
		* Name the project something like **SampleAppBuiler**
		* Add References for other project in the **SampleAppBuiler** app
			* Right-Click **References**, Select **Add References**
			* Select the follow projects and select **OK**
				* SampleAgent
				* SampleCustomPage
				* SamplePreSaveEventHandler
		* Right click **SampleAppBuilder**, Select **Manage Nuget Packages**
			* Click **Restore**
	* Add **FolderProfile** publishing profile for **SampleCustomPage** project if it does not exist
		* Should be a folder publishing profle

* Update application.xml
	* under the **sample-application** folder, open the **application** folder
	* Copy the contents of the **application.xml** file and paste it into the **application.xml** file found in the **SampleAppBuilder** project you created in the previous steps.  The paste should replace all text in the xml file.
	
* Build **SampleAppBuilder** project

* Deploy *SammpleAppBuilder**
	* Find *sample-application\Source\SampleAppBuilder\bin\SampleAppBuilder.rap*
	* Install to Relativity
	* Install to a workspace
	
* View the Sample Custom Page in a workspace

* Make Change
	* Update the string returned in the **HomeController.cs** file of the SampleCustomPage project
	* Clear out Bin directory of the **SampleAppBuilder** project.
	* Build **SampleAppBuilder**
	* Update Application in Relativity with newly created rap file.
	
##Notes
* The custom page name in the application.xml must be same name as custom page project in Visual Studio
* Add following code to Custom Page csproj file
```
        <PropertyGroup>
        <AutoDeployOnVSBuild>true</AutoDeployOnVSBuild>
        <!--<AutoDeployOnVSBuild>false</AutoDeployOnVSBuild>-->
        <AutoDeployPublishProfileName>FolderProfile</AutoDeployPublishProfileName>
        </PropertyGroup>
 
	<Target Name="AfterBuild">
        <MSBuild Condition="'$(AutoDeployOnVSBuild)'=='true' AND '$(DeployOnBuild)'!='true'" Projects="$(MSBuildProjectFullPath)" Properties="DeployOnBuild=true;PublishProfile=$(AutoDeployPublishProfileName);BuildingInsideVisualStudio=False" />
        </Target>
	
		
				
		
