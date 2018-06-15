# There are several things that need to change in this script to make it functional in your environment

* Every instance of Add-Type -Path "S:\PS1DeployScript\kCura.Relativity.Client.dll" needs to point to a valid Client.dll for your target instance
  * This should be parameterized
  * Should be refactored so there is only one instance of this
* **USER CONFIG SECTION** needs to be updated with valid values for your target instance.
