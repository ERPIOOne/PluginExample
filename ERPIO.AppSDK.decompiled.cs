
### BEGIN_DOTNET_DECOMPILED_SOURCE
```csharp
// Decompiled from ERPIO.AppSDK.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Xml.Serialization;
using ERPIO.AppSDK.Shared.Const;
using ERPIO.AppSDK.Shared.Enums;
using ERPIO.AppSDK.Shared.Interfaces;
using ERPIO.AppSDK.Shared.Models;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETFramework,Version=v4.5.2", FrameworkDisplayName = ".NET Framework 4.5.2")]
[assembly: AssemblyCompany("ERPIO.AppSDK")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("ERPIO.AppSDK")]
[assembly: AssemblyTitle("ERPIO.AppSDK")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace ERPIO.AppSDK.Interfaces
{
        public interface ILogger
        {
                void Write(string logMessage);
        }
        public class Logger : ILogger
        {
                private static readonly object _syncObject = new object();

                private static readonly Lazy<ILogger> instance = new Lazy<ILogger>(() => new Logger(), LazyThreadSafetyMode.ExecutionAndPublication);

                private readonly TextWriter textWriter;

                public static ILogger Instance => instance.Value;

                private Logger()
                {
                        string text = DateTime.Now.ToString("yyyy-MM-dd");
                        textWriter = TextWriter.Synchronized(File.AppendText(global.RootFolderPath + "\\Log_" + text + ".txt"));
                }

                public void Write(string logMessage)
                {
                        try
                        {
                                Log(logMessage, textWriter);
                        }
                        catch (IOException)
                        {
                                textWriter.Close();
                        }
                }

                private void Log(string logMessage, TextWriter w)
                {
                        lock (_syncObject)
                        {
                                DateTime now = DateTime.Now;
                                w.WriteLine("{0}_{1}:", now, logMessage);
                                w.Flush();
                        }
                }
        }
}
namespace ERPIO.AppSDK.Shared.Models
{
        public class HelpParamModel
        {
                public short paramType { get; set; }

                public string SysName { get; set; }

                public string Language { get; set; } = "EN";

                public string Description { get; set; }

                public string CodingSysName { get; set; }
        }
        [Serializable]
        public class ParamsObject1
        {
                public enParType ParType { get; set; }

                public List<EParams1> Params { get; set; }
        }
        [Serializable]
        public class EParams1
        {
                public string ParName { get; set; }

                public string ValName { get; set; }

                public object Val { get; set; }

                public string Cond { get; set; }

                public enDataType DType { get; set; } = enDataType.Uknown;
        }
        [Serializable]
        public class PluginRequest1
        {
                public short SqlTimeout;

                public string SQLcmd { get; set; }

                public int RowsLimit { get; set; }

                public int PageIndex { get; set; } = -1;

                public int PageSize { get; set; } = -1;

                public int RowsOffset { get; set; } = -1;

                public Guid PluginGIDModel { get; set; }

                public Guid? PluginGIDAction { get; set; }

                public ParamsObject1 Params { get; set; }

                public List<ParamsObject1> TypedPars { get; set; }
        }
        [Serializable]
        public class PluginBase
        {
                public Guid PluginGIDModel { get; set; }

                public string PubName { get; set; }
        }
        [Serializable]
        public class PluginProvider1 : PluginBase
        {
                public ParamsObject1 AvailableParams { get; set; }

                public string SysName { get; set; }

                public string Description { get; set; }

                public List<PluginProvider1> AvailableActions { get; set; }
        }
}
namespace ERPIO.AppSDK.Shared.Interfaces
{
        public interface IPluginHost
        {
                ISQLData SQLData { get; }
        }
        public interface ISQLData
        {
                string TabPrefix { get; set; }

                void SetCustomSQLConnection(string connString, string providerName);

                int ExecuteCommand(enSQLConnectionType connType, string sqlcmd, Dictionary<string, object> Parameters = null);

                int ExecuteCommand(enSQLConnectionType connType, string sqlcmd, PluginRequest1 request = null);

                DataSet Select(enSQLConnectionType connType, string sqlcmd, Dictionary<string, object> Parameters = null);

                DataSet Select(enSQLConnectionType connType, string sqlcmd, PluginRequest1 request = null);

                bool Save(enSQLConnectionType connType, string sqlcmd, DataTable data, Dictionary<string, object> Parameters = null);

                bool Save(enSQLConnectionType connType, string sqlcmd, DataTable data, PluginRequest1 request = null);
        }
}
namespace ERPIO.AppSDK.Shared.Plugins.Interfaces
{
        public interface IgwPlugin1
        {
                Guid PluginGID { get; }

                string PluginName { get; }

                string PluginDescription { get; }

                string PluginVersion { get; }

                void RunPluginConfiguration(IPluginHost IHost);

                List<PluginProvider1> GetAvailableModels(IPluginHost IHost);

                DataSet GetDataTable(PluginRequest1 request, IPluginHost IHost);

                void SetLargeDataTable(PluginRequest1 request, DataTable tab, IPluginHost IHost);
        }
}
namespace ERPIO.AppSDK.Shared.Const
{
        public static class global
        {
                public const string SysParUser = "__currusername";

                public const string SysParDate = "__currdatetime";

                public const string SysParCulture = "__currculture";

                public const string SysParDummy = "__dummy";

                public const string SysParSrcModify = "__srcmodify";

                public const string SysParContext = "__dtcontext";

                public const string SysValLastError = "__dtlasterror";

                public const string SysParAnyColumWhere = "__whereany";

                public const string SysParColumnList = "__columnlist";

                public const string SysParSchemaOnly = "__schemaonly";

                public const string SysParRowsOffset = "__qrowsoffset";

                public const string SysParRowsCount = "__qrowscnt";

                public const string SysParGlobal = "@__glob";

                public const string SysParLogLevel = "__loglevel";

                public const string SysParGWSQLInternal = "__gwsqlinternal";

                public const string SysParGWPlugin = "__gwplugin";

                public const string SysParGWApiService = "__gwapiservice";

                public const string SysParNotifyService = "__notifyservice";

                public const string SysParColColorFore = "__qc_cf_";

                public const string SysParColColorBack = "__qc_cb_";

                public const string SysParColFont = "__qc_ff_";

                public const string SysParRowColorBack = "__qc_rb_";

                public const string SysParComputed = "_qcdt";

                public const string c_GWPlugin_ColumModelBinary = "modelbinary";

                public static readonly string[] SysParamList = new string[10] { "__currusername", "__currdatetime", "__currculture", "__srcmodify", "__dummy", "__gwplugin", "__dtcontext", "@__glob", "__loglevel", "__notifyservice" };

                public static readonly string[] SysParamRemoveList = new string[11]
                {
                        "__schemaonly", "__srcmodify", "__qrowsoffset", "__qrowscnt", "__gwplugin", "__gwplugin_gidmodel", "__gwplugin_gidaction", "__columnlist", "__gwsqlinternal", "__gwapiservice",
                        "__notifyservice"
                };

                public static readonly string[] ServiceNotifyListParams = new string[5] { "@__nrusername", "@__nrtitle", "@__nbody", "@__nimageurl", "@__nconfig" };

                public const string SysParamGWSupportedList = "SysParamGWSupportedList";

                public static readonly string RootFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                public const string DCols_ComputedColumn = "__*C";

                private static List<HelpParamModel> _GetParamsDescription;

                public static List<HelpParamModel> GetParamsDescription()
                {
                        if (_GetParamsDescription == null)
                        {
                                _GetParamsDescription = new List<HelpParamModel>
                                {
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParUser",
                                                SysName = "__currusername",
                                                Description = "String, User name, which called action"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParDate",
                                                SysName = "__currdatetime",
                                                Description = "Datetime, when action was executed, from clientside, if not specified, from servserside MW"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParCulture",
                                                SysName = "__currculture",
                                                Description = "String, .NET culture of clientside executor cz-CS,en-US ..."
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParDummy",
                                                SysName = "__dummy",
                                                Description = "any date type, value entered by user, or predefined on MW"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParSrcModify",
                                                SysName = "__srcmodify",
                                                Description = "String, pattern in command string will be replaced by value in this parameter: ie select*from {replace_me}.table  result: select*from VALUExyz.table "
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParContext",
                                                SysName = "__dtcontext",
                                                Description = "String, json array of row data values"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysValLastError",
                                                SysName = "__dtlasterror",
                                                Description = "Last error value(automatization action steps) ParName=@anything ValName=SysParContext Val=SysValLastError"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParAnyColumWhere",
                                                SysName = "__whereany",
                                                Description = "String, specify to perform fulltext search in data, by entered value"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParColumnList",
                                                SysName = "__columnlist",
                                                Description = "String, comma separate list of requested columns from data"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParSchemaOnly",
                                                SysName = "__schemaonly",
                                                Description = "String-empty. When parameter is presented in request, returning of descriptive schema only of datasource is expected"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParRowsOffset",
                                                SysName = "__qrowsoffset",
                                                Description = "Integer, paging datasource. Offset of rows. When <1, nothing to do"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParRowsCount",
                                                SysName = "__qrowscnt",
                                                Description = "Integer, paging datasource. Number of rows(page size). When <1, nothing to do"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParGlobal",
                                                SysName = "@__glob",
                                                Description = "String, global MW parameter prefix value getter/setter"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParGWSQLInternal",
                                                SysName = "__gwsqlinternal",
                                                Description = "When parameter is presented, GW will perform getting/executing data againts internall DB_sqlite.Values: json - for Enable JSON Extension."
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParGWPlugin",
                                                SysName = "__gwplugin",
                                                Description = ""
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "__gwplugin_gidmodel",
                                                SysName = "__gwplugin_gidmodel",
                                                Description = "Guid, model againts which perform data operations on GW plugin"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "__gwplugin_gidaction",
                                                SysName = "__gwplugin_gidaction",
                                                Description = "Guid, action againts which perform data operations on GW plugin"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParGWApiService",
                                                SysName = "__gwapiservice",
                                                Description = "serialized definition for GW REST API Client proccessing"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParNotifyService",
                                                SysName = "__notifyservice",
                                                Description = "PUSH Notification MW endpoint- accepting columns mapped to params: " + string.Join(",", ServiceNotifyListParams)
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParamRemoveList",
                                                SysName = "",
                                                Description = "String[], parameters not for QUERY. are commanding"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParamGWSupportedList",
                                                SysName = "SysParamGWSupportedList",
                                                Description = "String, comma separated list of supported parameters by GW"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "SysParLogLevel",
                                                SysName = "__loglevel",
                                                Description = "Level of logging. quick data fetch; for GateWay:  1=Exception details"
                                        },
                                        new HelpParamModel
                                        {
                                                CodingSysName = "c_GWPlugin_ColumModelBinary",
                                                SysName = "modelbinary",
                                                Description = "serialized column name with models per plugin definition"
                                        },
                                        new HelpParamModel
                                        {
                                                paramType = 1,
                                                CodingSysName = "SysParColColorFore",
                                                SysName = "__qc_cf_",
                                                Description = "INT Foreground Color of text in grid cell. FIELDNAME=name of colored column. __qc_cf_FIELDNAME  "
                                        },
                                        new HelpParamModel
                                        {
                                                paramType = 1,
                                                CodingSysName = "SysParColColorBack",
                                                SysName = "__qc_cb_",
                                                Description = "INT Background color of text in grid cell. FIELDNAME=name of colored column. __qc_cb_FIELDNAME  "
                                        },
                                        new HelpParamModel
                                        {
                                                paramType = 1,
                                                CodingSysName = "SysParColFont",
                                                SysName = "__qc_ff_",
                                                Description = "Font attribute in grid cell. FIELDNAME=name of fonted. 1=Bold,2=Italic,0=None __qc_ff_FIELDNAME  "
                                        },
                                        new HelpParamModel
                                        {
                                                paramType = 1,
                                                CodingSysName = "SysParRowColorBack",
                                                SysName = "__qc_rb_",
                                                Description = "INT Background color of cell in grid row. FIELDNAME=name of colored column. __qc_cb_FIELDNAME  "
                                        }
                                };
                        }
                        return _GetParamsDescription;
                }
        }
}
namespace ERPIO.AppSDK.Shared.helpers
{
        public class WTypedParams
        {
                public static string[] EnStepType = new string[2] { "Request", "Extract" };

                private const string _wprefix = "W_";

                private WStepItem wstep;

                private List<EParams1> dt;

                public WTypedParams(List<EParams1> TypedPars)
                {
                        dt = TypedPars;
                }

                public WStepItem WStep()
                {
                        EParams1 t = dt.FirstOrDefault((EParams1 p) => p.Cond == "W_SType");
                        if (t != null)
                        {
                                wstep.STypeTxt = (t.Val ?? string.Empty).ToString();
                                wstep.SName = t.ValName;
                                wstep.SType = enWSType.Unknown;
                                if (wstep.STypeTxt != string.Empty && Enum.TryParse<enWSType>(wstep.STypeTxt, ignoreCase: true, out var result))
                                {
                                        wstep.SType = result;
                                }
                        }
                        t = dt.FirstOrDefault((EParams1 p) => p.Cond == "W_CmdType");
                        if (t != null)
                        {
                                wstep.CmdTypeTxt = (t.Val ?? string.Empty).ToString();
                                wstep.Url = t.ValName;
                                wstep.CmdType = enWSCmd.Unknown;
                                if (wstep.CmdTypeTxt != string.Empty && Enum.TryParse<enWSCmd>(wstep.CmdTypeTxt, ignoreCase: true, out var result2))
                                {
                                        wstep.CmdType = result2;
                                }
                        }
                        t = dt.FirstOrDefault((EParams1 p) => p.Cond == "W_PostData");
                        if (t != null)
                        {
                                wstep.PostData = (t.Val ?? string.Empty).ToString();
                        }
                        t = dt.FirstOrDefault((EParams1 p) => p.Cond == "W_ExtractFrom");
                        if (t != null)
                        {
                                wstep.ExtractFromTxt = (t.Val ?? string.Empty).ToString();
                                wstep.ExtractName = t.ValName.ToString();
                                wstep.ExtractFrom = enWSExtractType.Unknown;
                                if (wstep.ExtractFromTxt != string.Empty && Enum.TryParse<enWSExtractType>(wstep.ExtractFromTxt, ignoreCase: true, out var result3))
                                {
                                        wstep.ExtractFrom = result3;
                                }
                        }
                        t = dt.FirstOrDefault((EParams1 p) => p.Cond == "W_ExtractValuePath");
                        if (t != null)
                        {
                                wstep.ExtractValuePath = (t.Val ?? string.Empty).ToString();
                                wstep.ExtractValueSaveTo = t.ValName.ToString();
                        }
                        IEnumerable<EParams1> enumerable = dt.Where((EParams1 p) => p.Cond == "W_HParams");
                        wstep.HParams = new List<ItemVal>();
                        if (enumerable != null)
                        {
                                wstep.HParams.AddRange(enumerable.Select((EParams1 p) => new ItemVal(p.ValName, (t.Val ?? string.Empty).ToString())));
                        }
                        return wstep;
                }

                public void WStepAdd(WStepItem item)
                {
                }
        }
        public enum enWSType
        {
                Request = 0,
                Extract = 1,
                Save = 2,
                Unknown = 99
        }
        public enum enWSCmd
        {
                POST = 0,
                GET = 1,
                PUT = 2,
                PATCH = 3,
                DELETE = 4,
                JSON = 30,
                XML = 31,
                Unknown = 99
        }
        public enum enWSExtractType
        {
                Body = 0,
                Header = 1,
                Custom = 2,
                Unknown = 99
        }
        [Serializable]
        public struct ItemVal
        {
                public string Item1 { get; set; }

                public string Item2 { get; set; }

                public ItemVal(string item1, string item2)
                {
                        Item1 = item1;
                        Item2 = item2;
                }
        }
        [Serializable]
        public class WStepItemBase
        {
                public enWSType SType { get; set; }

                public string SName { get; set; }

                [XmlIgnore]
                public enWSCmd CmdType { get; set; }

                [XmlElement("CmdType")]
                public string CmdTypeS
                {
                        get
                        {
                                return CmdType.ToString();
                        }
                        set
                        {
                                if (Enum.TryParse<enWSCmd>(value, ignoreCase: true, out var result))
                                {
                                        CmdType = result;
                                }
                                else
                                {
                                        CmdType = enWSCmd.Unknown;
                                }
                        }
                }

                public string Url { get; set; }

                public string PostData { get; set; }

                public enWSExtractType ExtractFrom { get; set; }

                public string ExtractName { get; set; }

                public string ExtractValuePath { get; set; }

                public string ExtractValueSaveTo { get; set; }

                public List<ItemVal> HParams { get; set; }

                public bool CanRepeat { get; set; }

                public string DBSave { get; set; }

                public string StepCustom { get; set; }
        }
        [Serializable]
        public class WStepItem : WStepItemBase
        {
                public string STypeTxt { get; set; }

                public string CmdTypeTxt { get; set; }

                public string ExtractFromTxt { get; set; }
        }
        public class ywsHelpers
        {
                public static class XmlSerializerCache
                {
                        private static readonly Dictionary<string, XmlSerializer> cache = new Dictionary<string, XmlSerializer>();

                        public static XmlSerializer Create(Type type, XmlRootAttribute root)
                        {
                                string key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[2] { type, root.ElementName });
                                if (!cache.ContainsKey(key))
                                {
                                        cache.Add(key, new XmlSerializer(type, root));
                                }
                                return cache[key];
                        }
                }

                private static XmlSerializerNamespaces _NSXML;

                private static XmlSerializerNamespaces NSXML
                {
                        get
                        {
                                if (_NSXML == null)
                                {
                                        _NSXML = new XmlSerializerNamespaces();
                                        _NSXML.Add("", "");
                                }
                                return _NSXML;
                        }
                }

                public static object GetNULLwhenEmpty(DbType dbt, object val)
                {
                        if (val == null)
                        {
                                return DBNull.Value;
                        }
                        if (val != DBNull.Value && val.ToString().Trim() == string.Empty && (dbt != DbType.String || dbt != DbType.AnsiString || dbt != DbType.Object))
                        {
                                return DBNull.Value;
                        }
                        return val;
                }

                public static DataTable CreateDataTable<T>(IEnumerable<T> list)
                {
                        PropertyInfo[] properties = typeof(T).GetProperties();
                        DataTable dataTable = new DataTable();
                        PropertyInfo[] array = properties;
                        foreach (PropertyInfo propertyInfo in array)
                        {
                                propertyInfo.GetCustomAttributes();
                                object obj = propertyInfo.GetCustomAttributes(typeof(EditableAttribute), inherit: false).FirstOrDefault();
                                object obj2 = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute), inherit: false).FirstOrDefault();
                                dataTable.Columns.Add(new DataColumn
                                {
                                        ColumnName = propertyInfo.Name,
                                        DataType = (Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType),
                                        ReadOnly = (obj != null && ((EditableAttribute)obj).AllowEdit),
                                        Caption = ((obj2 == null) ? null : ((DisplayAttribute)obj2).Name)
                                });
                        }
                        if (list != null)
                        {
                                foreach (T item in list)
                                {
                                        object[] array2 = new object[properties.Length];
                                        for (int j = 0; j < properties.Length; j++)
                                        {
                                                array2[j] = properties[j].GetValue(item);
                                        }
                                        dataTable.Rows.Add(array2);
                                }
                        }
                        return dataTable;
                }

                public static string XMLfromObj(object L, string root = "cd", bool nullIfNoRows = false)
                {
                        if (nullIfNoRows)
                        {
                                try
                                {
                                        if (L == null)
                                        {
                                                return string.Empty;
                                        }
                                        if (string.IsNullOrEmpty(L.ToString()))
                                        {
                                                return string.Empty;
                                        }
                                        Type type = L.GetType();
                                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && !(L is ICollection { Count: not 0 }))
                                        {
                                                return string.Empty;
                                        }
                                }
                                catch (Exception)
                                {
                                }
                        }
                        XmlSerializer xmlSerializer = XmlSerializerCache.Create(L.GetType(), new XmlRootAttribute(root));
                        using StringWriter stringWriter = new StringWriter();
                        xmlSerializer.Serialize(stringWriter, L, _NSXML);
                        return stringWriter.ToString();
                }

                public static T XMLtoObj<T>(string data, string root = "cd", bool throwError = false)
                {
                        T result = default(T);
                        if (string.IsNullOrEmpty(data))
                        {
                                return result;
                        }
                        XmlSerializer xmlSerializer = XmlSerializerCache.Create(typeof(T), new XmlRootAttribute(root));
                        try
                        {
                                using TextReader textReader = new StringReader(data);
                                result = (T)xmlSerializer.Deserialize(textReader);
                                return result;
                        }
                        catch (Exception ex)
                        {
                                if (throwError)
                                {
                                        throw ex;
                                }
                        }
                        return result;
                }
        }
}
namespace ERPIO.AppSDK.Shared.Enums
{
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
        public enum enSQLConnectionType
        {
                s_gw = 0,
                s_internal_sqlite = 1,
                s_custom = 999
        }
        [Serializable]
        public enum enParType
        {
                q_where,
                q_order,
                q_save,
                q_cols,
                q_glob,
                q_wsteps
        }
}


```
### END_DOTNET_DECOMPILED_SOURCE
