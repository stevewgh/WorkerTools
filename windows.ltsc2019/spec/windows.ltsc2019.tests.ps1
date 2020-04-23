$pesterModules = @( Get-Module -Name "Pester" -ErrorAction "SilentlyContinue" );

Write-Host 'Running tests with Pester v'+$($pesterModules[0].Version)

$PowershellVersion = '7.0.0';

Describe  'installed dependencies' {
    It 'has powershell $PowershellVersion is installed' {
        Write-Host $PSVersionTable.PSVersion
        $PSVersionTable.PSVersion.ToString() | Should Contain $PowershellVersion
    }

    It 'has Octopus.Client is installed ' {
      [Reflection.AssemblyName]::GetAssemblyName("C:\Program Files\PackageManagement\NuGet\Packages\
      Octopus.Client.8.4.2\lib\net452\Octopus.Client.dll").Version.ToString() | Should Contain '8.4.2.0'
    }

    It 'has dotnet is installed' {
        dotnet --version | Should Contain '3.1.201'
    }

    It 'has java is installed' {
        java --version | Should Contain '14+36-1461'
    }

    It 'has az is installed' {
        az --version | Should Contain '2.4.0'
    }

    It 'has aws is installed' {
      Get-AWSPowerShellVersion | Should Contain '4.0.5'
    }

    It 'has node is installed' {
        node --version | Should Contain '12.16.2'
    }

    It 'has kubectl is installed' {
        kubectl version --client | Should Contain '1.18.0'
    }

    It 'has helm is installed' {
        helm version | Should Contain '3.1.2'
    }

    It 'has terraform is installed' {
        terraform version | Should Contain 'x.y.z'
    }

    It 'has python is installed' {
        python --version | Should Contain '3.8.2'
    }

    It 'has gcloud is installed' {
        gcloud --version | Should Contain '289.0.0'
    }

    It 'has octo is installed' {
        octo --version | Should Contain '7.3.4'
    }
}
