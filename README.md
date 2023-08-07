# Winamp to Spotify Web Project Version
Winamp to Spotify : Select your mp3 archieve folder and see the magic :)
The aim of this side project is collecting mp3 filenames from harddisk and create Spotify Playlist based on selected folder. To use this you need a Spotify Developer Account. To use the Web API, start by creating a Spotify user account (Premium or Free). To do that, simply sign up at https://developer.spotify.com/dashboard After creating a spotify developer account you should register an application through Dashboard.
![Creating_Spotify_App](WinamptoSpotifyWeb/Resources/creating_app_spotify.gif) 

After doing these 3 step your application should be created successfully.
![Dashboard](WinamptoSpotifyWeb/Resources/after_creating_app_on_dashboard.png)

After creating you will have ClientID and Client Secret values. After creating app from Edit Settings tab you should set Redirection URLs.
![Redirect URls](WinamptoSpotifyWeb/Resources/setting_redirect_urls.png)

By using https://developer.spotify.com/console/get-current-user/ link you can get your UserID of Spotify User ID.
![GettingUserId](WinamptoSpotifyWeb/Resources/getting_user_id.png)

ClientID, SecretID and UserID should be placed in appsettings.json
![ConfigSettings](WinamptoSpotifyWeb/Resources/appsettings_config.PNG)
You can run project by opening solution file(.sln) with Visual Studio or Rider
![Run Project](WinamptoSpotifyWeb/Resources/run_web.png)
After running webspotify project http://localhost:5000 welcomes you
![Web App 1](WinamptoSpotifyWeb/Resources/spotify_web_app.PNG)
Selecting example folder as D:\Müzik Arşivi\Yabancı\Evanescence 
![Web App 2](WinamptoSpotifyWeb/Resources/spotify_web_app3.PNG)
Submit process folder clicked
![Web App 2](WinamptoSpotifyWeb/Resources/spotify_web_app2.PNG)
Folder successfully processed and added tracks displayed on browser.
![Web App 4](WinamptoSpotifyWeb/Resources/spotify_web_app4.PNG)

You can see Spotify List on Spotify itself. That's the magic :)
![Web App 5](WinamptoSpotifyWeb/Resources/spotify_web_app5.PNG)
