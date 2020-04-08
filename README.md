# Step Execution Container
Officially sanctioned worker images for Octopus Deploy

## Ubuntu
Run the following commands within the `/ubuntu.18.04` directory
```bash
root@LOCAL-MACHINE:/# docker build . -t octopus-ubuntu 
root@LOCAL-MACHINE:/# docker run -it -v `pwd`:/app octopus-ubuntu
```

# Testing
Our tests are implemented in `serverspec`, which relys on `ruby` and `rspec`. To run these tests, you can see the instructions for Ubuntu and Windows

```bash
root@LOCAL-MACHINE:/# docker run -it -v `pwd`:/app octopus-ubuntu
root@17f0de1857a4:/# cd app/ubuntu.18.04
root@17f0de1857a4:/app/ubuntu.18.04# eval "$(rbenv init -)"
root@17f0de1857a4:/app/ubuntu.18.04# bundle install
root@17f0de1857a4:/app/ubuntu.18.04# bundle exec rspec
```

Then within the running docker container

```bash
cd tests && bundle install && bundle exec rspec
```
