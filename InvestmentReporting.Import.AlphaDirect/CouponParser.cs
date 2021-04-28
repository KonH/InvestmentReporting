using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Import.AlphaDirect {
	public sealed class CouponParser {
		// To receive organization name and series from coupon comment
		readonly Regex _couponCommonRegex = new("\\(Облигации (.*) сери.*(\\w{4}-\\w{2})\\)");
		readonly Regex _couponOfzRegex    = new("\\((.*) выпуск (\\w{5})\\)");

		// To receive organization name and series from asset name
		readonly Regex _bondCommonRegex = new("(.*) сери.*(\\w{4}-\\w{2})");
		readonly Regex _bondOfzRegex    = new("ОФЗ ПД (\\w{5}) в.");

		readonly ILogger _logger;

		public CouponParser(ILogger<CouponParser> logger) {
			_logger = logger;
		}

		public AssetId DetectAssetFromTransfer(string comment, IReadOnlyCollection<Trade> trades, Dictionary<string, AssetId> assets) {
			var holder = DetectCouponHolder(comment);
			foreach ( var trade in trades ) {
				_logger.LogTrace($"Process trade: {trade}");
				var isin = TryGetIsinForHolder(holder, trade);
				if ( isin == null ) {
					_logger.LogTrace("Isin not found");
					continue;
				}
				if ( assets.TryGetValue(isin, out var assetId) ) {
					_logger.LogTrace($"Asset Id {assetId} found for ISIN {isin}");
					return assetId;
				}
				throw new InvalidOperationException($"Failed to find asset for ISIN '{isin}'");
			}
			throw new InvalidOperationException($"Failed to find asset for {holder}");
		}

		enum CouponType {
			Common,
			Ofz
		}

		record CouponHolder(CouponType Type, string Organization, string? ShortOrganization, string Series);

		CouponHolder DetectCouponHolder(string comment) {
			var couponMatch = _couponCommonRegex.Match(comment);
			if ( couponMatch.Success ) {
				return CreateCouponHolder(CouponType.Common, couponMatch);
			}
			couponMatch = _couponOfzRegex.Match(comment);
			if ( couponMatch.Success ) {
				return CreateCouponHolder(CouponType.Ofz, couponMatch);
			}
			throw new UnexpectedFormatException($"Failed to detect organization and/or series from comment '{comment}'");
		}

		string? TryGetIsinForHolder(CouponHolder holder, Trade trade) {
			_logger.LogTrace($"Receive ISIN from trade {trade} with holder {holder}");
			switch ( holder.Type ) {
				case CouponType.Common: {
					var tradeCommonMatch = _bondCommonRegex.Match(trade.Name);
					if ( !tradeCommonMatch.Success ) {
						_logger.LogTrace($"Trade is not matched by {nameof(_bondCommonRegex)}");
						return null;
					}
					var tradeSeries = tradeCommonMatch.Groups[2].Value;
					if ( holder.Series != tradeSeries ) {
						_logger.LogTrace($"Trade series doesn't match ({holder.Series}, {tradeSeries})");
						return null;
					}
					var tradeOrganization      = tradeCommonMatch.Groups[1].Value;
					var tradeShortOrganization = TryGetShortOrganizationName(tradeOrganization);
					if ( !tradeOrganization.StartsWith(holder.Organization) && (holder.ShortOrganization != tradeShortOrganization) ) {
						_logger.LogTrace($"Trade organizations doesn't match ({tradeOrganization}, {holder.Organization})");
						return null;
					}
					return trade.Isin;
				}

				case CouponType.Ofz: {
					var tradeOfzMatch = _bondOfzRegex.Match(trade.Name);
					if ( !tradeOfzMatch.Success ) {
						_logger.LogTrace($"Trade is not matched by {nameof(_bondOfzRegex)}");
						return null;
					}
					var tradeSeries = tradeOfzMatch.Groups[1].Value;
					if ( holder.Series != tradeSeries ) {
						_logger.LogTrace($"Trade series doesn't match ({holder.Series}, {tradeSeries})");
						return null;
					}
					return trade.Isin;
				}
			}
			return null;
		}

		CouponHolder CreateCouponHolder(CouponType type, Match match) {
			_logger.LogTrace($"Create coupon holder for {type} and {match}");
			var organization      = match.Groups[1].Value.Trim();
			var shortOrganization = TryGetShortOrganizationName(organization);
			var series            = match.Groups[2].Value;
			var holder = new CouponHolder(type, organization, shortOrganization, series);
			_logger.LogTrace($"Create holder: {holder}");
			return holder;
		}

		string? TryGetShortOrganizationName(string organization) {
			var parts = organization.Split('"');
			return (parts.Length > 1) ? parts[^2] : null;
		}
	}
}