define("SqlSourceCodeEditPage", ["SourceCodeEditEnums", "SqlSourceCodeEditPageResources", "SourceCodeEdit", "css!SqlSourceCodeEditPageCSS"],
    function (sourceCodeEditEnums) {
        return {
            attributes: {
                "Tag": {
                    dataValueType: this.BPMSoft.DataValueType.TEXT,
                    type: this.BPMSoft.ViewModelColumnType.VIRTUAL_COLUMN
                }
            },
            messages: {
                "GetSqlSourceCodeData": {
                    "direction": BPMSoft.MessageDirectionType.PUBLISH,
                    "mode": BPMSoft.MessageMode.PTP
                },

                "SetSqlSourceCodeData": {
                    "direction": BPMSoft.MessageDirectionType.SUBSCRIBE,
                    "mode": BPMSoft.MessageMode.PTP
                }
            },
            methods: {
                init: function () {
                    this.callParent(arguments);

                    this.initSettings();
                    this.subscribeEvents();
                    this.getSqlSourceCodeData();
                },

                subscribeEvents: function () {
                    this.sandbox.subscribe("SetSqlSourceCodeData", this.setSqlSourceCodeData, this, [this.get("Tag")]);
                },

                initSettings: function () {
                    this.set("Language", sourceCodeEditEnums.Language.SQL);
                    this.set("Theme", sourceCodeEditEnums.Theme.CRIMSON_EDITOR);
                    this.set("IsSqlSourceCodeReadonly", true);
                    this.set("IsSqlSourceCodeEnabled", true);
                },

                onRender: function () {
                    this.callParent(arguments);

                    var tag = this.get("Tag");

                    var parentId = document.querySelector(`#SqlSourceCodeEditPage-${tag}-schema`).querySelector('.source-code-edit-tools-container').id
                    var parentEl = Ext.get(parentId);

                    var div = document.createElement('div');
                    div.id = `tooltip-${tag}`;
                    div.className = "tooltip";

                    parentEl.appendChild(div);
                },

                onExpandButtonClick: function () {
                    var isExpanded = this.get("IsExpanded");
                    var markerValue = "source-code-edit-page" + (isExpanded ? "" : "-expand");
                    this.set("MainContainerMarkerValue", markerValue);
                    this.set("IsExpanded", !isExpanded);
                },

                getExpandButtonImageConfig: function () {
                    if (this.get("IsExpanded")) {
                        return this.get("Resources.Images.CollapseImage");
                    }
                    return this.get("Resources.Images.ExpandImage");
                },

                getCopyButtonImageConfig: function () {
                    return this.get("Resources.Images.CopyImage");
                },

                onCopyButtonClick: function () {
                    navigator.clipboard.writeText(this.get("SqlSourceCode"));

                    var tag = this.get("Tag");
                    var tooltip = document.getElementById(`tooltip-${tag}`);
                    tooltip.classList.add('active');

                    setTimeout(() => {
                        tooltip.classList.remove('active');
                    }, 1500);
                },

                getSqlSourceCodeData: function () {
                    var tag = this.get("Tag");
                    this.sandbox.publish("GetSqlSourceCodeData", tag, [this.sandbox.id]);
                },

                setSqlSourceCodeData: function (data) {
                    this.set("SqlSourceCode", data + "\n\n\n");
                }
            },
            diff: [
                {
                    "operation": "insert",
                    "name": "SqlSourceCodeEditMainContainer",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "wrapClass": ["source-code-edit-page-container"],
                        "markerValue": {bindTo: "MainContainerMarkerValue"},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "name": "BackgroundContainer",
                    "parentName": "SqlSourceCodeEditMainContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["background-container"]},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "name": "SqlSourceCodeEditMaskContainer",
                    "parentName": "SqlSourceCodeEditMainContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["source-code-edit-mask-container"]},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "name": "SqlSourceCodeEditExpandableContainer",
                    "parentName": "SqlSourceCodeEditMainContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["source-code-edit-expandable-container"]},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "name": "SqlSourceCodeEditContainer",
                    "parentName": "SqlSourceCodeEditExpandableContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["source-code-edit-container"]},
                        "markerValue": {bindTo: "getSourceCodeEditContainerMarkerValue"},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "parentName": "SqlSourceCodeEditContainer",
                    "propertyName": "items",
                    "name": "SqlSourceCodeEditToolsContainer",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["source-code-edit-tools-container"]},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "parentName": "SqlSourceCodeEditToolsContainer",
                    "propertyName": "items",
                    "name": "ScriptTaskExpandLabelContainer",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "parentName": "ScriptTaskExpandLabelContainer",
                    "propertyName": "items",
                    "name": "ScriptTaskExpandLabel",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.LABEL,
                        "caption": {bindTo: "Caption"},
                        "classes": {"labelClass": ["source-code-caption"]}
                    }
                },
                {
                    "operation": "insert",
                    "name": "CopyButton",
                    "parentName": "SqlSourceCodeEditToolsContainer",
                    "propertyName": "items",
                    "values": {
                        "itemType": BPMSoft.ViewItemType.BUTTON,
                        "imageConfig": {bindTo: "getCopyButtonImageConfig"},
                        "classes": {"wrapperClass": ["source-code-edit-tool-button", "copy-button"]},
                        "click": {bindTo: "onCopyButtonClick"},
                    }
                },
                {
                    "operation": "insert",
                    "name": "ExpandButton",
                    "parentName": "SqlSourceCodeEditToolsContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.BUTTON,
                        "imageConfig": {bindTo: "getExpandButtonImageConfig"},
                        "classes": {"wrapperClass": ["source-code-edit-tool-button", "expand-button"]},
                        "click": {bindTo: "onExpandButtonClick"}
                    }
                },
                {
                    "operation": "insert",
                    "name": "SqlSourceCodeEditControlWrapContainer",
                    "parentName": "SqlSourceCodeEditContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "itemType": BPMSoft.ViewItemType.CONTAINER,
                        "classes": {"wrapClassName": ["space-source-code-edit-control-wrap-container"]},
                        "items": []
                    }
                },
                {
                    "operation": "insert",
                    "name": "SqlSourceCode",
                    "parentName": "SqlSourceCodeEditControlWrapContainer",
                    "propertyName": "items",
                    "values": {
                        "generateId": false,
                        "generator": "SourceCodeEditGenerator.generate",
                        "markerValue": {bindTo: "SourceCodeEditMarkerValue"},
                        "readonly": {bindTo: "IsSqlSourceCodeReadonly"},
                        "enabled": {bindTo: "IsSqlSourceCodeEnabled"},
                        "language": {bindTo: "Language"},
                        "theme": {bindTo: "Theme"},
                        "expandEditor": {bindTo: "onExpandButtonClick"}
                    }
                },
            ]
        }
    });