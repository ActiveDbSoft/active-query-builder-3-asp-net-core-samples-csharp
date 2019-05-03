docker build -t crossdomain .
docker run -d --name aqbcrossdomain crossdomain

echo 'Docker is running! Now set ip to AQB.Web.host in your index.html file:'
docker inspect -f "{{ .NetworkSettings.Networks.nat.IPAddress }}" aqbcrossdomain

pause