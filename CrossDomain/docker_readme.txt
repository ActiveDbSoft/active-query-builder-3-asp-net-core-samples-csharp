To run this demo in a docker container, run the following commands:
	$ docker build -t crossdomain .
	$ docker run -d -p 8080:80 --name crossdomaindemo crossdomain
	
The backend will be executed inside a container and will be accessible on the port 8080.
To the attach the front-end to it, change host in the \FrontEnd\index.html by setting  the AQB.Web.host property to 'http://localhost:8080' and run it as a static web server.
