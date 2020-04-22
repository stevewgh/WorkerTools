$pesterModules = @( Get-Module -Name "Pester" -ErrorAction "SilentlyContinue" );

Write-Host 'Running tests with Pester v'+$($pesterModules[0].Version)

$PowershellVersion = '7.0.0';

Describe  'installed dependencies' {
    It 'powershell $PowershellVersion is installed' {
        Write-Host $PSVersionTable.PSVersion
        $PSVersionTable.PSVersion.ToString() | Should Contain $PowershellVersion
    }

    # It 'Octopus.Client is installed ' {
    #     [Reflection.AssemblyName]::GetAssemblyName("C:\Octopus.Client.dll").Version.ToString() | Should Contain '8.4.0.0'
    # }

    # It 'dotnet is installed' {
    #     dotnet --version | Should Contain '3.1.201'
    # }


    # It 'java is installed' {
    #     java --version | Should Contain 'x.y.z'
    # }

    # It 'az is installed' {
    #     az --version | Should Contain 'x.y.z'
    # }

    It 'aws is installed' {
      Get-AWSPowerShellVersion | Should Contain '4.0.5'
    }

    # It 'node is installed' {
    #     node --version | Should Contain 'x.y.z'
    # }

    It 'kubectl is installed' {
        kubectl version --client | Should Contain '1.18.0'
    }

    It 'helm is installed' {
        helm version | Should Contain 'v3.1.2'
    }

    It 'terraform is installed' {
        terraform version | Should Contain 'x.y.z'
    }

    It 'python is installed' {
        python --version | Should Contain '3.8.2'
    }

    # It 'gcloud is installed' {
    #     gcloud --version | Should Contain 'x.y.z'
    # }

    # It 'octo is installed' {
    #     octo --version | Should Contain 'x.y.z'
    # }
}
