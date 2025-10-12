using System.Diagnostics.CodeAnalysis;
using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Data;
using System.Linq;

namespace BPMSoft.Configuration
{
    using Common;
    using Core.DB;
    using Core;
    using Core.Process;
    using SerializedEsqFilterConvertation;
    using Core.Entities;
    using BPMSoft.Web.Common;
    
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class SpaceConvertFilterDataToEsqService : BaseService
    {
        #region Methods: Query
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryReadDataUserTask(ConvertFilterInputData request)
        {
            return GetQuery(request, GetSelectReadDataUserTask);
        }
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryEditDataUserTask(ConvertFilterInputData request)
        {
            return GetQuery(request, GetSelectEditDataUserTask);
        }
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryDeleteDataUserTask(ConvertFilterInputData request)
        {
            return GetQuery(request, GetSelectDeleteDataUserTask);
        }
        
        private static ConvertFilterOutputData GetQuery(ConvertFilterInputData request, Func<ConvertFilterInputData, Select> getSelectFunc)
        {
            try
            {
                var select = getSelectFunc(request);

                return new ConvertFilterOutputData 
                { 
                    Query = select
                        .GetSqlText()
                        .Replace("{", "")
                        .Replace("}", "")
                        .Trim() 
                };
            }
            catch (Exception e)
            {
                return new ConvertFilterOutputData 
                { 
                    Exception = e.ToString() 
                };
            }
        }

        private Select GetSelectReadDataUserTask(ConvertFilterInputData request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, request.SchemaName)
            {
                UseAdminRights = false, 
                IgnoreDisplayValues = request.IgnoreDisplayValues
            };

            var aggregationColumnName = "Function";
            var type = (ProcessReadDataResultType)request.ResultType;

            SpecifyEsqColumns(esq, type, request, ref aggregationColumnName);
            SpecifyEsqOrderByStatements(esq, type, request);
            
            request.Filters = SpaceConvertFilterToDataEsqParametersMapper.MapFilterParameters(request.Filters);
            
            esq.AddSerializedFilter(UserConnection, request.Filters);

            if (GlobalAppSettings.FeatureReadDataUserTaskUseChunks && type == ProcessReadDataResultType.EntityCollection)
            {
                esq.ChunkSize = Core.Configuration.SysSettings.GetValue(UserConnection, "ReadDataChunkSize", 50);
            }

            var select = esq.GetSelectQuery(UserConnection);
            select.BuildParametersAsValue = true;
            
            return select;
        }

        private void SpecifyEsqColumns(EntitySchemaQuery esq, ProcessReadDataResultType resultType, ConvertFilterInputData request, ref string aggregationColumnName)
        {
            if (resultType == ProcessReadDataResultType.Function)
            {
                var aggregationType = (AggregationTypeStrict)request.FunctionType;

                if (string.IsNullOrEmpty(request.AggregationColumnName) && aggregationType == AggregationTypeStrict.Count)
                {
                    var entitySchema = esq.RootSchema;
                    aggregationColumnName = entitySchema.PrimaryColumn.Name;
                }
                else
                {
                    aggregationColumnName = request.AggregationColumnName;
                }

                aggregationColumnName = esq.AddColumn(esq.CreateAggregationFunction(aggregationType, aggregationColumnName)).Name;
                return;
            }

            SetColumns(esq, request.Columns);

            if (resultType == ProcessReadDataResultType.EntityCollection && !UserConnection.GetIsFeatureEnabled("ReadDataThrowExceptionsOnUnsupportedTypes"))
            {
                RemoveUnsupportedColumns(esq);
            }

            esq.PrimaryQueryColumn.IsAlwaysSelect = true;
            esq.RowCount = resultType == ProcessReadDataResultType.Entity ? 1 : request.NumberOfRecords;
        }
        
        private static void SetColumns(EntitySchemaQuery esq, string[] columns)
        {
            if (columns.Length == 0)
            {
                esq.AddAllSchemaColumns();
            }
            else
            {
                var entitySchema = esq.RootSchema;

                foreach (var metaPath in columns)
                {
                    var columnPath = entitySchema.GetSchemaColumnPathByMetaPath(metaPath);

                    if (columnPath == esq.PrimaryQueryColumn.Path)
                    {
                        continue;
                    }

                    esq.AddColumn(columnPath);
                }
            }
        }
        
        private void RemoveUnsupportedColumns(EntitySchemaQuery esq)
        {
            var columns = esq.Columns;

            for (var i = columns.Count; i != 0; i--)
            {
                var column = columns[i - 1];
                var resultDataValueType = column.GetResultDataValueType(UserConnection.DataValueTypeManager);
                if (!CompositeObject.IsTypeSupported(resultDataValueType.ValueType))
                {
                    columns.Remove(column);
                }
            }
        }
        
        protected virtual void SpecifyEsqOrderByStatements(EntitySchemaQuery esq, ProcessReadDataResultType resultType, ConvertFilterInputData request)
        {
            if (resultType == ProcessReadDataResultType.Function)
            {
                return;
            }

            ProcessUserTaskUtilities.SpecifyESQOrderByStatement(esq, request.OrderInfo);
        }

        private Select GetSelectEditDataUserTask(ConvertFilterInputData request)
        {
            var entitySchemaQuery = new EntitySchemaQuery(UserConnection.EntitySchemaManager, request.SchemaName)
            {
                UseAdminRights = false, 
                IgnoreDisplayValues = request.IgnoreDisplayValues
            };
            entitySchemaQuery.AddAllSchemaColumns();
            
            request.Filters = SpaceConvertFilterToDataEsqParametersMapper.MapFilterParameters(request.Filters);
            
            entitySchemaQuery.AddSerializedFilter(UserConnection, request.Filters);

            var select = entitySchemaQuery.GetSelectQuery(UserConnection);
            select.BuildParametersAsValue = true;
            
            return select;
        }

        private Select GetSelectDeleteDataUserTask(ConvertFilterInputData request)
        {
            var entitySchemaQuery = new EntitySchemaQuery(UserConnection.EntitySchemaManager, request.SchemaName)
            {
                UseAdminRights = false, 
                IgnoreDisplayValues = request.IgnoreDisplayValues
            };
            entitySchemaQuery.AddAllSchemaColumns();
            
            request.Filters = SpaceConvertFilterToDataEsqParametersMapper.MapFilterParameters(request.Filters);
            
            entitySchemaQuery.AddSerializedFilter(UserConnection, request.Filters);

            var select = entitySchemaQuery.GetSelectQuery(UserConnection);
            select.BuildParametersAsValue = true;
            
            return select;
        }

        #endregion
        
        #region Methods: QueryPlan
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryPlanReadDataUserTask(ConvertFilterInputData request)
        {
            return GetQueryPlan(request, GetQueryReadDataUserTask);
        }
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryPlanEditDataUserTask(ConvertFilterInputData request)
        {
            return GetQueryPlan(request, GetQueryEditDataUserTask);
        }
        
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public ConvertFilterOutputData GetQueryPlanDeleteDataUserTask(ConvertFilterInputData request)
        {
            return GetQueryPlan(request, GetQueryDeleteDataUserTask);
        }
        
        private ConvertFilterOutputData GetQueryPlan(ConvertFilterInputData request, Func<ConvertFilterInputData, ConvertFilterOutputData> getQueryFunc)
        {
            try
            {
                var query = getQueryFunc(request).Query;
                var queryPlan = ExecuteExplain(query);

                return new ConvertFilterOutputData 
                { 
                    Query = queryPlan 
                };
            }
            catch (Exception e)
            {
                return new ConvertFilterOutputData 
                { 
                    Exception = e.ToString() 
                };
            }
        }
        
        private string ExecuteExplain(string query)
        {
            var explain = new CustomQuery(UserConnection, $"explain {query}");

            using (var dbExecutor = UserConnection.EnsureDBConnection())
            {
                using (var dr = explain.ExecuteReader(dbExecutor))
                {
                    var dataTable = new DataTable();
                    dataTable.Load(dr);
                    
                    var list = dataTable
                        .AsEnumerable()
                        .Select(row => row["QUERY PLAN"].ToString())
                        .ToList();
                    
                    return string.Join("\n", list);
                }
            }
        }
        
        #endregion
    }

    [DataContract]
    public class ConvertFilterInputData
    {
        [DataMember(Name = "schemaName")] 
        public string SchemaName;

        [DataMember(Name = "filters")] 
        public string Filters;

        [DataMember(Name = "columns")] 
        public string[] Columns;

        [DataMember(Name = "ignoreDisplayValues")]
        public bool IgnoreDisplayValues;

        [DataMember(Name = "resultType")] 
        public int ResultType;

        [DataMember(Name = "orderInfo")]
        public string OrderInfo;

        [DataMember(Name = "aggregationColumnName")]
        public string AggregationColumnName;

        [DataMember(Name = "functionType")] 
        public int FunctionType;

        [DataMember(Name = "readSomeTopRecords")] 
        public bool ReadSomeTopRecords;

        [DataMember(Name = "numberOfRecords")] 
        public int NumberOfRecords;
    }

    [DataContract]
    public class ConvertFilterOutputData
    {
        [DataMember(Name = "query")] 
        public string Query { get; set; }

        [DataMember(Name = "exception")] 
        public string Exception { get; set; }
    }
}