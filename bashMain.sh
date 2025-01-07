#!/bin/bash
set -e
IFS='|'

REACTCONFIG="{\
\"SourceDir\":\"yAppWeb/src\",\
\"DistributionDir\":\"yAppWeb/dist\",\
\"BuildCommand\":\"npm run-script build\",\
\"StartCommand\":\"npm run-script start\"\
}"
AWSCLOUDFORMATIONCONFIG="{\
\"configLevel\":\"project\",\
\"useProfile\":false,\
\"profileName\":\"default\",\
\"accessKeyId\":\"\",\
\"secretAccessKey\":\"\",\
\"region\":\"us-east-2\"\
}"
AMPLIFY="{\
\"projectName\":\"yApp\",\
\"envName\":\"main\",\
\"defaultEditor\":\"code\"\
}"
FRONTEND="{\
\"frontend\":\"javascript\",\
\"framework\":\"vue\",\
\"config\":$REACTCONFIG\
}"
PROVIDERS="{\
\"awscloudformation\":$AWSCLOUDFORMATIONCONFIG\
}"

amplify init \
--amplify $AMPLIFY \
--frontend $FRONTEND \
--providers $PROVIDERS \
--yes

amplify status

echo "Pushing..."
amplify push --yes --debug


echo "changing path..."
cd yAppWeb
echo "installing pnpm packages..."
pnpm install

echo "Publishing..."
amplify publish --yes  --debug