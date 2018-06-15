
function GetRsapiClient([string] $relativityServicesUrl, [string] $username, [string] $password) {
  Write-Host "Creating Rsapi Client"
  $retVal = $NULL
  Add-Type -Path "S:\PS1DeployScript\kCura.Relativity.Client.dll"
  $credentials = New-Object -TypeName kCura.Relativity.Client.UsernamePasswordCredentials -ArgumentList $username, $password
  $clientSettings = New-Object -TypeName kCura.Relativity.Client.RSAPIClientSettings
  $retVal = New-Object -TypeName kCura.Relativity.Client.RSAPIClient -ArgumentList $relativityServicesUrl, $credentials, $clientSettings
  Write-Host "New Rsapi Client Created"

  $retVal
}
    
function InstallApplication([kCura.Relativity.Client.RSAPIClient] $rsapiClient, [int] $workspaceArtifactID, [string] $rapFileLocation) {
  Write-Host "Installing Application in Workspace $($workspaceArtifactID)"
  $retVal = $FALSE
  Add-Type -Path "S:\PS1DeployScript\kCura.Relativity.Client.dll"
  $app = New-Object -TypeName kCura.Relativity.Client.AppInstallRequest -Property @{
    'FullFilePath' = $rapFileLocation
  }

  $rsapiClient.APIOptions.WorkspaceID = $workspaceArtifactID

  $installationRequest = $rsapiClient.InstallApplication($rsapiClient.APIOptions, $app)
  Write-Host "Application installed....Checking Status..."
  if ($installationRequest.Success) {
    $info = $rsapiClient.GetProcessState($rsapiClient.APIOptions, $installationRequest.ProcessID)
    $iteration = 0
    while ($info.State -eq [kCura.Relativity.Client.ProcessStateValue]::Running) {
      # Sleeping for 5 secs for the application installation to complete
      Start-Sleep -s 5
      $info = $rsapiClient.GetProcessState($rsapiClient.APIOptions, $info.ProcessID)

      if ($iteration -gt 90) {
        Write-Error "Application Install creation timed out"
        Return
      }
      else
      {
        Write-Host "Checking Status..."
      }
      $iteration++
    }

    if ($info.Success -and $info.State -ne [kCura.Relativity.Client.ProcessStateValue]::HandledException -and $info.State -ne [kCura.Relativity.Client.ProcessStateValue]::UnhandledException -and $info.State -ne [kCura.Relativity.Client.ProcessStateValue]::CompletedWithError) {
      $retVal = $TRUE
      Write-Host "Application $($rapFileLocation) successfully installed in Workspace: $($workspaceArtifactID)"
    }
    else {
      Write-Error "Unable to install Application"
      Return
    }
  }
  else {
    Write-Error "Unable to install App $($installationRequest.Message)"
    Return
  }

  $retVal
}

############################USER CONFIG SECTION###########################

$fullFilePathRap = "S:\SomePath\SomeApplication.rap"
$relativityServicesUrl = "http://myInstanceName/Relativity.Services"
$relativityUsername = "username@domain.com"
$relativityPassword = "password"
$existingWorkspaceArtifactID = "1234567"
###########################################################################

Add-Type -Path "S:\PS1DeployScript\kCura.Relativity.Client.dll"
  $rsapiClient = GetRsapiClient $relativityServicesUrl $relativityUsername $relativityPassword
      Write-Host "Writing test value."
      Write-Host $existingWorkspaceArtifactID
      $appInstalled = InstallApplication $rsapiClient $existingWorkspaceArtifactID $fullFilePathRap

