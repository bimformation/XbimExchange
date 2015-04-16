﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.COBieLiteUK;
using Xbim.CobieLiteUK.Validation.Extensions;

namespace Xbim.CobieLiteUK.Validation.Reporting
{
    public class AssetTypeRequirementPointer<T, TSub> 
        where T : CobieObject
        where TSub : CobieObject
    {
        public readonly string ExternalId;
        public readonly string ExternalSystem;

        public AssetTypeRequirementPointer(string externalSystem, string externalId, string name)
        {
            ExternalId = externalId;
            ExternalSystem = externalSystem;
            Name = name;
        }

        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to AssetTypeRequirementPointer return false.
            var p = obj as AssetTypeRequirementPointer<T, TSub>;
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (ExternalId == p.ExternalId) && (ExternalSystem == p.ExternalSystem);
        }

        public override int GetHashCode()
        {
            return (ExternalId + ExternalSystem).GetHashCode();
        }

        public List<CobieObject> ProvidedAssetTypes = new List<CobieObject>();

        public void AddSumission(CobieObject providedAsset)
        {
            ProvidedAssetTypes.Add(providedAsset);
        }

        internal int GetSubmittedAssetsCount()
        {
            return ProvidedAssetTypes.Sum(providedAsset => providedAsset.GetSubmittedAssetsCount());
        }

        public string Name { get; set; }

        public IEnumerable<TSub> Assets()
        {
            return ProvidedAssetTypes.SelectMany(providedAssetType => providedAssetType.GetChildObjects<TSub>());
        }

        public IEnumerable<Category> MatchingCategories
        {
            get
            {
                var first = ProvidedAssetTypes.FirstOrDefault();
                return first != null
                    ? first.GetMatchingCategories()
                    : Enumerable.Empty<Category>();
            }
        }


        public IEnumerable<Category> RequirementCategories
        {
            get
            {
                var first = ProvidedAssetTypes.FirstOrDefault();
                return first != null
                    ? first.GetRequirementCategories()
                    : Enumerable.Empty<Category>();
            }
        }
    }
}
