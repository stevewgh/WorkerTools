#!/bin/bash

eval "$(rbenv init -)"
bundle install
bundle exec rspec
