using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace QuickFieldToggle
{
    #region Configuration Classes

    /// <summary>
    /// Represents an additional action to perform when a menu item is selected
    /// </summary>
    public class AdditionalAction
    {
        public string Field { get; set; }
        public string Action { get; set; } // "set" or "remove"
        public string Value { get; set; } = "true";
    }

    /// <summary>
    /// A single condition rule for conditional display
    /// </summary>
    public class ConditionRule
    {
        public string Field { get; set; }        // "Platform", "Genre", or custom field name
        public string Operator { get; set; }     // "equals", "notEquals", "contains", "notContains", "exists", "notExists"
        public string Value { get; set; }        // Value to compare against (can be null for exists/notExists)
    }

    /// <summary>
    /// A group of conditions with a logical operator
    /// </summary>
    public class ConditionGroup
    {
        public string Logic { get; set; } = "and";  // "and" or "or"
        public List<ConditionRule> Rules { get; set; } = new List<ConditionRule>();
    }

    /// <summary>
    /// Main toggle item configuration
    /// </summary>
    public class ToggleConfig
    {
        public string FieldName { get; set; }
        public string MenuLabel { get; set; }
        public string ToggledMenuLabel { get; set; }  // Optional: label when field is enabled (instead of checkmark)
        public string EnableValue { get; set; } = "true";
        public string OperationType { get; set; } = "toggle";  // "toggle", "set", "remove"
        public bool Enabled { get; set; } = true;
        public List<AdditionalAction> AdditionalActions { get; set; } = new List<AdditionalAction>();
        public List<ConditionGroup> Conditions { get; set; } = new List<ConditionGroup>();  // All groups must pass (AND)
        
        // For multi-value picker mode
        public string Mode { get; set; } = "simple";  // "simple" or "multiValue"
        public List<string> Values { get; set; } = new List<string>();  // Predefined values for multiValue mode
        public string ValueSource { get; set; }  // "config" or "field" - where to get the values list
        
        // Icon: "default", "media:Name", "path:filename.png", or null
        // Note: "playlist:Name" is deprecated and will be removed in a future release
        public string Icon { get; set; }
        
        // If true, always show confirmation dialog regardless of bulk threshold
        public bool RequireConfirmation { get; set; } = false;
        
        // Sort order for multi-value pickers: "alphabetical", "reverseAlphabetical", "mostCommon", "leastCommon", "none", or null (inherit)
        public string ValueSortOrder { get; set; }
        
        // Selection indicator override for this item: "symbol" or "count" (null = inherit from group/root)
        public string SelectionIndicator { get; set; }
    }

    public class GroupConfig
    {
        public string GroupName { get; set; }
        public bool Enabled { get; set; } = true;
        public List<ToggleConfig> Items { get; set; } = new List<ToggleConfig>();
        public List<ConditionGroup> Conditions { get; set; } = new List<ConditionGroup>();  // Conditions for entire group
        
        // Icon: "default", "media:Name", "path:filename.png", or null
        // Note: "playlist:Name" is deprecated and will be removed in a future release
        public string Icon { get; set; }
        
        // IconCascade: "inherit" = items without icons inherit group's icon, "none" = no inheritance (default)
        public string IconCascade { get; set; } = "none";
        
        // Sort order for multi-value pickers in this group: "alphabetical", "reverseAlphabetical", "mostCommon", "leastCommon", "none", or null (inherit from root)
        public string ValueSortOrder { get; set; }
        
        // Selection indicator override for this group: "symbol" or "count" (null = inherit from root)
        public string SelectionIndicator { get; set; }
    }

    public class PluginConfiguration
    {
        public List<GroupConfig> Groups { get; set; } = new List<GroupConfig>();
        public List<ToggleConfig> UngroupedItems { get; set; } = new List<ToggleConfig>();
        
        // Root-level defaults (users can override per-group/item)
        public string DefaultIcon { get; set; } = "default";
        public string DefaultIconCascade { get; set; } = "none";
        
        // Bulk confirmation settings
        public bool ConfirmBulkChanges { get; set; } = false;
        public int ConfirmBulkThreshold { get; set; } = 10;
        
        // Clear All Custom Fields feature
        public bool InjectClearAllFields { get; set; } = false;
        public string ClearAllFieldsLabel { get; set; } = "Clear All Custom Fields";
        public string ClearAllFieldsIcon { get; set; } = "delete";
        
        // Selective Clear Fields feature
        public bool InjectSelectiveClear { get; set; } = false;
        public string SelectiveClearLabel { get; set; } = "Selectively Clear Fields";
        public string SelectiveClearIcon { get; set; } = "delete";
        
        // Selection indicator style: "symbol" (default: ✓/◐) or "count" (X of Y)
        public string SelectionIndicator { get; set; } = "symbol";
        
        // Override selection indicator for SelectiveClear feature (null = use root SelectionIndicator)
        public string SelectiveClearIndicator { get; set; }
        
        // Default sort order for multi-value pickers: "alphabetical", "reverseAlphabetical", "mostCommon", "leastCommon", "none"
        public string ValueSortOrder { get; set; } = "alphabetical";

        private static PluginConfiguration _instance;
        private static readonly object _lock = new object();

        public static PluginConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = Load();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void Reload()
        {
            lock (_lock)
            {
                _instance = Load();
            }
        }

        private static PluginConfiguration Load()
        {
            try
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string pluginDir = Path.GetDirectoryName(assemblyLocation);
                string configPath = Path.Combine(pluginDir, "quickfieldtoggle.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    return ParseJson(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Config load error: {ex.Message}");
            }

            return GetDefaultConfig();
        }

        private static PluginConfiguration ParseJson(string json)
        {
            var config = new PluginConfiguration();

            // Parse root-level defaults
            config.DefaultIcon = ExtractStringValue(json, "defaultIcon") ?? "default";
            config.DefaultIconCascade = ExtractStringValue(json, "defaultIconCascade") ?? "none";
            
            // Parse bulk confirmation settings
            config.ConfirmBulkChanges = ExtractBoolValue(json, "confirmBulkChanges", false);
            config.ConfirmBulkThreshold = ExtractIntValue(json, "confirmBulkThreshold", 10);
            
            // Parse clear all fields settings
            config.InjectClearAllFields = ExtractBoolValue(json, "injectClearAllFields", false);
            config.ClearAllFieldsLabel = ExtractStringValue(json, "clearAllFieldsLabel") ?? "Clear All Custom Fields";
            config.ClearAllFieldsIcon = ExtractStringValue(json, "clearAllFieldsIcon") ?? "delete";
            
            // Parse selective clear settings
            config.InjectSelectiveClear = ExtractBoolValue(json, "injectSelectiveClear", false);
            config.SelectiveClearLabel = ExtractStringValue(json, "selectiveClearLabel") ?? "Selectively Clear Fields";
            config.SelectiveClearIcon = ExtractStringValue(json, "selectiveClearIcon") ?? "delete";
            
            // Parse selection indicator and sort settings
            config.SelectionIndicator = ExtractStringValue(json, "selectionIndicator") ?? "symbol";
            config.SelectiveClearIndicator = ExtractStringValue(json, "selectiveClearIndicator");
            config.ValueSortOrder = ExtractStringValue(json, "valueSortOrder") ?? "alphabetical";

            // Parse groups
            int groupsStart = json.IndexOf("\"groups\"");
            if (groupsStart != -1)
            {
                int groupsArrayStart = json.IndexOf('[', groupsStart);
                int groupsArrayEnd = FindMatchingBracket(json, groupsArrayStart);
                
                if (groupsArrayStart != -1 && groupsArrayEnd != -1)
                {
                    string groupsContent = json.Substring(groupsArrayStart + 1, groupsArrayEnd - groupsArrayStart - 1);
                    var groupObjects = ExtractObjects(groupsContent);

                    foreach (var groupObj in groupObjects)
                    {
                        var group = new GroupConfig();
                        
                        // Extract group-level properties by removing the "items" array content
                        // This ensures we don't accidentally pick up item-level properties
                        // JSON objects are unordered, so items can appear anywhere
                        string groupPropsOnly = RemoveJsonArray(groupObj, "items");
                        
                        group.GroupName = ExtractStringValue(groupPropsOnly, "groupName");
                        group.Enabled = ExtractBoolValue(groupPropsOnly, "enabled");
                        group.Conditions = ParseConditions(groupObj);  // Conditions have their own logic
                        group.Icon = ExtractStringValue(groupPropsOnly, "icon") ?? config.DefaultIcon;
                        group.IconCascade = ExtractStringValue(groupPropsOnly, "iconCascade") ?? config.DefaultIconCascade;
                        group.ValueSortOrder = ExtractStringValue(groupPropsOnly, "valueSortOrder");
                        group.SelectionIndicator = ExtractStringValue(groupPropsOnly, "selectionIndicator");

                        // Parse items within this group
                        int itemsStart = groupObj.IndexOf("\"items\"");
                        if (itemsStart != -1)
                        {
                            int itemsArrayStart = groupObj.IndexOf('[', itemsStart);
                            int itemsArrayEnd = FindMatchingBracket(groupObj, itemsArrayStart);
                            
                            if (itemsArrayStart != -1 && itemsArrayEnd != -1)
                            {
                                string itemsContent = groupObj.Substring(itemsArrayStart + 1, itemsArrayEnd - itemsArrayStart - 1);
                                var itemObjects = ExtractObjects(itemsContent);

                                foreach (var itemObj in itemObjects)
                                {
                                    var toggle = ParseToggleConfig(itemObj);
                                    if (toggle != null)
                                    {
                                        group.Items.Add(toggle);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(group.GroupName))
                        {
                            config.Groups.Add(group);
                        }
                    }
                }
            }

            // Parse ungroupedItems
            int ungroupedStart = json.IndexOf("\"ungroupedItems\"");
            if (ungroupedStart != -1)
            {
                int ungroupedArrayStart = json.IndexOf('[', ungroupedStart);
                int ungroupedArrayEnd = FindMatchingBracket(json, ungroupedArrayStart);
                
                if (ungroupedArrayStart != -1 && ungroupedArrayEnd != -1)
                {
                    string ungroupedContent = json.Substring(ungroupedArrayStart + 1, ungroupedArrayEnd - ungroupedArrayStart - 1);
                    var itemObjects = ExtractObjects(ungroupedContent);

                    foreach (var itemObj in itemObjects)
                    {
                        var toggle = ParseToggleConfig(itemObj);
                        if (toggle != null)
                        {
                            config.UngroupedItems.Add(toggle);
                        }
                    }
                }
            }

            return (config.Groups.Count > 0 || config.UngroupedItems.Count > 0) ? config : GetDefaultConfig();
        }

        private static int FindMatchingBracket(string json, int openBracketPos)
        {
            if (openBracketPos == -1 || openBracketPos >= json.Length) return -1;
            
            char openChar = json[openBracketPos];
            char closeChar = openChar == '[' ? ']' : '}';
            
            int depth = 1;
            bool inString = false;
            for (int i = openBracketPos + 1; i < json.Length; i++)
            {
                char c = json[i];
                
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                    continue;
                }
                
                if (inString) continue;
                
                if (c == openChar) depth++;
                else if (c == closeChar)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes a named array property from a JSON object string.
        /// Used to exclude nested content (like "items") when parsing parent-level properties.
        /// This ensures JSON property order doesn't matter.
        /// </summary>
        private static string RemoveJsonArray(string json, string arrayName)
        {
            string pattern = $"\"{arrayName}\"";
            int keyStart = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (keyStart == -1) return json;
            
            // Find the opening bracket of the array
            int bracketStart = json.IndexOf('[', keyStart);
            if (bracketStart == -1) return json;
            
            // Find the matching closing bracket
            int bracketEnd = FindMatchingBracket(json, bracketStart);
            if (bracketEnd == -1) return json;
            
            // Remove from the key to the end of the array (including any trailing comma)
            int removeEnd = bracketEnd + 1;
            
            // Skip trailing whitespace and comma
            while (removeEnd < json.Length && (char.IsWhiteSpace(json[removeEnd]) || json[removeEnd] == ','))
            {
                removeEnd++;
            }
            
            // Also handle leading comma if this wasn't the first property
            int removeStart = keyStart;
            int checkPos = keyStart - 1;
            while (checkPos >= 0 && char.IsWhiteSpace(json[checkPos]))
            {
                checkPos--;
            }
            if (checkPos >= 0 && json[checkPos] == ',')
            {
                removeStart = checkPos;
            }
            
            return json.Substring(0, removeStart) + json.Substring(removeEnd);
        }

        private static List<string> ExtractObjects(string arrayContent)
        {
            var objects = new List<string>();
            int depth = 0;
            int start = 0;
            bool inObject = false;
            bool inString = false;

            for (int i = 0; i < arrayContent.Length; i++)
            {
                char c = arrayContent[i];
                
                if (c == '"' && (i == 0 || arrayContent[i - 1] != '\\'))
                {
                    inString = !inString;
                    continue;
                }
                
                if (inString) continue;
                
                if (c == '{')
                {
                    if (depth == 0) start = i;
                    depth++;
                    inObject = true;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && inObject)
                    {
                        objects.Add(arrayContent.Substring(start, i - start + 1));
                        inObject = false;
                    }
                }
            }

            return objects;
        }

        private static ToggleConfig ParseToggleConfig(string obj)
        {
            var toggle = new ToggleConfig();
            toggle.FieldName = ExtractStringValue(obj, "fieldName");
            toggle.MenuLabel = ExtractStringValue(obj, "menuLabel");
            toggle.ToggledMenuLabel = ExtractStringValue(obj, "toggledMenuLabel");
            toggle.EnableValue = ExtractStringValue(obj, "enableValue") ?? "true";
            toggle.OperationType = ExtractStringValue(obj, "operationType") ?? "toggle";
            toggle.Enabled = ExtractBoolValue(obj, "enabled");
            toggle.Mode = ExtractStringValue(obj, "mode") ?? "simple";
            toggle.ValueSource = ExtractStringValue(obj, "valueSource") ?? "config";
            toggle.Icon = ExtractStringValue(obj, "icon");
            toggle.RequireConfirmation = ExtractBoolValue(obj, "requireConfirmation", false);
            toggle.ValueSortOrder = ExtractStringValue(obj, "valueSortOrder");
            toggle.SelectionIndicator = ExtractStringValue(obj, "selectionIndicator");
            
            // Parse additionalActions
            toggle.AdditionalActions = ParseAdditionalActions(obj);
            
            // Parse conditions
            toggle.Conditions = ParseConditions(obj);
            
            // Parse values array for multiValue mode
            toggle.Values = ParseStringArray(obj, "values");

            if (!string.IsNullOrEmpty(toggle.FieldName) && !string.IsNullOrEmpty(toggle.MenuLabel))
            {
                return toggle;
            }
            return null;
        }

        private static List<AdditionalAction> ParseAdditionalActions(string obj)
        {
            var actions = new List<AdditionalAction>();

            int actionsStart = obj.IndexOf("\"additionalActions\"");
            if (actionsStart == -1) return actions;

            int actionsArrayStart = obj.IndexOf('[', actionsStart);
            if (actionsArrayStart == -1) return actions;

            int actionsArrayEnd = FindMatchingBracket(obj, actionsArrayStart);
            if (actionsArrayEnd == -1) return actions;

            string actionsContent = obj.Substring(actionsArrayStart + 1, actionsArrayEnd - actionsArrayStart - 1);
            var actionObjects = ExtractObjects(actionsContent);

            foreach (var actionObj in actionObjects)
            {
                var action = new AdditionalAction();
                action.Field = ExtractStringValue(actionObj, "field");
                action.Action = ExtractStringValue(actionObj, "action");
                action.Value = ExtractStringValue(actionObj, "value") ?? "true";

                if (!string.IsNullOrEmpty(action.Field) && !string.IsNullOrEmpty(action.Action))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        private static List<ConditionGroup> ParseConditions(string obj)
        {
            var groups = new List<ConditionGroup>();

            int conditionsStart = obj.IndexOf("\"conditions\"");
            if (conditionsStart == -1) return groups;

            // IMPORTANT: Make sure we're not finding conditions inside nested "items"
            // Only parse conditions that appear BEFORE the items array
            int itemsStart = obj.IndexOf("\"items\"");
            if (itemsStart != -1 && conditionsStart > itemsStart)
            {
                // The conditions we found are inside an item, not at the group level
                return groups;
            }

            int conditionsArrayStart = obj.IndexOf('[', conditionsStart);
            if (conditionsArrayStart == -1) return groups;

            int conditionsArrayEnd = FindMatchingBracket(obj, conditionsArrayStart);
            if (conditionsArrayEnd == -1) return groups;

            string conditionsContent = obj.Substring(conditionsArrayStart + 1, conditionsArrayEnd - conditionsArrayStart - 1);
            var groupObjects = ExtractObjects(conditionsContent);

            foreach (var groupObj in groupObjects)
            {
                var group = new ConditionGroup();
                group.Logic = ExtractStringValue(groupObj, "logic") ?? "and";
                
                // Parse rules array
                int rulesStart = groupObj.IndexOf("\"rules\"");
                if (rulesStart != -1)
                {
                    int rulesArrayStart = groupObj.IndexOf('[', rulesStart);
                    if (rulesArrayStart != -1)
                    {
                        int rulesArrayEnd = FindMatchingBracket(groupObj, rulesArrayStart);
                        if (rulesArrayEnd != -1)
                        {
                            string rulesContent = groupObj.Substring(rulesArrayStart + 1, rulesArrayEnd - rulesArrayStart - 1);
                            var ruleObjects = ExtractObjects(rulesContent);

                            foreach (var ruleObj in ruleObjects)
                            {
                                var rule = new ConditionRule();
                                rule.Field = ExtractStringValue(ruleObj, "field");
                                rule.Operator = ExtractStringValue(ruleObj, "operator") ?? "equals";
                                rule.Value = ExtractStringValue(ruleObj, "value");

                                if (!string.IsNullOrEmpty(rule.Field))
                                {
                                    group.Rules.Add(rule);
                                }
                            }
                        }
                    }
                }

                if (group.Rules.Count > 0)
                {
                    groups.Add(group);
                }
            }

            return groups;
        }

        private static List<string> ParseStringArray(string obj, string key)
        {
            var values = new List<string>();

            string pattern = $"\"{key}\"";
            int keyStart = obj.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (keyStart == -1) return values;

            int arrayStart = obj.IndexOf('[', keyStart);
            if (arrayStart == -1) return values;

            int arrayEnd = FindMatchingBracket(obj, arrayStart);
            if (arrayEnd == -1) return values;

            string arrayContent = obj.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
            
            // Extract string values from the array
            int pos = 0;
            while (pos < arrayContent.Length)
            {
                int quoteStart = arrayContent.IndexOf('"', pos);
                if (quoteStart == -1) break;
                
                int quoteEnd = arrayContent.IndexOf('"', quoteStart + 1);
                if (quoteEnd == -1) break;
                
                values.Add(arrayContent.Substring(quoteStart + 1, quoteEnd - quoteStart - 1));
                pos = quoteEnd + 1;
            }

            return values;
        }

        private static string ExtractStringValue(string json, string key)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (keyIndex == -1) return null;

            int colonIndex = json.IndexOf(':', keyIndex);
            if (colonIndex == -1) return null;

            // Skip whitespace and find the quote
            int valueStart = -1;
            for (int i = colonIndex + 1; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"')
                {
                    valueStart = i + 1;
                    break;
                }
                else if (c == '[' || c == '{' || c == 't' || c == 'f' || c == 'n' || char.IsDigit(c))
                {
                    // Not a string value
                    return null;
                }
            }
            
            if (valueStart == -1) return null;

            int valueEnd = json.IndexOf('"', valueStart);
            if (valueEnd == -1) return null;

            return json.Substring(valueStart, valueEnd - valueStart);
        }

        private static bool ExtractBoolValue(string json, string key, bool defaultValue = true)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (keyIndex == -1) return defaultValue;

            int colonIndex = json.IndexOf(':', keyIndex);
            if (colonIndex == -1) return defaultValue;

            string afterColon = json.Substring(colonIndex + 1, Math.Min(20, json.Length - colonIndex - 1)).Trim().ToLower();
            if (afterColon.StartsWith("true")) return true;
            if (afterColon.StartsWith("false")) return false;
            return defaultValue;
        }

        private static int ExtractIntValue(string json, string key, int defaultValue = 0)
        {
            string pattern = $"\"{key}\"";
            int keyIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (keyIndex == -1) return defaultValue;

            int colonIndex = json.IndexOf(':', keyIndex);
            if (colonIndex == -1) return defaultValue;

            // Find the number after the colon
            int start = colonIndex + 1;
            while (start < json.Length && char.IsWhiteSpace(json[start])) start++;
            
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            
            if (end > start && int.TryParse(json.Substring(start, end - start), out int result))
            {
                return result;
            }
            return defaultValue;
        }

        private static PluginConfiguration GetDefaultConfig()
        {
            return new PluginConfiguration
            {
                Groups = new List<GroupConfig>
                {
                    new GroupConfig
                    {
                        GroupName = "Quick Tags",
                        Enabled = true,
                        Items = new List<ToggleConfig>
                        {
                            new ToggleConfig { FieldName = "Discovery Bin", MenuLabel = "Discovery Bin", EnableValue = "true", Enabled = true }
                        }
                    }
                }
            };
        }
    }

    #endregion

    #region Field Helper

    /// <summary>
    /// Helper class for common custom field operations
    /// </summary>
    public static class FieldHelper
    {
        public static void SetField(IGame game, string fieldName, string value)
        {
            try
            {
                var customFields = game.GetAllCustomFields();
                var existingField = customFields?.FirstOrDefault(cf =>
                    cf.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (existingField != null)
                {
                    existingField.Value = value;
                }
                else
                {
                    var newField = game.AddNewCustomField();
                    newField.Name = fieldName;
                    newField.Value = value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetField error for {fieldName}: {ex.Message}");
            }
        }

        public static void RemoveField(IGame game, string fieldName)
        {
            try
            {
                var customFields = game.GetAllCustomFields();
                var existingField = customFields?.FirstOrDefault(cf =>
                    cf.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (existingField != null)
                {
                    game.TryRemoveCustomField(existingField);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveField error for {fieldName}: {ex.Message}");
            }
        }

        public static string GetFieldValue(IGame game, string fieldName)
        {
            var customFields = game.GetAllCustomFields();
            var field = customFields?.FirstOrDefault(cf =>
                cf.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            return field?.Value;
        }

        public static bool HasFieldWithValue(IGame game, string fieldName, string value)
        {
            var customFields = game.GetAllCustomFields();
            return customFields?.Any(cf =>
                cf.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase) &&
                cf.Value.Equals(value, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static bool FieldExists(IGame game, string fieldName)
        {
            var customFields = game.GetAllCustomFields();
            return customFields?.Any(cf =>
                cf.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)) == true;
        }

        public static void ProcessAdditionalActions(IGame game, List<AdditionalAction> actions)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                switch (action.Action.ToLower())
                {
                    case "set":
                        SetField(game, action.Field, action.Value);
                        break;
                    case "remove":
                        RemoveField(game, action.Field);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets a game property value by name - uses reflection to find IGame properties first,
        /// then falls back to custom fields if no matching property is found.
        /// This allows supporting any IGame property (Platform, GenresString, Developer, etc.)
        /// without hardcoding them.
        /// </summary>
        public static string GetGameProperty(IGame game, string propertyName)
        {
            // First, try to find a property on IGame using reflection
            try
            {
                var property = game.GetType().GetProperty(propertyName, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                
                if (property != null)
                {
                    var value = property.GetValue(game);
                    return value?.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reflection error for {propertyName}: {ex.Message}");
            }

            // Fall back to custom field if no IGame property found
            return GetFieldValue(game, propertyName);
        }
    }

    #endregion

    #region Confirmation Helper

    /// <summary>
    /// Helper class for bulk operation confirmation dialogs
    /// </summary>
    public static class ConfirmationHelper
    {
        /// <summary>
        /// Shows a confirmation dialog if needed based on config settings
        /// Returns true if the operation should proceed, false to cancel
        /// </summary>
        public static bool ShouldProceed(int gameCount, bool itemRequiresConfirmation, string actionDescription)
        {
            var config = PluginConfiguration.Instance;
            
            // Check if confirmation is needed
            bool needsConfirmation = itemRequiresConfirmation || 
                (config.ConfirmBulkChanges && gameCount >= config.ConfirmBulkThreshold);
            
            if (!needsConfirmation)
                return true;
            
            string message = gameCount == 1
                ? $"Are you sure you want to {actionDescription}?"
                : $"Are you sure you want to {actionDescription} for {gameCount} games?";
            
            var result = System.Windows.Forms.MessageBox.Show(
                message,
                "Confirm Action",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Question);
            
            return result == System.Windows.Forms.DialogResult.Yes;
        }
    }

    #endregion

    #region Icon Helper

    /// <summary>
    /// Helper class for loading and caching menu icons
    /// Supports: "default", "media:Name", "path:filename.png"
    /// Note: "playlist:Name" is deprecated and will be removed in a future release
    /// Prioritizes the user's active Platform Icon pack from LaunchBox settings.
    /// </summary>
    public static class IconHelper
    {
        private static readonly Dictionary<string, Image> _iconCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static Image _defaultIcon;
        private static Image _deleteIcon;
        private static string _launchBoxRoot;
        private static string _mediaPackPath;
        private static string _activePlatformIconPack;
        private static bool _activePackLoaded = false;
        private static readonly object _cacheLock = new object();

        /// <summary>
        /// Gets the LaunchBox root folder by walking up from the plugin DLL location
        /// until we find a folder containing "Data" and "Images" subfolders
        /// </summary>
        private static string LaunchBoxRoot
        {
            get
            {
                if (_launchBoxRoot == null)
                {
                    // Start from the DLL location and walk up until we find LaunchBox root
                    var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    
                    // Walk up the directory tree looking for LaunchBox root
                    // (identified by having Data and Images folders)
                    for (int i = 0; i < 5; i++) // Max 5 levels up
                    {
                        if (currentDir == null) break;
                        
                        var dataFolder = Path.Combine(currentDir, "Data");
                        var imagesFolder = Path.Combine(currentDir, "Images");
                        
                        if (Directory.Exists(dataFolder) && Directory.Exists(imagesFolder))
                        {
                            _launchBoxRoot = currentDir;
                            break;
                        }
                        
                        currentDir = Path.GetDirectoryName(currentDir);
                    }
                    
                    // Fallback: if not found, assume we're in Plugins subfolder
                    if (_launchBoxRoot == null)
                    {
                        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        // Try going up 2 levels (Plugins\QuickFieldToggle -> Plugins -> LaunchBox)
                        _launchBoxRoot = Path.GetDirectoryName(Path.GetDirectoryName(dllDir));
                    }
                }
                return _launchBoxRoot;
            }
        }

        /// <summary>
        /// Gets the Platform Icons folder (contains all icon packs)
        /// </summary>
        private static string PlatformIconsRoot
        {
            get
            {
                if (_mediaPackPath == null)
                {
                    _mediaPackPath = Path.Combine(LaunchBoxRoot, "Images", "Media Packs", "Platform Icons");
                }
                return _mediaPackPath;
            }
        }

        /// <summary>
        /// Gets the active Platform Icon pack name from LaunchBox settings
        /// </summary>
        private static string ActivePlatformIconPack
        {
            get
            {
                if (!_activePackLoaded)
                {
                    _activePackLoaded = true;
                    try
                    {
                        var settingsPath = Path.Combine(LaunchBoxRoot, "Data", "Settings.xml");
                        if (File.Exists(settingsPath))
                        {
                            var settingsXml = File.ReadAllText(settingsPath);
                            // Simple XML parsing for PlatformIconPack element
                            var startTag = "<PlatformIconPack>";
                            var endTag = "</PlatformIconPack>";
                            var startIdx = settingsXml.IndexOf(startTag);
                            if (startIdx >= 0)
                            {
                                startIdx += startTag.Length;
                                var endIdx = settingsXml.IndexOf(endTag, startIdx);
                                if (endIdx > startIdx)
                                {
                                    _activePlatformIconPack = settingsXml.Substring(startIdx, endIdx - startIdx).Trim();
                                    System.Diagnostics.Debug.WriteLine($"Active Platform Icon Pack: {_activePlatformIconPack}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error reading PlatformIconPack setting: {ex.Message}");
                    }
                }
                return _activePlatformIconPack;
            }
        }

        /// <summary>
        /// Gets an icon based on the icon specification string
        /// </summary>
        public static Image GetIcon(string iconSpec)
        {
            if (string.IsNullOrWhiteSpace(iconSpec))
                return null;

            lock (_cacheLock)
            {
                // Check cache first
                if (_iconCache.TryGetValue(iconSpec, out var cached))
                    return cached;

                Image icon = null;

                try
                {
                    if (iconSpec.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        icon = GetDefaultIcon();
                    }
                    else if (iconSpec.Equals("delete", StringComparison.OrdinalIgnoreCase))
                    {
                        icon = GetDeleteIcon();
                    }
                    else if (iconSpec.StartsWith("media:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Look in ALL Platform Icon packs for this icon
                        var iconName = iconSpec.Substring(6);
                        icon = GetPlatformIconFromPacks(iconName);
                    }
                    else if (iconSpec.StartsWith("platform:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Look in ALL Platform Icon packs (same as media:)
                        // We do NOT fall back to Clear Logos - wrong size/format
                        var platformName = iconSpec.Substring(9);
                        icon = GetPlatformIconFromPacks(platformName);
                    }
                    else if (iconSpec.StartsWith("playlist:", StringComparison.OrdinalIgnoreCase))
                    {
                        var playlistName = iconSpec.Substring(9);
                        icon = GetPlaylistIcon(playlistName);
                    }
                    else if (iconSpec.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = iconSpec.Substring(5);
                        icon = GetIconFromPath(path);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Icon load error for '{iconSpec}': {ex.Message}");
                }

                // Cache the result (even if null, to avoid repeated failed lookups)
                _iconCache[iconSpec] = icon;
                return icon;
            }
        }

        /// <summary>
        /// Gets an icon from Platform Icon packs.
        /// FIRST checks the user's active pack (from LaunchBox settings),
        /// THEN falls back to searching all other packs.
        /// </summary>
        private static Image GetPlatformIconFromPacks(string iconName)
        {
            try
            {
                if (!Directory.Exists(PlatformIconsRoot))
                {
                    System.Diagnostics.Debug.WriteLine($"Platform Icons folder not found: {PlatformIconsRoot}");
                    return null;
                }

                string[] extensions = { ".png", ".ico", ".bmp", ".jpg", ".gif" };

                // Get all pack directories
                var allPacks = Directory.GetDirectories(PlatformIconsRoot).ToList();

                // Prioritize the active pack - check it first
                var activePack = ActivePlatformIconPack;
                if (!string.IsNullOrEmpty(activePack))
                {
                    var activePackPath = Path.Combine(PlatformIconsRoot, activePack);
                    if (Directory.Exists(activePackPath))
                    {
                        // Try to find icon in active pack first
                        var result = FindIconInPack(activePackPath, iconName, extensions);
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found '{iconName}' in active pack: {activePack}");
                            return result;
                        }

                        // Remove active pack from fallback list (we already checked it)
                        allPacks.RemoveAll(p => p.Equals(activePackPath, StringComparison.OrdinalIgnoreCase));
                    }
                }

                // Fall back to searching all other packs
                foreach (var packDir in allPacks)
                {
                    var result = FindIconInPack(packDir, iconName, extensions);
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found '{iconName}' in fallback pack: {Path.GetFileName(packDir)}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Platform icon pack error for '{iconName}': {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Searches for an icon within a pack directory and its subdirectories.
        /// Icon packs often have subfolders like "Playlists", "Categories", etc.
        /// </summary>
        private static Image FindIconInPack(string packDir, string iconName, string[] extensions)
        {
            // First, check the pack root directory
            var result = FindIconInDirectory(packDir, iconName, extensions);
            if (result != null) return result;

            // Then check all subdirectories (Playlists, etc.)
            try
            {
                foreach (var subDir in Directory.GetDirectories(packDir))
                {
                    result = FindIconInDirectory(subDir, iconName, extensions);
                    if (result != null) return result;
                }
            }
            catch { /* Ignore directory access errors */ }

            return null;
        }

        /// <summary>
        /// Searches for an icon in a specific directory (no recursion)
        /// </summary>
        private static Image FindIconInDirectory(string directory, string iconName, string[] extensions)
        {
            // Try exact extension matches first
            foreach (var ext in extensions)
            {
                var filePath = Path.Combine(directory, iconName + ext);
                if (File.Exists(filePath))
                {
                    return ResizeImage(Image.FromFile(filePath), 16, 16);
                }
            }

            // Try case-insensitive wildcard search
            try
            {
                var files = Directory.GetFiles(directory, iconName + ".*");
                if (files.Length > 0)
                {
                    return ResizeImage(Image.FromFile(files[0]), 16, 16);
                }
            }
            catch { /* Ignore search errors */ }

            return null;
        }

        /// <summary>
        /// Loads the default QFT logo icon from embedded resource (16x16)
        /// </summary>
        private static Image GetDefaultIcon()
        {
            if (_defaultIcon != null)
                return _defaultIcon;

            try
            {
                // Load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("QuickFieldToggle.qft-logo-16x16.png"))
                {
                    if (stream != null)
                    {
                        _defaultIcon = Image.FromStream(stream);
                        return _defaultIcon;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading embedded icon: {ex.Message}");
            }

            // Fallback: generate a simple icon if embedded resource not found
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(Color.FromArgb(100, 149, 237)))
                {
                    g.FillRectangle(brush, 1, 1, 14, 14);
                }

                using (var pen = new Pen(Color.White, 1.5f))
                {
                    g.DrawEllipse(pen, 3, 3, 8, 8);
                    g.DrawLine(pen, 9, 9, 12, 12);
                }

                using (var brush = new SolidBrush(Color.LightGreen))
                {
                    g.FillEllipse(brush, 10, 2, 4, 4);
                }
            }

            _defaultIcon = bmp;
            return _defaultIcon;
        }

        /// <summary>
        /// Loads the delete icon from embedded resource
        /// </summary>
        private static Image GetDeleteIcon()
        {
            if (_deleteIcon != null)
                return _deleteIcon;

            try
            {
                // Load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("QuickFieldToggle.delete.png"))
                {
                    if (stream != null)
                    {
                        var img = Image.FromStream(stream);
                        // Resize if needed
                        if (img.Width != 16 || img.Height != 16)
                        {
                            _deleteIcon = ResizeImage(img, 16, 16);
                        }
                        else
                        {
                            _deleteIcon = img;
                        }
                        return _deleteIcon;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading embedded delete icon: {ex.Message}");
            }

            // Fallback to default icon if delete icon not found
            return GetDefaultIcon();
        }

        /// <summary>
        /// Gets a platform's icon from LaunchBox data (fallback if not in media pack)
        /// </summary>
        private static Image GetPlatformIconFromData(string platformName)
        {
            try
            {
                var platforms = PluginHelper.DataManager.GetAllPlatforms();
                var platform = platforms?.FirstOrDefault(p => 
                    p.Name.Equals(platformName, StringComparison.OrdinalIgnoreCase));

                if (platform != null)
                {
                    // Try clear logo first, then other images
                    string imagePath = null;
                    
                    // Use reflection to find image path properties
                    var type = platform.GetType();
                    var clearLogoPath = type.GetProperty("ClearLogoImagePath")?.GetValue(platform) as string;
                    var iconPath = type.GetProperty("IconImagePath")?.GetValue(platform) as string;
                    
                    imagePath = !string.IsNullOrEmpty(clearLogoPath) ? clearLogoPath : iconPath;

                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        return ResizeImage(Image.FromFile(imagePath), 16, 16);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Platform icon error for '{platformName}': {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets a playlist's icon, resized to 16x16
        /// </summary>
        private static Image GetPlaylistIcon(string playlistName)
        {
            try
            {
                var playlists = PluginHelper.DataManager.GetAllPlaylists();
                var playlist = playlists?.FirstOrDefault(p => 
                    p.Name.Equals(playlistName, StringComparison.OrdinalIgnoreCase));

                if (playlist != null)
                {
                    // Use reflection to find image path
                    var type = playlist.GetType();
                    var iconPath = type.GetProperty("IconImagePath")?.GetValue(playlist) as string;
                    
                    if (string.IsNullOrEmpty(iconPath))
                    {
                        iconPath = type.GetProperty("ImagePath")?.GetValue(playlist) as string;
                    }

                    if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    {
                        return ResizeImage(Image.FromFile(iconPath), 16, 16);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Playlist icon error for '{playlistName}': {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Loads an icon from a file path (relative to plugin folder or absolute)
        /// </summary>
        private static Image GetIconFromPath(string path)
        {
            try
            {
                string fullPath = path;

                // If relative path, resolve from plugin directory
                if (!Path.IsPathRooted(path))
                {
                    var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    fullPath = Path.Combine(pluginDir, path);
                }

                if (File.Exists(fullPath))
                {
                    var img = Image.FromFile(fullPath);
                    // Resize if not already 16x16
                    if (img.Width != 16 || img.Height != 16)
                    {
                        return ResizeImage(img, 16, 16);
                    }
                    return img;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Path icon error for '{path}': {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Resizes an image to fit within the specified dimensions while preserving aspect ratio
        /// </summary>
        private static Image ResizeImage(Image source, int maxWidth, int maxHeight)
        {
            // Calculate scale to fit within bounds while preserving aspect ratio
            double scaleX = (double)maxWidth / source.Width;
            double scaleY = (double)maxHeight / source.Height;
            double scale = Math.Min(scaleX, scaleY);

            // If image is already smaller than max, don't upscale
            if (scale >= 1.0)
                return new Bitmap(source);

            int newWidth = (int)(source.Width * scale);
            int newHeight = (int)(source.Height * scale);

            // Ensure minimum size of 1 pixel
            newWidth = Math.Max(1, newWidth);
            newHeight = Math.Max(1, newHeight);

            var destImage = new Bitmap(newWidth, newHeight);
            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(source, new Rectangle(0, 0, newWidth, newHeight), 
                        0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Clears the icon cache (called on config reload)
        /// </summary>
        /// <summary>
        /// Clears the icon cache and resets active pack detection.
        /// Called on config reload to pick up any changes to LaunchBox settings.
        /// </summary>
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                foreach (var icon in _iconCache.Values)
                {
                    icon?.Dispose();
                }
                _iconCache.Clear();
                _defaultIcon?.Dispose();
                _defaultIcon = null;
                _deleteIcon?.Dispose();
                _deleteIcon = null;
                
                // Reset active pack detection so it re-reads from settings on next icon load
                _activePlatformIconPack = null;
                _activePackLoaded = false;
            }
        }

        /// <summary>
        /// Returns diagnostic information about icon paths for debugging
        /// </summary>
        public static string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"LaunchBox Root: {LaunchBoxRoot}");
            info.AppendLine($"Platform Icons Root: {PlatformIconsRoot}");
            info.AppendLine($"Platform Icons Exists: {Directory.Exists(PlatformIconsRoot)}");
            info.AppendLine($"Active Pack: {ActivePlatformIconPack ?? "(none)"}");
            
            if (Directory.Exists(PlatformIconsRoot))
            {
                info.AppendLine("\nInstalled Packs:");
                foreach (var pack in Directory.GetDirectories(PlatformIconsRoot))
                {
                    var packName = Path.GetFileName(pack);
                    var subDirs = Directory.GetDirectories(pack);
                    info.AppendLine($"  - {packName} ({subDirs.Length} subdirs)");
                    foreach (var subDir in subDirs)
                    {
                        info.AppendLine($"      └── {Path.GetFileName(subDir)}");
                    }
                }
            }
            
            return info.ToString();
        }

        /// <summary>
        /// Tests loading a specific icon and returns debug info
        /// </summary>
        public static string TestIconLoad(string iconName)
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Testing icon: {iconName}");
            
            string[] extensions = { ".png", ".ico", ".bmp", ".jpg", ".gif" };
            
            if (!Directory.Exists(PlatformIconsRoot))
            {
                info.AppendLine($"ERROR: Platform Icons folder not found!");
                return info.ToString();
            }

            // Check active pack first
            var activePack = ActivePlatformIconPack;
            if (!string.IsNullOrEmpty(activePack))
            {
                var activePackPath = Path.Combine(PlatformIconsRoot, activePack);
                info.AppendLine($"\nChecking active pack: {activePack}");
                info.AppendLine($"  Path exists: {Directory.Exists(activePackPath)}");
                
                if (Directory.Exists(activePackPath))
                {
                    // Check root
                    foreach (var ext in extensions)
                    {
                        var testPath = Path.Combine(activePackPath, iconName + ext);
                        info.AppendLine($"  {testPath}: {File.Exists(testPath)}");
                    }
                    
                    // Check subdirs
                    foreach (var subDir in Directory.GetDirectories(activePackPath))
                    {
                        foreach (var ext in extensions)
                        {
                            var testPath = Path.Combine(subDir, iconName + ext);
                            if (File.Exists(testPath))
                            {
                                info.AppendLine($"  FOUND: {testPath}");
                            }
                        }
                    }
                }
            }

            // Check Default pack
            var defaultPackPath = Path.Combine(PlatformIconsRoot, "Default");
            info.AppendLine($"\nChecking Default pack:");
            info.AppendLine($"  Path exists: {Directory.Exists(defaultPackPath)}");
            
            if (Directory.Exists(defaultPackPath))
            {
                foreach (var subDir in Directory.GetDirectories(defaultPackPath))
                {
                    foreach (var ext in extensions)
                    {
                        var testPath = Path.Combine(subDir, iconName + ext);
                        if (File.Exists(testPath))
                        {
                            info.AppendLine($"  FOUND: {testPath}");
                        }
                    }
                }
            }
            
            return info.ToString();
        }
    }

    #endregion

    #region Condition Evaluator

    /// <summary>
    /// Evaluates conditions against games
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// Checks if ALL condition groups pass for ANY of the selected games
        /// </summary>
        public static bool ShouldShow(List<ConditionGroup> conditions, IGame[] games)
        {
            // No conditions = always show
            if (conditions == null || conditions.Count == 0)
                return true;

            // Need at least one game to pass all condition groups
            return games.Any(game => AllGroupsPass(conditions, game));
        }

        private static bool AllGroupsPass(List<ConditionGroup> groups, IGame game)
        {
            // All groups must pass (AND between groups)
            return groups.All(group => GroupPasses(group, game));
        }

        private static bool GroupPasses(ConditionGroup group, IGame game)
        {
            if (group.Rules == null || group.Rules.Count == 0)
                return true;

            if (group.Logic?.ToLower() == "or")
            {
                // Any rule can pass
                return group.Rules.Any(rule => RulePasses(rule, game));
            }
            else
            {
                // All rules must pass (default AND)
                return group.Rules.All(rule => RulePasses(rule, game));
            }
        }

        private static bool RulePasses(ConditionRule rule, IGame game)
        {
            string actualValue = FieldHelper.GetGameProperty(game, rule.Field);
            string expectedValue = rule.Value;

            switch (rule.Operator?.ToLower())
            {
                case "equals":
                    return string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
                
                case "notequals":
                    return !string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
                
                case "contains":
                    return actualValue?.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) >= 0;
                
                case "notcontains":
                    return actualValue == null || actualValue.IndexOf(expectedValue, StringComparison.OrdinalIgnoreCase) < 0;
                
                case "exists":
                    return !string.IsNullOrEmpty(actualValue);
                
                case "notexists":
                    return string.IsNullOrEmpty(actualValue);
                
                case "in":
                    // Value is comma-separated list
                    if (string.IsNullOrEmpty(expectedValue)) return false;
                    var allowedValues = expectedValue.Split(',').Select(v => v.Trim());
                    return allowedValues.Any(v => string.Equals(v, actualValue, StringComparison.OrdinalIgnoreCase));
                
                case "notin":
                    if (string.IsNullOrEmpty(expectedValue)) return true;
                    var disallowedValues = expectedValue.Split(',').Select(v => v.Trim());
                    return !disallowedValues.Any(v => string.Equals(v, actualValue, StringComparison.OrdinalIgnoreCase));
                
                default:
                    return true;
            }
        }
    }

    #endregion

    #region Menu Item Implementations

    /// <summary>
    /// A menu item that toggles a custom field
    /// Supports: checkmarks, toggledMenuLabel, operationType (toggle/set/remove)
    /// </summary>
    public class ToggleMenuItem : IGameMenuItem
    {
        private readonly ToggleConfig _config;
        private readonly IGame[] _selectedGames;
        private readonly string _inheritedIcon;
        private readonly GroupConfig _groupConfig;

        public ToggleMenuItem(ToggleConfig config, IGame[] selectedGames, string inheritedIcon = null, GroupConfig groupConfig = null)
        {
            _config = config;
            _selectedGames = selectedGames;
            _inheritedIcon = inheritedIcon;
            _groupConfig = groupConfig;
        }

        // Resolve selection indicator with inheritance: item -> group -> root
        private string GetSelectionIndicator()
        {
            var rootConfig = PluginConfiguration.Instance;
            return _config.SelectionIndicator 
                ?? _groupConfig?.SelectionIndicator 
                ?? rootConfig.SelectionIndicator;
        }

        public string Caption
        {
            get
            {
                int enabledCount = _selectedGames.Count(g => FieldHelper.HasFieldWithValue(g, _config.FieldName, _config.EnableValue));
                int totalCount = _selectedGames.Length;
                bool allEnabled = enabledCount == totalCount;
                bool noneEnabled = enabledCount == 0;

                // If we have a toggledMenuLabel and ALL selected games have the value, use that
                if (allEnabled && !string.IsNullOrEmpty(_config.ToggledMenuLabel))
                {
                    return _config.ToggledMenuLabel;
                }

                // Apply selection indicator based on inherited config
                string indicatorMode = GetSelectionIndicator();
                if (totalCount > 1 && indicatorMode == "count")
                {
                    // Count format: "Label (X of Y)"
                    return $"{_config.MenuLabel} ({enabledCount} of {totalCount})";
                }
                else
                {
                    // Symbol format (default): ✓ (all) / ◐ (some) / blank (none)
                    string indicator = "";
                    if (allEnabled)
                        indicator = "✓ ";
                    else if (!noneEnabled && totalCount > 1)
                        indicator = "◐ ";
                    
                    return $"{indicator}{_config.MenuLabel}";
                }
            }
        }

        // Use item's icon if set, otherwise use inherited icon from group
        public Image Icon
        {
            get
            {
                try
                {
                    return IconHelper.GetIcon(_config.Icon ?? _inheritedIcon);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children => null;

        public void OnSelect(params IGame[] selectedGames)
        {
            var gamesToProcess = selectedGames ?? _selectedGames;
            if (gamesToProcess == null || gamesToProcess.Length == 0) return;

            // Check for confirmation
            string actionDesc = $"modify '{_config.FieldName}'";
            if (!ConfirmationHelper.ShouldProceed(gamesToProcess.Length, _config.RequireConfirmation, actionDesc))
                return;

            string opType = _config.OperationType?.ToLower() ?? "toggle";
            
            // IMPORTANT: For toggle operations, compute this ONCE before the loop
            // Otherwise the check flips after the first game is modified, causing
            // inconsistent behavior on bulk operations
            bool shouldRemove = (opType == "toggle") && IsEnabledForGames(gamesToProcess);

            foreach (var game in gamesToProcess)
            {
                switch (opType)
                {
                    case "set":
                        // Always set the value (force/action mode)
                        FieldHelper.SetField(game, _config.FieldName, _config.EnableValue);
                        FieldHelper.ProcessAdditionalActions(game, _config.AdditionalActions);
                        break;

                    case "remove":
                        // Always remove the field
                        FieldHelper.RemoveField(game, _config.FieldName);
                        break;

                    default: // "toggle"
                        if (shouldRemove)
                        {
                            // All games had it at start - remove from all
                            FieldHelper.RemoveField(game, _config.FieldName);
                        }
                        else
                        {
                            // Not all had it at start - set on all (and process additional actions)
                            FieldHelper.SetField(game, _config.FieldName, _config.EnableValue);
                            FieldHelper.ProcessAdditionalActions(game, _config.AdditionalActions);
                        }
                        break;
                }
            }

            PluginHelper.DataManager.Save(false);
        }

        private bool IsEnabledForAllGames()
        {
            return IsEnabledForGames(_selectedGames);
        }

        private bool IsEnabledForGames(IGame[] games)
        {
            if (games == null || games.Length == 0) return false;
            return games.All(game => FieldHelper.HasFieldWithValue(game, _config.FieldName, _config.EnableValue));
        }
    }

    /// <summary>
    /// A menu item for multi-value selection (semicolon-separated values)
    /// Shows a submenu with checkable values
    /// </summary>
    public class MultiValueMenuItem : IGameMenuItem
    {
        private readonly ToggleConfig _config;
        private readonly IGame[] _selectedGames;
        private readonly string _inheritedIcon;
        private readonly GroupConfig _groupConfig;

        public MultiValueMenuItem(ToggleConfig config, IGame[] selectedGames, string inheritedIcon = null, GroupConfig groupConfig = null)
        {
            _config = config;
            _selectedGames = selectedGames;
            _inheritedIcon = inheritedIcon;
            _groupConfig = groupConfig;
        }

        public string Caption => _config.MenuLabel;

        // Use item's icon if set, otherwise use inherited icon from group
        public Image Icon
        {
            get
            {
                try
                {
                    return IconHelper.GetIcon(_config.Icon ?? _inheritedIcon);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool Enabled => true;

        // Resolve selection indicator with inheritance: item -> group -> root
        private string GetSelectionIndicator()
        {
            var rootConfig = PluginConfiguration.Instance;
            return _config.SelectionIndicator 
                ?? _groupConfig?.SelectionIndicator 
                ?? rootConfig.SelectionIndicator;
        }

        public IEnumerable<IGameMenuItem> Children
        {
            get
            {
                var values = GetAvailableValues();
                string indicator = GetSelectionIndicator();
                return values.Select(v => new MultiValueOptionMenuItem(
                    _config.FieldName, v, _selectedGames, _config.AdditionalActions, 
                    _config.RequireConfirmation, indicator));
            }
        }

        public void OnSelect(params IGame[] selectedGames)
        {
            // Parent doesn't do anything - children handle selection
        }

        private List<string> GetAvailableValues()
        {
            // Determine effective sort order: item > group > root default
            var rootConfig = PluginConfiguration.Instance;
            string sortOrder = _config.ValueSortOrder ?? _groupConfig?.ValueSortOrder ?? rootConfig.ValueSortOrder ?? "alphabetical";
            
            if (_config.ValueSource?.ToLower() == "field")
            {
                // Get values from the field across ALL games in the library (not just selected)
                // Track frequency for potential frequency-based sorting
                var valueCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                
                // Scan all games in the library for this field's values
                var allGames = PluginHelper.DataManager.GetAllGames();
                foreach (var game in allGames)
                {
                    var fieldValue = FieldHelper.GetFieldValue(game, _config.FieldName);
                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        foreach (var v in fieldValue.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)))
                        {
                            if (valueCounts.ContainsKey(v))
                                valueCounts[v]++;
                            else
                                valueCounts[v] = 1;
                        }
                    }
                }
                
                return SortValues(valueCounts.Keys.ToList(), valueCounts, sortOrder);
            }
            else
            {
                // Use predefined values from config - no frequency data available
                var values = _config.Values ?? new List<string>();
                return SortValues(values, null, sortOrder);
            }
        }
        
        private List<string> SortValues(List<string> values, Dictionary<string, int> frequencyCounts, string sortOrder)
        {
            switch (sortOrder?.ToLower())
            {
                case "mostcommon":
                    // Sort by frequency (most common first), then alphabetically for ties
                    if (frequencyCounts != null)
                    {
                        return values
                            .OrderByDescending(v => frequencyCounts.TryGetValue(v, out int count) ? count : 0)
                            .ThenBy(v => v)
                            .ToList();
                    }
                    // Fall through to alphabetical if no frequency data
                    goto case "alphabetical";
                    
                case "leastcommon":
                    // Sort by frequency (least common first), then alphabetically for ties
                    if (frequencyCounts != null)
                    {
                        return values
                            .OrderBy(v => frequencyCounts.TryGetValue(v, out int count) ? count : 0)
                            .ThenBy(v => v)
                            .ToList();
                    }
                    // Fall through to alphabetical if no frequency data
                    goto case "alphabetical";
                    
                case "reversealphabetical":
                    // Z-A sort
                    return values.OrderByDescending(v => v).ToList();
                    
                case "none":
                    // Return in original order
                    return values;
                    
                case "alphabetical":
                default:
                    // A-Z sort
                    return values.OrderBy(v => v).ToList();
            }
        }
    }

    /// <summary>
    /// A single option within a multi-value submenu
    /// </summary>
    public class MultiValueOptionMenuItem : IGameMenuItem
    {
        private readonly string _fieldName;
        private readonly string _value;
        private readonly IGame[] _selectedGames;
        private readonly List<AdditionalAction> _additionalActions;
        private readonly bool _requireConfirmation;
        private readonly string _selectionIndicator;

        public MultiValueOptionMenuItem(string fieldName, string value, IGame[] selectedGames, List<AdditionalAction> additionalActions, 
            bool requireConfirmation = false, string selectionIndicator = null)
        {
            _fieldName = fieldName;
            _value = value;
            _selectedGames = selectedGames;
            _additionalActions = additionalActions;
            _requireConfirmation = requireConfirmation;
            
            // Use passed value or fall back to root default
            var rootConfig = PluginConfiguration.Instance;
            _selectionIndicator = selectionIndicator ?? rootConfig.SelectionIndicator;
        }

        public string Caption
        {
            get
            {
                int haveCount = _selectedGames.Count(g => HasValue(g));
                int totalCount = _selectedGames.Length;
                bool allHaveValue = haveCount == totalCount;
                bool noneHaveValue = haveCount == 0;
                
                // Apply selection indicator based on config
                if (totalCount > 1 && _selectionIndicator == "count")
                {
                    // Count format: "Label (X of Y)"
                    return $"{_value} ({haveCount} of {totalCount})";
                }
                else
                {
                    // Symbol format (default): ✓ (all) / ◐ (some) / blank (none)
                    string indicator = "";
                    if (allHaveValue)
                        indicator = "✓ ";
                    else if (!noneHaveValue && totalCount > 1)
                        indicator = "◐ ";
                    
                    return $"{indicator}{_value}";
                }
            }
        }

        public Image Icon => null;

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children => null;

        public void OnSelect(params IGame[] selectedGames)
        {
            var gamesToProcess = selectedGames ?? _selectedGames;
            if (gamesToProcess == null || gamesToProcess.Length == 0) return;

            // Check for confirmation
            string actionDesc = $"modify '{_fieldName}'";
            if (!ConfirmationHelper.ShouldProceed(gamesToProcess.Length, _requireConfirmation, actionDesc))
                return;

            bool allHaveValue = gamesToProcess.All(g => HasValue(g));

            foreach (var game in gamesToProcess)
            {
                if (allHaveValue)
                {
                    // Remove this value from all
                    RemoveValueFromField(game);
                }
                else
                {
                    // Add this value to all
                    AddValueToField(game);
                    FieldHelper.ProcessAdditionalActions(game, _additionalActions);
                }
            }

            PluginHelper.DataManager.Save(false);
        }

        private bool HasValue(IGame game)
        {
            var fieldValue = FieldHelper.GetFieldValue(game, _fieldName);
            if (string.IsNullOrEmpty(fieldValue)) return false;
            
            var values = fieldValue.Split(';').Select(s => s.Trim());
            return values.Any(v => v.Equals(_value, StringComparison.OrdinalIgnoreCase));
        }

        private void AddValueToField(IGame game)
        {
            var fieldValue = FieldHelper.GetFieldValue(game, _fieldName);
            
            if (string.IsNullOrEmpty(fieldValue))
            {
                FieldHelper.SetField(game, _fieldName, _value);
            }
            else
            {
                // Check if already present
                var values = fieldValue.Split(';').Select(s => s.Trim()).ToList();
                if (!values.Any(v => v.Equals(_value, StringComparison.OrdinalIgnoreCase)))
                {
                    values.Add(_value);
                    FieldHelper.SetField(game, _fieldName, string.Join("; ", values));
                }
            }
        }

        private void RemoveValueFromField(IGame game)
        {
            var fieldValue = FieldHelper.GetFieldValue(game, _fieldName);
            if (string.IsNullOrEmpty(fieldValue)) return;

            var values = fieldValue.Split(';')
                .Select(s => s.Trim())
                .Where(v => !v.Equals(_value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (values.Count == 0)
            {
                FieldHelper.RemoveField(game, _fieldName);
            }
            else
            {
                FieldHelper.SetField(game, _fieldName, string.Join("; ", values));
            }
        }
    }

    /// <summary>
    /// A menu item that serves as a group/submenu container
    /// </summary>
    public class GroupMenuItem : IGameMenuItem
    {
        private readonly GroupConfig _groupConfig;
        private readonly IGame[] _selectedGames;

        public GroupMenuItem(GroupConfig groupConfig, IGame[] selectedGames)
        {
            _groupConfig = groupConfig;
            _selectedGames = selectedGames;
        }

        public string Caption => _groupConfig.GroupName;

        public Image Icon
        {
            get
            {
                try
                {
                    return IconHelper.GetIcon(_groupConfig.Icon);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children
        {
            get
            {
                try
                {
                    // Determine inherited icon based on iconCascade setting
                    string inheritedIcon = null;
                    if (_groupConfig.IconCascade?.ToLower() == "inherit")
                    {
                        inheritedIcon = _groupConfig.Icon;
                    }

                    var items = new List<IGameMenuItem>();
                    
                    foreach (var item in _groupConfig.Items)
                    {
                        try
                        {
                            if (!item.Enabled)
                                continue;
                                
                            if (!ConditionEvaluator.ShouldShow(item.Conditions, _selectedGames))
                                continue;
                                
                            items.Add(CreateMenuItem(item, inheritedIcon));
                        }
                        catch
                        {
                            // Skip this item silently
                        }
                    }
                    
                    return items;
                }
                catch
                {
                    // Return empty list if anything fails
                    return new List<IGameMenuItem>();
                }
            }
        }

        private IGameMenuItem CreateMenuItem(ToggleConfig item, string inheritedIcon)
        {
            if (item.Mode?.ToLower() == "multivalue")
            {
                return new MultiValueMenuItem(item, _selectedGames, inheritedIcon, _groupConfig);
            }
            return new ToggleMenuItem(item, _selectedGames, inheritedIcon, _groupConfig);
        }

        public void OnSelect(params IGame[] selectedGames)
        {
            // Groups don't do anything when clicked
        }
    }

    /// <summary>
    /// Special menu item that clears ALL custom fields from selected games
    /// </summary>
    public class ClearAllFieldsMenuItem : IGameMenuItem
    {
        private readonly IGame[] _selectedGames;
        private readonly string _label;
        private readonly string _icon;

        public ClearAllFieldsMenuItem(IGame[] selectedGames, string label, string icon = "default")
        {
            _selectedGames = selectedGames;
            _label = label;
            _icon = icon;
        }

        public string Caption => _label;

        public Image Icon => IconHelper.GetIcon(_icon);

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children => null;

        public void OnSelect(params IGame[] selectedGames)
        {
            var gamesToProcess = selectedGames ?? _selectedGames;
            if (gamesToProcess == null || gamesToProcess.Length == 0) return;

            // This action ALWAYS requires confirmation
            string actionDesc = "clear ALL custom fields";
            if (!ConfirmationHelper.ShouldProceed(gamesToProcess.Length, true, actionDesc))
                return;

            int totalFieldsCleared = 0;
            foreach (var game in gamesToProcess)
            {
                var customFields = game.GetAllCustomFields()?.ToList();
                if (customFields != null)
                {
                    foreach (var field in customFields)
                    {
                        game.TryRemoveCustomField(field);
                        totalFieldsCleared++;
                    }
                }
            }

            PluginHelper.DataManager.Save(false);

            System.Windows.Forms.MessageBox.Show(
                $"Cleared {totalFieldsCleared} custom fields from {gamesToProcess.Length} game(s).",
                "Clear All Fields Complete",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }
    }

    /// <summary>
    /// Menu item that shows a submenu of all custom fields on selected games,
    /// allowing selective clearing of individual fields
    /// </summary>
    public class SelectiveClearMenuItem : IGameMenuItem
    {
        private readonly IGame[] _selectedGames;
        private readonly string _label;
        private readonly string _icon;

        public SelectiveClearMenuItem(IGame[] selectedGames, string label, string icon = "default")
        {
            _selectedGames = selectedGames;
            _label = label;
            _icon = icon;
        }

        public string Caption => _label;

        public Image Icon => IconHelper.GetIcon(_icon);

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children
        {
            get
            {
                // Collect all unique field names across selected games, with counts
                var fieldCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                
                foreach (var game in _selectedGames)
                {
                    var customFields = game.GetAllCustomFields();
                    if (customFields != null)
                    {
                        foreach (var field in customFields)
                        {
                            if (!string.IsNullOrEmpty(field.Name))
                            {
                                if (fieldCounts.ContainsKey(field.Name))
                                    fieldCounts[field.Name]++;
                                else
                                    fieldCounts[field.Name] = 1;
                            }
                        }
                    }
                }

                // Sort alphabetically and create menu items
                return fieldCounts
                    .OrderBy(kv => kv.Key)
                    .Select(kv => new SelectiveClearFieldMenuItem(
                        kv.Key, 
                        kv.Value, 
                        _selectedGames.Length, 
                        _selectedGames));
            }
        }

        public void OnSelect(params IGame[] selectedGames)
        {
            // Parent doesn't do anything - children handle selection
        }
    }

    /// <summary>
    /// A single field item within the selective clear submenu
    /// </summary>
    public class SelectiveClearFieldMenuItem : IGameMenuItem
    {
        private readonly string _fieldName;
        private readonly int _countWithField;
        private readonly int _totalSelected;
        private readonly IGame[] _selectedGames;

        public SelectiveClearFieldMenuItem(string fieldName, int countWithField, int totalSelected, IGame[] selectedGames)
        {
            _fieldName = fieldName;
            _countWithField = countWithField;
            _totalSelected = totalSelected;
            _selectedGames = selectedGames;
        }

        public string Caption
        {
            get
            {
                var config = PluginConfiguration.Instance;
                bool allHaveField = _countWithField == _totalSelected;
                bool noneHaveField = _countWithField == 0;
                
                // Use selectiveClearIndicator if set, otherwise fall back to root selectionIndicator
                string indicatorMode = config.SelectiveClearIndicator ?? config.SelectionIndicator;
                
                // Apply selection indicator based on config
                if (_totalSelected > 1 && indicatorMode == "count")
                {
                    // Count format: "FieldName (X of Y)"
                    return $"{_fieldName} ({_countWithField} of {_totalSelected})";
                }
                else
                {
                    // Symbol format (default): ✓ (all) / ◐ (some) / blank (none)
                    string indicator = "";
                    if (allHaveField)
                        indicator = "✓ ";
                    else if (!noneHaveField && _totalSelected > 1)
                        indicator = "◐ ";
                    
                    return $"{indicator}{_fieldName}";
                }
            }
        }

        public Image Icon => null;

        public bool Enabled => true;

        public IEnumerable<IGameMenuItem> Children => null;

        public void OnSelect(params IGame[] selectedGames)
        {
            var gamesToProcess = selectedGames ?? _selectedGames;
            if (gamesToProcess == null || gamesToProcess.Length == 0) return;

            // Check for confirmation using standard bulk threshold
            string actionDesc = $"clear '{_fieldName}'";
            if (!ConfirmationHelper.ShouldProceed(gamesToProcess.Length, false, actionDesc))
                return;

            int clearedCount = 0;
            foreach (var game in gamesToProcess)
            {
                var customFields = game.GetAllCustomFields();
                var fieldToRemove = customFields?.FirstOrDefault(cf => 
                    cf.Name.Equals(_fieldName, StringComparison.OrdinalIgnoreCase));
                
                if (fieldToRemove != null)
                {
                    game.TryRemoveCustomField(fieldToRemove);
                    clearedCount++;
                }
            }

            PluginHelper.DataManager.Save(false);
        }
    }

    #endregion

    #region Main Game Menu Plugin

    /// <summary>
    /// Main plugin that provides the right-click menu items
    /// </summary>
    public class QuickFieldTogglePlugin : IGameMultiMenuItemPlugin
    {
        public IEnumerable<IGameMenuItem> GetMenuItems(params IGame[] selectedGames)
        {
            var config = PluginConfiguration.Instance;
            var menuItems = new List<IGameMenuItem>();

            // Add groups as submenus (with condition checking)
            foreach (var group in config.Groups.Where(g => g.Enabled))
            {
                if (ConditionEvaluator.ShouldShow(group.Conditions, selectedGames))
                {
                    menuItems.Add(new GroupMenuItem(group, selectedGames));
                }
            }

            // Add ungrouped items directly (with condition checking)
            foreach (var item in config.UngroupedItems.Where(i => i.Enabled))
            {
                if (ConditionEvaluator.ShouldShow(item.Conditions, selectedGames))
                {
                    if (item.Mode?.ToLower() == "multivalue")
                    {
                        menuItems.Add(new MultiValueMenuItem(item, selectedGames));
                    }
                    else
                    {
                        menuItems.Add(new ToggleMenuItem(item, selectedGames));
                    }
                }
            }

            // Add "Selectively Clear Fields" menu item if enabled
            if (config.InjectSelectiveClear)
            {
                menuItems.Add(new SelectiveClearMenuItem(selectedGames, config.SelectiveClearLabel, config.SelectiveClearIcon));
            }

            // Add "Clear All Custom Fields" menu item if enabled (last, as it's most destructive)
            if (config.InjectClearAllFields)
            {
                menuItems.Add(new ClearAllFieldsMenuItem(selectedGames, config.ClearAllFieldsLabel, config.ClearAllFieldsIcon));
            }

            return menuItems;
        }
    }

    #endregion

    #region System Menu Plugin (Tools Menu)

    /// <summary>
    /// Adds a "Reload Quick Field Toggle Config" option to the Tools menu
    /// </summary>
    public class ReloadConfigMenuPlugin : ISystemMenuItemPlugin
    {
        public string Caption => "Reload Quick Field Toggle Config";

        public Image IconImage => IconHelper.GetIcon("default");

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            try
            {
                // Clear icon cache first (in case icons were changed)
                IconHelper.ClearCache();
                
                // Reload configuration
                PluginConfiguration.Reload();
                var config = PluginConfiguration.Instance;
                
                System.Windows.Forms.MessageBox.Show(
                    $"Configuration reloaded successfully!\n\n" +
                    $"Loaded {config.Groups.Count} groups and {config.UngroupedItems.Count} ungrouped items.",
                    "Quick Field Toggle",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error reloading configuration:\n\n{ex.Message}",
                    "Quick Field Toggle - Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    #endregion
}
