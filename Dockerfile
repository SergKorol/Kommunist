FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive
ENV ANDROID_SDK_ROOT=/opt/android-sdk
ENV PATH=$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$ANDROID_SDK_ROOT/platform-tools:$ANDROID_SDK_ROOT/emulator:$PATH
ENV JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
ENV DISPLAY=:0

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

# Встановлення SDK-компонентів - використовуємо API 30 для кращої сумісності
RUN sdkmanager --install \
    "platform-tools" \
    "emulator" \
    "platforms;android-30" \
    "system-images;android-30;google_apis;x86"

RUN echo "no" | avdmanager create avd \
    -n testAVD \
    -k "system-images;android-30;google_apis;x86" \
    --device "pixel" && \
    echo "disk.dataPartition.size=2048M" >> /root/.android/avd/testAVD.avd/config.ini && \
    echo "hw.ramSize=1536" >> /root/.android/avd/testAVD.avd/config.ini

# Create startup script
RUN echo '#!/bin/bash\n\
set -e\n\
\n\
echo "Starting Android Emulator in software mode (no hardware acceleration)..."\n\
\n\
# Start VNC server\n\
Xvnc :0 -geometry 1920x1080 -depth 24 -SecurityTypes None &\n\
sleep 3\n\
\n\
# Start window manager\n\
DISPLAY=:0 fluxbox &\n\
sleep 2\n\
\n\
# Start websockify for web VNC access\n\
websockify --web=/usr/share/novnc/ 6080 localhost:5900 &\n\
sleep 2\n\
\n\
echo "Starting emulator..."\n\
echo "This will be SLOW without hardware acceleration. Please be patient..."\n\
\n\
# Run emulator with software rendering (no KVM/HAXM required)\n\
DISPLAY=:0 emulator -avd testAVD \\\n\
  -no-accel \\\n\
  -no-snapshot \\\n\
  -no-audio \\\n\
  -no-boot-anim \\\n\
  -gpu swiftshader_indirect \\\n\
  -no-metrics \\\n\
  -memory 1536 \\\n\
  -wipe-data\n\
' > /start.sh && chmod +x /start.sh

CMD ["/start.sh"]