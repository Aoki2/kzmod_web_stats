#pragma semicolon 1

public Plugin:myinfo = 
{
    name = "KZmod Web Stats",
    author = "Aoki",
    description = "Store player records in remote database through web API",
    version = "1.1.2",
    url = "http://www.kzmod.com/"
}

//-------------------------------------------------------------------------
// Includes
//-------------------------------------------------------------------------
#include <sourcemod>
#include <sdktools>
#include <geoip>
#include <socket>

//-------------------------------------------------------------------------
// Defines 
//-------------------------------------------------------------------------
#define LOG_DEBUG_ENABLE 0
#define LOG_TO_CHAT 0
#define LOG_TO_SERVER 1

#define STR_LEN (256)
#define STR_LEN_LONG (1024)
#define COURSE_NAME_MAX_LEN (64)
#define MAP_NAME_MAX_LEN (COURSE_NAME_MAX_LEN)
#define SERVER_NAME_MAX_LEN (128)
#define PLAYER_NAME_MAX_LEN (65)
#define ITEM_NAME_MAX_LEN (64)

#define MAX_NUM_COURSES_NEW (2048)
#define MAX_NUM_COURSES_OLD   (20)

#define MEDALS_MAX (4)
#define MAX_PLAYERS_PER_TEAM 4

#define SV_TAGARENA_ROUNDTIME_DEFAULT 10
#define SV_TAGARENA_ROUNDCOUNT 3

//When a tag map is changed to, restart the tag round after this amount
//of time so that players have a chance to join before the first round
#define TAG_FIRST_ROUND_DELAY_SEC 20.0

#define CONV_EVENT_MSEC_TO_SEC (0.01) //1 "ms" = 10 msec in the event

#define DEV_HTTP_SERVER 0
#define DEV_HTTP_ADDRESS "127.0.0.1"
#define DEV_HTTP_PORT (58480)

//-------------------------------------------------------------------------
// Types 
//-------------------------------------------------------------------------
enum teMedal
{
	eeMedalBronze = 0,
	eeMedalSilver = 1,
	eeMedalGold = 2,
	eeMedalPlatinum = 3
};

//-------------------------------------------------------------------------
// Globals 
//-------------------------------------------------------------------------

//Connection configuration
new Handle:ghToplistUrl = INVALID_HANDLE;
new Handle:ghToplistPort = INVALID_HANDLE;
new Handle:ghServerKey = INVALID_HANDLE;
new String:ganToplistUrl[STR_LEN] = "http://invalid.com:80";
new String:ganServerKey[STR_LEN] = "invalid_key";
new gnToplistPort = 80;

//Map info
new String:ganCurrentMap[MAP_NAME_MAX_LEN];
new String:ganCurrentMapUrlEncoded[MAP_NAME_MAX_LEN*4];

//Player info
new String:gaanPlayerAuth[MAXPLAYERS+1][23];
new ganPlayerJumpCount[MAXPLAYERS+1] = {0,...};
new gaanPlayerMedals[MAXPLAYERS+1][MEDALS_MAX];

//Entity index for the player_manager entity
new gnPlayerManagerEntIdx = -1;

//Multiplayer timer variables
new ganMpMode[MAXPLAYERS] = {-1, ...};
new gaanMpClientIndexes[MAXPLAYERS][MAX_PLAYERS_PER_TEAM];

//Tag
new ganRoundTags[MAXPLAYERS+1] = { 0, ... };
new ganRoundTagged[MAXPLAYERS+1] = { 0, ... };
new ganRoundNinjaTags[MAXPLAYERS+1] = { 0, ... };
new ganRoundPowerups[MAXPLAYERS+1] = { 0, ... };
new bool:gaePlayingRoundFromStart[MAXPLAYERS+1] = { false, ... };
new bool:geValidTagConVars = false;
new bool:geIsTagMap = false;

//-------------------------------------------------------------------------
// Initialization 
//-------------------------------------------------------------------------

public OnPluginStart()
{
	InitConVars();
	HttpRequest("&sel=create_tables");
	
	AddServerToDatabase();
	
	//add players
	for(new lnClient=1;lnClient<MaxClients;lnClient++)
	{
		if(IsClientConnected(lnClient) && IsClientAuthorized(lnClient))
		{
			AddPlayerToDatabase(lnClient);
		}
	}
	
	gnPlayerManagerEntIdx = GetPlayerManagerEntIndex();

	//Old map entity system events
	HookEvent("player_starttimer",ev_StartTimer,EventHookMode_Post);
	HookEvent("player_stoptimer",ev_StopTimer,EventHookMode_Post);

	//New map entity system events
	HookEvent("player_starttimer2",ev_StartTimer2,EventHookMode_Post);
	HookEvent("player_stoptimer2",ev_StopTimer2,EventHookMode_Post);
	
	//Other KZMod specific events
	HookEvent("player_medalspend",ev_MedalSpend,EventHookMode_Post);
	HookEvent("player_getpowerup",ev_GetPowerUp,EventHookMode_Pre);
	HookEvent("player_usepowerup",ev_UserPowerUp,EventHookMode_Pre);
	HookEvent("tagarena_tagged_event",ev_PlayerTagged,EventHookMode_Pre);
	HookEvent("tagarena_round_end",ev_TagRoundEnd,EventHookMode_Pre);
	HookEvent("tagarena_map_end",ev_TagMapEnd,EventHookMode_Pre);
	HookEvent("tagarena_start_round",ev_TagRoundStart,EventHookMode_Pre);
	HookEvent("player_jump",ev_PlayerJump,EventHookMode_Post);
			
	gnPlayerManagerEntIdx = GetPlayerManagerEntIndex();
}

//-------------------------------------------------------------------------
// Misc functions 
//-------------------------------------------------------------------------

public LogDebug(const String:aanFormat[], any:...)
{
#if LOG_DEBUG_ENABLE == 1
	decl String:panBuffer[512];
	
	VFormat(panBuffer, sizeof(panBuffer), aanFormat, 2);
#if LOG_TO_CHAT == 1
	PrintToChatAll("%s", panBuffer);
#endif
#if LOG_TO_SERVER == 1
	PrintToServer("%s", panBuffer);
#endif
#endif
}

public bool:GetCourseNameOld(anCourseId,String:aanCourseName[],anStringLength)
{
	new bool: leReturn = false;
	new String: lanCourse[16] = "Course";
	new String: lanCourseNumber[4] = "";
	
	IntToString(anCourseId,lanCourseNumber,sizeof(lanCourseNumber));
	StrCat(lanCourse,sizeof(lanCourse),lanCourseNumber);
	
	new Handle:lhCvar = FindConVar(lanCourse);

	if(lhCvar != INVALID_HANDLE)
	{
		GetConVarString(lhCvar,aanCourseName,anStringLength);
		
		if(strcmp(aanCourseName,"") != 0)
		{
			LogDebug("GetCourseNameOld: found id %d, %s",anCourseId,aanCourseName);
			leReturn = true;
		}
	}
	
	return leReturn;
}

GetPlayerManagerEntIndex()
{
	new lnReturn = -1;
	
	decl String:panClassname[32];
	for(new i=0;i<GetMaxEntities() && lnReturn < 0;i++)
	{
		if(IsValidEntity(i))
		{
			GetEntityClassname(i,panClassname,sizeof(panClassname));
			
			if(strcmp("player_manager",panClassname) == 0)
			{
				lnReturn = i;
			}
		}
	}
	
	return lnReturn;
}

//This function will get a player property if it is an int (name not supported)
GetPlayerProperty(anClient,String:aanProperty[])
{
	//Properties:
	//m_szName, m_iPing, m_iPacketloss, m_iScore, m_iDeaths, m_bConnected, m_iTeam, m_bAlive, m_iHealth, 
	//m_iCheckpoints, m_iTeleports, m_iUntaggedTime, m_iTagged, m_bTagged, m_iRoundsWon, m_bMapWinner, m_iActiveCourse
	new lnReturn = -1;
	
	if(gnPlayerManagerEntIdx < 0)
	{
		gnPlayerManagerEntIdx = GetPlayerManagerEntIndex();
		
		if(gnPlayerManagerEntIdx < 0)
		{
			SetFailState("Failed to find player_manager");
		}
	}
	
	if(gnPlayerManagerEntIdx >= 0)
	{
		lnReturn = GetEntProp(gnPlayerManagerEntIdx,Prop_Send,aanProperty,4,anClient);
	}
	
	return lnReturn;
}

//-------------------------------------------------------------------------
// Convars
//-------------------------------------------------------------------------
InitConVars()
{
	ghToplistUrl = InitAndHookCvar("kz_toplist_url", "invalid.com", "Address of the remote toplist server");
	ghToplistPort = InitAndHookCvar("kz_toplist_port", "80", "Port of the remote toplist server");
	ghServerKey = InitAndHookCvar("kz_toplist_key", "invalid_key", "Toplist auth key for this server");

	GetConVarString(ghServerKey,ganServerKey,sizeof(ganServerKey));
	GetConVarString(ghToplistUrl,ganToplistUrl,sizeof(ganToplistUrl));
	gnToplistPort = GetConVarInt(ghToplistPort);
}

Handle:InitAndHookCvar(String:aanCvarName[],String:aanValue[],String:aanDescription[])
{
	new Handle:lhConvar = FindConVar(aanCvarName);
	
	if(lhConvar == INVALID_HANDLE)
	{
		lhConvar = CreateConVar(aanCvarName, aanValue, aanDescription);
	}
	
	if(lhConvar != INVALID_HANDLE)
	{
		HookConVarChange(lhConvar, cbConVarChange);
	}
	else
	{
		SetFailState("Failed to initialize convar");
	}
	
	return lhConvar;
}

public cbConVarChange(Handle:ahConvar, const String:aanOldVal[], const String:aanNewVal[])
{
	if(ahConvar == ghToplistUrl)
	{
		strcopy(ganToplistUrl,sizeof(ganToplistUrl),aanNewVal);
	}
	else if(ahConvar == ghToplistPort)
	{
		gnToplistPort = GetConVarInt(ahConvar);
	}
	else if(ahConvar == ghServerKey)
	{
		strcopy(ganServerKey,sizeof(ganServerKey),aanNewVal);
	}
}

//-------------------------------------------------------------------------
// Socket functions 
//-------------------------------------------------------------------------

//Function from joropito (http://forums.alliedmods.net/showthread.php?t=117744)
static String:sHexTable[] = "0123456789abcdef";
UrlEncode(String:sString[], String:sResult[], len)
{
    new from, c;
    new to;

    while(from < len)
    {
        c = sString[from++];
        if(c == 0)
        {
            sResult[to++] = c;
            break;
        }
        else if(c == ' ')
        {
            sResult[to++] = '+';
        }
        else if((c < '0' && c != '-' && c != '.') ||
                (c < 'A' && c > '9') ||
                (c > 'Z' && c < 'a' && c != '_') ||
                (c > 'z'))
        {
            if((to + 3) > len)
            {
                sResult[to] = 0;
                break;
            }
            sResult[to++] = '%';
            sResult[to++] = sHexTable[c >> 4];
            sResult[to++] = sHexTable[c & 15];
        }
        else
        {
            sResult[to++] = c;
        }
    }
}  

HttpRequest(String:aanFormat[],any:...)
{
	new Handle:lhSocket = SocketCreate(SOCKET_TCP,cbOnSocketError);
	new String:lanBuffer[4096];
	VFormat(lanBuffer, sizeof(lanBuffer), aanFormat, 2);
	
	new Handle:lhDataPack = CreateDataPack();
	WritePackString(lhDataPack,lanBuffer);
	
	SocketSetArg(lhSocket,lhDataPack);
	
#if DEV_HTTP_SERVER == 1
	SocketConnect(lhSocket, cbOnSocketConnection, cbOnSocketReceive, cbOnSocketDisconnected,
		DEV_HTTP_ADDRESS, DEV_HTTP_PORT);
#else
	SocketConnect(lhSocket, cbOnSocketConnection, cbOnSocketReceive, cbOnSocketDisconnected,
		ganToplistUrl, gnToplistPort);
#endif
}

public cbOnSocketConnection(Handle:ahSocket, any:ahDatapack)
{
	new String:lanBuffer[4096];
	new String:lanRequest[4096];
	
	ResetPack(ahDatapack);
	ReadPackString(ahDatapack,lanBuffer,sizeof(lanBuffer));
	CloseHandle(ahDatapack);
	
	Format(lanRequest,sizeof(lanRequest),"GET /submit.aspx?key=%s%s HTTP/1.0\r\nHost: %s\r\nConnection: close\r\n\r\n",
		ganServerKey,lanBuffer,ganToplistUrl);
	
	if(ahSocket && SocketIsConnected(ahSocket) == true)
	{
		SocketSend(ahSocket,lanRequest);
	
		LogDebug("cbOnSocketConnection: %s",lanRequest);
	}
	else
	{
		LogDebug("cbOnSocketConnection: Socket is not open: %s",lanRequest);
	}
}

public AddServerToDatabase()
{
	decl String:panHostname[SERVER_NAME_MAX_LEN];
	decl String:panHostnameUrlEncoded[SERVER_NAME_MAX_LEN*4];
	
	//Get hostname
	//Doesn't work any more past 1.2: GetClientName(0,panHostname,sizeof(panHostname));

	new Handle:lhConvar = FindConVar("hostname");
	if(lhConvar != INVALID_HANDLE)
	{
		GetConVarString(lhConvar,panHostname,sizeof(panHostname));
	}
	else
	{
		SetFailState("Failed to find hostname cvar");
	}

	//Get the server's port number
	new lnPort = GetConVarInt(FindConVar("hostport"));
	
	UrlEncode(panHostname,panHostnameUrlEncoded,sizeof(panHostnameUrlEncoded));	
	HttpRequest("&sel=add_server&hostname=%s&port=%i",panHostnameUrlEncoded,lnPort);
}

//Add entry to the players SQL table
public AddPlayerToDatabase(anClient)
{
	decl String:panIpAddr[17];
	decl String:panCc[3];
	decl String:panPlayerName[PLAYER_NAME_MAX_LEN];
	decl String:panPlayerNameUrlEncoded[PLAYER_NAME_MAX_LEN*4];
	
	if (IsClientConnected(anClient) && IsClientAuthorized(anClient) && !IsFakeClient(anClient))
	{
		GetClientAuthString(anClient,gaanPlayerAuth[anClient],sizeof(gaanPlayerAuth[]));
		GetClientName(anClient, panPlayerName, sizeof(panPlayerName));
		
		//If client IP is found
		if(GetClientIP(anClient, panIpAddr, sizeof(panIpAddr)) == true)
		{
			//If client country is not found
			if(GeoipCode2(panIpAddr,panCc) == false)
			{
				strcopy(panCc,sizeof(panCc),"A1");
			}
		}
		
		UrlEncode(panPlayerName,panPlayerNameUrlEncoded,sizeof(panPlayerNameUrlEncoded));
		HttpRequest("&sel=add_player&name=%s&auth=%s&ip=%s&country=%s",
			panPlayerNameUrlEncoded,gaanPlayerAuth[anClient],panIpAddr,panCc);
		
		//NOTE: GeoIP database may need to be updated periodically
		//http://www.maxmind.com/download/geoip/database/GeoLiteCountry/
	}
}

public UpdatePlayerInfoInDatabase(anClient)
{
	decl String:panIpAddr[17];
	decl String:panCc[3];
	decl String:panPlayerName[PLAYER_NAME_MAX_LEN];
	decl String:panPlayerNameUrlEncoded[PLAYER_NAME_MAX_LEN*4];
	
	//Update the player name and IP tables
	GetClientName(anClient, panPlayerName, sizeof(panPlayerName));
	
	//If client IP is found
	if(GetClientIP(anClient, panIpAddr, sizeof(panIpAddr)) == true)
	{
		//If client country is not found
		if(GeoipCode2(panIpAddr,panCc) == false)
		{
			strcopy(panCc,sizeof(panCc),"A1");
		}
	}
	else
	{
		strcopy(panCc,sizeof(panCc),"A1");
		strcopy(panIpAddr,sizeof(panIpAddr),"000.000.000");
	}
	
	UrlEncode(panPlayerName,panPlayerNameUrlEncoded,sizeof(panPlayerNameUrlEncoded));
	HttpRequest("&sel=update_player_info&auth=%s&name=%s&ip=%s&country=%s",
		gaanPlayerAuth[anClient],panPlayerNameUrlEncoded,panIpAddr,panCc);
}

public cbOnSocketDisconnected(Handle:ahSocket, any:ahFile)
{
	CloseHandle(ahSocket);
}

public cbOnSocketReceive(Handle:ahSocket, String:aanReceiveData[], const anDataSize, any:ahArg)
{
	//Do nothing
}

public cbOnSocketError(Handle:ahSocket, const anErrorType, const anErrorNum, any:ahArg)
{
	decl String:paanErrType[LISTEN_ERROR+1][32] = 
		{ "0", "EMPTY_HOST", "NO_HOST", "CONNECT_ERROR", "SEND_ERROR", 
		  "BIND_ERROR", "RECV_ERROR", "LISTEN_ERROR" };
	
	if(anErrorType <= LISTEN_ERROR)
	{
		LogDebug("Socket error %s (errno %d)", paanErrType[anErrorType], anErrorNum);
	}
	else
	{
		LogDebug("Socket error %d (errno %d)", anErrorType, anErrorNum);
	}
	
	CloseHandle(ahSocket);
}

//Note that aanMedals contains current medal counts after award of new medals
public UpdatePlayerMedals(anClient, aanMedals[])
{
	new bool:leMedalAdded = false;
	new lnBronze = 0;
	new lnSilver = 0;
	new lnGold = 0;
	new lnPlatinum = 0;
	
	//Create SQL string based on which medal counts changed
	for(new lnMedalIndex=0;lnMedalIndex<MEDALS_MAX;lnMedalIndex++)
	{
		//If this medal type incremented by one.
		if(aanMedals[lnMedalIndex] - gaanPlayerMedals[anClient][lnMedalIndex] == 1)
		{
			switch(lnMedalIndex)
			{
				case 0:
				{
					lnBronze = 1;
				}
				case 1:
				{
					lnSilver = 1;
				}
				case 2:
				{
					lnGold = 1;
				}
				case 3:
				{
					lnPlatinum = 1;
				}
			}

			leMedalAdded = true;
		}
	}
	
	//If a medal count field is being updated
	if(leMedalAdded)
	{
		HttpRequest("&sel=update_medals&auth=%s&bronze=%i&silver=%i&gold=%i&platinum=%i",
			gaanPlayerAuth[anClient],lnBronze,lnSilver,lnGold,lnPlatinum);
	}

	//Copy the new medal counts into the player's array
	for(new lnMedalIndex=0;lnMedalIndex<MEDALS_MAX;lnMedalIndex++)
	{
		gaanPlayerMedals[anClient][lnMedalIndex] = aanMedals[lnMedalIndex];
	}
}

//-------------------------------------------------------------------------
// Non-timer Hooks/Events
//-------------------------------------------------------------------------

//Event called on starting a new map.
public OnMapStart()
{
	new String:lanCurrentMap[MAP_NAME_MAX_LEN];
	gnPlayerManagerEntIdx = GetPlayerManagerEntIndex();

	strcopy(ganCurrentMap,sizeof(ganCurrentMap),"");
	GetCurrentMap(ganCurrentMap,sizeof(ganCurrentMap));
	UrlEncode(ganCurrentMap,ganCurrentMapUrlEncoded,sizeof(ganCurrentMapUrlEncoded));
	
	HttpRequest("&sel=add_map&map=%s",ganCurrentMapUrlEncoded);
		
	GetCurrentMap(lanCurrentMap,sizeof(lanCurrentMap));
	if(StrContains(lanCurrentMap,"kztag_",false) != -1)
	{
		geIsTagMap = true;
		CreateTimer(TAG_FIRST_ROUND_DELAY_SEC, cbRestartTagRound);
	}
	else
	{
		geIsTagMap = false;
	}
}

//Event called on client disconnect.  Reset player related globals.
public OnClientDisconnect_Post(anClient)
{
	gaanPlayerAuth[anClient] = "";
	
	for(new lnMedalIndex=0;lnMedalIndex<MEDALS_MAX;lnMedalIndex++)
	{
		gaanPlayerMedals[anClient][lnMedalIndex] = 0;
	}
	
	ResetClientTagGlobals(anClient);
}

//Event called once the client's Steam ID is known.  Adds the connected player into the database.
public OnClientAuthorized(anClient, const String:aanAuth[])
{
	for(new lnMedalIndex=0;lnMedalIndex<MEDALS_MAX;lnMedalIndex++)
	{
		gaanPlayerMedals[anClient][lnMedalIndex] = 0;
	}
	
	AddPlayerToDatabase(anClient);
}

public Action:ev_PlayerJump(Handle:ahEvent, String:aanName[], bool:aeDontBroadcast)
{
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent, "userid"));
	ganPlayerJumpCount[lnClient]++;
}

public ev_MedalSpend(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	decl String:panItemName[ITEM_NAME_MAX_LEN];
	decl String:panItemNameUrlEncoded[ITEM_NAME_MAX_LEN*4];
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	new lnMedalIndex = GetEventInt(ahEvent,"medal");
	
	//Need to update the player's medal count so that we can tell which type of medal they get next
	if(gaanPlayerMedals[lnClient][lnMedalIndex] > 0)
	{
		gaanPlayerMedals[lnClient][lnMedalIndex]--;
	}
	
	// Item names: timebomb, painkillers, stats_readout, moonboots, strange_mojo, slowmo, soccerball,
	// burningfeet, cloak, fastmo, custom_title, psychic_anti_gravity, boots_of_height
	GetEventString(ahEvent,"itemname",panItemName,sizeof(panItemName));
	
	UrlEncode(panItemName,panItemNameUrlEncoded,sizeof(panItemNameUrlEncoded));
	HttpRequest("&sel=medal_spend&auth=%s&item=%s",gaanPlayerAuth[lnClient],panItemNameUrlEncoded);
}

public ev_GetPowerUp(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	// Item names: timebomb, painkillers, stats_readout, moonboots, strange_mojo, slowmo,
	// burningfeet, cloak, fastmo, custom_title, psychic_anti_gravity, boots_of_height
	
	//"userid" "short"
	//"powerupname" "string" // See item list names above.
	//"poweruplength" "float" // How long, in seconds, the powerup lasts. 0 = infinite.
}

public ev_UserPowerUp(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	// Item names: timebomb, painkillers, stats_readout, moonboots, strange_mojo, slowmo,
	// burningfeet, cloak, fastmo, custom_title, psychic_anti_gravity, boots_of_height
	
	//"userid" "short"
	//"powerupname" "string" // See item list names above. NOTE: If this is strange_mojo, then
	//				     // a player had their actual powerup replaced by strange_mojo in tag arena!
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	ganRoundPowerups[lnClient]++;
}

//-------------------------------------------------------------------------
// Tag events/functions
//-------------------------------------------------------------------------
public ev_PlayerTagged(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	//"tagger" "short" // UserID who is the seeker who tagged someone.
	//"tagged" "short" // UsetID who is the player who was tagged.
	//"taggertags" "short" // The number of tags the seeker has.
	//"taggedtagged" "short" // The number of times this player has been tagged.
	//"taggedtotaltagged" "short"// The total amount of times this player has been tagged this map.
	//"tagger_untagged_time" "short" // The tagger's untagged time.
	//"tagged_untagged_time" "short" // The tagged's untagged time.
	//"ninjatag" "bool" // Whether or not this was a "ninja tag".
	new lnTaggerClientIdx = GetClientOfUserId(GetEventInt(ahEvent,"tagger"));
	new lnTaggedClientIdx = GetClientOfUserId(GetEventInt(ahEvent,"tagged"));
	new bool:leNinjaTag = GetEventBool(ahEvent,"ninjatag");

	if(leNinjaTag)
	{
		ganRoundNinjaTags[lnTaggerClientIdx]++;
	}
	
	ganRoundTags[lnTaggerClientIdx]++;
	
	HttpRequest("&sel=add_tag&tagger_auth=%s&ninja=%i&tagged_auth=%s",
		gaanPlayerAuth[lnTaggerClientIdx],leNinjaTag,gaanPlayerAuth[lnTaggedClientIdx]);

	ganRoundTagged[lnTaggedClientIdx]++;
}

// This event is fired for each player.
public ev_TagRoundEnd(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	//"userid" "short"
	//"winner" "bool"
	//"time_untagged" "short" // Total untagged time this round.
	//"time_untagged_total" "short" // Total untagged time for the entire map.
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	new bool:leWinner = GetEventBool(ahEvent,"winner");
	new lnRoundTimeUntagged = GetEventInt(ahEvent,"time_untagged");
	
	if (IsFakeClient(lnClient))
	{
		return;
	}
	
	CheckTagConVarValidity();
	
	//If the player is valid and has been playing for the entire round
	if(gaePlayingRoundFromStart[lnClient] && geValidTagConVars == true && geIsTagMap == true)
	{
		//Update round-based stats
		HttpRequest("&sel=tag_round_end&auth=%s&winner=%i&round_untagged_time=%i&round_powerups=%i&round_tags=%i&round_tagged=%i",
			gaanPlayerAuth[lnClient],leWinner,lnRoundTimeUntagged,ganRoundPowerups[lnClient],ganRoundTags[lnClient],ganRoundTagged[lnClient]);
	}
}

// This event is fired for each player.
public ev_TagMapEnd(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	//"userid" "short"
	//"winner" "bool"
	//"time_untagged" "short" // Total untagged time this round.
	//"time_untagged_total" "short" // Total untagged time for the entire map.
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	new bool:leWinner = GetEventBool(ahEvent,"winner");
	
	if (IsFakeClient(lnClient))
	{
		return;
	}
	
	CheckTagConVarValidity();
	
	if(gaePlayingRoundFromStart[lnClient] == true && leWinner == true && 
	   geValidTagConVars == true && geIsTagMap == true)
	{
		//Update match winner stats
		HttpRequest("&sel=tag_map_end&auth=%s",gaanPlayerAuth[lnClient]);
	}
}

public ev_TagRoundStart(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	for(new lnClient=1;lnClient<MaxClients+1;lnClient++)
	{
		ResetClientTagGlobals(lnClient);
		
		if(lnClient<MaxClients+1)
		{
			gaePlayingRoundFromStart[lnClient] = IsClientConnected(lnClient);
		}
		else
		{
			gaePlayingRoundFromStart[lnClient] = false;
		}
	}
	
	CheckTagConVarValidity();
}

public Action:cbRestartTagRound(Handle:timer)
{
	ServerCommand("tagarena_restart_round");
	PrintToChatAll("\x04[STATS]\x03 Starting tag match.");
}

ResetClientTagGlobals(anClient)
{
	gaePlayingRoundFromStart[anClient] = false;
	ganRoundTags[anClient] = 0;
	ganRoundTagged[anClient] = 0;
	ganRoundNinjaTags[anClient] = 0;
	ganRoundPowerups[anClient] = 0;
	ganPlayerJumpCount[anClient] = 0;
}

CheckTagConVarValidity()
{
	static Float:prLastLogTime = 0.0;
	
	//If the tag con vars are set to default values
	if(IsTagConVarValid("sv_tagarena_roundtime",SV_TAGARENA_ROUNDTIME_DEFAULT) == true &&
	   IsTagConVarValid("sv_tagarena_roundcount",SV_TAGARENA_ROUNDCOUNT) == true)
	{
		geValidTagConVars = true;
	}
	else
	{
		geValidTagConVars = false;
		
		new Float:lrCurrentTime = GetGameTime();
		
		//Prevent log spam when events are called all at once and run these checks
		if(lrCurrentTime - prLastLogTime > 100.0)
		{
			PrintToChatAll("\x04[STATS]\x03 Tag stats disabled due to non-default settings.");
			prLastLogTime = lrCurrentTime;
		}
	}
}

bool:IsTagConVarValid(String:aanConVarName[],anExpectedValue)
{
	new bool:leValid = false;
	new Handle:lhHandle = FindConVar(aanConVarName);
	
	if(lhHandle != INVALID_HANDLE)
	{
		if(anExpectedValue == GetConVarInt(lhHandle))
		{
			leValid = true;
		}
	}
	else
	{
		LogDebug("ConVar (%s) not set to expected value (%i)",aanConVarName,anExpectedValue);
	}

	return leValid;
}

//-------------------------------------------------------------------------
// Timer specific events, functions and callbacks to insert records
//-------------------------------------------------------------------------

CheckMpCheckpointUsage(anClient)
{
	//always return false if not an MP timer
	new lnDidTeamUseCheckpoints = 0;
	
	//If this was an MP timer
	if(ganMpMode[anClient] > 0)
	{
		//For each possible player on the team
		for(new i=0;i<MAX_PLAYERS_PER_TEAM;i++)
		{
			//If the player is valid
			if(gaanMpClientIndexes[anClient][i] > 0)
			{
				//If one of the team members used teleports
				if(GetPlayerProperty(gaanMpClientIndexes[anClient][i],"m_iTeleports") != 0)
				{
					lnDidTeamUseCheckpoints = 1;
					break;
				}
			}
		}
	}
	
	return lnDidTeamUseCheckpoints;
}

//Event called when the player starts a course.
public ev_StartTimer(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	//"userid"	"short"
	//"courseid"	"short"
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent, "userid"));
	ganPlayerJumpCount[lnClient] = 0;
	
	ganMpMode[lnClient] = -1;
	gaanMpClientIndexes[lnClient][0] = -1;
	gaanMpClientIndexes[lnClient][1] = -1;
	gaanMpClientIndexes[lnClient][2] = -1;
	gaanMpClientIndexes[lnClient][3] = -1;
}

//Event called when a player completes a course.  Stores the record in the database.
public ev_StopTimer(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	new lnCourseId = GetEventInt(ahEvent,"courseid");
	new Float:lrTime = GetEventFloat(ahEvent,"time");
	new lnChecks = GetEventInt(ahEvent,"checkpoints");
	new lnTeles = GetEventInt(ahEvent,"teleports");
	lrTime += GetEventInt(ahEvent,"milliseconds") * CONV_EVENT_MSEC_TO_SEC;
	new lanMedals[MEDALS_MAX] = {0,...};
	
	decl String:panCourseName[COURSE_NAME_MAX_LEN];

	if (IsFakeClient(lnClient))
	{
		return;
	}
	
	//Try to get the course name and add record if success
	if(GetCourseNameOld(lnCourseId,panCourseName,sizeof(panCourseName)) == true)
	{
		//Attempt to insert the record to the database
		ProcessPlayerRecord(lnClient,lrTime,lnChecks,lnTeles,panCourseName);
		
		lanMedals[eeMedalGold] = GetEventInt(ahEvent,"goldmedals");
		lanMedals[eeMedalSilver] = GetEventInt(ahEvent,"silvermedals");
		lanMedals[eeMedalPlatinum] = GetEventInt(ahEvent,"platinummedals");
		lanMedals[eeMedalBronze] = GetEventInt(ahEvent,"bronzemedals");
		
		//Note: If the plugin is reloaded in mid game, players will be awarded for up to one medal of each type received
		//      in that play session upon finishing their next course.  This "issue" cannot be solved via SM.
		UpdatePlayerMedals(lnClient,lanMedals);
	}
	else
	{
		LogDebug("ev_StopTimer: Failed to get course name for course id %i",lnCourseId);
	}
}

//Event called when the player starts a course.
public ev_StartTimer2(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	//"userid"	"short" // The userid of the player who started the timer.
	//"coursename" "string" // The name of the course.
	//"starttime" "float" // The current game time when the player started the timer (NOT gpGlobals->curtime)
	//"multiplayermode" "short" // Is this a multi-player timer? 
	////If this is greater than 0, add 1 to get the max player count. 
	////For example, if  this is 1, add 1 = 2 player course.
	//"multiplayer1" "short" // The userid of the first player in this multiplayer course (0 if not valid)
	//"multiplayer2" "short" // The userid of the second player in this multiplayer course (0 if not valid)
	//"multiplayer3" "short" // The userid of the third player in this multiplayer course (0 if not valid)
	//"multiplayer4" "short" // The userid of the fourth player in this multiplayer course (0 if not valid)
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent, "userid"));
	ganPlayerJumpCount[lnClient] = 0;
	
	ganMpMode[lnClient] = GetEventInt(ahEvent, "multiplayermode");
	gaanMpClientIndexes[lnClient][0] = GetEventInt(ahEvent, "multiplayer1");
	gaanMpClientIndexes[lnClient][1] = GetEventInt(ahEvent, "multiplayer2");
	gaanMpClientIndexes[lnClient][2] = GetEventInt(ahEvent, "multiplayer3");
	gaanMpClientIndexes[lnClient][3] = GetEventInt(ahEvent, "multiplayer4");
}

//Event called when a player completes a course.  Stores the record in the database.
public ev_StopTimer2(Handle:ahEvent,const String:aanEventName[],bool:aeDontBroadcast)
{
	new lnClient = GetClientOfUserId(GetEventInt(ahEvent,"userid"));
	new Float:lrTime = GetEventFloat(ahEvent,"time");
	new lnChecks = GetEventInt(ahEvent,"checkpoints");
	new lnTeles = GetEventInt(ahEvent,"teleports");
	lrTime += GetEventInt(ahEvent,"milliseconds") * CONV_EVENT_MSEC_TO_SEC;
	
	new lanMedals[MEDALS_MAX] = {0,...};
	
	new String:lanCourseName[COURSE_NAME_MAX_LEN];
	GetEventString(ahEvent,"coursename",lanCourseName,sizeof(lanCourseName));

	if (IsFakeClient(lnClient))
	{
		return;
	}
	
	//Attempt to insert the record to the database
	ProcessPlayerRecord(lnClient,lrTime,lnChecks,lnTeles,lanCourseName);
	
	lanMedals[eeMedalGold] = GetEventInt(ahEvent,"goldmedals");
	lanMedals[eeMedalSilver] = GetEventInt(ahEvent,"silvermedals");
	lanMedals[eeMedalPlatinum] = GetEventInt(ahEvent,"platinummedals");
	lanMedals[eeMedalBronze] = GetEventInt(ahEvent,"bronzemedals");
	
	//Note: If the plugin is reloaded in mid game, players will be awarded for up to one medal of each type received
	//      in that play session upon finishing their next course.  This "issue" cannot be solved via SM.
	UpdatePlayerMedals(lnClient,lanMedals);
}

//Prep the record for processing
public ProcessPlayerRecord(anClient,Float:arTime,anChecks,anTeles,String:aanCourseName[])
{
	new lnJumps = ganPlayerJumpCount[anClient];
	new lnFlags = 0; //For future expansion
	new lnTeammateUsedCheckpoint = CheckMpCheckpointUsage(anClient);
	decl String:panCourseNameUrlEncoded[COURSE_NAME_MAX_LEN*4];
	UrlEncode(aanCourseName,panCourseNameUrlEncoded,sizeof(panCourseNameUrlEncoded));
	
	HttpRequest("&sel=add_record&auth=%s&map=%s&course=%s&tele=%i&cp=%i&jumps=%i&flags=%i&time=%f&teamcheck=%i",
		gaanPlayerAuth[anClient],ganCurrentMapUrlEncoded,panCourseNameUrlEncoded,anTeles,anChecks,lnJumps,lnFlags,arTime,lnTeammateUsedCheckpoint);
	
	UpdatePlayerInfoInDatabase(anClient);
}


