# Step Execution Container
Officially sanctioned worker images for Octopus Deploy

# Testing
Our tests are implemented in `serverspec`, which relys on `ruby` and `rspec`. To run these tests, you can see the instructions for Ubuntu and Windows

## Ubuntu
Run the following commands within the `/ubuntu.18.04` directory
```bash
docker build . -t octopus-ubuntu 
docker run -it -v `pwd`:/app octopus-ubuntu
```

Then within the running docker container

```bash
cd app && bundle install && bundle exec rspec
```