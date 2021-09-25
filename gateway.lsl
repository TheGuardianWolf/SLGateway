string API_KEY = "SLGAPI-Babavjnxb_oWjHzasurN";

integer DEBUG = TRUE;
float HEARTBEAT_INTERVAL_SEC = 900; // 1 hour
integer TOKEN_LENGTH = 12;

integer registered = FALSE;
key objectId;

key regRequestId;
key urlRequestId;
key eventRequestId;
string objectUrl;
string currentToken;
string serverApiUrl = "http://pixelcollider.net/api";

debugSay(string text)
{
    if (DEBUG)
    {
        llOwnerSay(text);
    }
}

integer startswith(string haystack, string needle) // http://wiki.secondlife.com/wiki/llSubStringIndex
{
    return llDeleteSubString(haystack, llStringLength(needle), 0x7FFFFFF0) == needle;
}

// From http://wiki.secondlife.com/wiki/Random_Password_Generator
string generateToken()
{
    integer length = TOKEN_LENGTH;
    string CharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnoppqrstuvwxyz123456789_";    // omitting confusable characters
    string password;
    integer CharSetLen = llStringLength(CharSet);
    // Note: We do NOT add 1 to the length, because the range we want from llFrand() is 0 to length-1 inclusive

    while (length--)
    {
        integer rand = (integer) llFrand(CharSetLen);
        password += llGetSubString(CharSet, rand, rand);
    }
    return password;
}

integer verifyToken(key requestId)
{
    // I don't know why SL doesn't accept the Authorization header but here we are.
    string userAgents = llGetHTTPHeader(requestId, "user-agent");
    debugSay("Auth header received: " + userAgents);

    list userAgentList = llParseString2List(userAgents, [" "], []);

    integer i = llGetListLength(userAgentList);
    while (i--)
    {
        list productInfo = llParseString2List(llList2String(userAgentList, i), ["/"], []);

        if (llGetListLength(productInfo) == 2)
        {
            if (llToLower(llList2String(productInfo, 0)) == "slotoken")
            {
                if (llList2String(productInfo, 1) == currentToken)
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}


list readEventData(string eventData)
{
    debugSay("Read: " + eventData);
    list data = llJson2List(eventData);
    return data;
}

key post(string path, string jsonData)
{
    return llHTTPRequest(serverApiUrl + "/" + path,
        [
            HTTP_METHOD, "POST",
            HTTP_MIMETYPE, "application/json",
            HTTP_CUSTOM_HEADER, "Authorization", "Bearer " + (string)API_KEY
        ],
        jsonData);
}

jsonResponse(key requestId, integer status, string data)
{
    llSetContentType(requestId, CONTENT_TYPE_JSON);
    llHTTPResponse(requestId, status, data);
}

string jsonString(string str)
{
    return "\"" + str + "\"";
}

badRequest(key requestId)
{
    jsonResponse(requestId, 400, "{}");
}

unsupportedMediaType(key requestId)
{
    jsonResponse(requestId, 415, "{}");
}

ok(key requestId)
{
    jsonResponse(requestId, 200, "{}");
}

okPayload(key requestId, string jsonData)
{
    jsonResponse(requestId, 200, jsonData);
}

unauthorized(key requestId)
{
    jsonResponse(requestId, 401, "{}");
}

notFound(key requestId)
{
    jsonResponse(requestId, 404, "{}");
}

init()
{
    objectId = llGetKey();
    urlRequestId = llRequestURL();
}

registerWithGateway(string objectUrl)
{
    currentToken = generateToken();
    string jsonData = llList2Json(JSON_OBJECT, ["url", objectUrl, "token", currentToken, "apiKey", API_KEY]);
    regRequestId = post("object/register/" + (string)objectId, jsonData);
}

registrationHandler(key requestId, integer status)
{
    if (status == 200)
    {
        debugSay("Object registered as: " + (string)objectId);

        if (!registered)
        {
            // Establish one hour heartbeat with token exchange
            llSetTimerEvent(HEARTBEAT_INTERVAL_SEC);
        }
        registered = TRUE;
    }
    else
    {
        debugSay("Object registration failed with status: " + (string)status);
        registered = FALSE;
    }
}

urlRequestHandler(key requestId, string requestStatus, string url)
{
    if (requestStatus == URL_REQUEST_DENIED)
    {
        debugSay("The following error occurred while attempting to get a URL for this object:\n \n" + url);
    }
    else if (requestStatus == URL_REQUEST_GRANTED)
    {
        objectUrl = url;
        debugSay("Url retrieved: " + objectUrl);

        // Register object and url with gateway
        registerWithGateway(objectUrl);
    }
}

eventRequest(integer eventCode, list args)
{
    string jsonArgs = llList2Json(JSON_ARRAY, args);
    string jsonData = llList2Json(JSON_OBJECT, ["code", eventCode, "args", jsonArgs]);
    debugSay("Sent: " + jsonData);
    eventRequestId = post("events/receive/" + (string)objectId, jsonData);
}

// Command Event Codes
integer CEC_LLOwnerSay = 0;
integer CEC_LLSay = 1;
integer CEC_LLRegionSay = 2;
integer CEC_LLRegionSayTo = 3;
integer CEC_LLApplyRotationalImpulse = 4;
integer CEC_LLListen = 5;
integer CEC_LLListenRemove = 6;
integer CEC_LLDie = 7;
integer CEC_LLEjectFromLand = 8;
integer CEC_LLGetOwner = 9;
integer CEC_LLGetAgentList = 10;
integer CEC_LLGetAgentInfo = 11;
integer CEC_LLRequestAgentData = 12;
integer CEC_LLRequestDisplayName = 13;

// Object Event Codes
integer OEC_Listen = 0;
integer OEC_Dataserver = 1;

// Internal codes
integer INVALID = -1;
integer NOT_HANDLED = -2;
integer OK = 0;

integer commandSwitch(key requestId, list args)
{
    integer eventCode = (integer)llList2String(args, 0);
    integer argLength = llGetListLength(args);
    if (eventCode == CEC_LLOwnerSay)
    {
        if (argLength < 1)
        {
            return INVALID;
        }

        string text = llList2String(args, 1);
        llOwnerSay(text);
        ok(requestId);
    }
    else if (eventCode == CEC_LLSay)
    {
        if (argLength < 2)
        {
            return INVALID;
        }

        integer channel = llList2Integer(args, 1);
        string msg = llList2String(args, 2);
        llSay(channel, msg);
        ok(requestId);
    }
    else if (eventCode == CEC_LLRegionSay)
    {
        if (argLength < 2)
        {
            return INVALID;
        }

        integer channel = llList2Integer(args, 1);
        string msg = llList2String(args, 2);
        llRegionSay(channel, msg);
        ok(requestId);
    }
    else if (eventCode == CEC_LLRegionSayTo)
    {
        if (argLength < 3)
        {
            return INVALID;
        }

        key target = llList2Key(args, 1);
        integer channel = llList2Integer(args, 2);
        string msg = llList2String(args, 3);
        llRegionSayTo(target, channel, msg);
        ok(requestId);
    }
    else if (eventCode == CEC_LLApplyRotationalImpulse)
    {
        if (argLength < 4)
        {
            return INVALID;
        }

        float forceX = llList2Float(args, 1);
        float forceY = llList2Float(args, 2);
        float forceZ = llList2Float(args, 3);
        integer local = llList2Integer(args, 4);
        vector force = <forceX, forceY, forceZ>;
        llApplyRotationalImpulse(force, local);
        ok(requestId);
    }
    else if (eventCode == CEC_LLListen)
    {
        if (argLength < 4)
        {
            return INVALID;
        }

        integer channel = llList2Integer(args, 1);
        string name = llList2String(args, 2);
        key id = llList2Key(args, 3);
        string msg = llList2String(args, 4);
        integer retVal = llListen(channel, name, id, msg);
        okPayload(requestId, (string)retVal);
    }
    else if (eventCode == CEC_LLListenRemove)
    {
        if (argLength < 1)
        {
            return INVALID;
        }

        integer handle = llList2Integer(args, 1);
        llListenRemove(handle);
        ok(requestId);
    }
    else if (eventCode == CEC_LLDie)
    {
        // This is the only one that has its request/function call reversed
        ok(requestId);
        llDie();
    }
    else if (eventCode == CEC_LLEjectFromLand)
    {
        if (argLength < 1)
        {
            return INVALID;
        }

        key avatar = llList2Key(args, 1);
        llEjectFromLand(avatar);
        ok(requestId);
    }
    else if (eventCode == CEC_LLGetOwner)
    {
        okPayload(requestId, jsonString(llGetOwner()));
    }
    else if (eventCode == CEC_LLGetAgentList)
    {
        if (argLength < 2)
        {
            return INVALID;
        }

        integer scope = llList2Integer(args, 1);
        list li = llDeleteSubList(args, 0, 1);
        list result = llGetAgentList(scope, li);
        okPayload(requestId, llList2Json(JSON_ARRAY, result));
    }
    else if (eventCode == CEC_LLGetAgentInfo)
    {
        if (argLength < 1)
        {
            return INVALID;
        }

        key id = llList2Key(args, 1);
        integer result = llGetAgentInfo(id);
        // Json number
        okPayload(requestId, (string)result);
    }
    else if (eventCode == CEC_LLRequestAgentData)
    {
        if (argLength < 2)
        {
            return INVALID;
        }

        key id = llList2Key(args, 1);
        integer data = llList2Integer(args, 2);
        key result = llRequestAgentData(id, data);
        okPayload(requestId, jsonString(result));
    }
    else if (eventCode == CEC_LLRequestDisplayName)
    {
        if (argLength < 1)
        {
            return INVALID;
        }

        key id = llList2Key(args, 1);
        key result = llRequestDisplayName(id);
        okPayload(requestId, jsonString(result));
    }
    else
    {
        return NOT_HANDLED;
    }

    return OK;
}

commandEventHandler(key requestId, string data)
{
    list parsedData = readEventData(data);
    integer dataLength = llGetListLength(parsedData);
    integer argLength = dataLength - 1;

    if (dataLength < 1)
    {
        badRequest(requestId);
        return;
    }

    integer result = commandSwitch(requestId, parsedData);

    if (result != OK)
    {
        if (result == NOT_HANDLED)
        {
            notFound(requestId);
            return;
        }
        else if (result == INVALID)
        {
            badRequest(requestId);
            return;
        }
    }
}

default
{
    state_entry()
    {
        if (API_KEY)
        {
            init();
        }
        else
        {
            llSay(0, "Warning: Object is not initialised due to missing API_KEY! Please edit the script and set this on the first line.");
        }
    }

    // Heartbeat timer
    timer()
    {
        if (registered)
        {
            registerWithGateway(objectUrl);
        }
    }

    // Handle remote server response
    http_response(key requestId, integer status, list metadata, string body)
    {
        if (requestId) {
            if (requestId == regRequestId)
            {
                registrationHandler(requestId, status);
            }
            else if (requestId == eventRequestId)
            {
                debugSay("Event sent with status: " + (string)status + "\n\nResponse: " + body);
            }
        }
    }

    // Handle remote server request
    http_request(key requestId, string method, string body)
    {
        if (requestId)
        {
            if (requestId == urlRequestId)
            {
                urlRequestHandler(requestId, method, body);
            }
            else if (method == "POST" && registered)
            {
                if (verifyToken(requestId))
                {
                    commandEventHandler(requestId, body);
                }
                else
                {
                    unauthorized(requestId);
                }
            }
        }
    }

    listen(integer channel, string name, key id, string message)
    {
        if (registered)
        {
            eventRequest(OEC_Listen, [channel, name, id, message]);
        }
    }

    dataserver(key queryId, string data)
    {
        if (registered)
        {
            eventRequest(OEC_Dataserver, [data]);
        }
    }

    changed(integer change)
    {
        if (change & (CHANGED_OWNER | CHANGED_INVENTORY))
        {
            llReleaseURL(objectUrl);
            objectUrl = "";

            init();
        }

        if (change & (CHANGED_REGION_START | CHANGED_REGION))
        {
            // Reinitialise in this case as url is invalidated
            init();
        }
    }

    on_rez(integer startParam)
    {
        init();
    }
}
