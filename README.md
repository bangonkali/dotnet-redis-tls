```sh
# Generate a private key for Redis
openssl genrsa -out redis.key 2048

# Create a certificate signing request (CSR)
openssl req -new -key redis.key -out redis.csr
# Country Name (2 letter code) [AU]:PH
# State or Province Name (full name) [Some-State]: 
# Locality Name (eg, city) []:
# Organization Name (eg, company) [Internet Widgits Pty Ltd]:
# Organizational Unit Name (eg, section) []:
# Common Name (e.g. server FQDN or YOUR name) []:redis-tls-server
# Email Address []:
# 
# Please enter the following 'extra' attributes
# to be sent with your certificate request
# A challenge password []:
# An optional company name []:

# Sign the certificate
openssl x509 -req -days 365 -in redis.csr -signkey redis.key -out redis.pem

# Generate a certificate for the ASP.NET app (for HTTPS)
openssl req -x509 -newkey rsa:2048 -keyout aspnetapp.key -out aspnetapp.crt -days 365 -nodes
# Country Name (2 letter code) [AU]:PH
# State or Province Name (full name) [Some-State]:
# Locality Name (eg, city) []:
# Organization Name (eg, company) [Internet Widgits Pty Ltd]:
# Organizational Unit Name (eg, section) []:
# Common Name (e.g. server FQDN or YOUR name) []:localhost
# Email Address []:

openssl pkcs12 -export -out aspnetapp.pfx -inkey aspnetapp.key -in aspnetapp.crt -password pass:password

docker-compose up --build 

docker build -f redis/Dockerfile -t redis-tls-server --no-cache .
docker build -f src/Dockerfile -t redis-tls-aspnet --no-cache .

docker compose up -d --force-recreate --build
```