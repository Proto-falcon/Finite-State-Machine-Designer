# Finite State Machine Designer
## Summary
I made a Finite State Machine Designer website based on Evan Wallace and improvemetns from Nathan Otterness with:


- Integer Superscripts
- Resize individual States
- Export PNG & SVG at any scale
- Backup your Finite State Machines to the website it self

Inspired by [Evan Wallace's](https://madebyevan.com/) -
[Finite Machine Designer](https://madebyevan.com/fsm/) in 2010
And modifications from
[Nathan Otterness's](https://www.cs.unc.edu/~otternes/) - 
[FSM Designer](https://www.cs.unc.edu/~otternes/comp455/fsm_designer/) in 2019.

## Configuration
### Appsettings.json

    {
        "EmailContentPaths": {
            "EmailLayout": ".\\EmailContent\\EmailLayout.html"
        },
        "UsersConfig": {
            "VisibleFsmsLimit": 5,
            "FsmsLimit": 6
        },
    }

- EmailLayout - Path of email layout path in code.
- VisibleFsmsLimit - Number of FSMs that can be viewed per page.
- FsmsLimit - Maximum number of FSMs that each user can store remotely.

### Environment Variables

- FSM__ExternalAuths__GoogleAuth__ClientId - Client Id of Google app from Google Cloud.
- FSM__ExternalAuths__GoogleAuth__ClientSecret - Client Secret of Google app from Google Cloud.

To get the Client Id and Secret any other provider [follow these steps](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio).

- FSM__EmailServiceConfig__DisplayAddress - Address of email account to send emails to users.
- FSM__EmailServiceConfig__Password - Password of email account to send emails to users.
- FSM__EmailServiceConfig__SmtpServer - SMTP Server to send the emails via SMTP.
- FSM__EmailServiceConfig__Port - Port of the SMTP server
- FSM__EmailServiceConfig__SecureSocketOptions - Number corresponding to values in MailKit.Security.SecureSocketOptions

## Build

Build the app in visual studio or via CLI:  
``dotnet build --output ./build_output``

## Run

Run the executable or plug it in IIS to run the server, and your done 🥳👌.