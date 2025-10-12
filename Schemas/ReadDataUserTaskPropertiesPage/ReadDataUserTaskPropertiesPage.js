define("ReadDataUserTaskPropertiesPage", ["ServiceHelper"], function(ServiceHelper) {
	const serviceName = "SpaceConvertFilterDataToEsqService";
	const getQueryMethodName = "GetQueryReadDataUserTask";
	const getQueryPlanMethodName = "GetQueryPlanReadDataUserTask";

	return {
		messages: {
			"GetSqlSourceCodeData": {
				"direction": BPMSoft.MessageDirectionType.SUBSCRIBE,
				"mode": BPMSoft.MessageMode.PTP
			},

			"SetSqlSourceCodeData": {
				"direction": BPMSoft.MessageDirectionType.PUBLISH,
				"mode": BPMSoft.MessageMode.PTP
			}
		},
		methods: {
			init: function() {
				this.callParent(arguments);
				
				this.sandbox.subscribe("GetSqlSourceCodeData", this.onGetQueryButtonClick, this, [this.getModuleId("SqlQueryModule")]);
				this.sandbox.subscribe("GetSqlSourceCodeData", this.onGetQueryPlanButtonClick, this, [this.getModuleId("SqlQueryPlanModule")]);
			},

			onGetQueryButtonClick: function(tag) {
				this.loadSqlSourceCodeData(getQueryMethodName, tag);
			},

			onGetQueryPlanButtonClick: function(tag) {
				this.loadSqlSourceCodeData(getQueryPlanMethodName, tag);
			},

			getInputData: function() {
				const serializationInfo = this.$FilterEditData.getDefSerializationInfo();
				serializationInfo.serializeFilterManagerInfo = true;
				const serializedFilterEditData = this.$FilterEditData.serialize(serializationInfo);

				const columns = this.$EntitySchemaColumnsControlsSelect.collection.items.map(x => x.$Id);

				const parameters = this.$Parameters.collection;

				const ignoreDisplayValues = parameters.findBy(x => x.values.Id === 'IgnoreDisplayValues').$Value.value === '[#BooleanValue.True#]';
				const resultType = parameters.findBy(x => x.values.Id === 'ResultType').$Value.value;
				const orderInfo = parameters.findBy(x => x.values.Id === 'OrderInfo').$Value.value;
				const aggregationColumnName = parameters.findBy(x => x.values.Id === 'AggregationColumnName').$Value.value;
				const functionType = parameters.findBy(x => x.values.Id === 'FunctionType').$Value.value;
				const readSomeTopRecords = parameters.findBy(x => x.values.Id === 'ReadSomeTopRecords').$Value.value;
				const numberOfRecords = parameters.findBy(x => x.values.Id === 'NumberOfRecords').$Value.value;

				return {
					request: {
						schemaName: this.$EntitySchemaSelect.name,
						filters:  serializedFilterEditData,
						columns: columns,
						ignoreDisplayValues: ignoreDisplayValues,
						resultType: resultType,
						orderInfo: orderInfo,
						aggregationColumnName: aggregationColumnName,
						functionType: functionType,
						readSomeTopRecords: readSomeTopRecords,
						numberOfRecords: numberOfRecords
					}
				};
			},

			loadSqlSourceCodeData: function(methodName, tag) {
				const inputData = this.getInputData();

				ServiceHelper.callService({
					serviceName: serviceName,
					methodName: methodName,
					scope: this,
					data: inputData,
					callback: function(response) {
						const result = response[`${methodName}Result`];

						this.sandbox.publish("SetSqlSourceCodeData", result.query || result.exception, [tag]);
					}
				});
			}
		}
	};
 });