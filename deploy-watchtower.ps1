docker build -t slwatchtower:heroku -f Dockerfile.watchtower.heroku .
docker tag slwatchtower:heroku registry.heroku.com/slwatchtower/web
docker push registry.heroku.com/slwatchtower/web
heroku container:release web -a slwatchtower
