# ---------- Build Stage ----------
# FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS build

# # Force DNS for all adapters
# RUN powershell -Command \
#     "Get-NetAdapter | ForEach-Object { Set-DnsClientServerAddress -InterfaceAlias $_.Name -ServerAddresses ('8.8.8.8','1.1.1.1') }; \
#     Invoke-WebRequest https://nodejs.org/dist/v18.19.0/node-v18.19.0-x64.msi -OutFile node.msi ; \
#     Start-Process msiexec.exe -ArgumentList '/i node.msi /quiet /norestart' -Wait ; \
#     Remove-Item node.msi"

# WORKDIR C:\\app

# COPY package*.json ./

# ENV NODE_OPTIONS="--dns-result-order=ipv4first"
# RUN npm config set prefer-offline false
# RUN npm config set fetch-retries 5
# RUN npm config set fetch-timeout 60000
# RUN npm config set registry https://registry.npmjs.org/

# RUN npm ci


# COPY . .
# RUN npm run build --prod


# # ---------- Runtime Stage ----------
# FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# # Clean default IIS site
# RUN powershell -NoProfile -Command \
#     Remove-Item -Recurse -Force C:\inetpub\wwwroot\*

# # Copy Angular dist output
# COPY --from=build C:\app\dist\asmx-reversi\ C:\inetpub\wwwroot\
# # Expose IIS port
# EXPOSE 80

# IIS starts automatically in this base image


# Meta AI build
# escape=\

# Use the official Windows image with Node.js
FROM mcr.microsoft.com/windows/servercore:1809
# Switch to PowerShell
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Set the working directory
WORKDIR /app

# Install Node.js and npm
RUN Get-NetAdapter | ForEach-Object { Set-DnsClientServerAddress -InterfaceAlias $_.Name -ServerAddresses ('8.8.8.8','1.1.1.1') }; \
    Invoke-WebRequest -Uri https://nodejs.org/dist/v16.14.2/node-v16.14.2-x64.msi -OutFile node.msi; \
    Start-Process msiexec -ArgumentList '/i', 'node.msi', '/quiet', '/qn' -Wait; \
    Remove-Item node.msi

# Update PATH environment variable
RUN setx PATH "$env:PATH;C:\Program Files\nodejs"

# Copy package files
COPY package*.json ./

# Use npm cache to speed up install
RUN npm config set cache C:/npm-cache
RUN npm config set timeout 60000
#RUN npm install --cache C:/npm-cache --prefer-offline
RUN npm ci --prefer-offline --no-audit 

# Copy the rest of the code
COPY . .

# Build the Angular app
RUN npm run build --prod

# Expose the port
EXPOSE 80

# Run the command to start the app
CMD ["npm", "start"]