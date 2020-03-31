require 'spec_helper'

describe command('pwsh --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/PowerShell 7.0.0/) }
end

describe command('dotnet --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/3.1.201/) }
end

describe command('java --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/11.0.6/) }
end

describe command('az --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/2.2.0/) }
end

describe command('aws --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/1.18.31/) }
end

describe command('node --version') do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/v12.16.1/) }
end

describe command('kubectl version') do
  its(:stdout) { should contain(/v1.11.1/) }
end

describe command("helm version") do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/v3.0.2/)}
end

describe command("terraform version") do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/Terraform v0.12.24/)}
end

describe "python" do
  describe command("python --version") do
    its(:exit_status) { should eq(0) }
    # python 2 prints it's version to stderr, for some reason
    its(:stderr) { should contain(/Python 2.7.17/)}
  end

  describe command("python3 --version") do
    its(:exit_status) { should eq(0) }
    its(:stdout) { should contain(/Python 3.6.9/)}
  end
end

describe command("gcloud --version") do
  its(:exit_status) { should eq(0) }
  its(:stdout) { should contain(/286.0.0/)}
end

