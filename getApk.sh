
echo "cd to the yAppMobile folder"
cd yAppMobile

echo "Make sure the gradlew file is executable"
#make executable
chmod +x gradlew

echo "Build the apk"
#build the apk
./gradlew assembleDebug


