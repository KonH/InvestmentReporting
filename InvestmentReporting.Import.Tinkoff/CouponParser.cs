using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class CouponParser {
		readonly ILogger _logger;

		readonly Regex _targetNameRegex = new("(Выплата купонов|Погашение облигации) (.*)\\/");

		public CouponParser(ILogger<CouponParser> logger) {
			_logger = logger;
		}

		public AssetId DetectAssetFromTransfer(string comment, IReadOnlyCollection<Trade> trades, IReadOnlyDictionary<string, AssetId> assets) {
			var targetName = _targetNameRegex.Match(comment).Groups[2].Value;
			foreach ( var trade in trades ) {
				_logger.LogTrace($"Process trade: {trade}");
				if ( trade.Name != targetName ) {
					continue;
				}
				var isin = trade.Isin;
				if ( assets.TryGetValue(isin, out var assetId) ) {
					_logger.LogTrace($"Asset Id {assetId} found for ISIN {isin}");
					return assetId;
				}
				throw new InvalidOperationException($"Failed to find asset for ISIN '{isin}'");
			}
			throw new InvalidOperationException($"Failed to find asset for {targetName}");
		}
	}
}