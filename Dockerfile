FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive
ENV ANDROID_SDK_ROOT=/opt/android-sdk
ENV PATH=$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$ANDROID_SDK_ROOT/platform-tools:$ANDROID_SDK_ROOT/emulator:$PATH
ENV JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64

# Залежності (додаю ca-certificates + expect)
RUN apt-get update && apt-get install -y \
    unzip wget curl git zip ca-certificates expect \
    openjdk-17-jdk \
    libc6 libstdc++6 zlib1g \
    libglu1-mesa libpulse0 libnss3 libxcursor1 \
    libxcomposite1 libxrandr2 libxi6 libxtst6 \
    libxdamage1 x11-utils \
    tigervnc-standalone-server fluxbox \
    websockify novnc \
    && rm -rf /var/lib/apt/lists/*

# Android cmdline-tools
RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools \
    && wget -q https://dl.google.com/android/repository/commandlinetools-linux-10406996_latest.zip -O /tmp/cmdline-tools.zip \
    && unzip -q /tmp/cmdline-tools.zip -d ${ANDROID_SDK_ROOT}/cmdline-tools \
    && mv ${ANDROID_SDK_ROOT}/cmdline-tools/cmdline-tools ${ANDROID_SDK_ROOT}/cmdline-tools/latest \
    && rm /tmp/cmdline-tools.zip

# Fix repositories.cfg (інакше sdkmanager падає)
RUN mkdir -p /root/.android && touch /root/.android/repositories.cfg

# Ліцензії
RUN yes | sdkmanager --licenses || true

# Встановлення SDK-компонентів
RUN sdkmanager --install \
    "platform-tools" \
    "emulator" \
    "platforms;android-35" \
    "system-images;android-35;google_apis;x86_64"
    
RUN echo "no" | avdmanager create avd \
    -n testAVD \
    -k "system-images;android-35;google_apis;x86_64" \
    --device "pixel"

CMD fluxbox & \
    websockify --web=/usr/share/novnc/ 6080 localhost:5900 & \
    emulator -avd testAVD -no-audio -no-boot-anim -gpu swiftshader_indirect -no-snapshot -verbose