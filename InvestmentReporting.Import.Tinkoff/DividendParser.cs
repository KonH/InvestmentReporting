using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;
using Asset = InvestmentReporting.Import.Dto.Asset;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class DividendParser {
		readonly ILogger _logger;

		readonly Regex _targetNameRegex = new("(Выплата дивидендов) (.*)\\/");

		public DividendParser(ILogger<CouponParser> logger) {
			_logger = logger;
		}

		public AssetId DetectAssetFromTransfer(string comment, IReadOnlyCollection<Asset> assets, IReadOnlyDictionary<string, AssetId> assetsIds) {
			var targetName = _targetNameRegex.Match(comment).Groups[2].Value;
			foreach ( var asset in assets ) {
				_logger.LogTrace($"Process asset: {asset}");
				if ( asset.Name != targetName ) {
					continue;
				}
				var isin = asset.Isin;
				if ( assetsIds.TryGetValue(isin, out var assetId) ) {
					_logger.LogTrace($"Asset Id {assetId} found for ISIN '{isin}'");
					return assetId;
				}
				throw new InvalidOperationException($"Failed to find asset for ISIN '{isin}'");
			}
			throw new InvalidOperationException($"Failed to find asset for '{targetName}'");
		}
	}
}