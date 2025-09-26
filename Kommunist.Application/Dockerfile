FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install dependencies
RUN apt-get update && \
    apt-get install -y openjdk-17-jdk wget unzip adb && \
    rm -rf /var/lib/apt/lists/*

# Install Android SDK commandline tools
ENV ANDROID_SDK_ROOT=/usr/local/android-sdk
RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools
WORKDIR ${ANDROID_SDK_ROOT}/cmdline-tools

# Download and install commandline tools into expected location
RUN wget https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip -O tools.zip && \
    unzip -q tools.zip && rm tools.zip && \
    mv cmdline-tools latest

# Make sure sdkmanager is on PATH
ENV PATH=${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin:${ANDROID_SDK_ROOT}/platform-tools:${PATH}

# Prepare Android SDK config to avoid first-run warnings
RUN mkdir -p /root/.android && touch /root/.android/repositories.cfg

# Accept licenses and install SDK components
RUN yes | sdkmanager --licenses && \
    sdkmanager "platform-tools" "platforms;android-34" "build-tools;34.0.0"

# Install MAUI workloads
RUN dotnet workload install maui-android --skip-sign-check --skip-manifest-update

WORKDIR /workspace

# Keep the container alive by default; override this entrypoint/command when needed
ENTRYPOINT [ "sleep", "infinity" ]
