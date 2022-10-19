Architecture
adworks-frontend        -> reefbook.ca      192.168.5.117

adworks-web-api         -> eworkspace.ca    192.168.5.116

adworks-media-processor -> eworkspace.ca  192.168.5.108, 192.168.5.109

adworks-streaming(Nginx, Rtmp, Ftp, MySql, RabbitMQ) -> eworkspace.co  192.168.5.115


==============================================================================================

docker build -t adworks .
docker run -d -p 5000:5000 --name adworks adworks

dotnet add package System.IdentityModel.Tokens.Jwt;
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer;

request header:
'Authorization': 'Bearer ' + token


OpenResty(nginx + lua)
https://www.digitalocean.com/community/tutorials/how-to-use-the-openresty-web-framework-for-nginx-on-ubuntu-16-04

This web server is used to handle media streaming and lua script for content protection


nginx + rtmp module config

rtmp://eworkspace.ca:1935/vod//jeff@jeffjin.com/4a851902-efee-421b-a915-ee50fda8f3aa/video2.mp4
rtmp://eworkspace.ca:1935/vod//jeff@jeffjin.com/d0b93049-69f0-4643-be44-bef704deaf08/video4.mp4
rtmp://eworkspace.ca:1935/vod//jeff@eworks.io/d0b93049-69f0-4643-be44-bef704deaf08/video4.mp4

==============================================================================================


#user  nobody;
worker_processes  1;

#error_log  logs/error.log;
#error_log  logs/error.log  notice;
pid        /run/nginx.pid;


events {
    worker_connections  1024;
}

rtmp {

    server {

        listen 1935;
        chunk_size 4000;

        # video on demand
        application vod1 {
            play /var/videos/flvs;
        }

        application vod {
            play /var/videos/mp4s;
        }

        application hls {
            live on;
            hls on;
            hls_path /tmp/hls;
        }
    }
}

# HTTP can be used for accessing RTMP stats
http {
    server {

        error_log logs/rtmp.error.log;
        access_log logs/rtmp.access.log;
        listen      8080;
        # This URL provides RTMP statistics in XML
        location /stat {
            rtmp_stat all;
            rtmp_stat_stylesheet stat.xsl;
        }

        location /stat.xsl {
            # XML stylesheet to view RTMP stats.
            # Copy stat.xsl wherever you want
            # and put the full directory path here
            root /var/videos/stats/stat.xsl;
        }

        location /hls {
            # Serve HLS fragments
            types {
                application/vnd.apple.mpegurl m3u8;
                video/mp2t ts;
            }
            root /tmp/hls;
            add_header Cache-Control no-cache;
            expires -1;
        }
    }

     server {
            listen 80 default_server;
            listen [::]:80;

            index index.html;

            server_name _; # This is just an invalid value which will never trigger on a real hostname.

            server_name_in_redirect off;

            root  /home/jeff/deployments/ngspa/dist;

            location / {
                try_files $uri $uri/ /index.html;
             }
       }
     }



     Install dotnet core on Ubuntu 16.04 LTS

     curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
     sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg

     sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
     sudo apt-get update

     sudo apt-get install dotnet-sdk-2.1.4

     dotnet --version
