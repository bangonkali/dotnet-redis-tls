# redis/Dockerfile
# Use the official Redis image as the base
FROM redis/redis-stack:latest

# Copy the custom redis.conf file to the container
COPY redis/redis.conf /usr/local/etc/redis/redis.conf

# Copy the generated certificates to a specific directory in the container
COPY certs/ /etc/redis/certs/

# Expose the TLS port
EXPOSE 6379

# Start the Redis server with the specified configuration file
CMD [ "redis-server", "/usr/local/etc/redis/redis.conf" ]
