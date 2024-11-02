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
\"accessKeyId\":\"AKIA23WHT5F5J2CZOKNS\",\
\"secretAccessKey\":\"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\",\
\"region\":\"us-east-2\"\
}"
AMPLIFY="{\
\"projectName\":\"yApp\",\
\"envName\":\"dev\",\
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

echo "changing path to tests..."
cd amplify/backend/function/Tests
echo "running tests..."
dotnet test Tests.csproj


echo "Pushing..."
amplify push --yes --debug


echo "changing path..."
cd yAppWeb
echo "installing pnpm packages..."
pnpm install

echo "Publishing..."
amplify publish --yes  --debug