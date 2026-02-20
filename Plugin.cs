using ERPIO.AppSDK.Shared.Enums;
using ERPIO.AppSDK.Shared.Interfaces;
using ERPIO.AppSDK.Shared.Models;
using ERPIO.AppSDK.Shared.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace e1_pluginexample
{
    public class Plugin : IgwPlugin1
    {
        public static HttpClient Client = new HttpClient();

        // version of the plugin, should be updated with every change to the plugin
        // mandatory from IgwPlugin1
        public string PluginVersion
        {
            get => "1.1";
        }

        // CHANGELOG
        // 1.0 - Initial version
        // 1.1 - Addded Custom search

        // should be new for every plugin
        // mandatory from IgwPlugin1
        public Guid PluginGID
        {
            get
            {
                return Guid.Parse("19016041-24AB-4297-B9C8-1DE9138075DB");
            }
        }

        // short name, should be unique for every plugin
        // mandatory from IgwPlugin1
        public string PluginName
        {
            get
            {
                return "e1_pluginexample";
            }
        }

        // description of the plugin
        // mandatory from IgwPlugin1
        public string PluginDescription
        {
            get
            {
                return "Example of ERPIO One plugin";
            }
        }

        // this method should return a list of models that the plugin provides
        // mandatory from IgwPlugin1
        public List<PluginProvider1> GetAvailableModels(IPluginHost IHost)
        {
            var pluginProviders = new List<PluginProvider1>();

            PluginProvider1 sqlprovider = new PluginProvider1();
            sqlprovider.PluginGIDModel = ProviderSQL.PluginGIDModel;
            sqlprovider.PubName = ProviderSQL.PubName;
            sqlprovider.SysName = ProviderSQL.SysName;
            sqlprovider.Description = ProviderSQL.Description;
            sqlprovider.AvailableParams = ProviderSQL.AvailableParams;
            sqlprovider.AvailableActions = ProviderSQL.AvailableActions;

            pluginProviders.Add(sqlprovider);

            PluginProvider1 wsprovider = new PluginProvider1();
            wsprovider.PluginGIDModel = ProviderWS.PluginGIDModel;
            wsprovider.PubName = ProviderWS.PubName;
            wsprovider.SysName = ProviderWS.SysName;
            wsprovider.Description = ProviderWS.Description;
            wsprovider.AvailableParams = ProviderWS.AvailableParams;
            wsprovider.AvailableActions = ProviderWS.AvailableActions;

            pluginProviders.Add(wsprovider);

            return pluginProviders;
        }

        // main method for getting data from the plugin, used in datasource, should return a DataSet with the data for the requested model
        // mandatory from IgwPlugin1
        public DataSet GetDataTable(PluginRequest1 request, IPluginHost IHost)
        {
            DataSet pluginDataSet = new DataSet("DB");
            DataTable dt = null;

            // Provider SQL
            if (request.PluginGIDModel == ProviderSQL.PluginGIDModel)
            {
                // if action is called, don't return data, after action is performed, the source system will call refresh withou PluginGIDAction, so the updated data will be returned then
                if (request.PluginGIDAction == ProviderSQL.ActionNewGuid)
                {
                    ProviderSQL.Add(request, IHost);                    
                }
                else if (request.PluginGIDAction == ProviderSQL.ActionEditGuid)
                {
                    ProviderSQL.Edit(request, IHost);
                }
                else if (request.PluginGIDAction == ProviderSQL.ActionDeleteGuid)
                {
                    ProviderSQL.Delete(request, IHost);
                }
                else
                {
                    dt = ProviderSQL.GetTable(request, IHost);
                }
            }
            // Provider WS
            else if (request.PluginGIDModel == ProviderWS.PluginGIDModel)
            {
                // if action is called, don't return data, after action is performed, the source system will call refresh withou PluginGIDAction, so the updated data will be returned then
                if (request.PluginGIDAction == ProviderWS.ActionSendGuid)
                {
                    ProviderWS.Send(request, IHost);
                }
                else
                {
                    dt = ProviderWS.GetTable(request, IHost);
                }
            }
            // Unknown plugin provider
            else
            {
                dt = Tools.CreateDemoDataTable(this, request);
            }

            dt = Tools.CustomSearch(request, dt);

            if (dt == null)
            {
                dt = new DataTable();
            }

            pluginDataSet.Tables.Add(dt);
            return pluginDataSet;
        }

        // executed during gateway startup, use this method to initialize your plugin
        // mandatory from IgwPlugin1
        public void RunPluginConfiguration(IPluginHost IHost)
        {
            // configuration for HttpClient() 1x for whole instance (https://stackoverflow.com/questions/52150004/httpclient-does-not-use-servicepointmanager-service-points), can be different according the version of .NET
            // check if really needed - this is not recommended to be used in general, but some web services might require specific settings, e.g. TLS 1.2 or ignoring certificate errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                                                                                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                                                                System.Security.Cryptography.X509Certificates.X509Chain chain,
                                                                                System.Net.Security.SslPolicyErrors sslPolicyErrors)
            { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = // SecurityProtocolType.Tls |
                                                   // SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12
                                                   // | SecurityProtocolType.Tls13 // turn on if supported by OS/Schannel
                                                   // | SecurityProtocolType.Ssl3
                                                 ;

            Log.PrepareTable(IHost);
            ProviderSQL.PrepareTable(IHost);
        }

        // destination for actions of type Datarepeater (pump) for batch processing
        // mandatory from IgwPlugin1
        public void SetLargeDataTable(PluginRequest1 request, DataTable tab, IPluginHost IHost)
        {
            // mapping of destination parameters (with @) to column caption, e.g..: "ParName": "@invoiceType", "ValName": "BookingName"
            foreach (DataColumn col in tab.Columns)
                if (request.Params.Params.Exists(p => p.ValName != null && p.ValName.Equals(col.ColumnName) && p.Val == null))
                    col.Caption = request.Params.Params.Where(p => p.ValName != null && p.ValName.Equals(col.ColumnName)).First().ParName;

            foreach (DataRow row in tab.Rows)
            {
                try
                {
                    PluginRequest1 docRequest = new PluginRequest1();
                    docRequest.Params = new ParamsObject1();
                    docRequest.Params.Params = new List<EParams1>();
                    docRequest.SQLcmd = request.SQLcmd;

                    for (int i = 0; i < row.ItemArray.Count(); i++)
                    {
                        object sValue = row[i] != null ? row[i] : null;
                        if (sValue != null)
                        {
                            docRequest.Params.Params.Add(new EParams1()
                            {
                                ParName = string.IsNullOrEmpty(row.Table.Columns[i].Caption) ? row.Table.Columns[i].ColumnName : row.Table.Columns[i].Caption,
                                Val = sValue
                            });
                        }
                    }

                    foreach (var par in request.Params.Params)
                    {
                        if (par.ValName != null && row.Table.Columns.Contains(par.ValName))
                        {
                            object sValue = par.Val != null ? par.Val : row[par.ValName] != null ? row[par.ValName] : null;
                            if (sValue != null)
                            {
                                if (!docRequest.Params.Params.Any(p => p.ParName == par.ParName))
                                {
                                    docRequest.Params.Params.Add(new EParams1()
                                    {
                                        ParName = par.ParName,
                                        Val = sValue
                                    });
                                }
                            }
                        }
                    }

                    if (request.PluginGIDModel == ProviderSQL.PluginGIDModel && request.PluginGIDAction == ProviderSQL.ActionNewGuid)
                    {
                        ProviderSQL.Add(docRequest, IHost);
                    }
                    else if (request.PluginGIDModel == ProviderSQL.PluginGIDModel && request.PluginGIDAction == ProviderSQL.ActionEditGuid)
                    {
                        ProviderSQL.Edit(docRequest, IHost);
                    }
                    else if (request.PluginGIDModel == ProviderSQL.PluginGIDModel && request.PluginGIDAction == ProviderSQL.ActionDeleteGuid)
                    {
                        ProviderSQL.Delete(docRequest, IHost);
                    }
                    else if (request.PluginGIDModel == ProviderWS.PluginGIDModel && request.PluginGIDAction == ProviderWS.ActionSendGuid)
                    {
                        ProviderWS.Send(docRequest, IHost);
                    }
                    else
                    {
                        System.IO.StringWriter writer = new System.IO.StringWriter();
                        tab.WriteXml(writer, XmlWriteMode.WriteSchema, true);

                        throw new Exception("SetLargeDataTable " + tab.Rows.Count.ToString() + " | Data : " + writer.ToString());
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLog("SetLargeDataTable", Log.MessageType.ERROR, e.Message + " Detail: " + e.InnerException?.Message, IHost);
                }
            }

        }
    }

    public static class ProviderSQL
    {
        private static string ParamNameAccountCode = "@AccountCode";
        private static string ParamNameDepartment = "@Department";
        private static string ParamNameYear = "@Year";
        private static string ParamNameAmount = "@Amount";
        private static string ParamNameID = "@ID";

        public static Guid PluginGIDModel = Guid.Parse("B1A0F1C2-3D4E-5678-9ABC-DEF012345678");
        public const string PubName = "ExampleModelSQL";
        public const string SysName = "e1_pluginexample_modelsql";
        public const string Description = "Example model using SQL for ERPIO One plugin";
        public static ParamsObject1 AvailableParams = new ParamsObject1()
        {
            Params = new List<EParams1>()
            {
                new EParams1()
                {
                    ParName = ParamNameYear,
                    DType = enDataType.Int
                },
                new EParams1()
                {
                    ParName = ParamNameDepartment,
                    DType = enDataType.String
                }
            }
        };
        public static Guid ActionNewGuid = Guid.Parse("CE069144-5FB2-4893-B8FE-22AF382DDFF6");
        public static Guid ActionEditGuid = Guid.Parse("54A9211C-49C9-4CFB-B37E-09CC590B4405");
        public static Guid ActionDeleteGuid = Guid.Parse("ED3B23E1-258B-47EE-98F0-5A45C2ECCA37");
        public static List<PluginProvider1> AvailableActions = new List<PluginProvider1>()
        {
            new PluginProvider1()
            {
                SysName = "NewRecord",
                PubName = "New",
                Description = "Creates a new record",
                PluginGIDModel = ActionNewGuid,
                AvailableParams = new ParamsObject1()
                {
                    Params = new List<EParams1>()
                    {
                        new EParams1()
                        {
                            ParName = ParamNameAccountCode,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameDepartment,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameYear,
                            DType = enDataType.Int
                        },
                        new EParams1()
                        {
                            ParName = ParamNameAmount,
                            DType = enDataType.Decimal
                        }
                    }
                }
            },
            new PluginProvider1()
            {
                SysName = "EditRecord",
                PubName = "Edit",
                Description = "Edits a record",
                PluginGIDModel = ActionEditGuid,
                AvailableParams = new ParamsObject1()
                {
                    Params = new List<EParams1>()
                    {
                        new EParams1()
                        {
                            ParName = ParamNameAccountCode,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameDepartment,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameYear,
                            DType = enDataType.Int
                        },
                        new EParams1()
                        {
                            ParName = ParamNameAmount,
                            DType = enDataType.Decimal
                        },
                        new EParams1()
                        {
                            ParName = ParamNameID,
                            DType = enDataType.Int
                        }
                    }
                }
            },
            new PluginProvider1()
            {
                SysName = "DeleteRecord",
                PubName = "Delete",
                Description = "Delete a new record",
                PluginGIDModel = ActionDeleteGuid,
                AvailableParams = new ParamsObject1()
                {
                    Params = new List<EParams1>()
                    {
                        new EParams1()
                        {
                            ParName = ParamNameID,
                            DType = enDataType.Int
                        }
                    }
                }
            }
        };

        public static string TableName = "p" + System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + "_Accounts";

        public static void PrepareTable(IPluginHost IHost)
        {
            try
            {
                IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "CREATE TABLE IF NOT EXISTS " + TableName + " (id INTEGER PRIMARY KEY, accountCode TEXT NOT NULL, department TEXT NOT NULL, year INTEGER NOT NULL, amount NUMERIC NOT NULL)", new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static DataTable GetTable(PluginRequest1 request, IPluginHost IHost)
        {
            request.SQLcmd = $"SELECT * FROM \"{TableName}\" WHERE Year = @Year AND Department = @Department";

            if (Tools.IsRequestSchemaOnly(request))
            {
                request.SQLcmd = $"SELECT * FROM ({request.SQLcmd}) WHERE 0";
            }

            Tools.SetParamValue(request, "__gwsqlinternal", "");
            return Tools.ExecuteSelect(request, IHost);
        }

        public static void Add(PluginRequest1 request, IPluginHost IHost)
        {
            IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "INSERT INTO " + TableName + " (accountCode, department, year, amount) VALUES (@AccountCode, @Department, @Year, @Amount)", request);
        }

        public static void Edit(PluginRequest1 request, IPluginHost IHost)
        {
            IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "UPDATE " + TableName + " SET accountCode = @AccountCode, department = @Department, year = @Year, amount = @Amount WHERE id = @ID", request);
        }

        public static void Delete(PluginRequest1 request, IPluginHost IHost)
        {
            IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "DELETE FROM " + TableName + " WHERE id = @ID", request);
        }
    }

    public static class ProviderWS
    {
        private const string ParamNameClientId = "@ClientId";
        private const string ParamNameClientSecret = "@ClientSecret";
        private const string ParamNameParticipantId = "@ParticipantId";
        private const string ParamNameReceiverParticipantId = "@ReceiverParticipantId";
        private const string ParamNameSenderParticipantId = "@SenderParticipantId";
        private const string ParamNameDocument = "@Document";
        private const string ParamNameDocumentId = "@DocumentId";

        public static Guid PluginGIDModel = Guid.Parse("346EFD05-5BC5-47F4-8789-86875470E80D");
        public const string PubName = "ExampleModelWS";
        public const string SysName = "e1_pluginexample_modelws";
        public const string Description = "Example model using web service for ERPIO One plugin";
        public static ParamsObject1 AvailableParams = new ParamsObject1()
        {
            Params = new List<EParams1>()
            {
                new EParams1()
                {
                    ParName = ParamNameClientId,
                    DType = enDataType.String
                },
                new EParams1()
                {
                    ParName = ParamNameClientSecret,
                    DType = enDataType.String
                },
                new EParams1()
                {
                    ParName = ParamNameParticipantId,
                    DType = enDataType.String
                }
            }
        };
        public static Guid ActionSendGuid = Guid.Parse("ACA9F20E-B3A5-43FD-84A4-1C78CDE002DC");
        public static List<PluginProvider1> AvailableActions = new List<PluginProvider1>()
        {
            new PluginProvider1()
            {
                SysName = "SendDocument",
                PubName = "Send",
                Description = "Sends a document",
                PluginGIDModel = ActionSendGuid,
                AvailableParams = new ParamsObject1()
                {
                    Params = new List<EParams1>()
                    {
                        new EParams1()
                        {
                            ParName = ParamNameClientId,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameClientSecret,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameReceiverParticipantId,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameSenderParticipantId,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameDocumentId,
                            DType = enDataType.String
                        },
                        new EParams1()
                        {
                            ParName = ParamNameDocument,
                            DType = enDataType.String
                        }
                    }
                }
            }
        };

        // Serializer settings: case-insensitive + ISO dates
        private static JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        private static TokenResponse GetToken(string clientId, string clientSecret)
        {
            var bodyObj = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                grant_type = "client_credentials",
                scope = "document:send document:receive"
            };

            using (var req = new HttpRequestMessage(HttpMethod.Post, "https://www.sapi-sk.sk/sapi/auth/token"))
            {
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Content = new StringContent(JsonSerializer.Serialize(bodyObj, _json), Encoding.UTF8, "application/json");

                using (var resp = Plugin.Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    var content = resp.Content.ReadAsStringAsync().Result;

                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            string.Format("Token request failed: {0} {1}. Body: {2}", (int)resp.StatusCode, resp.ReasonPhrase, content));
                    }

                    var token = JsonSerializer.Deserialize<TokenResponse>(content, _json);
                    if (token == null) throw new InvalidOperationException("Empty token response.");
                    if (string.IsNullOrWhiteSpace(token.AccessToken))
                        throw new InvalidOperationException("Token response does not contain access_token.");

                    return token;
                }
            }
        }

        public static DataTable GetTable(PluginRequest1 request, IPluginHost IHost)
        {
            if (Tools.IsRequestSchemaOnly(request))
            {
                return ToDataTable(new ReceiveResponse());
            }

            string clientId = Tools.GetParamByNameFromRequestAsString(request, ParamNameClientId);
            string clientSecret = Tools.GetParamByNameFromRequestAsString(request, ParamNameClientSecret);
            string participantId = Tools.GetParamByNameFromRequestAsString(request, ParamNameParticipantId);

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException(ParamNameClientId + ", " + ParamNameClientSecret + " and " + ParamNameParticipantId + " are required.");

            var token = GetToken(clientId, clientSecret);

            var url = "https://www.sapi-sk.sk/sapi/document/receive?limit=20";
            using (var req = new HttpRequestMessage(HttpMethod.Get, url))
            {
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                req.Headers.TryAddWithoutValidation("X-Peppol-Participant-Id", participantId);

                using (var resp = Plugin.Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    var body = resp.Content.ReadAsStringAsync().Result;

                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            string.Format("Receive failed: {0} {1}. Body: {2}", (int)resp.StatusCode, resp.ReasonPhrase, body));
                    }

                    var dto = JsonSerializer.Deserialize<ReceiveResponse>(body, _json);
                    if (dto == null) throw new InvalidOperationException("Empty response.");

                    return ToDataTable(dto);
                }
            }
        }

        public static void Send(PluginRequest1 request, IPluginHost IHost)
        {
            string clientId = Tools.GetParamByNameFromRequestAsString(request, ParamNameClientId);
            string clientSecret = Tools.GetParamByNameFromRequestAsString(request, ParamNameClientSecret);
            string senderParticipantId = Tools.GetParamByNameFromRequestAsString(request, ParamNameSenderParticipantId);
            string receiverParticipantId = Tools.GetParamByNameFromRequestAsString(request, ParamNameReceiverParticipantId);
            string documentId = Tools.GetParamByNameFromRequestAsString(request, ParamNameDocumentId);
            string document = Tools.GetParamByNameFromRequestAsString(request, ParamNameDocument);

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(senderParticipantId) || string.IsNullOrWhiteSpace(receiverParticipantId) ||
                string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(document))
            {
                throw new ArgumentException(
                    ParamNameClientId + ", " + ParamNameClientSecret + ", " + ParamNameSenderParticipantId + ", " +
                    ParamNameReceiverParticipantId + ", " + ParamNameDocumentId + " and " + ParamNameDocument + " are required.");
            }

            var token = GetToken(clientId, clientSecret);

            var idempotencyKey = Guid.NewGuid().ToString();
            var nowUtc = DateTimeOffset.UtcNow;

            var payloadObj = new
            {
                metadata = new
                {
                    documentId = documentId,
                    documentTypeId = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##" +
                                     "urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0::2.1",
                    processId = "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0",
                    senderParticipantId = senderParticipantId,
                    receiverParticipantId = receiverParticipantId,
                    creationDateTime = nowUtc
                },
                payload = document,
                payloadFormat = "XML",
                payloadEncoding = "UTF-8"
            };

            var json = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using (var req = new HttpRequestMessage(HttpMethod.Post, "https://www.sapi-sk.sk/sapi/document/send"))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                req.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
                req.Headers.TryAddWithoutValidation("X-Peppol-Participant-Id", senderParticipantId);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var resp = Plugin.Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    var responseJson = resp.Content.ReadAsStringAsync().Result;

                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            string.Format("Document send failed: {0} {1}. Body: {2}", (int)resp.StatusCode, resp.ReasonPhrase, responseJson));
                    }

                    JsonDocument doc = null;
                    try
                    {
                        doc = JsonDocument.Parse(responseJson);
                        JsonElement stElem;
                        if (!doc.RootElement.TryGetProperty("status", out stElem) || stElem.ValueKind != JsonValueKind.String)
                            throw new Exception("Invalid JSON response: " + responseJson);

                        var status = stElem.GetString();
                        if (!string.Equals(status, "ACCEPTED", StringComparison.OrdinalIgnoreCase))
                            throw new Exception(responseJson);
                    }
                    finally
                    {
                        if (doc != null) doc.Dispose();
                    }
                }
            }
        }

        private static DataTable ToDataTable(ReceiveResponse dto)
        {
            var table = new DataTable("sapi_receive");

            // Definícia stĺpcov
            table.Columns.Add("requestId", typeof(string));
            table.Columns.Add("responseId", typeof(string));
            table.Columns.Add("timestamp", typeof(DateTime)); // ISO8601 -> DateTime
            table.Columns.Add("status", typeof(string));

            table.Columns.Add("documentId", typeof(string));
            table.Columns.Add("documentTypeId", typeof(string));
            table.Columns.Add("processId", typeof(string));
            table.Columns.Add("senderParticipantId", typeof(string));
            table.Columns.Add("receiverParticipantId", typeof(string));
            table.Columns.Add("creationDateTime", typeof(DateTime));

            var requestId = dto.RequestId;
            var responseId = dto.ResponseId;
            var timestamp = dto.Timestamp?.UtcDateTime ?? DateTime.MinValue;
            var status = dto.Status;

            var docs = dto.Payload?.Documents;
            if (docs != null)
            {
                foreach (var d in docs)
                {
                    var row = table.NewRow();
                    row["requestId"] = requestId ?? string.Empty;
                    row["responseId"] = responseId ?? string.Empty;
                    row["timestamp"] = timestamp;
                    row["status"] = status ?? string.Empty;

                    row["documentId"] = d.DocumentId ?? string.Empty;
                    row["documentTypeId"] = d.DocumentTypeId ?? string.Empty;
                    row["processId"] = d.ProcessId ?? string.Empty;
                    row["senderParticipantId"] = d.SenderParticipantId ?? string.Empty;
                    row["receiverParticipantId"] = d.ReceiverParticipantId ?? string.Empty;
                    row["creationDateTime"] = d.CreationDateTime?.UtcDateTime ?? DateTime.MinValue;

                    table.Rows.Add(row);
                }
            }

            return table;
        }

        // ===== WS DTOs =====

        public sealed class TokenResponse
        {
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
        }

        private sealed class ReceiveResponse
        {
            [JsonPropertyName("requestId")] public string RequestId { get; set; }
            [JsonPropertyName("responseId")] public string ResponseId { get; set; }
            [JsonPropertyName("timestamp")] public DateTimeOffset? Timestamp { get; set; }
            [JsonPropertyName("status")] public string Status { get; set; }
            [JsonPropertyName("payload")] public ReceivePayload Payload { get; set; }
        }

        private sealed class ReceivePayload
        {
            [JsonPropertyName("documents")] public DocumentHeader[] Documents { get; set; }
        }

        private sealed class DocumentHeader
        {
            [JsonPropertyName("documentId")] public string DocumentId { get; set; }
            [JsonPropertyName("documentTypeId")] public string DocumentTypeId { get; set; }
            [JsonPropertyName("processId")] public string ProcessId { get; set; }
            [JsonPropertyName("senderParticipantId")] public string SenderParticipantId { get; set; }
            [JsonPropertyName("receiverParticipantId")] public string ReceiverParticipantId { get; set; }
            [JsonPropertyName("creationDateTime")] public DateTimeOffset? CreationDateTime { get; set; }
        }
    }
   
    public static class Tools
    {
        // if end user entered value in search input in UI, the search term is in request.Params.Params with ValName = "__whereany"
        // this method performs a case-insensitive search for the term in all columns of the datatable and returns only matching rows
        // if search term is empty or not provided, original datatable is returned, this method can be used in any plugin provider after getting the data
        public static DataTable CustomSearch(PluginRequest1 request, DataTable dt)
        {
            if (dt == null) return null;

            var customSearch = request?.Params?.Params?
                .FirstOrDefault(p => p.ValName == "__whereany");

            var term = customSearch?.Val?.ToString();

            if (string.IsNullOrWhiteSpace(term))
                return dt;

            // Trim whitespace for safety
            term = term.Trim();

            // Prepare the resulting table
            var result = dt.Clone();

            // Loop through all rows and columns – no ItemArray (allocates)
            // IndexOf with OrdinalIgnoreCase is fast and culturally stable
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                var row = dt.Rows[r];
                bool match = false;

                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    var value = row[c];
                    var s = Convert.ToString(value);
                    if (string.IsNullOrEmpty(s)) continue;

                    if (s.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        match = true;
                        break;
                    }
                }

                if (match)
                    result.ImportRow(row);
            }

            return result;
        }

        // for insert/update/delete operations, SQLcmd contains the command with parameters, e.g. "INSERT INTO MyTable (Col1, Col2) VALUES (@Val1, @Val2)", parameters are in request.Params.Params
        // possible to choose if command will be executed on gateway SQL connection (enSQLConnectionType.s_gw) or internal SQLite database (enSQLConnectionType.s_internal_sqlite) by setting parameter "__gwsqlinternal" in request.Params.Params
        public static void ExecuteQuery(PluginRequest1 request, IPluginHost IHost)
        {
            if (!string.IsNullOrEmpty(request.SQLcmd) && request.SQLcmd.ToLower() != "none" && !Guid.TryParse(request.SQLcmd, out var tmpguid) && !request.SQLcmd.Contains(request.PluginGIDModel != null ? request.PluginGIDModel.ToString() : Guid.NewGuid().ToString()) && !request.SQLcmd.Contains(request.PluginGIDAction != null ? request.PluginGIDAction.ToString() : Guid.NewGuid().ToString()))
            {
                if (request.Params.Params.Exists(p => p.ParName == "__gwsqlinternal"))
                    IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, request.SQLcmd, request);
                else
                    IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_gw, request.SQLcmd, request);
            }
        }

        // for insert/update operations, DataTable contains the data to be saved, SQLcmd contains the command with parameters, e.g. "INSERT INTO MyTable (Col1, Col2) VALUES (@Val1, @Val2)"
        // possible to choose if command will be executed on gateway SQL connection (enSQLConnectionType.s_gw) or internal SQLite database (enSQLConnectionType.s_internal_sqlite) by setting parameter "__gwsqlinternal" in request.Params.Params
        public static void ExecuteSave(PluginRequest1 request, DataTable data, IPluginHost IHost)
        {
            if (!string.IsNullOrEmpty(request.SQLcmd) && request.SQLcmd.ToLower() != "none" && !Guid.TryParse(request.SQLcmd, out var tmpguid) && !request.SQLcmd.Contains(request.PluginGIDModel != null ? request.PluginGIDModel.ToString() : Guid.NewGuid().ToString()) && !request.SQLcmd.Contains(request.PluginGIDAction != null ? request.PluginGIDAction.ToString() : Guid.NewGuid().ToString()))
            {
                if (request.Params.Params.Exists(p => p.ParName == "__gwsqlinternal"))
                    IHost.SQLData.Save(enSQLConnectionType.s_internal_sqlite, request.SQLcmd, data, request);
                else
                    IHost.SQLData.Save(enSQLConnectionType.s_gw, request.SQLcmd, data, request);
            }
        }

        // for select operations, SQLcmd contains the command with parameters, e.g. "SELECT * FROM MyTable WHERE Col1 = @Val1", parameters are in request.Params.Params
        // possible to choose if command will be executed on gateway SQL connection (enSQLConnectionType.s_gw) or internal SQLite database (enSQLConnectionType.s_internal_sqlite) by setting parameter "__gwsqlinternal" in request.Params.Params
        public static DataTable ExecuteSelect(PluginRequest1 request, IPluginHost IHost)
        {
            if (!string.IsNullOrEmpty(request.SQLcmd)
                && request.SQLcmd.ToLower() != "none"
                && !Guid.TryParse(request.SQLcmd, out var _)
                && !request.SQLcmd.Contains(request.PluginGIDModel != null ? request.PluginGIDModel.ToString() : Guid.NewGuid().ToString())
                && !request.SQLcmd.Contains(request.PluginGIDAction != null ? request.PluginGIDAction.ToString() : Guid.NewGuid().ToString()))
            {
                DataSet result;

                if (request.Params.Params.Exists(p => p.ParName == "__gwsqlinternal"))
                    result = IHost.SQLData.Select(enSQLConnectionType.s_internal_sqlite, request.SQLcmd, request);
                else
                    result = IHost.SQLData.Select(enSQLConnectionType.s_gw, request.SQLcmd, request);

                if (result != null && result.Tables != null && result.Tables.Count > 0)
                    return result.Tables[0].Copy();

                return new DataTable();
            }
            else
            {
                return null;
            }
        }

        // helper methods to get parameter values from request.Params.Params by name with case-insensitive search, returns null if not found or in case of any error
        public static object GetParamByNameFromRequest(PluginRequest1 request, string paramName)
        {
            try
            {
                object value = null;
                if (request != null && request.Params != null && request.Params.Params != null)
                {
                    var param = request.Params.Params.FirstOrDefault(p => string.Equals(p.ParName, paramName, StringComparison.CurrentCultureIgnoreCase));
                    if (param != null)
                    {
                        value = param.Val;
                    }
                }
                return value;
            }
            catch
            {
                return null;
            }
        }

        // helper methods to get parameter values from request.Params.Params by name with case-insensitive search and convert to specific type, returns null if not found or in case of any error or conversion failure
        public static string GetParamByNameFromRequestAsString(PluginRequest1 request, string paramName)
        {
            return Convert.ToString(GetParamByNameFromRequest(request, paramName));
        }

        // helper methods to get parameter values from request.Params.Params by name with case-insensitive search and convert to specific type, returns null if not found or in case of any error or conversion failure
        public static int? GetParamByNameFromRequestAsInt(PluginRequest1 request, string paramName)
        {
            return int.TryParse(GetParamByNameFromRequestAsString(request, paramName), out var r) ? r : (int?)null;
        }

        // helper methods to get parameter values from request.Params.Params by name with case-insensitive search and convert to specific type, returns null if not found or in case of any error or conversion failure
        public static double? GetParamByNameFromRequestAsDouble(PluginRequest1 request, string paramName)
        {
            return double.TryParse(GetParamByNameFromRequestAsString(request, paramName), out var r) ? r : (double?)null;
        }

        // helper methods to get parameter values from request.Params.Params by name with case-insensitive search and convert to specific type, returns null if not found or in case of any error or conversion failure
        public static bool? GetParamByNameFromRequestAsBool(PluginRequest1 request, string paramName)
        {
            return bool.TryParse(GetParamByNameFromRequestAsString(request, paramName), out var r) ? r : (bool?)null;
        }

        // helper method to set parameter value in request.Params.Params by name with case-insensitive search, if parameter with provided name is not found, it will be added to the list, returns modified request object
        public static PluginRequest1 SetParamValue(PluginRequest1 request, string paramName, object paramValue, string valName = "__dummy", enDataType dType = enDataType.String)
        {
            try
            {
                if (request != null && request.Params != null && request.Params.Params != null)
                {
                    var param = request.Params.Params.FirstOrDefault(p => p.ParName.ToLower() == paramName.ToLower());
                    if (param != null)
                    {
                        var index = request.Params.Params.IndexOf(param);
                        request.Params.Params[index].Val = paramValue;
                        request.Params.Params[index].ValName = valName;
                        request.Params.Params[index].DType = dType;
                    }
                    else
                    {
                        request.Params.Params.Add(new EParams1()
                        {
                            ParName = paramName,
                            Val = paramValue,
                            ValName = valName,
                            DType = dType
                        });
                    }
                }
                return request;
            }
            catch
            {
                return request;
            }
        }

        // creates a demo datatable with some info about plugin and request parameters, can be used for testing purposes in any plugin provider
        public static DataTable CreateDemoDataTable(IgwPlugin1 plugin, PluginRequest1 request)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("gidrow", typeof(Guid)));
            dt.Columns[0].Caption = "Row";
            dt.Columns.Add(new DataColumn(nameof(IgwPlugin1.PluginName), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(IgwPlugin1.PluginVersion), typeof(string)));
            dt.Columns.Add(new DataColumn(nameof(IgwPlugin1.PluginDescription), typeof(string)));
            dt.Columns.Add(new DataColumn("DateSent", typeof(DateTime)));

            var row = dt.NewRow();
            row[0] = Guid.NewGuid();
            row[1] = plugin.PluginName;
            row[2] = plugin.PluginVersion;
            row[3] = plugin.PluginDescription;
            try
            {
                if (request != null && request.Params != null && request.Params.Params != null)
                {
                    row[3] = row[3] + request.Params.Params.Count().ToString();
                    foreach (var ii in request.Params.Params)
                    {
                        if (ii.Val != null)
                        {
                            row[3] += ii.Val.ToString();
                        }
                    }
                    var filter = request.Params.Params.FirstOrDefault(p => p.ValName != null && p.ValName.Equals(ERPIO.AppSDK.Shared.Const.global.SysParAnyColumWhere));
                    if (filter != null && filter.Val != null && !string.IsNullOrEmpty(filter.Val.ToString()))
                    {
                        row[3] = row[3] + filter.Val.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                row[3] += ex.Message;
            }

            row[4] = DateTime.Now;

            dt.Rows.Add(row);

            return dt;
        }

        // helper method to check if request contains parameter with name "__schemaonly" which can be used to return only schema of the datatable without any data
        public static bool IsRequestSchemaOnly(PluginRequest1 request)
        {
            return request.Params.Params.Any(p => string.Equals(p.ParName, "__schemaonly", StringComparison.CurrentCultureIgnoreCase));
        }        
    }

    public static class Log
    {
        public enum MessageType
        {
            INFO,
            WARNING,
            ERROR,
            OTHER
        }

        public static string LogTableName = "p" + System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + "_Log";

        public static void PrepareTable(IPluginHost IHost)
        {
            try
            {
                IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "CREATE TABLE IF NOT EXISTS " + LogTableName + " (id INTEGER PRIMARY KEY, created_date TEXT NOT NULL, method TEXT NOT NULL, location TEXT NOT NULL, type TEXT NOT NULL, message TEXT NOT NULL)", new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static bool IsVerboseLog(PluginRequest1 request)
        {
            string verboseLogStr = Tools.GetParamByNameFromRequestAsString(request, "@verboseLog");
            if (!string.IsNullOrEmpty(verboseLogStr) && (verboseLogStr == "1" || verboseLogStr.ToLower() == "true"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void WriteLog(string MessageLocation, MessageType Type, string Message, IPluginHost IHost,
        [System.Runtime.CompilerServices.CallerMemberName] string SourceName = "",
        //[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int SouceLineNumber = 0)
        {
            if (IHost == null)
            {
                return;
            }
            else
            {
                try
                {                    
                    Dictionary<string, object> Parameters = new Dictionary<string, object>();
                    Parameters.Add("@createdDate", DateTime.Now.ToString("o"));
                    Parameters.Add("@method", SourceName + "(" + SouceLineNumber.ToString() + ")");
                    Parameters.Add("@location", MessageLocation);
                    Parameters.Add("@type", Type.ToString());
                    Parameters.Add("@message", Regex.Replace(Message, @"\r\n?|\n", ""));

                    IHost.SQLData.ExecuteCommand(enSQLConnectionType.s_internal_sqlite, "INSERT INTO " + LogTableName + " (created_date, method, location, type, message) VALUES (@createdDate, @method, @location, @type, IFNULL(@message, 'NULL'))", Parameters);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        public static void WriteVerboseLog(PluginRequest1 request, string MessageLocation, MessageType Type, string Message, IPluginHost IHost,
        [System.Runtime.CompilerServices.CallerMemberName] string SourceName = "",
        //[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int SouceLineNumber = 0)
        {
            if (IsVerboseLog(request))
            {
                WriteLog(MessageLocation, Type, Message, IHost, SourceName, SouceLineNumber);
            }
        }
    }
}

/* Used models:

public enum enDataType
{
    String = 0,
    Boolean = 1,
    Byte = 2,
    Int16 = 3,
    Int32 = 4,
    Int64 = 5,
    Decimal = 6,
    Float = 7,
    Double = 8,
    DateTime = 9,
    ByteArray = 10,
    Int = 11,
    Uknown = 999
}

public class EParams1
{
    public string ParName { get; set; }

    public string ValName { get; set; }

    public object Val { get; set; }

    public string Cond { get; set; }

    public enDataType DType { get; set; } = enDataType.Uknown;

}

public enum enSQLConnectionType
{
    s_gw = 0,
    s_internal_sqlite = 1,
    s_custom = 999
}
*/ 