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
\"accessKeyId\":\"AKIA23WHT5F5IDKP5JEE\",\
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

# amplify push --yes

# amplify publish --yes