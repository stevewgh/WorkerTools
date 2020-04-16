# Step Execution Container
Officially sanctioned worker images for Octopus Deploy

# Testing
Our tests are implemented in `serverspec`, which relys on `ruby` and `rspec`. To run these tests, you can see the instructions for [Ubuntu](#Ubuntu) and [Windows](#Windows)

N.B. all commands below should be run from the project root directory.

## Ubuntu

### Option 1: Build and Test scripts

```bash
./build.sh --image-directory=`ubuntu.18.04`
```

Runs a build and test of the `ubuntu.18.04` container

### Option 2: DIY

```bash
cd ubuntu.18.04
docker build . -t worker-tools
docker run -it -v `pwd`:/app worker-tools
```

Then within the running docker container

```bash
cd app && bundle install && bundle exec rspec
```

## Windows

### Option 1: Build and Test scripts

```powershell
build.ps1 -image-directory 'windows.ltsc2019'
```

Runs a build and test of the `windows.ltsc2019` container

### Option 2: DIY

```powershell
cd windows.ltsc2019
docker build . -t worker-tools
docker run -it -v ${pwd}:/app worker-tools
```

Then within the running docker container

```powershell
cd app 
bundle install 
bundle exec rspec
```