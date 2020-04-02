FROM mcr.microsoft.com/windows/servercore:ltsc2019
SHELL ["powershell", "-Command"]

# Install Choco
# ADD scripts/download_chocolatey.ps1 /download_chocolatey.ps1
# RUN download_chocolatey.ps1
RUN Set-ExecutionPolicy Bypass -Scope Process -Force;
RUN [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
RUN iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# # Install ruby
RUN choco install ruby --version 2.7.0 --allow-empty-checksums --yes --no-progress
RUN refreshenv

