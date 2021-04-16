export default class OperationData {
	/** @format date-time */
	date?: string;
	kind?: string | null;
	currency?: string | null;

	/** @format double */
	amount?: number;
	category?: string | null;

	brokerName = '';
	accountName = '';
	assetIsin = '';
}
