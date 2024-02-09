# KasaRESTAPI

This is a simple REST API that allows you to communicate with TP-Link Kasa devices on your local network. They don't need to be registered with an online account. They just need to be on the same network as your computer.

## Usage

Run KasaRESTAPI, and it will start listening on `localhost:5000`. It will search your local network for any Kasa devices in the background.

## REST API

Any of the following endpoints can return an error response, which indicates an error either with the request or with the server. In these cases, the HTTP status code will be set to an error status code (4xx), and the endpoint will return JSON data in the following format:

```json
{
  "error": "some helpful text describing the error"
}
```

If the HTTP status code is a 2xx code, you can assume the request was successful.

### Finding Devices

As noted earlier, KasaRESTAPI continuously scans your local network for Kasa devices in the background, so if any new devices join the network after the server starts up, they'll be detected.

You can view a list of devices it's found via the `/api/devices` endpoint, which will return a list of device names and MAC addresses like this:

```json
[
  {
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "name": "Smart Wi-Fi Plug Mini"
  },
  {
    "macAddress": "FF:EE:DD:CC:BB:AA",
    "name": "Smart Wi-Fi Plug Mini"
  }
]
```

Once you've identified the device you want to communicate with, you'll use its MAC address in later requests. Replace `MAC_ADDRESS` in the following examples with the MAC address of the device in question.

### Energy Monitoring

To get the energy monitoring stats from smart plugs that support this feature, use the `/api/MAC_ADDRESS/energymonitor` endpoint. You'll receive data like this:

```json
{
  "voltage": 119.222,
  "current": 1.279,
  "power": 135.435,
  "total": 47.367
}
```

### Outlet

To view the state of a smart plug's outlet, use the `/api/MAC_ADDRESS/outlet` endpoint. You'll receive a response like this:

```json
{
  "state": false
}
```

You can also turn the outlet `on`, `off`, or `toggle` it from this endpoint by adding the new state after `outlet`, like `/api/MAC_ADDRESS/outlet/toggle`.

### Countdown Rules

To retrieve all countdown rules that are stored on a smart plug, send a `GET` request to `/api/MAC_ADDRESS/countdown/all`. You'll receive a list of rules like this:

```json
[
  {
    "id": "0123456789ABCDEF0123456789ABCDEF",
    "name": "name of rule",
    "enabled": false,
    "delay": 0,
    "action": false,
    "remain": 0
  }
]
```

Likewise, to delete all countdown rules, send a `DELETE` request to `/api/MAC_ADDRESS/countdown/all`.

To delete a specific countdown rule, get its ID from the list of countdown rules, then send a `DELETE` request to `/api/MAC_ADDRESS/countdown/RULE_ID`.

To create a new countdown rule, send a `POST` request to `/api/MAC_ADDRESS/countdown/new`, and include a JSON body with the following fields:

|key      |type  |value                                                                                           |
|:--------|:-----|:-----------------------------------------------------------------------------------------------|
|`name`   |string|Name of the countdown rule (32 characters max)                                                  |
|`enabled`|bool  |Whether this countdown should start as soon as the device receives it (`true`) or not (`false`) |
|`delay`  |int   |How many seconds the countdown should take                                                      |
|`action` |bool  |Whether the smart plug's outlet should turn on (`true`) or off (`false`) once the countdown ends|

## Energy Monitor Web UI

I wrote this REST API to make it easier to interface with smart plugs that have energy monitoring to get a better idea of how much power certain devices were drawing. To make this easier to visualize, I wrote a small web UI that uses the API and Google Charts to show energy monitoring stats.

To access it, simply run KasaRESTAPI and visit `http://localhost:5000` in a browser. It will then let you pick from a list of devices that the server was able to locate on the network, and graph their energy monitoring stats.

![Screenshot of the Energy Monitor Web UI](webui.png)
