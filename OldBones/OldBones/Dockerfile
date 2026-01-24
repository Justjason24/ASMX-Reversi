FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

# Create app directory
RUN mkdir C:\inetpub\wwwroot\OldBones

# Copy ASMX site files
COPY . C:/inetpub/wwwroot/OldBones/

# Set IIS to serve from OldBones
RUN powershell -NoProfile -Command \
    Remove-WebSite -Name 'Default Web Site' ; \
    New-WebSite -Name 'OldBones' -Port 80 -PhysicalPath 'C:\inetpub\wwwroot\OldBones'

EXPOSE 80