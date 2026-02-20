# ERPIO One Plugin Example (.NET Framework 4.8)

This repository contains a fully functional example plugin for **ERPIO One Gateway**, implemented using **.NET Framework 4.8**.  
The plugin demonstrates two independent data providers (SQL‑based and WebService‑based), logging, batch processing, request parameter handling, and optional full‑table search.

---

## Features

### ✔ Two data models (providers)
The plugin exposes two independent models through `GetAvailableModels()`:

1. **SQL Provider (`ProviderSQL`)**  
   - Internal SQLite storage  
   - Table auto‑creation on startup  
   - `SELECT / INSERT / UPDATE / DELETE`  
   - Three actions: New, Edit, Delete  
   - Parameterized SQL execution  
   - Supports schema‑only mode and custom search

2. **Web Service Provider (`ProviderWS`)**  
   - Token retrieval via OAuth2 client_credentials  
   - Document receive (GET)  
   - Document send (POST)  
   - Strict parameter validation  
   - JSON serialization/deserialization  
   - Converts WS responses into a typed `DataTable`

---

## Plugin Class (`Plugin.cs`)

The main plugin class implements `IgwPlugin1` and provides:

### **Metadata**
- `PluginVersion` – must be updated on each change  
- `PluginGID` – unique static GUID  
- `PluginName` – internal short identifier  
- `PluginDescription` – human‑readable description  

### **Model registration**
`GetAvailableModels()` returns both providers (SQL + WS) with their:
- Model GUIDs  
- Public and system names  
- Descriptions  
- Available parameters  
- Available actions  

### **Data retrieval**
`GetDataTable(request, host)`:
- Detects which provider is requested  
- Executes SQL or WS logic accordingly  
- Executes CRUD actions when `PluginGIDAction` is set  
- Applies `Tools.CustomSearch()` filtering  
- Returns a single DataTable inside a DataSet

### **Batch processing**
`SetLargeDataTable()`:
- Handles “datarepeater / pump” operations  
- Maps incoming DataTable rows into repeated plugin requests  
- Supports SQL CRUD + WS Send  
- Logs any errors per row  

### **Initialization**
`RunPluginConfiguration()`:
- Configures TLS and certificate validation for HttpClient  
- Prepares internal SQLite tables for logging and SQL provider

---

## ProviderSQL

Implements a simple internal database using the gateway’s SQLite connection.

### Capabilities
- Creates the table:  
  `id, accountCode, department, year, amount`
- Supports parameters: `@Year`, `@Department`
- Actions: New, Edit, Delete
- Uses static SQL with parameters
- Select statements respect schema‑only requests (`__schemaonly`)
- Uses helper methods from `Tools` for param handling & SQL execution

---

## ProviderWS

Communicates with external endpoints under `https://www.sapi-sk.sk/sapi/`.

### Receive (GET)
Requires:
- `@ClientId`
- `@ClientSecret`
- `@ParticipantId`

Flow:
1. Retrieve OAuth token  
2. Call `/document/receive?limit=20`  
3. Validate HTTP response  
4. Convert JSON to typed DataTable

### Send (POST)
Requires:
- IDs of sender/receiver participants  
- `@DocumentId`  
- `@Document` (XML payload)

Flow:
1. Retrieve OAuth token  
2. Build metadata + payload object  
3. POST to `/document/send` with Idempotency-Key  
4. Validate returned JSON (`status == ACCEPTED`)

---

## Tools

Utility class containing shared helpers.

### Included functionality
- **CustomSearch**: full‑table case‑insensitive substring filtering using `__whereany`
- SQL helpers: `ExecuteSelect`, `ExecuteQuery`, `ExecuteSave`
- Parameter accessors: string/int/double/bool
- Schema‑only detection: `__schemaonly`
- Demo fallback table for unknown model requests

---

## Logging

`Log` class implements a structured plugin‑internal logging mechanism.

### Features
- Auto‑creates internal SQLite log table at startup  
- `WriteLog()` with timestamp, method, location, message, type  
- Optional verbose logging using parameter `@verboseLog`  
- Sanitizes multiline messages (removes newlines)

---

## Requirements

- ERPIO One Gateway  
- .NET Framework 4.8  
- Visual Studio 2019+  
- External internet connectivity for WS provider (if needed)

---

## Installation

1. Build the plugin in **Release** mode.  
2. Copy the resulting DLL to: /plugins/e1_pluginexample/
3. Restart ERPIO Gateway.  
4. The plugin will register both providers automatically.

Support: support@erpio.one
