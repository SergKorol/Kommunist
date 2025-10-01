# Kommunist.Application/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0.304 AS build

# Install dependencies
RUN apt-get update && \
    apt-get install -y openjdk-17-jdk wget unzip adb && \
    rm -rf /var/lib/apt/lists/*

# Android SDK CLI tools
ENV ANDROID_SDK_ROOT=/usr/local/android-sdk
RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools
WORKDIR ${ANDROID_SDK_ROOT}/cmdline-tools
RUN wget https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip -O tools.zip && \
    unzip -q tools.zip && rm tools.zip && \
    mv cmdline-tools latest

ENV PATH=${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin:${ANDROID_SDK_ROOT}/platform-tools:${PATH}

RUN mkdir -p /root/.android && touch /root/.android/repositories.cfg

# Accept licenses and install required SDK components
RUN yes | sdkmanager --licenses && \
    sdkmanager "platform-tools" "platforms;android-35" "build-tools;35.0.0"

# Install MAUI workloads
RUN dotnet workload install maui-android --skip-sign-check --skip-manifest-update

# Use /src as working dir
WORKDIR /src

# ---- Copy minimal files to leverage Docker cache (fast layer for restore) ----
# Copy solution and project files first
COPY ./Kommunist.Application/Kommunist.Application.csproj ./Kommunist.Application/
COPY ./Kommunist.Core/Kommunist.Core.csproj ./Kommunist.Core/
# (copy any other project csproj lines if you have them)
# Perform restore (will use cached layers when csproj unchanged)
# Specify framework to avoid iOS workload dependency
RUN dotnet restore "./Kommunist.Application/Kommunist.Application.csproj" /p:TargetFramework=net9.0-android

# ---- Copy remaining source ----
COPY . .

# Publish the MAUI android app (adjust project path if different)
RUN dotnet publish ./Kommunist.Application/Kommunist.Application.csproj \
    -f net9.0-android -c Release -o /app \
    --no-restore \
    /p:TargetFramework=net9.0-android

# Set working directory to app output
WORKDIR /app

# keep container alive for adb install / debugging; override command when running if needed
ENTRYPOINT ["sleep","infinity"]
