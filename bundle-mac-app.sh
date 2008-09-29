#!/bin/sh

BUNDLE=Tasque.app/

rm -rf $BUNDLE

CONTENTS=Contents/
MACOS=Contents/MacOS/
RESOURCES=/Contents/Resources/

mkdir -p $BUNDLE/$MACOS
mkdir -p $BUNDLE/$RESOURCES

cp osx/$CONTENTS/Info.plist $BUNDLE/$CONTENTS
cp osx/$MACOS/Tasque $BUNDLE/$MACOS
cp osx/$RESOURCES/tasque.icns $BUNDLE/$RESOURCES

cp bin/Debug/tasque.exe Tasque.app/Contents/MacOS/
cp macbin/ige-mac-integration-sharp.dll Tasque.app/Contents/MacOS/