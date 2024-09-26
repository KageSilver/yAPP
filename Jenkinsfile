def project = "yApp"
def major = 0
def minor = 0
def revision = 0

def appversion = "$major.$minor.$revision"
def buildName = "$Software $env.BRANCH_NAME"

def publishedApps = "yAppWeb/dist/**"

def amplify_env = ""
def cognito_pool = ""
def pool_id = ""
def cloudfront_id = ""
def awsProfile = "yapp"
def awsRegion = "us-east-2"

def AWSCLOUDFORMATIONCONFIG = "{\
\"configLevel\":\"project\",\
\"useProfile\":true,\
\"profileName\":\"${awsProfile}\",\
}"
def EXE_PATH = "" // the path where to find the amplify executable

def PROVIDERS = "{\"awscloudformation\":$AWSCLOUDFORMATIONCONFIG}"

//environment name
def MAIN_ENV = "main"
def DEV_ENV = "dev"
def TEST_ENV = "test"

// app id
def AMPLIFY_APP_ID = "d34oy4u1icq89e"
def MAIN_CLOUDFRONT_ID = "E1J2AXX98R4GJP" 
def DEV_CLOUDFRONT_ID = "E31QWRSIU8H00V"
def TEST_CLOUDFRONT_ID = "" 

//file path we need to update
def VERSION_FILE_PATH = "yAppWeb/src/version.json"
def LAMBDA_PATH = "amplify/backend/function/yAppLambda/src/appsettings.json"

//cognito
def POOL_URL= "https://cognito-idp.us-east-2.amazonaws.com/"
def TEST_POOL = "us-east-2_DpPbDoSUa"
def DEV_POOL = "us-east-2_AoahD29kD"
def MAIN_POOL = "us-east-2_itXR8ULJ4"


//dotnet tests
//path
def solution = "amplify/backend/function/Lambdas.sln"

pipeline {
    environment {
        def label = null // TODO:Update later
        def title = null // filled in later
        AWS_SDK_LOAD_CONFIG = "1"
        AWS_PROFILE = "${awsProfile}"
    }
    agent {
        node {
            // TODO update the node label
            // label "${label}"
            // customWorkspace "./workspace/${env.BRANCH_NAME.toLowerCase()}"
        }
    }
    options {
        timeout(time: 1, unit: "HOURS")
        parallelsAlwaysFailFast()
        disableConcurrentBuilds()
    }
    stages {
        stage("Prepare workspace") {
            steps {
                echo "Preparing workspace"
                echo "My branch is: ${env.BRANCH_NAME}"
                script {
                    //TODO: update label
                    title = "${project}_${appversion}"
                    echo "Title: ${title}"
                }
            }
        }
        stage("Install npm packages") {
            steps {
                script {
                    sh(script: """
                     pnpm --version
                     pnpm install --frozen-lockfile
                    """)
                }
            }
        }
        
        stage('Initialize environment') {
            steps {
                script {
                    echo 'Initializing Amplify'
                    if (env.BRANCH_NAME == "main") {
                        amplify_env = MAIN_ENV
                        pool_id = MAIN_POOL
                        cloudfront_id = MAIN_CLOUDFRONT_ID
                    } else if (env.BRANCH_NAME == "dev") {
                        amplify_env = DEV_ENV
                        pool_id = DEV_POOL
                        cloudfront_id = DEV_CLOUDFRONT_ID
                    } 
                    else {
                        amplify_env = TEST_ENV
                        pool_id = TEST_POOL
                    }

                    cognito_pool = "${POOL_URL}${pool_id}"

                    def amplify_project = "{\
                    \"projectName\":\"${project}\",\
                    \"envName\":\"${amplify_env}\",\
                    \"defaultEditor\":\"code\"\
                    }"
                    sh(script: "cd yAppWeb ${EXE_PATH} init --appId ${AMPLIFY_APP_ID} --amplify '${amplify_project}' --providers '${PROVIDERS}' --yes")
                }
            }
        }
        stage('Update Version & App settings') {
            steps {
                script {
                    //To update the version number that display in the app
                    echo "Update version number in version.json"
                    def jsonData = [
                        version: "$version",
                        env: "$amplify_env",
                    ]
                    // Convert JSON data to string
                    def jsonString = groovy.json.JsonOutput.toJson(jsonData)
                    // Write JSON string to file
                    writeFile file: VERSION_FILE_PATH, text: jsonString
                }
            }
        }
        stage('Run tests') {
            steps {
                script {
                    echo 'Running tests'
                    def testStatus = sh(script: "dotnet test ${solution}", returnStatus: true)
                    if (testStatus != 0) {
                        error("Tests failed. Exiting...")
                    }
                }
            }
        }
        stage('Pushing backend') {
            steps {
                script {
                    echo 'Amplify push'
                    sh(script: "pipenv run ${EXE_PATH} push --yes")
                }
            }
        }
        stage('Publish app') {
            steps {
                script {
                    if (env.BRANCH_NAME == "main" || env.BRANCH_NAME == "dev") {
                        echo 'Amplify publish'
                        sh(script: "pipenv run ${EXE_PATH} publish --yes")
                    }
                }
            }
        }
        stage('Verify amplify status') {
            steps {
                script {
                    echo 'Amplify status...'
                    sh(script: "${EXE_PATH} status")
                }
            }
        }
        stage('Invalidate Cloudfront cache') {
            steps {
                script {
                    echo 'Invalidate Cloudfront cache'
                    if (env.BRANCH_NAME == "main" || env.BRANCH_NAME == "dev") {
                        sh(script: "aws cloudfront create-invalidation --distribution-id ${cloudfront_id} --paths \"/*\" --profile ${awsProfile}")
                    } 
                    
                }
            }
        }
    }
    post {
        always {
            echo "Post run"
            // to create a webhook for the github repo
        }
        cleanup {
            echo "Clean up Post"
            cleanWs()
        }
    }
}
