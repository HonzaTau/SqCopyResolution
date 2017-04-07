# SqCopyResolution
A command line tool for copying **Won't Fix** and **False-Positive** resolution flags between SonarQube projects.

## Background
When you use multiple branches, you must create a separate SonarQube project for each branch. The thing is, that when you resolve an issue as **Won't Fix** or **False Positive** in one SonarQube project, the issue still remain open in SonarQube projects for other branches, even after you merge your code changes to other branches. This tool can copy **Won't Fix** and **False Positive** resolution flags from one SonarQube project to another.

## How to use this tool
### Requirements
- Windows with .NET Framework 4.6.1
- SonarQube 6.x (the tool is tested with SonarQube 6.3 but it should work with older versions as well)

### Run the tool
Download the **SqCopyResolution_1_0_0.zip** file from the **Releases** tab and extract its content into some folder. Then run the tool from Windows command line with following arguments:

|Parameter|Description|
|---|---|
|`-url`|SonarQube Url address, e.g. `-url http://sonarqube-staging.ourcompany.com`|
|`-username`|SonarQube credentials - username|
|`-password`|SonarQube credentials - password|
|`-sourceProjectKey`|Key of the SonarQube project, from which you want to copy resolution flags, e.g. `-sourceProjectKey OurProject:FeatureA`|
|`-destinationProjectKeys`|Comma separated list of keys of the SonarQube projects, to which you want to copy resolution flags, e.g. `-destinationProjectKeys OurProject:HEAD,OurProject:FeatureB`|
|`-logLevel` (optional)|Log level (All, Debug, Info, Warn, Error), e.g. `-logLevel Debug`. The default log level is Info.|

Here is an example of the command, which copies resolution flags from the `OurProject:FeatureA` to `OurProject:HEAD` and `OurProject:FeatureB` SonarQube projects:
```
SqCopyResolution -url http://sonarqube-staging.ourcompany.com -username HonzaTau -password SecretPwd -sourceProjectKey OurProject:FeatureA -destinationProjectKeys OurProject:HEAD,OurProject:FeatureB
```
