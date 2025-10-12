using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using BPMSoft.Common;
using BPMSoft.Common.Json;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Nui.ServiceModel.DataContract;
using FilterType = BPMSoft.Nui.ServiceModel.DataContract.FilterType;

namespace BPMSoft.Configuration
{
    public static class SpaceConvertFilterToDataEsqParametersMapper
    {
        public static string MapFilterParameters(string serailizedFilters)
        {
            var filters = Json.Deserialize<FiltersExt>(serailizedFilters);

            foreach (var filterItemValuePair in filters.Items)
            {
                var filter = filterItemValuePair.Value;
                
                SetParameterDefaultValue(filter.DataValueType, filter.RightExpression);

                RecursiveSubFiltersMap(filter);
            }
            
            return JsonConvert.SerializeObject(filters, 
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    NullValueHandling = NullValueHandling.Include
                }
            );
        }

        private static void RecursiveSubFiltersMap(FilterExt filter)
        {
            MapRightExpressionParameters(filter);
            
            if (filter.SubFilters?.Items != null)
            {
                foreach (var subItem in filter.SubFilters.Items)
                {
                    MapRightExpressionParameters(subItem.Value);
                    RecursiveSubFiltersMap(subItem.Value);
                }
            }

            if (filter.LeftExpression?.SubFilters?.Items != null)
            {
                foreach (var subItem in filter.LeftExpression.SubFilters.Items)
                {
                    MapRightExpressionParameters(subItem.Value);
                    RecursiveSubFiltersMap(subItem.Value);
                }
            }
            
            if (filter.Items != null)
            {
                foreach (var subItem in filter.Items)
                {
                    MapRightExpressionParameters(subItem.Value);
                    RecursiveSubFiltersMap(subItem.Value);
                }
            }
        }

        private static void MapRightExpressionParameters(FilterExt subItem)
        {
            var dataValueType = subItem.DataValueType;
            
            var rightExpressions = subItem.RightExpressions;

            if (rightExpressions == null)
            {
                var rightExpression = subItem.RightExpression;
                        
                SetParameterDefaultValue(dataValueType, rightExpression);
            }
            else
            {
                foreach (var rightExpression in rightExpressions)
                {
                    SetParameterDefaultValue(dataValueType, rightExpression);
                }
            }
        }

        private static void SetParameterDefaultValue(DataValueType dataValueType, BaseExpressionExt rightExpression)
        {
            if (rightExpression?.Parameter == null ||(rightExpression?.Parameter.DataValueType != DataValueType.Mapping && rightExpression?.Parameter.DataValueType != DataValueType.Lookup))
            {
                return;
            }

            rightExpression.Parameter.DataValueType = dataValueType;
            
            switch (dataValueType)
            {
                case DataValueType.Guid:
                case DataValueType.Lookup:
                    rightExpression.Parameter.Value = default(Guid);
                    return;
                case DataValueType.Text:
                case DataValueType.ShortText:
                case DataValueType.MediumText:
                case DataValueType.MaxSizeText:
                case DataValueType.LongText:
                    rightExpression.Parameter.Value = default(string);
                    return;
                case DataValueType.Integer:
                    rightExpression.Parameter.Value = default(int);
                    return;
                case DataValueType.Money:
                case DataValueType.Float:
                case DataValueType.Float1:
                case DataValueType.Float2:
                case DataValueType.Float3:
                case DataValueType.Float4:
                case DataValueType.Float8:
                    rightExpression.Parameter.Value = default(decimal);
                    return;
                case DataValueType.DateTime:
                    rightExpression.Parameter.Value = $"\"{DateTime.MinValue:O}\"";
                    return;
                case DataValueType.Date:
                    rightExpression.Parameter.Value = $"\"{DateTime.MinValue:yyyy-MM-dd}\"";
                    return;
                case DataValueType.Time:
                    rightExpression.Parameter.Value = $"\"{DateTime.MinValue:hh:mm:ss}\"";
                    return;
                case DataValueType.Boolean:
                    rightExpression.Parameter.Value = default(bool);
                    return;
                case DataValueType.Blob:
                case DataValueType.Enum:
                case DataValueType.Image:
                case DataValueType.Object:
                case DataValueType.ImageLookup:
                case DataValueType.ValueList:
                case DataValueType.Color:
                case DataValueType.LocalizableStringDataValueType:
                case DataValueType.EntityDataValueType:
                case DataValueType.EntityCollectionDataValueType:
                case DataValueType.EntityColumnMappingCollectionDataValueType:
                case DataValueType.HashText:
                case DataValueType.SecureText:
                case DataValueType.File:
                case DataValueType.Mapping:
                case DataValueType.LocalizableParameterValuesListDataValueType:
                case DataValueType.MetaDataTextDataValueType:
                case DataValueType.ObjectList:
                case DataValueType.CompositeObjectList:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public class FiltersExt : FilterExt
    {
        public string RootSchemaName { get; set; }
    }

    public class FilterExt
    {
        private bool _isNull = true;
        private bool _isEnabled = true;

        public FilterType FilterType { get; set; }

        public FilterComparisonType ComparisonType { get; set; }

        public LogicalOperationStrict LogicalOperation { get; set; }

        public DataValueType DataValueType { get; set; }

        public bool IsNull
        {
            get => this._isNull;
            set => this._isNull = value;
        }

        public bool IsEnabled
        {
            get => this._isEnabled;
            set => this._isEnabled = value;
        }

        public bool IsNot { get; set; }

        public FilterExt SubFilters { get; set; }

        public Dictionary<string, FilterExt> Items { get; set; }

        public BaseExpressionExt LeftExpression { get; set; }

        public BaseExpressionExt RightExpression { get; set; }

        public BaseExpressionExt[] RightExpressions { get; set; }

        public BaseExpressionExt RightLessExpression { get; set; }

        public BaseExpressionExt RightGreaterExpression { get; set; }

        public bool TrimDateTimeParameterToDate { get; set; }

        public string Key { get; set; }

        public bool IsAggregative { get; set; }

        public string LeftExpressionCaption { get; set; }

        public string ReferenceSchemaName { get; set; }
    }
    
    public class BaseExpressionExt
    {
        public const int PrimaryColumnMacrosType = 34;
        public const int PrimaryDisplayColumnMacrosType = 35;
        public const int PrimaryImageColumnMacrosType = 36;

        public EntitySchemaQueryExpressionType ExpressionType { get; set; }

        public bool IsBlock { get; set; }

        public string ColumnPath { get; set; }

        public Parameter Parameter { get; set; }

        public FunctionType FunctionType { get; set; }

        public EntitySchemaQueryMacrosType MacrosType { get; set; }

        public BaseExpressionExt FunctionArgument { get; set; }

        public BaseExpressionExt[] FunctionArguments { get; set; }

        public DateDiffQueryFunctionInterval DateDiffInterval { get; set; }

        public DatePart DatePartType { get; set; }

        public AggregationType AggregationType { get; set; }

        public AggregationEvalType AggregationEvalType { get; set; }

        public FiltersExt SubFilters { get; set; }

        public ArithmeticOperation ArithmeticOperation { get; set; }

        public BaseExpressionExt LeftArithmeticOperand { get; set; }

        public BaseExpressionExt RightArithmeticOperand { get; set; }
    }
}