docker build -t slgateway:heroku -f Dockerfile.heroku .
docker tag slgateway:heroku registry.heroku.com/slgateway/web
docker push registry.heroku.com/slgateway/web
heroku container:release web -a slgateway
