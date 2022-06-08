// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DATABASE.DTOModels.UI;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace COMMON.Components
{
    public class GroupedDocumentDTO
    {
        public DocumentDTO Dto { get; set; }
        public int Count { get; set; }
    }

    [ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))]
    public class DataSourceLoadOptions : DataSourceLoadOptionsBase
    {
    }
    public class DataSourceLoadOptionsBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var loadOptions = new DataSourceLoadOptions();
            DataSourceLoadOptionsParser.Parse(loadOptions, key => bindingContext.ValueProvider.GetValue(key).FirstOrDefault());
            bindingContext.Result = ModelBindingResult.Success(loadOptions);
            return Task.CompletedTask;
        }
    }
    public static class LoadOptions
    {
        private static string GetVal(string val)
        {
            if (val == "contractorName")
                return "contractor.name";
            else if (val == "projectName")
                return "project.name";
            else if (val == "organizationName")
                return "organization.name";
            else if (val == "docType")
                return "docType.name";
            else if (val == "docKind")
                return "docKind.name";
            else if (val == "contractName")
                return "contract.name";
            else if (val.EndsWith("R"))
                return val.Substring(0, val.Length - 1);
            return val;
        }
        public static bool CheckWrongFilterFields(DataSourceLoadOptions loadOptions)
        {
            bool res = false;
            try
            {
                var sortFilterExist = loadOptions.Sort == null ? false : loadOptions.Sort.Any(x => x.Selector == "contract.name" || x.Selector == "signed");
                var groupFilterExist = loadOptions.Group == null ? false : loadOptions.Group.Any(x => x.Selector == "contract.name" || x.Selector == "signed");
                var filterFieldsExist = false;
                if (loadOptions.Filter != null)
                {
                    foreach (var filter in loadOptions.Filter)
                    {
                        if (filter.GetType().Name == "JArray")
                        {
                            var first = ((JContainer)filter).First;
                            var last = ((JContainer)filter).Last;
                            if (first.HasValues)
                            {
                                var lastFirst = ((JContainer)last).First;
                                var lastLast = ((JContainer)last).Last;
                                if (lastFirst.HasValues)
                                {
                                    var valLastFirst = ((JValue)((JContainer)lastFirst).First).Value;
                                    var valLastLast = ((JValue)((JContainer)lastLast).First).Value;
                                    filterFieldsExist = valLastFirst.ToString().Contains("signed") || valLastFirst.ToString().Contains("contract.name") ||
                                        valLastLast.ToString().Contains("signed") || valLastLast.ToString().Contains("contract.name");
                                    if (filterFieldsExist)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    var valLast = ((JValue)lastFirst).Value;
                                    filterFieldsExist = valLast.ToString().Contains("signed") || valLast.ToString().Contains("contract.name");
                                    if (filterFieldsExist)
                                    {
                                        break;
                                    }
                                }

                                var firstFirst = ((JContainer)first).First;
                                var firstLast = ((JContainer)first).Last;
                                if (firstFirst.HasValues)
                                {
                                    var valLast = ((JValue)((JContainer)firstFirst).First).Value;
                                    var valLastLast = ((JValue)((JContainer)firstLast).First).Value;
                                    filterFieldsExist = valLast.ToString().Contains("signed") || valLast.ToString().Contains("contract.name") ||
                                        valLastLast.ToString().Contains("signed") || valLastLast.ToString().Contains("contract.name");
                                    if (filterFieldsExist)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    var valFirst = ((JValue)firstFirst).Value;
                                    filterFieldsExist = valFirst.ToString().Contains("signed") || valFirst.ToString().Contains("contract.name");
                                    if (filterFieldsExist)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var val = ((JValue)first).Value;
                                filterFieldsExist = val.ToString().Contains("signed") || val.ToString().Contains("contract.name");
                                if (filterFieldsExist)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            filterFieldsExist = loadOptions.Filter.Contains("signed") || loadOptions.Filter.Contains("contract.name");
                            if (filterFieldsExist)
                            {
                                break;
                            }
                        }
                    }
                }
                return groupFilterExist || filterFieldsExist || sortFilterExist;
            }
            catch
            {
                return res;
            }
        }

        public static void CheckFilter(DataSourceLoadOptions loadOptions)
        {
            if (loadOptions.Filter != null)
            {
                if (loadOptions.Filter.Count > 0)
                {
                    if (loadOptions.Filter[0].GetType().Name == "JArray")
                    {
                        foreach (var filter in loadOptions.Filter)
                        {
                            if (filter.GetType().Name == "JArray")
                            {
                                var first = ((JContainer)filter).First;
                                var last = ((JContainer)filter).Last;
                                if (first.HasValues)
                                {
                                    var lastFirst = ((JContainer)last).First;
                                    var lastLast = ((JContainer)last).Last;
                                    if (lastFirst.HasValues)
                                    {
                                        var valLastFirst = ((JValue)((JContainer)lastFirst).First).Value;
                                        valLastFirst = GetVal(valLastFirst.ToString());
                                        ((JValue)((JContainer)lastFirst).First).Value = valLastFirst;

                                        var valLastLast = ((JValue)((JContainer)lastLast).First).Value;
                                        valLastLast = GetVal(valLastLast.ToString());
                                        ((JValue)((JContainer)lastLast).First).Value = valLastLast;
                                    }
                                    else
                                    {
                                        var valLast = ((JValue)lastFirst).Value;
                                        valLast = GetVal(valLast.ToString());
                                        ((JValue)lastFirst).Value = valLast;
                                    }

                                    var firstFirst = ((JContainer)first).First;
                                    var firstLast = ((JContainer)first).Last;
                                    if (firstFirst.HasValues)
                                    {
                                        var valLast = ((JValue)((JContainer)firstFirst).First).Value;
                                        valLast = GetVal(valLast.ToString());
                                        ((JValue)((JContainer)firstFirst).First).Value = valLast;

                                        var valLastLast = ((JValue)((JContainer)firstLast).First).Value;
                                        valLastLast = GetVal(valLastLast.ToString());
                                        ((JValue)((JContainer)firstLast).First).Value = valLastLast;
                                    }
                                    else
                                    {
                                        var valFirst = ((JValue)firstFirst).Value;
                                        valFirst = GetVal(valFirst.ToString());
                                        ((JValue)firstFirst).Value = valFirst;
                                    }
                                }
                                else
                                {
                                    var val = ((JValue)first).Value;
                                    val = GetVal(val.ToString());
                                    ((JValue)first).Value = val;
                                }
                            }
                        }
                    }
                    else
                    {
                        var val = loadOptions.Filter[0].ToString();
                        val = GetVal(val);
                        loadOptions.Filter[0] = val;
                    }
                }
            }

            if (loadOptions.Group != null && loadOptions.Group.Count() > 0)
            {
                List<GroupingInfo> groups = new List<GroupingInfo>();
                foreach (GroupingInfo group in loadOptions.Group)
                {
                    var val = group.Selector;
                    val = GetVal(val);
                    group.Selector = val;
                    groups.Add(group);
                }

                loadOptions.Group = groups.ToArray();
            }

            if (loadOptions.Sort != null && loadOptions.Sort.Count() > 0)
            {
                List<SortingInfo> sortArr = new List<SortingInfo>();
                foreach (SortingInfo sort in loadOptions.Sort)
                {
                    var val = sort.Selector;
                    val = GetVal(val);
                    sort.Selector = val;
                    sortArr.Add(sort);
                }
            }
        }
    }
}
