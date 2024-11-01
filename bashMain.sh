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


echo "Publishing..."
amplify publish --yes  --debug