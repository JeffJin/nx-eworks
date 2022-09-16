sudo pip install --upgrade youtube-dl

youtube-dl -g url    //get vurl

youtube-dl -f 18 https://www.youtube.com/watch?v=RKQYlBY3RCA   // mp4 format



Task: Install the .NET Core Runtime on the Raspberry Pi.

Run sudo apt-get install curl libunwind8 gettext. This will use the apt-get package manager to install three prerequiste packages.
Run curl -sSL -o dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/release/2.0.0/dotnet-runtime-latest-linux-arm.tar.gz to download the latest .NET Core Runtime for ARM32. This is refereed to as armhf on the Daily Builds page.
Run sudo mkdir -p /opt/dotnet && sudo tar zxf dotnet.tar.gz -C /opt/dotnet to create a destination folder and extract the downloaded package into it.
Run sudo ln -s /opt/dotnet/dotnet /usr/local/bin   to set up a symbolic link...a shortcut to you Windows folks 😉 to the dotnet executable.
Test the installation by typing dotnet --help.


dotnet publish -r linux-arm

scp -r bin/Debug/netcoreapp2.0/linux-arm/publish pi@192.168.200.248:adworks/
scp -r bin/Debug/netcoreapp2.0/publish jeff@172.20.10.13:deployments/webapi
scp -r bin/Debug/netcoreapp2.0/publish jeff@172.20.10.13:deployments/processor

cat /proc/cpuinfo


Task: Install MySql on Raspbian

sudo apt-get update
sudo apt-get install mariadb-server
mysql_secure_installation

systemctl status mysql.service

sudo vi /etc/passwd
--remove x in the line  with root:x:0:0:root:/root:/bin/bash

sudo reboot 

mysql -p -u root

create user 'dbuser'@'localhost' identified by 'Pr0t3ct3d!@#';

GRANT ALL PRIVILEGES ON * . * TO 'dbuser'@'localhost';


source tables_mysql_innodb.sql

ASPNETCORE_ENVIRONMENT=prod ./adworks.pi_player



db initialization

dotnet ef migrations add DbCreation



