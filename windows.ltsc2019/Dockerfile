FROM mcr.microsoft.com/windows/servercore:ltsc2019
SHELL ["powershell", "-Command"]

# Install Choco
RUN Set-ExecutionPolicy Bypass -Scope Process -Force;
RUN [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
RUN iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# Install dotnet 3+
RUN Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -outFile 'dotnet-install.ps1'
RUN .\dotnet-install.ps1
RUN rm dotnet-install.ps1

# Install JDK
RUN choco install openjdk14 --allow-empty-checksums --yes --no-progress
RUN refreshenv

# Install Azure CLI
# This currently isn't working, and I'm unsure why
# https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest
RUN Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi; Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'; rm .\AzureCLI.msi

# # Install AWS CLI
# # https://docs.aws.amazon.com/powershell/latest/userguide/pstools-getting-set-up-windows.html#ps-installing-awspowershellnetcore
# RUN Install-Module -name AWSPowerShell.NetCore

# # Install NodeJS
# RUN Invoke-WebRequest "https://nodejs.org/dist/latest-v12.x/node-v12.16.1-x64.msi" -OutFile node-v12.16.1.msi; Start-Process msiexec.exe -Wait -ArgumentList '/I node-v12.16.1.msi /quiet'; rm .\nodev12.16.1.msi
