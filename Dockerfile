# Use a lightweight base image like Ubuntu (or another Linux distribution)
FROM ubuntu:22.04

# Set environment variables to avoid interactive prompts during package installation
ENV DEBIAN_FRONTEND=noninteractive

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    build-essential \
    libssl-dev \
    libicu-dev \
    zlib1g \
    unzip \
    zip \
    ca-certificates \
    sudo \
    wget \
    apt-transport-https \
    software-properties-common \
    && rm -rf /var/lib/apt/lists/*

# --------------------------------------------------
# Install .NET 6 using the dotnet-install script
# --------------------------------------------------
# Download and run the dotnet-install script to install .NET 6
# Download and install Microsoft's package repository for Ubuntu 22.04
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 6.0 --install-dir /usr/share/dotnet

# Set environment variables for the dotnet command
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH=$PATH:/usr/share/dotnet

# Verify .NET 6 installation
RUN dotnet --version

RUN dotnet tool install -g Amazon.Lambda.Tools

RUN dotnet tool install -g Amazon.Lambda.TestTool-6.0

ENV PATH="${PATH}:/root/.dotnet/tools"

# --------------------------------------------------
# Install Node.js using NVM (Node Version Manager)
# --------------------------------------------------
# Set NVM environment variables
ENV NVM_DIR=/root/.nvm
# Install NVM, Node.js v22, and NPM
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.0/install.sh | bash && \
    . "$NVM_DIR/nvm.sh" && \
    nvm install 22 && \
    nvm use 22 && \
    nvm alias default 22

# Add Node.js and NPM to the PATH
ENV PATH=$NVM_DIR/versions/node/v22.11.0/bin:$PATH

# Verify Node.js and NPM installation
RUN node -v && npm -v

# --------------------------------------------------
# Install PNPM (Performant Node.js Package Manager)
# --------------------------------------------------
# Install PNPM globally using NPM
RUN npm install -g pnpm
ENV PNPM_HOME="/pnpm"
ENV PATH=$PNPM_HOME:$PATH

# Verify PNPM installation
RUN pnpm -v

# --------------------------------------------------
# Install AWS CLI (version 2) using simplified command
# --------------------------------------------------
# Download, unzip, and install AWS CLI version 2
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
    unzip awscliv2.zip && \
    sudo ./aws/install --bin-dir /usr/local/bin --install-dir /usr/local/aws-cli --update && \
    rm -rf awscliv2.zip aws


# Verify AWS CLI installation
RUN aws --version

# --------------------------------------------------
# Install Amplify CLI
# --------------------------------------------------
# Install Amplify CLI globally using NPM
RUN npm install -g @aws-amplify/cli

# Verify Amplify CLI installation
RUN amplify --version

# --------------------------------------------------
# Set working directory (optional) and expose ports
# --------------------------------------------------
WORKDIR /app

# Optionally, you can copy your application files here
COPY . .


ARG AWS_ACCESS_KEY_ID
ARG AWS_SECRET_ACCESS_KEY
ARG AWS_DEFAULT_REGION
ENV AWS_ACCESS_KEY_ID=$AWS_ACCESS_KEY_ID
ENV AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY
ENV AWS_DEFAULT_REGION=$AWS_DEFAULT_REGION

RUN aws configure list



