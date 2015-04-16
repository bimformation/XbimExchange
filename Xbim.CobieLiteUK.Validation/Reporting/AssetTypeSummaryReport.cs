﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.COBieLiteUK;
using Xbim.CobieLiteUK.Validation.Extensions;

namespace Xbim.CobieLiteUK.Validation.Reporting
{
    public class SummaryReport<T> where T : CobieObject
    {
        private readonly IEnumerable<T> _validatedAssetTypes;

        public SummaryReport(IEnumerable<T> validatedAssetTypes)
        {
            _validatedAssetTypes = validatedAssetTypes;
        }

        public DataTable GetReport(string mainClassification)
        {
            if (_validatedAssetTypes == null || _validatedAssetTypes.FirstOrDefault() == null)
                return null;
            if (mainClassification == @"")
            {
                var firstRequirement = _validatedAssetTypes.FirstOrDefault();
                if (firstRequirement == null)
                    return null;

                var firstClassification = firstRequirement.Categories.FirstOrDefault();
                if (firstClassification == null)
                    return null;
                mainClassification = firstClassification.Classification;
            }

            var retTable = PrepareTable(mainClassification);

            // the progressive variable allows grouping by Maincategory and MatchingCategory values
            //
            var progressive = new Dictionary<Tuple<string, string>, ValidationSummary>();
            foreach (var reportingAsset in _validatedAssetTypes)
            {
                var mainCatCode = "";
                var mainCat =
                    reportingAsset.GetRequirementCategories().FirstOrDefault(c => c.Classification == mainClassification);
                if (mainCat != null)
                {
                    mainCatCode = mainCat.Code;
                }

                var thisQuantities = new ValidationSummary()
                {
                    Passes = reportingAsset.GetValidAssetsCount(),
                    Total = reportingAsset.GetSubmittedAssetsCount(),
                    MainCatDescription = mainCat != null 
                        ? mainCat.Description 
                        : ""
                };
                    
                var matchingCat = reportingAsset.GetMatchingCategories().FirstOrDefault();
                // var sClass = (matchingCat != null) ? matchingCat.Classification : "";
                var matchCatValue = (matchingCat != null) ? matchingCat.Code : "";

                var thiItem = new Tuple<string, string>(mainCatCode, matchCatValue);

                if (!progressive.ContainsKey(thiItem))
                {
                    progressive.Add(thiItem, thisQuantities);
                }
                else
                {
                    progressive[thiItem].Add(thisQuantities);
                }
            }

            var sortedKeys = progressive.Keys.ToList();
            sortedKeys.Sort(Comparison);

            // now populates the datatable.
            foreach (var sortedKey in sortedKeys)
            {
                var value = progressive[sortedKey];
                int i = 1;
                var thisRow = retTable.NewRow();

                thisRow[i++] = sortedKey.Item1; // main classification
                thisRow[i++] = value.MainCatDescription; // main classification
                thisRow[i++] = sortedKey.Item2;
                thisRow[i++] = value.Total;

                var aStyle = VisualAttentionStyle.Green;
                if (value.Passes < value.Total)
                    aStyle = VisualAttentionStyle.Red;
                if (value.Total == 0)
                    aStyle = VisualAttentionStyle.Amber;
                thisRow[i++] = new VisualValue(value.Passes) { AttentionStyle = aStyle};
                retTable.Rows.Add(thisRow);
            }
            return retTable;
        }

        private int Comparison(Tuple<string, string> tuple, Tuple<string, string> tuple1)
        {
            return tuple.Item1 == tuple1.Item1 
                ? System.String.Compare(tuple.Item2, tuple1.Item2, System.StringComparison.Ordinal) 
                : System.String.Compare(tuple.Item1, tuple1.Item1, System.StringComparison.Ordinal);
        }

        private class ValidationSummary
        {
            public int Total;
            public int Passes;
            public string MainCatDescription;

            public void Add(ValidationSummary addendum)
            {
                Total += addendum.Total;
                Passes += addendum.Passes;
            }
        }


        private static DataTable PrepareTable(string mainClassification)
        {
            var repName = typeof (T).Name + "s validation report";
            var retTable = new DataTable(repName, "DPoW");
            var workCol = retTable.Columns.Add("DPoW_ID", typeof(Int32));
            workCol.AllowDBNull = false;
            workCol.Unique = true;
            workCol.AutoIncrement = true;

            retTable.Columns.Add(new DataColumn("DPoW_mainClassification", typeof(String)) { Caption = mainClassification + " code" });
            retTable.Columns.Add(new DataColumn("DPoW_mainClassificationDescription", typeof(String)) { Caption = mainClassification + " title" });
            // retTable.Columns.Add("Matching classification", typeof (String));
            retTable.Columns.Add(new DataColumn("DPoW_MatchingCode", typeof(String)) { Caption = "Matching code" });
            retTable.Columns.Add(new DataColumn("DPoW_Submitted", typeof(int)) { Caption = "No. Submitted" });
            retTable.Columns.Add(new DataColumn("DPoW_Valid", typeof(VisualValue)) { Caption = "No. Valid" });
            return retTable;
        }
    }
}
