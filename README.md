# SpeakWrite.NET

.NET client library for interfacing with the SpeakWrite https API

## Supported Features

  - Upload (submit) an audio file for transcription
  - Check the status of your submitted transcriptions
  - Download 
    - Completed document of your transcribed audio submission
    - The original source audio

## Help/Support

Please open an issue on the repository for any help, feature requests, or bug reports.  Pull requests are accepted.  For detailed integration help or to discuss integration partnerships you can contact SpeakWrite directly at [technology@speakwrite.com](mailto:technology@speakwrite.com)

----

## API Documentation
### Submit Job
    Mechanism to submit a file (audio, video, pdf) to the SpeakWrite system.
  
- **URL**<br/>
  `/api/v1/submitjob.ashx`

- **Method**<br/>
  `POST`

- **URL Paramaters**<br/>
  None

- **Form Parameters**

  Name|Type|Required|Description
  ----|----|----|----
  `applicationid`|string|**Y**|Unique ID for your specific integration application
  `accountnumber`|string|**Y**|Your account number
  `pin`|string|**Y**|Your pin
  `customFilename`|string|N|Descriptive name for the job being submitted
  `audiofile`|binary|**Y**|Binary representation of the file to upload
  
- **Success Response**

  - **Code**: 200 <br/>
    **Content**: 
```json
{ 
  "FileName" : "ExampleFileName" 
}
```

- **Error Response**
  - **Code**: 200 <br/>
    **Content**: 
```json
{ 
  "error" : "Error Message" 
}
```
    Or
```json
{
  "message" : "Failure Message",
  "success" : false
}
```

- **Example**<br/>
    See the `SpeakWrite.API.Client.UploadJob` method for a detailed example.
  
### Get Completed Jobs
    Mechanism to query the SpeakWrite system for all completed jobs since a given date.
  
- **URL**<br/>
  `/api/v1/completedjobs.ashx`

- **Method**<br/>
  `GET`

- **URL Paramaters**
  
  Name|Type|Required|Description
  ----|----|----|----
  `applicationid`|string|**Y**|Unique ID for your specific integration application
  `accountnumber`|string|**Y**|Your account number
  `pin`|string|**Y**|Your pin
  `maxage`|string|N|Maximum date to search back through

- **Form Parameters**<br/>
  None

- **Success Response**

  - **Code**: 200 <br/>
    **Content**: 
```JSON 
{
  "success" : true,
  "jobs" : [
    {
      "customFileName" : "Custom File Name",
      "fileName" : "ExampleFileName",
      "accountNumber" : "123456789",
      "downloadUrl" : "https://service.speak-write.com/IntegrationService/api/v1/download.ashx?a=EQepXAs1yChcfYXmctinB"
    }
  ]
}
```

- **Error Response**
  - **Code**: 200 <br/>
    **Content**: 
```json
{ 
  "error" : "Error Message" 
}
```
    Or
```json
{
  "message" : "Failure Message",
  "success" : false
}
```

- **Example**

    See the `SpeakWrite.API.Client.GetCompletedJobs` method for a detailed example.
  
### Download Job
    Mechanism to download a completed job from the SpeakWrite System.
  
- **URL**<br/>
  `integration/api/v1/download.ashx`

- **Method**<br/>
  `GET`

- **URL Paramaters**
  
  Name|Type|Required|Description
  ----|----|----|----
  `applicationid`|string|**Y**|Unique ID for your specific integration application
  `accountnumber`|string|**Y**|Your account number
  `pin`|string|**Y**|Your pin
  `filetype`|string|**Y**|File type to download. Options are `document` or `audio-source`
  `filename`|string|N|Filename of the job to download. Either this field or `customfilename` is required
  `customfilename`|string|N|Custom filename of the job to download. Either this field or `filename` is required

- **Form Parameters**<br/>
  None

- **Success Response**

  - **Code**: 200 <br/>
    **Content**: Type `application/octet-stream` containing binary data of the file being downloaded.

- **Error Response**
  - **Code**: 200 <br/>
    **Content**: 
```json
{ 
  "error" : "Error Message" 
}
```
    Or
```json
{
  "message" : "Failure Message",
  "success" : false
}
```

- **Example**

    See the `SpeakWrite.API.Client.Download` method for a detailed example.

